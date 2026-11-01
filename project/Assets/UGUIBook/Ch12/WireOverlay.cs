// 自定义 Graphic：把另一个 Graphic（挂了 MeshStats）的三角形画成线框，叠在它上面。
// 也是一个 OnPopulateMesh 的小例子：每条边就是一个细长的四边形。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    // 注意：Graphic 基类只要求 RectTransform。自定义 Graphic 一定要自己声明需要 CanvasRenderer，
    // 否则用 AddComponent 加到空物体上时没有 CanvasRenderer，重建时会抛 MissingComponentException（本书实测踩到）
    [ExecuteAlways]
    [RequireComponent(typeof(CanvasRenderer))]
    public class WireOverlay : MaskableGraphic
    {
        [SerializeField] MeshStats m_Source;
        [SerializeField] float m_Thickness = 2f;

        // Graphic 基类默认 useLegacyMeshGeneration = true（走 Mesh 版本的旧流程），Image/Text 都在构造函数里改成了 false
        protected WireOverlay() { useLegacyMeshGeneration = false; }

        public MeshStats source { get => m_Source; set { m_Source = value; SetVerticesDirty(); } }

        //#region populate
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (m_Source == null) return;
            var src = m_Source.transform;
            var pts = m_Source.positions;
            for (int t = 0; t + 2 < pts.Count; t += 3)
                for (int e = 0; e < 3; e++)
                {
                    // 源物体本地坐标 → 世界 → 本物体本地坐标
                    Vector2 a = transform.InverseTransformPoint(src.TransformPoint(pts[t + e]));
                    Vector2 b = transform.InverseTransformPoint(src.TransformPoint(pts[t + (e + 1) % 3]));
                    AddLine(vh, a, b);
                }
        }

        void AddLine(VertexHelper vh, Vector2 a, Vector2 b)
        {
            Vector2 d = b - a;
            if (d.sqrMagnitude < 1e-6f) return;
            Vector2 n = new Vector2(-d.y, d.x).normalized * (m_Thickness * 0.5f);
            int i = vh.currentVertCount;
            Color32 c = color;
            vh.AddVert(a - n, c, Vector2.zero); vh.AddVert(a + n, c, Vector2.zero);
            vh.AddVert(b + n, c, Vector2.zero); vh.AddVert(b - n, c, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2); vh.AddTriangle(i + 2, i + 3, i);
        }
        //#endregion
    }
}
