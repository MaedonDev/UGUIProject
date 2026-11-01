// 贝塞尔连线：把两个 RectTransform（节点上的“端口”）用三次贝塞尔曲线连起来，节点编辑器、技能树、任务连线都用得上。
// 支持粗细、首尾渐变、虚线、末端箭头。端口移动时自动重建网格。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Bezier Link")]
    public class BezierLink : MaskableGraphic
    {
        //#region fields
        [SerializeField] RectTransform m_From, m_To;
        [SerializeField] float m_Width = 6;
        [SerializeField, Range(4, 128)] int m_Segments = 48;
        [SerializeField, Range(0, 1)] float m_Tangent = 0.5f;   // 控制柄长度 = 水平距离 × tangent
        [SerializeField] Color m_EndColor = Color.white;
        [SerializeField] bool m_UseGradient = false;
        [SerializeField] float m_Dash = 0, m_Gap = 10;          // dash > 0 时画虚线
        [SerializeField] float m_ArrowSize = 0;                 // > 0 时在末端画箭头

        public RectTransform from { get => m_From; set { m_From = value; SetVerticesDirty(); } }
        public RectTransform to { get => m_To; set { m_To = value; SetVerticesDirty(); } }
        public float width { get => m_Width; set { m_Width = value; SetVerticesDirty(); } }
        public float dash { get => m_Dash; set { m_Dash = value; SetVerticesDirty(); } }
        public float arrowSize { get => m_ArrowSize; set { m_ArrowSize = value; SetVerticesDirty(); } }
        public Color endColor { get => m_EndColor; set { m_EndColor = value; m_UseGradient = true; SetVerticesDirty(); } }
        //#endregion

        readonly List<Vector2> m_Pts = new List<Vector2>();
        readonly List<Vector2> m_Run = new List<Vector2>();
        Vector3 m_LastFrom, m_LastTo;

        //#region track
        // 端口在别的节点下面，它们移动时本组件收不到任何通知，所以每帧比较一下世界坐标
        void LateUpdate()
        {
            if (m_From == null || m_To == null) return;
            Vector3 f = m_From.position, t = m_To.position;
            if (f != m_LastFrom || t != m_LastTo)
            {
                m_LastFrom = f; m_LastTo = t;
                SetVerticesDirty();
            }
        }
        //#endregion

        Vector2 LocalCenter(RectTransform rt) =>
            rectTransform.InverseTransformPoint(rt.TransformPoint(rt.rect.center));

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_From == null || m_To == null) return;
            Vector2 p0 = LocalCenter(m_From), p3 = LocalCenter(m_To);
            // 节点编辑器风格：从左节点右侧水平出发、水平进入右节点
            float h = Mathf.Max(40, Mathf.Abs(p3.x - p0.x) * m_Tangent);
            Vector2 p1 = p0 + new Vector2(h, 0), p2 = p3 - new Vector2(h, 0);

            m_Pts.Clear();
            for (int i = 0; i <= m_Segments; i++) m_Pts.Add(Bezier(p0, p1, p2, p3, (float)i / m_Segments));

            // 末端留出箭头的长度
            if (m_ArrowSize > 0) TrimEnd(m_Pts, m_ArrowSize * 0.8f);

            Color endC = m_UseGradient ? m_EndColor : color;
            if (m_Dash <= 0) Emit(vh, m_Pts, color, endC, 0, 1);
            else
            {
                // 虚线：沿弧长走，在每个“实/虚”交界处精确切断，实的部分收集成一小段折线输出
                float total = Length(m_Pts), pos = 0, period = m_Dash + m_Gap;
                m_Run.Clear();
                for (int i = 1; i < m_Pts.Count; i++)
                {
                    Vector2 a = m_Pts[i - 1], b = m_Pts[i];
                    float L = Vector2.Distance(a, b), t0 = 0;
                    if (L < 1e-4f) continue;
                    while (t0 < L)
                    {
                        float phase = (pos + t0) % period;
                        if (phase < m_Dash)
                        {
                            float remain = m_Dash - phase, t1 = Mathf.Min(L, t0 + Mathf.Max(remain, 1e-3f));
                            if (m_Run.Count == 0) m_Run.Add(Vector2.Lerp(a, b, t0 / L));
                            m_Run.Add(Vector2.Lerp(a, b, t1 / L));
                            if (t1 - t0 >= remain - 1e-4f) Flush(vh, endC, (pos + t1) / total);   // 一段实线走完
                            t0 = t1;
                        }
                        else t0 = Mathf.Min(L, t0 + Mathf.Max(period - phase, 1e-3f));   // 最小步长防止浮点误差卡死
                    }
                    pos += L;
                }
                Flush(vh, endC, 1);
            }

            if (m_ArrowSize > 0)
            {
                Vector2 tip = p3, dir = (p3 - m_Pts[m_Pts.Count - 1]).normalized;
                if (dir.sqrMagnitude < 0.5f) dir = Vector2.right;
                Vector2 nrm = new Vector2(-dir.y, dir.x);
                Vector2 back = tip - dir * m_ArrowSize;
                int k = vh.currentVertCount;
                vh.AddVert(tip, endC, Vector2.zero);
                vh.AddVert(back + nrm * m_ArrowSize * 0.55f, endC, Vector2.zero);
                vh.AddVert(back - nrm * m_ArrowSize * 0.55f, endC, Vector2.zero);
                vh.AddTriangle(k, k + 1, k + 2);
            }
        }

        void Flush(VertexHelper vh, Color endC, float t)
        {
            if (m_Run.Count >= 2) { var c = Color.Lerp(color, endC, t); Emit(vh, m_Run, c, c, 0, 1); }
            m_Run.Clear();
        }

        void Emit(VertexHelper vh, List<Vector2> pts, Color a, Color b, float t0, float t1)
        {
            // 渐变：先按单色生成，再按顶点序号改颜色（PolyLine 每个点生成 4 个顶点）
            int start = vh.currentVertCount;
            UIMeshUtil.PolyLine(vh, pts, m_Width, Color.white);
            var v = new UIVertex();
            for (int i = start; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                float t = Mathf.Lerp(t0, t1, (float)((i - start) / 4) / Mathf.Max(1, pts.Count - 1));
                Color32 c = Color.Lerp(a, b, t);
                c.a = (byte)(c.a * v.color.a / 255);   // 保留羽化顶点的 alpha = 0
                v.color = c;
                vh.SetUIVertex(v, i);
            }
        }

        static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1 - t;
            return u * u * u * p0 + 3 * u * u * t * p1 + 3 * u * t * t * p2 + t * t * t * p3;
        }

        static float Length(List<Vector2> pts)
        {
            float l = 0;
            for (int i = 1; i < pts.Count; i++) l += Vector2.Distance(pts[i - 1], pts[i]);
            return Mathf.Max(l, 0.001f);
        }

        static void TrimEnd(List<Vector2> pts, float len)
        {
            while (pts.Count > 2 && Vector2.Distance(pts[pts.Count - 1], pts[pts.Count - 2]) < len)
            {
                len -= Vector2.Distance(pts[pts.Count - 1], pts[pts.Count - 2]);
                pts.RemoveAt(pts.Count - 1);
            }
            int n = pts.Count;
            pts[n - 1] = Vector2.MoveTowards(pts[n - 1], pts[n - 2], len);
        }
        //#endregion
    }
}
