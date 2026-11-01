// 倾斜：x += (y - 轴心y) × tan(角度)。按钮做成平行四边形、文字假斜体都能用；顶点数不变，几乎零开销。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [AddComponentMenu("UGUIBook/Ch13/Skew")]
    public class SkewEffect : BaseMeshEffect
    {
        //#region skew
        [SerializeField, Range(-45, 45)] float m_Angle = 15;
        public float angle { get => m_Angle; set { m_Angle = value; Dirty(); } }

        void Dirty() { if (graphic != null) graphic.SetVerticesDirty(); }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            float k = Mathf.Tan(m_Angle * Mathf.Deg2Rad);
            float pivotY = graphic.rectTransform.rect.center.y;
            var v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                v.position.x += (v.position.y - pivotY) * k;
                vh.SetUIVertex(v, i);
            }
        }
        //#endregion
    }
}
