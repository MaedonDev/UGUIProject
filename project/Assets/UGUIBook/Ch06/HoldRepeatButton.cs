// 长按连发：按住不放时，先等一小段延迟，然后越来越快地重复触发（数量加减、音量调节常用）。
// 挂在任何 Selectable（通常是 Button）上，和 onClick 并存：点一下 = 触发一次。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    [RequireComponent(typeof(Selectable))]
    public class HoldRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        //#region fields
        [SerializeField] float m_InitialDelay = 0.4f;    // 按下多久后开始连发（短于它就是普通点击）
        [SerializeField] float m_StartInterval = 0.15f;  // 刚开始的连发间隔
        [SerializeField] float m_MinInterval = 0.03f;    // 最快的间隔
        [SerializeField] float m_Acceleration = 0.85f;   // 每次触发后间隔乘以它，越按越快
        public UnityEvent onRepeat = new UnityEvent();
        //#endregion

        Selectable m_Selectable;
        bool m_Holding;
        float m_NextTime, m_Interval;

        void Awake() => m_Selectable = GetComponent<Selectable>();

        //#region logic
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || !m_Selectable.IsInteractable()) return;
            m_Holding = true;
            m_Interval = m_StartInterval;
            m_NextTime = Time.unscaledTime + m_InitialDelay;    // 用不受 timeScale 影响的时间：暂停菜单里也能用
        }

        public void OnPointerUp(PointerEventData e) => m_Holding = false;
        public void OnPointerExit(PointerEventData e) => m_Holding = false;   // 手指滑出按钮就停
        void OnDisable() => m_Holding = false;

        void Update()
        {
            if (!m_Holding) return;
            if (!m_Selectable.IsInteractable()) { m_Holding = false; return; }   // 比如数量到上限后按钮被禁用
            while (Time.unscaledTime >= m_NextTime)
            {
                onRepeat.Invoke();
                m_Interval = Mathf.Max(m_MinInterval, m_Interval * m_Acceleration);
                m_NextTime += m_Interval;
            }
        }
        //#endregion
    }
}
