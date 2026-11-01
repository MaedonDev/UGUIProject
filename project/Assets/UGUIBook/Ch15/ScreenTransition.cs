// 屏幕转场：全屏 Image + UGUIBook/Ch15/Transition 材质，三种形状（斜向擦除、圆形收缩、噪声溶解）。
// Cover 把屏幕盖住（可以在回调里切换场景/界面），Reveal 再揭开。
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class ScreenTransition : MonoBehaviour, IMaterialModifier
    {
        //#region fields
        public enum Shape { Wipe = 0, Iris = 1, Dissolve = 2 }
        [SerializeField] Shape m_Shape = Shape.Wipe;
        [SerializeField, Range(0, 1)] float m_Progress;     // 0 = 完全透明，1 = 完全盖住
        [SerializeField] Vector2 m_Center = new Vector2(0.5f, 0.5f);   // 圆形收缩的圆心（屏幕 0~1）
        [SerializeField] float m_Duration = 0.5f;

        Material m_Instance;
        Graphic m_Graphic;
        TweenBase m_Tween;
        //#endregion

        public Shape shape { get => m_Shape; set { m_Shape = value; Dirty(); } }
        public float progress { get => m_Progress; set { m_Progress = Mathf.Clamp01(value); Dirty(); } }
        public Vector2 center { get => m_Center; set { m_Center = value; Dirty(); } }

        //#region api
        public TweenBase Cover(Action onCovered = null)
        {
            m_Tween?.Kill();
            var g = (Graphic)GetComponent<Graphic>(); g.raycastTarget = true;   // 转场期间挡住所有点击
            m_Tween = Tw.Value(this, m_Progress, 1, m_Duration, p => progress = p).SetEase(Ease.InOutCubic)
                        .SetUnscaled().OnComplete(onCovered).Play();
            return m_Tween;
        }

        public TweenBase Reveal()
        {
            m_Tween?.Kill();
            m_Tween = Tw.Value(this, m_Progress, 0, m_Duration, p => progress = p).SetEase(Ease.InOutCubic)
                        .SetUnscaled().OnComplete(() => GetComponent<Graphic>().raycastTarget = false).Play();
            return m_Tween;
        }
        //#endregion

        //#region modifier
        void Dirty()
        {
            if (m_Graphic == null) m_Graphic = GetComponent<Graphic>();
            m_Graphic.SetMaterialDirty();
        }

        // 只有一个全屏元素，用材质实例最省事（不存在“打断合批”的问题）
        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled) return baseMaterial;
            if (m_Instance == null || m_Instance.shader != baseMaterial.shader)
                m_Instance = new Material(baseMaterial) { hideFlags = HideFlags.HideAndDontSave };
            m_Instance.CopyPropertiesFromMaterial(baseMaterial);
            var rect = ((RectTransform)transform).rect;
            m_Instance.SetFloat("_Progress", m_Progress);
            m_Instance.SetFloat("_Mode", (float)m_Shape);
            m_Instance.SetVector("_Center", m_Center);
            m_Instance.SetFloat("_Aspect", rect.height > 0 ? rect.width / rect.height : 1);
            return m_Instance;
        }

        void OnEnable() => Dirty();
        void OnDestroy() { if (m_Instance != null) DestroyImmediate(m_Instance); }
#if UNITY_EDITOR
        void OnValidate() => Dirty();
#endif
        //#endregion
    }
}
