// 折线图：可选平滑（Catmull-Rom 样条）、面积渐变填充、数据点、水平网格线。数据改了调 SetValues 即可。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Line Chart")]
    public class LineChart : MaskableGraphic
    {
        //#region fields
        [SerializeField] List<float> m_Values = new List<float> { 3, 5, 4, 8, 6, 9, 7 };
        [SerializeField] float m_Min = 0, m_Max = 10;          // 纵轴范围
        [SerializeField] float m_LineWidth = 6;
        [SerializeField] bool m_Smooth = false;
        [SerializeField, Range(2, 32)] int m_SmoothSteps = 12;  // 平滑时每段细分几步
        [SerializeField] bool m_FillArea = true;
        [SerializeField] Color m_FillTop = new Color(0.36f, 0.29f, 0.84f, 0.35f);
        [SerializeField] Color m_FillBottom = new Color(0.36f, 0.29f, 0.84f, 0f);
        [SerializeField] float m_PointRadius = 8;
        [SerializeField, Range(0, 10)] int m_GridLines = 4;
        [SerializeField] Color m_GridColor = new Color(0.12f, 0.14f, 0.21f, 0.12f);

        public void SetValues(IList<float> values) { m_Values.Clear(); m_Values.AddRange(values); SetVerticesDirty(); }
        public bool smooth { get => m_Smooth; set { m_Smooth = value; SetVerticesDirty(); } }
        public bool fillArea { get => m_FillArea; set { m_FillArea = value; SetVerticesDirty(); } }
        public float pointRadius { get => m_PointRadius; set { m_PointRadius = value; SetVerticesDirty(); } }
        public float lineWidth { get => m_LineWidth; set { m_LineWidth = value; SetVerticesDirty(); } }
        public int count => m_Values.Count;
        //#endregion

        readonly List<Vector2> m_Keys = new List<Vector2>();
        readonly List<Vector2> m_Curve = new List<Vector2>();

        /// 第 i 个数据点在本地坐标中的位置（外部放标签时也用得上）
        public Vector2 PointAt(int i)
        {
            Rect r = GetPixelAdjustedRect();
            float x = m_Values.Count > 1 ? Mathf.Lerp(r.xMin, r.xMax, (float)i / (m_Values.Count - 1)) : r.center.x;
            float y = Mathf.Lerp(r.yMin, r.yMax, Mathf.InverseLerp(m_Min, m_Max, m_Values[i]));
            return new Vector2(x, y);
        }

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            for (int g = 0; g <= m_GridLines && m_GridLines > 0; g++)
            {
                float y = Mathf.Lerp(r.yMin, r.yMax, (float)g / m_GridLines);
                UIMeshUtil.Line(vh, new Vector2(r.xMin, y), new Vector2(r.xMax, y), 2, m_GridColor);
            }
            if (m_Values.Count < 2) return;

            m_Keys.Clear();
            for (int i = 0; i < m_Values.Count; i++) m_Keys.Add(PointAt(i));
            m_Curve.Clear();
            if (!m_Smooth) m_Curve.AddRange(m_Keys);
            else
            {
                // Catmull-Rom：曲线经过每个数据点，切线由前后两个点决定；首尾点用自身补齐
                for (int i = 0; i < m_Keys.Count - 1; i++)
                {
                    Vector2 p0 = m_Keys[Mathf.Max(i - 1, 0)], p1 = m_Keys[i], p2 = m_Keys[i + 1], p3 = m_Keys[Mathf.Min(i + 2, m_Keys.Count - 1)];
                    for (int s = 0; s < m_SmoothSteps; s++)
                        m_Curve.Add(CatmullRom(p0, p1, p2, p3, (float)s / m_SmoothSteps));
                }
                m_Curve.Add(m_Keys[m_Keys.Count - 1]);
            }

            // 面积填充：曲线上每个点往下拉到底边，顶部色 → 底部色 的竖直渐变（靠顶点色插值，不需要贴图）
            if (m_FillArea)
            {
                for (int i = 0; i < m_Curve.Count - 1; i++)
                {
                    Vector2 a = m_Curve[i], b = m_Curve[i + 1];
                    int k = vh.currentVertCount;
                    vh.AddVert(new Vector2(a.x, r.yMin), m_FillBottom, Vector2.zero);
                    vh.AddVert(a, Color.Lerp(m_FillBottom, m_FillTop, Mathf.InverseLerp(r.yMin, r.yMax, a.y)), Vector2.zero);
                    vh.AddVert(b, Color.Lerp(m_FillBottom, m_FillTop, Mathf.InverseLerp(r.yMin, r.yMax, b.y)), Vector2.zero);
                    vh.AddVert(new Vector2(b.x, r.yMin), m_FillBottom, Vector2.zero);
                    vh.AddTriangle(k, k + 1, k + 2);
                    vh.AddTriangle(k + 2, k + 3, k);
                }
            }

            UIMeshUtil.PolyLine(vh, m_Curve, m_LineWidth, color);

            if (m_PointRadius > 0)
                foreach (var p in m_Keys)
                {
                    UIMeshUtil.Dot(vh, p, m_PointRadius, color);
                    UIMeshUtil.Dot(vh, p, m_PointRadius * 0.5f, Color.white);
                }
        }

        static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
        }
        //#endregion
    }
}
