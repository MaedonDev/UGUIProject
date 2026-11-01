// 圆环 / 扇形进度：一个组件画出“底槽 + 进度弧 + 圆头”，颜色可沿弧渐变；厚度 ≥ 半径时就是扇形（饼图）。
// 还实现了 ICanvasRaycastFilter：只有点在圆环上才算命中（用来做环形菜单）。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Ring Progress")]
    public class RingProgress : MaskableGraphic, ICanvasRaycastFilter
    {
        //#region fields
        [SerializeField, Range(0, 1)] float m_Fill = 0.75f;
        [SerializeField] float m_Thickness = 24;              // 环的厚度；≥ 半径时画成实心扇形
        [SerializeField] float m_StartAngle = 90;             // 起点角度（度），90 = 12 点钟方向
        [SerializeField] bool m_Clockwise = true;
        [SerializeField, Range(3, 256)] int m_Segments = 96;  // 整圆用多少段；实际段数按 fill 比例缩减
        [SerializeField] bool m_RoundCaps = false;            // 两端画成半圆头
        [SerializeField] Color m_EndColor = Color.white;      // 弧末端颜色（与 color 渐变）
        [SerializeField] bool m_UseGradient = false;
        [SerializeField] bool m_ShowTrack = false;            // 同一网格里顺带画出底槽（省一个 Graphic）
        [SerializeField] Color m_TrackColor = new Color(0, 0, 0, 0.1f);
        [SerializeField, Range(0, 3)] float m_Feather = 1;
        [SerializeField] bool m_RaycastFilledOnly = false;    // 射线只命中已填充的弧段

        public float fill { get => m_Fill; set { m_Fill = Mathf.Clamp01(value); SetVerticesDirty(); } }
        public float thickness { get => m_Thickness; set { m_Thickness = value; SetVerticesDirty(); } }
        public float startAngle { get => m_StartAngle; set { m_StartAngle = value; SetVerticesDirty(); } }
        public bool clockwise { get => m_Clockwise; set { m_Clockwise = value; SetVerticesDirty(); } }
        public int segments { get => m_Segments; set { m_Segments = Mathf.Clamp(value, 3, 256); SetVerticesDirty(); } }
        public bool roundCaps { get => m_RoundCaps; set { m_RoundCaps = value; SetVerticesDirty(); } }
        public Color endColor { get => m_EndColor; set { m_EndColor = value; SetVerticesDirty(); } }
        public bool useGradient { get => m_UseGradient; set { m_UseGradient = value; SetVerticesDirty(); } }
        public bool showTrack { get => m_ShowTrack; set { m_ShowTrack = value; SetVerticesDirty(); } }
        public Color trackColor { get => m_TrackColor; set { m_TrackColor = value; SetVerticesDirty(); } }
        public float feather { get => m_Feather; set { m_Feather = value; SetVerticesDirty(); } }
        public bool raycastFilledOnly { get => m_RaycastFilledOnly; set => m_RaycastFilledOnly = value; }
        //#endregion

        static readonly List<Vector2> s_Pts = new List<Vector2>();

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            Vector2 c = r.center;
            float outer = Mathf.Min(r.width, r.height) * 0.5f;
            float inner = Mathf.Max(0, outer - m_Thickness);
            float dir = m_Clockwise ? -1 : 1;                 // UI 坐标 y 向上，角度增加是逆时针
            float a0 = m_StartAngle * Mathf.Deg2Rad;

            if (m_ShowTrack)
                Band(vh, c, inner, outer, a0, a0 + dir * Mathf.PI * 2, m_Segments, m_TrackColor, m_TrackColor, false);

            if (m_Fill <= 0) return;
            float sweep = dir * Mathf.PI * 2 * m_Fill;
            int seg = Mathf.Max(1, Mathf.CeilToInt(m_Segments * m_Fill));
            Color endC = m_UseGradient ? m_EndColor : color;
            bool full = m_Fill >= 0.9999f;
            Band(vh, c, inner, outer, a0, a0 + sweep, seg, color, endC, full);

            // 圆头：在弧的两端各补一个半圆（圆心在环厚度的中线上）
            if (m_RoundCaps && !full && inner > 0)
            {
                float mid = (inner + outer) * 0.5f, capR = (outer - inner) * 0.5f;
                Cap(vh, c + Polar(a0, mid), capR, a0 - dir * Mathf.PI * 0.5f, color);
                Cap(vh, c + Polar(a0 + sweep, mid), capR, a0 + sweep + dir * Mathf.PI * 0.5f, endC);
            }
        }

        /// 一段“环带”：内外两条圆弧之间铺三角形带；inner = 0 时退化成扇形
        void Band(VertexHelper vh, Vector2 c, float inner, float outer, float a0, float a1, int seg,
            Color colA, Color colB, bool closed)
        {
            float f = m_Feather;
            int count = seg + 1;
            int iOuter = vh.currentVertCount;
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg, a = Mathf.Lerp(a0, a1, t);
                vh.AddVert(c + Polar(a, outer - f * 0.5f), Color.Lerp(colA, colB, t), new Vector2(t, 1));
            }
            int iInner = vh.currentVertCount;
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg, a = Mathf.Lerp(a0, a1, t);
                float rin = inner > 0 ? inner + f * 0.5f : 0;
                vh.AddVert(c + Polar(a, rin), Color.Lerp(colA, colB, t), new Vector2(t, 0));
            }
            // 闭合整圆时最后一个点和第一个点重合，用开放带即可（首尾顶点颜色不同，渐变才不会被“抹平”）
            UIMeshUtil.Strip(vh, iOuter, iInner, count, false);
            if (f <= 0) return;
            // 外缘羽化
            int iOuterClear = vh.currentVertCount;
            for (int i = 0; i <= seg; i++)
            {
                float t = (float)i / seg, a = Mathf.Lerp(a0, a1, t);
                Color32 cc = Color.Lerp(colA, colB, t); cc.a = 0;
                vh.AddVert(c + Polar(a, outer + f * 0.5f), cc, new Vector2(t, 1));
            }
            UIMeshUtil.Strip(vh, iOuterClear, iOuter, count, false);
            // 内缘羽化
            if (inner > 0)
            {
                int iInnerClear = vh.currentVertCount;
                for (int i = 0; i <= seg; i++)
                {
                    float t = (float)i / seg, a = Mathf.Lerp(a0, a1, t);
                    Color32 cc = Color.Lerp(colA, colB, t); cc.a = 0;
                    vh.AddVert(c + Polar(a, Mathf.Max(0, inner - f * 0.5f)), cc, new Vector2(t, 0));
                }
                UIMeshUtil.Strip(vh, iInner, iInnerClear, count, false);
            }
        }

        /// 半圆头：朝 facing 方向鼓出去
        void Cap(VertexHelper vh, Vector2 center, float r, float facing, Color col)
        {
            s_Pts.Clear();
            int seg = Mathf.Max(4, m_Segments / 8);
            UIMeshUtil.Arc(s_Pts, center, r - m_Feather * 0.5f, facing - Mathf.PI * 0.5f, facing + Mathf.PI * 0.5f, seg);
            int ic = UIMeshUtil.Vert(vh, center, col, new Vector2(0.5f, 0.5f));
            int first = vh.currentVertCount;
            foreach (var p in s_Pts) vh.AddVert(p, col, new Vector2(0.5f, 0.5f));
            UIMeshUtil.Fan(vh, ic, first, s_Pts.Count, false);
            if (m_Feather <= 0) return;
            Color32 clear = col; clear.a = 0;
            int firstClear = vh.currentVertCount;
            foreach (var p in s_Pts) vh.AddVert(center + (p - center).normalized * (r + m_Feather * 0.5f), clear, new Vector2(0.5f, 0.5f));
            UIMeshUtil.Strip(vh, first, firstClear, s_Pts.Count, false);
        }

        static Vector2 Polar(float a, float r) => new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        //#endregion

        //#region raycast
        /// 射线过滤：GraphicRaycaster 先用矩形粗筛，再调用这里精确判断
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out var local))
                return false;
            Rect r = GetPixelAdjustedRect();
            Vector2 d = local - r.center;
            float outer = Mathf.Min(r.width, r.height) * 0.5f;
            float inner = Mathf.Max(0, outer - m_Thickness);
            float dist = d.magnitude;
            if (dist > outer || dist < inner) return false;
            if (!m_RaycastFilledOnly) return true;
            // 把点的角度换算成“从起点沿绘制方向走了多少圈”，与 fill 比较
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - m_StartAngle;
            if (m_Clockwise) ang = -ang;
            ang = Mathf.Repeat(ang, 360f);
            return ang / 360f <= m_Fill;
        }
        //#endregion
    }
}
