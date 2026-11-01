// 自定义 Selectable：页签按钮。继承 Selectable 就自动拥有导航、交互状态和 interactable 支持，
// 只需重写 DoStateTransition 决定每种状态长什么样，再自己实现点击（IPointerClickHandler / ISubmitHandler）。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public class TabButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        //#region fields
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_Label;
        [SerializeField] Image m_Underline;      // 当前页签下方的指示条
        [SerializeField] Image m_Hover;          // 悬停/按下时的浅色底
        [SerializeField] Image m_FocusRing;      // 手柄/键盘选中（Selected）时的焦点框
        [SerializeField] Color m_OnColor = new Color32(0x5B, 0x4B, 0xD6, 255);
        [SerializeField] Color m_OffColor = new Color32(0x7A, 0x82, 0x99, 255);

        public TabBar Bar { get; set; }
        bool m_IsOn;
        //#endregion

        //#region ison
        /// “是不是当前页签”和 Selectable 的 Selected（键盘焦点）是两回事：前者是业务状态，后者是交互状态
        public bool IsOn
        {
            get => m_IsOn;
            set { m_IsOn = value; DoStateTransition(currentSelectionState, true); }
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left && IsInteractable()) Bar?.Select(this);
        }

        public void OnSubmit(BaseEventData e)
        {
            if (IsInteractable()) Bar?.Select(this);
        }
        //#endregion

        //#region transition
        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);      // 保留 Inspector 里 Transition 的效果（本例设为 None）
            // 编辑器里 OnValidate、Reset 也会调到这里，此时引用可能还没拖好：缺一个就先不画
            if (m_Icon == null || m_Label == null || m_Underline == null || m_Hover == null || m_FocusRing == null) return;
            var c = m_IsOn ? m_OnColor : m_OffColor;
            if (state == SelectionState.Highlighted && !m_IsOn) c = Color.Lerp(m_OffColor, m_OnColor, 0.5f);
            if (state == SelectionState.Disabled) c.a = 0.35f;
            m_Icon.color = c;
            m_Label.color = c;
            m_Underline.enabled = m_IsOn;
            m_Hover.enabled = state == SelectionState.Highlighted || state == SelectionState.Pressed;
            m_Hover.color = new Color(m_OnColor.r, m_OnColor.g, m_OnColor.b, state == SelectionState.Pressed ? 0.18f : 0.08f);
            m_FocusRing.enabled = state == SelectionState.Selected;
            transform.localScale = state == SelectionState.Pressed ? new Vector3(0.96f, 0.96f, 1) : Vector3.one;
        }
        //#endregion
    }
}
