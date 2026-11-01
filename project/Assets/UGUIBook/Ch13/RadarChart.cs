// 雷达图（属性图）：背景网格 + 辐条 + 数据多边形 + 描边 + 顶点圆点，全部在一个网格里，一个 Draw Call。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Radar Chart")]
    public class RadarChart : MaskableGraphic, ICanvasRaycastFilter
    {
        //#region fields
        [SerializeField] float[] m_Values = { 0.8f, 0.6f, 0.9f, 0.5f, 0.7f, 0.4f };
        [SerializeField] float m_StartAngle = 90;
        [SerializeField, Range(0, 10)] int m_GridLevels = 4;
        [SerializeField] Color m_GridColor = new Color(0.12f, 0.14f, 0.21f, 0.18f);
        [SerializeField] Color m_GridFill = new Color(1, 1, 1, 0.6f);
        [SerializeField] float m_GridWidth = 2;
        [SerializeField] Color m_LineColor = new Color32(0x5B, 0x4B, 0xD6, 0xFF);
        [SerializeField] float m_LineWidth = 4;
        [SerializeField] float m_DotRadius = 7;
        // 数据区填充色用 Graphic 自带的 color（通常给一点透明度）

        public int axisCount => m_Values != null ? m_Values.Length : 0;
        public float GetValue(int i) => m_Values[i];
        public void SetValue(int i, float v) { m_Values[i] = Mathf.Clamp01(v); SetVerticesDirty(); }
        public void SetValues(params float[] values) { m_Values = (float[])values.Clone(); SetVerticesDirty(); }
        public Color lineColor { get => m_LineColor; set { m_LineColor = value; SetVerticesDirty(); } }
        public Color gridColor { get => m_GridColor; set { m_GridColor = value; SetVerticesDirty(); } }
        public int gridLevels { get => m_GridLevels; set { m_GridLevels = value; SetVerticesDirty(); } }
        //#endregion

        readonly List<Vector2> m_Pts = new List<Vector2>();

        /// 第 i 根轴的单位方向
        public Vector2 AxisDir(int i)
        {
            float a = m_StartAngle * Mathf.Deg2Rad - i * Mathf.PI * 2 / axisCount;   // 顺时针排列
            return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        }

        public float Radius => Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int n = axisCount;
            if (n < 3) return;
            Rect r = GetPixelAdjustedRect();
            Vector2 c = r.center;
            float R = Mathf.Min(r.width, r.height) * 0.5f;

            // 1) 底板：最外层网格多边形填充
            if (m_GridFill.a > 0)
            {
                int ic = UIMeshUtil.Vert(vh, c, m_GridFill, Vector2.zero);
                int first = vh.currentVertCount;
                for (int i = 0; i < n; i++) vh.AddVert(c + AxisDir(i) * R, m_GridFill, Vector2.zero);
                UIMeshUtil.Fan(vh, ic, first, n, true);
            }
            // 2) 网格：同心多边形 + 辐条
            for (int level = 1; level <= m_GridLevels; level++)
            {
                m_Pts.Clear();
                for (int i = 0; i < n; i++) m_Pts.Add(c + AxisDir(i) * R * level / m_GridLevels);
                UIMeshUtil.PolyLine(vh, m_Pts, m_GridWidth, m_GridColor, closed: true);
            }
            for (int i = 0; i < n; i++) UIMeshUtil.Line(vh, c, c + AxisDir(i) * R, m_GridWidth, m_GridColor);

            // 3) 数据多边形：从中心点扇形展开。它不一定是凸多边形，但对中心是“星形”的，扇形三角化不会出错
            m_Pts.Clear();
            for (int i = 0; i < n; i++) m_Pts.Add(c + AxisDir(i) * R * Mathf.Clamp01(m_Values[i]));
            int center = UIMeshUtil.Vert(vh, c, color, Vector2.zero);
            int start = vh.currentVertCount;
            foreach (var p in m_Pts) vh.AddVert(p, color, Vector2.zero);
            UIMeshUtil.Fan(vh, center, start, n, true);

            // 4) 描边与顶点
            UIMeshUtil.PolyLine(vh, m_Pts, m_LineWidth, m_LineColor, closed: true);
            if (m_DotRadius > 0)
                foreach (var p in m_Pts)
                {
                    UIMeshUtil.Dot(vh, p, m_DotRadius + 2, Color.white);
                    UIMeshUtil.Dot(vh, p, m_DotRadius - 1, m_LineColor);
                }
        }
        //#endregion

        //#region raycast
        /// 只有点落在数据多边形内部才算命中（射线法判断点在多边形内）
        public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, sp, cam, out var p)) return false;
            Rect r = GetPixelAdjustedRect();
            float R = Mathf.Min(r.width, r.height) * 0.5f;
            int n = axisCount;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                Vector2 a = r.center + AxisDir(i) * R * m_Values[i], b = r.center + AxisDir(j) * R * m_Values[j];
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }
        //#endregion

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (m_Values == null || m_Values.Length < 3) m_Values = new[] { 0.5f, 0.5f, 0.5f };
        }
#endif
    }
}
