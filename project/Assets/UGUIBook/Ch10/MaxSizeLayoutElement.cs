// 给布局元素加“最大尺寸”：UGUI 的 LayoutElement 只有 min / preferred / flexible，没有 max。
// 做法：自己实现 ILayoutElement，用更高的 layoutPriority 盖住同物体上其他组件（Image、LayoutGroup、TMP）
// 报上来的 preferred 值，并把它夹在 maxWidth / maxHeight 以内。聊天气泡、提示框都用得上。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class MaxSizeLayoutElement : UIBehaviour, ILayoutElement
    {
        [Tooltip("最大宽度，<=0 表示不限制")]
        [SerializeField] float m_MaxWidth = 560;
        [Tooltip("最大高度，<=0 表示不限制")]
        [SerializeField] float m_MaxHeight = -1;
        [Tooltip("要高于同物体上其他 ILayoutElement（Image/LayoutGroup/TMP 都是 0，LayoutElement 默认 1）")]
        [SerializeField] int m_Priority = 2;

        public float maxWidth { get => m_MaxWidth; set { m_MaxWidth = value; SetDirty(); } }
        public float maxHeight { get => m_MaxHeight; set { m_MaxHeight = value; SetDirty(); } }

        //#region core
        // 计算阶段什么都不用做：真正的值在属性 getter 里现算
        public void CalculateLayoutInputHorizontal() { }
        public void CalculateLayoutInputVertical() { }

        // 返回 -1 = “这一项我不管”，LayoutUtility 会跳过它、改用其他组件的值
        public float minWidth => -1;
        public float minHeight => -1;
        public float flexibleWidth => -1;
        public float flexibleHeight => -1;
        public int layoutPriority => m_Priority;

        public float preferredWidth => Clamp(OthersPreferred(0), m_MaxWidth);
        public float preferredHeight => Clamp(OthersPreferred(1), m_MaxHeight);

        static float Clamp(float v, float max) => max > 0 ? Mathf.Min(v, max) : -1;

        // 同一物体上“除我以外”的组件给出的 preferred 值（规则和 LayoutUtility 完全一致：取最高优先级里的最大值）
        float OthersPreferred(int axis)
        {
            var rt = (RectTransform)transform;
            return LayoutUtility.GetLayoutProperty(rt,
                e => ReferenceEquals(e, this) ? -1 : (axis == 0 ? e.preferredWidth : e.preferredHeight), 0);
        }
        //#endregion

        protected override void OnEnable() { base.OnEnable(); SetDirty(); }
        protected override void OnDisable() { SetDirty(); base.OnDisable(); }

        void SetDirty()
        {
            if (!IsActive()) return;
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }

#if UNITY_EDITOR
        protected override void OnValidate() { SetDirty(); }
#endif
    }
}
