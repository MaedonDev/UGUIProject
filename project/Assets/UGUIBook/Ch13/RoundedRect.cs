// 无图圆角矩形：不需要任何九宫格贴图，圆角半径、描边宽度都能随意调，还能把一张 Sprite “裁”成圆角（不用 Mask）。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Rounded Rect")]
    public class RoundedRect : MaskableGraphic
    {
        //#region fields
        [SerializeField] float m_Radius = 24;            // 圆角半径（会被限制在短边的一半以内）
        [SerializeField, Range(1, 32)] int m_Segments = 8; // 每个圆角用几段折线逼近
        [SerializeField] float m_BorderWidth = 0;        // 0 = 实心；>0 = 只画一圈描边
        [SerializeField, Range(0, 3)] float m_Feather = 1;  // 边缘羽化宽度（本地单位），起抗锯齿作用
        [SerializeField] Sprite m_Sprite;                // 可选：把这张图按矩形铺满，再裁成圆角

        public float radius { get => m_Radius; set { m_Radius = value; SetVerticesDirty(); } }
        public int segments { get => m_Segments; set { m_Segments = Mathf.Clamp(value, 1, 32); SetVerticesDirty(); } }
        public float borderWidth { get => m_BorderWidth; set { m_BorderWidth = value; SetVerticesDirty(); } }
        public float feather { get => m_Feather; set { m_Feather = value; SetVerticesDirty(); } }
        public Sprite sprite { get => m_Sprite; set { m_Sprite = value; SetAllDirty(); } }

        // 有 Sprite 就用它的贴图，否则用 Graphic 内置的 1×1 白图（这样所有“无图”元素能合批）
        public override Texture mainTexture => m_Sprite != null ? m_Sprite.texture : s_WhiteTexture;
        //#endregion

        static readonly List<Vector2> s_Outer = new List<Vector2>();
        static readonly List<Vector2> s_Normals = new List<Vector2>();

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            float rad = Mathf.Clamp(m_Radius, 0, Mathf.Min(r.width, r.height) * 0.5f);
            float f = Mathf.Max(0, m_Feather);

            // 1) 生成外轮廓：四个圆角各 segments+1 个点，逆时针（右上 → 左上 → 左下 → 右下）
            //    羽化带要“骑”在边界上：实色边缘向内缩 f/2，透明边缘向外扩 f/2，视觉尺寸不变
            BuildContour(r, rad, f * 0.5f, s_Outer, s_Normals);
            int n = s_Outer.Count;
            Color32 c = color;
            Color32 clear = c; clear.a = 0;
            Vector4 uvRect = m_Sprite != null ? UnityEngine.Sprites.DataUtility.GetOuterUV(m_Sprite) : new Vector4(0, 0, 1, 1);

            int outerSolid = vh.currentVertCount;
            for (int i = 0; i < n; i++) vh.AddVert(s_Outer[i], c, UV(r, uvRect, s_Outer[i]));

            if (m_BorderWidth <= 0)
            {
                // 2a) 实心：圆角矩形是凸多边形，从中心点连一圈扇形即可
                int center = UIMeshUtil.Vert(vh, r.center, c, UV(r, uvRect, r.center));
                UIMeshUtil.Fan(vh, center, outerSolid, n, true);
            }
            else
            {
                // 2b) 描边：再生成一圈内轮廓（半径减去描边宽度），两圈之间铺三角形带
                float bw = Mathf.Min(m_BorderWidth, Mathf.Min(r.width, r.height) * 0.5f);
                var inner = new Rect(r.x + bw, r.y + bw, r.width - bw * 2, r.height - bw * 2);
                int innerSolid = vh.currentVertCount;
                for (int i = 0; i < n; i++)
                {
                    // 内圈的点 = 外圈的点沿法线往里收，圆角处正好得到“同心”的小圆角；半径小于描边宽度时用 Clamp 收成直角
                    var p = s_Outer[i] - s_Normals[i] * (bw - f);
                    p.x = Mathf.Clamp(p.x, inner.xMin - f * 0.5f, inner.xMax + f * 0.5f);
                    p.y = Mathf.Clamp(p.y, inner.yMin - f * 0.5f, inner.yMax + f * 0.5f);
                    vh.AddVert(p, c, UV(r, uvRect, p));
                }
                UIMeshUtil.Strip(vh, outerSolid, innerSolid, n, true);
                if (f > 0)
                {
                    int innerClear = vh.currentVertCount;
                    for (int i = 0; i < n; i++)
                    {
                        var p = s_Outer[i] - s_Normals[i] * bw;
                        p.x = Mathf.Clamp(p.x, inner.xMin + f * 0.5f, inner.xMax - f * 0.5f);
                        p.y = Mathf.Clamp(p.y, inner.yMin + f * 0.5f, inner.yMax - f * 0.5f);
                        vh.AddVert(p, clear, UV(r, uvRect, p));
                    }
                    UIMeshUtil.Strip(vh, innerSolid, innerClear, n, true);
                }
            }

            // 3) 外羽化圈：同样的轮廓沿法线外扩 f，alpha = 0
            if (f > 0)
            {
                int outerClear = vh.currentVertCount;
                for (int i = 0; i < n; i++)
                {
                    var p = s_Outer[i] + s_Normals[i] * f;
                    vh.AddVert(p, clear, UV(r, uvRect, p));
                }
                UIMeshUtil.Strip(vh, outerSolid, outerClear, n, true);
            }
        }

        void BuildContour(Rect r, float rad, float inset, List<Vector2> pts, List<Vector2> normals)
        {
            pts.Clear(); normals.Clear();
            // 四个圆角的圆心（已向内缩 inset）和起始角
            var centers = new[]
            {
                new Vector2(r.xMax - rad, r.yMax - rad), new Vector2(r.xMin + rad, r.yMax - rad),
                new Vector2(r.xMin + rad, r.yMin + rad), new Vector2(r.xMax - rad, r.yMin + rad),
            };
            float rr = Mathf.Max(0, rad - inset);
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i <= m_Segments; i++)
                {
                    float a = (k * 90f + 90f * i / m_Segments) * Mathf.Deg2Rad;
                    var nrm = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    // 半径为 0 时圆心就是角点，向内缩 inset 要沿对角方向，保持直角
                    var corner = centers[k] + nrm * rr;
                    if (rad < inset)
                        corner = centers[k] - new Vector2(Mathf.Sign(centers[k].x - r.center.x), Mathf.Sign(centers[k].y - r.center.y)) * (inset - rad);
                    pts.Add(corner);
                    normals.Add(nrm);
                }
            }
        }

        static Vector2 UV(Rect r, Vector4 uvRect, Vector2 p)
        {
            float u = (p.x - r.xMin) / r.width, v = (p.y - r.yMin) / r.height;
            return new Vector2(Mathf.Lerp(uvRect.x, uvRect.z, u), Mathf.Lerp(uvRect.y, uvRect.w, v));
        }
        //#endregion
    }
}
