// 把一个 0~1 的参数写进每个顶点的 uv1.x（顺带把局部坐标写进 uv1.zw）。配合本章 Shader 的 _UseVertexParam 开关，
// 每个 UI 元素可以有自己的“进度/强度”，却仍然共用同一个材质——合批不会被打断。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    [AddComponentMenu("UGUIBook/Ch14/UI Param Effect")]
    public class UIParamEffect : BaseMeshEffect
    {
        //#region param
        [SerializeField, Range(0, 1)] float m_Value = 0.5f;

        public float value
        {
            get => m_Value;
            set
            {
                if (Mathf.Approximately(m_Value, value)) return;
                m_Value = value;
                if (graphic != null) graphic.SetVerticesDirty();   // 只重建这一个元素的网格，材质不变
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // 画布默认只把 position/color/uv0 送进 Shader，要用 uv1 必须打开这个通道
            var c = graphic != null ? graphic.canvas : null;
            if (c != null) c.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            // uv1 = (参数, 0, 局部u, 局部v)。局部坐标是顶点在 RectTransform 矩形里的相对位置，
            // 九宫格、图集都不会影响它——流光、溶解这类“按形状铺开”的效果用它比用贴图 uv 稳
            Rect r = graphic.rectTransform.rect;
            var v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                float lu = (v.position.x - r.xMin) / Mathf.Max(r.width, 0.0001f);
                float lv = (v.position.y - r.yMin) / Mathf.Max(r.height, 0.0001f);
                v.uv1 = new Vector4(m_Value, 0, lu, lv);
                vh.SetUIVertex(v, i);
            }
        }
        //#endregion
    }
}
