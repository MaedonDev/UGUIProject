// 挂在软遮罩下面的 Graphic 上：把它的材质换成 SoftMask 提供的共享材质。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UGUIBook/Ch14/Soft Maskable")]
    public class SoftMaskable : MonoBehaviour, IMaterialModifier
    {
        //#region maskable
        SoftMask m_Mask;

        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled) return baseMaterial;
            if (m_Mask == null) m_Mask = GetComponentInParent<SoftMask>();
            var mat = m_Mask != null ? m_Mask.GetMaterial() : null;
            return mat != null ? mat : baseMaterial;
        }

        public void Dirty()
        {
            var g = GetComponent<Graphic>();
            if (g != null) g.SetMaterialDirty();
        }

        void OnEnable() { m_Mask = null; Dirty(); }
        void OnDisable() => Dirty();
        void OnTransformParentChanged() { m_Mask = null; Dirty(); }
        //#endregion
    }
}
