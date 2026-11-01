// 顶点渐变：BaseMeshEffect 的入门例子。不改顶点位置，只按顶点在包围盒里的相对位置改颜色。
// 对 Image、Text、自定义 Graphic 都有效（TMP 不走 IMeshModifier，见 13.8 节）。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [AddComponentMenu("UGUIBook/Ch13/Vertex Gradient")]
    public class VertexGradient : BaseMeshEffect
    {
        //#region gradient
        public enum Direction { Vertical, Horizontal, Diagonal }

        [SerializeField] Direction m_Direction = Direction.Vertical;
        [SerializeField] Color m_ColorA = Color.white;            // 上 / 左
        [SerializeField] Color m_ColorB = new Color(1, 0.54f, 0.24f); // 下 / 右
        [SerializeField] bool m_Multiply = true;                   // true：与原顶点色相乘；false：直接替换

        public Color colorA { get => m_ColorA; set { m_ColorA = value; Dirty(); } }
        public Color colorB { get => m_ColorB; set { m_ColorB = value; Dirty(); } }
        public Direction direction { get => m_Direction; set { m_Direction = value; Dirty(); } }

        void Dirty() { if (graphic != null) graphic.SetVerticesDirty(); }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            var v = new UIVertex();
            // 第一遍：求所有顶点的包围盒
            Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                min = Vector2.Min(min, v.position);
                max = Vector2.Max(max, v.position);
            }
            Vector2 size = Vector2.Max(max - min, Vector2.one * 0.0001f);
            // 第二遍：按相对位置插值颜色，写回
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                Vector2 t = new Vector2((v.position.x - min.x) / size.x, (v.position.y - min.y) / size.y);
                float k = m_Direction == Direction.Vertical ? 1 - t.y
                        : m_Direction == Direction.Horizontal ? t.x
                        : (t.x + 1 - t.y) * 0.5f;
                Color g = Color.Lerp(m_ColorA, m_ColorB, k);
                v.color = m_Multiply ? (Color32)(g * v.color) : (Color32)new Color(g.r, g.g, g.b, g.a * v.color.a / 255f);
                vh.SetUIVertex(v, i);
            }
        }
        //#endregion
    }
}
