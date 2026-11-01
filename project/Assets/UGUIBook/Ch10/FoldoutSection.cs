// 案例 10.5：可折叠分组列表。
// 结构：Section(VLG) → Header(按钮) + Body(RectMask2D + LayoutElement) → Content(VLG + ContentSizeFitter)
// Body 的高度由 LayoutElement.preferredHeight 决定，我们把它在 0 和 Content 的真实高度之间插值，
// 外层的 VerticalLayoutGroup 会自动把下面的分组往下推/往上收——动画、排版都交给布局系统。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    public class FoldoutSection : MonoBehaviour
    {
        [SerializeField] Button m_Header;
        [SerializeField] RectTransform m_Arrow;
        [SerializeField] LayoutElement m_Body;       // 带 RectMask2D，把没展开的部分裁掉
        [SerializeField] RectTransform m_Content;    // 带 VerticalLayoutGroup + ContentSizeFitter
        [SerializeField] bool m_Expanded = true;
        [SerializeField] float m_Duration = 0.2f;

        float m_Progress;   // 0 = 收起，1 = 展开

        public bool expanded => m_Expanded;

        void Awake()
        {
            m_Header.onClick.AddListener(() => SetExpanded(!m_Expanded));
            SetProgress(m_Expanded ? 1 : 0);
        }

        //#region anim
        public void SetExpanded(bool on, bool instant = false)
        {
            m_Expanded = on;
            if (instant) SetProgress(on ? 1 : 0);
        }

        void Update()
        {
            float target = m_Expanded ? 1 : 0;
            if (Mathf.Approximately(m_Progress, target))
            {
                // 静止时只做一次比较：内容增删导致首选高度变了才重新设置（值没变时 setter 不会标脏）
                SetProgress(m_Progress);
                return;
            }
            float step = m_Duration <= 0 ? 1 : Time.unscaledDeltaTime / m_Duration;
            SetProgress(Mathf.MoveTowards(m_Progress, target, step));
        }

        /// 直接设到某个进度（0~1），配图和编辑器预览也用它
        public void SetProgress(float t)
        {
            m_Progress = Mathf.Clamp01(t);
            float eased = 1 - (1 - m_Progress) * (1 - m_Progress);           // 缓出
            // Content 的高度由它自己的 ContentSizeFitter 决定，这里读“首选高度”而不是 rect，
            // 这样即使 Content 本帧刚加了子项、rect 还没更新，也能拿到正确的目标值
            float full = LayoutUtility.GetPreferredHeight(m_Content);
            m_Body.preferredHeight = full * eased;                            // setter 内部会 MarkLayoutForRebuild
            if (m_Arrow != null) m_Arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(0, -90, eased));  // 箭头 → 转成 ↓
        }
        //#endregion
    }
}
