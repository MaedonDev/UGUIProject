// 横竖嵌套滚动：竖向列表里放横向列表时，手指斜着拖，事件只会发给最里层的 ScrollRect，外层完全不动。
// 解决办法：内层在开始拖拽时判断方向——不是自己负责的方向，就把这一整次拖拽转交给外层 ScrollRect。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    public class NestedScrollRect : ScrollRect
    {
        ScrollRect m_Parent;
        bool m_RouteToParent;

        protected override void Awake()
        {
            base.Awake();
            if (transform.parent != null) m_Parent = transform.parent.GetComponentInParent<ScrollRect>();
        }

        //#region route
        public override void OnInitializePotentialDrag(PointerEventData e)
        {
            // 按下时外层也要做同样的准备（清零惯性速度），否则外层还在惯性滑动时拖不住
            if (m_Parent != null) m_Parent.OnInitializePotentialDrag(e);
            base.OnInitializePotentialDrag(e);
        }

        public override void OnBeginDrag(PointerEventData e)
        {
            // 用“按下点 → 当前点”的总位移判断方向，比单帧 delta 稳定
            Vector2 d = e.position - e.pressPosition;
            bool mostlyHorizontal = Mathf.Abs(d.x) > Mathf.Abs(d.y);
            m_RouteToParent = m_Parent != null && ((!horizontal && mostlyHorizontal) || (!vertical && !mostlyHorizontal));
            if (m_RouteToParent) m_Parent.OnBeginDrag(e);
            else base.OnBeginDrag(e);
        }

        public override void OnDrag(PointerEventData e)
        {
            if (m_RouteToParent) m_Parent.OnDrag(e);
            else base.OnDrag(e);
        }

        public override void OnEndDrag(PointerEventData e)
        {
            if (m_RouteToParent) m_Parent.OnEndDrag(e);
            else base.OnEndDrag(e);
            m_RouteToParent = false;
        }

        public override void OnScroll(PointerEventData e)
        {
            // 鼠标滚轮：横向列表收到竖向滚动时也交给外层，否则鼠标停在横排上滚轮就“失灵”
            if (m_Parent != null && !vertical && Mathf.Abs(e.scrollDelta.y) > Mathf.Abs(e.scrollDelta.x)) m_Parent.OnScroll(e);
            else base.OnScroll(e);
        }
        //#endregion
    }
}
