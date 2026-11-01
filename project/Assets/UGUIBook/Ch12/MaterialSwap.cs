// IMaterialModifier 示例：在不改 Graphic.material 的前提下，临时把渲染用的材质换成另一个。
// Graphic.materialForRendering 会依次调用同物体上所有 IMaterialModifier（Mask、MaskableGraphic 自己也是）。
// 典型用途：禁用态换灰度材质、高亮时换描边材质。注意：换了材质就和别人合不了批（12.6 节实测）。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    //#region swap
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class MaterialSwap : UIBehaviour, IMaterialModifier
    {
        [SerializeField] Material m_Material;

        public Material material { get => m_Material; set { m_Material = value; Refresh(); } }

        public Material GetModifiedMaterial(Material baseMaterial) =>
            isActiveAndEnabled && m_Material != null ? m_Material : baseMaterial;

        // 自己启用/禁用/改参数时，通知 Graphic“材质脏了”，下一次 PreRender 会重新调 materialForRendering
        protected override void OnEnable() => Refresh();
        protected override void OnDisable() => Refresh();
#if UNITY_EDITOR
        protected override void OnValidate() => Refresh();
#endif
        void Refresh() { var g = GetComponent<Graphic>(); if (g != null) g.SetMaterialDirty(); }
    }
    //#endregion
}
