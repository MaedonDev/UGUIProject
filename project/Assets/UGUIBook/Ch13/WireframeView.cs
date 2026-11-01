// 线框视图：把目标 Graphic 的网格画成“红线 + 顶点圆点”。挂在目标的子物体上（撑满父物体，轴心与父物体相同），
// 因为子物体后画，线框会盖在目标上面。书中所有线框图都是它画的。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Wireframe View")]
    public class WireframeView : MaskableGraphic
    {
        [SerializeField] Graphic m_Target;
        [SerializeField] float m_LineWidth = 1.5f;
        [SerializeField] float m_DotRadius = 3f;
        [SerializeField] bool m_ShowTransparent = true;   // 也画出 alpha=0 的羽化顶点（用另一种颜色）
        [SerializeField] Color m_ClearColor = new Color(0.1f, 0.55f, 0.95f, 1f);

        public int vertexCount { get; private set; }
        public int triangleCount { get; private set; }

        public Graphic target { get => m_Target; set { m_Target = value; SetVerticesDirty(); } }
        public float lineWidth { get => m_LineWidth; set { m_LineWidth = value; SetVerticesDirty(); } }
        public float dotRadius { get => m_DotRadius; set { m_DotRadius = value; SetVerticesDirty(); } }
        public bool showTransparent { get => m_ShowTransparent; set { m_ShowTransparent = value; SetVerticesDirty(); } }

        protected override void Awake() { base.Awake(); raycastTarget = false; }

        /// 让线框与目标坐标系一致：撑满父物体、轴心对齐
        public void FitParent()
        {
            var rt = rectTransform; var p = (RectTransform)rt.parent;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = p.pivot;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_Target == null || m_Target == this) return;
            var verts = new List<UIVertex>();
            using (var src = new VertexHelper())
            {
                MeshProbe.Build(m_Target, src);
                vertexCount = src.currentVertCount;
                triangleCount = src.currentIndexCount / 3;
                src.GetUIVertexStream(verts);   // 三角形流：每 3 个一个三角形
                var v = new UIVertex();
                var unique = new List<UIVertex>();
                for (int i = 0; i < src.currentVertCount; i++) { src.PopulateUIVertex(ref v, i); unique.Add(v); }
                for (int i = 0; i + 2 < verts.Count; i += 3)
                {
                    Color32 lc = color;
                    if (verts[i].color.a == 0 && verts[i + 1].color.a == 0 || verts[i + 1].color.a == 0 && verts[i + 2].color.a == 0 || verts[i].color.a == 0 && verts[i + 2].color.a == 0)
                    {
                        if (!m_ShowTransparent) continue;
                        lc = m_ClearColor;
                    }
                    for (int k = 0; k < 3; k++)
                        UIMeshUtil.Line(vh, verts[i + k].position, verts[i + (k + 1) % 3].position, m_LineWidth, lc);
                }
                if (m_DotRadius > 0)
                    foreach (var u in unique)
                        UIMeshUtil.Dot(vh, u.position, m_DotRadius, u.color.a == 0 ? (Color32)m_ClearColor : (Color32)color, 8, 0);
            }
        }
    }
}
