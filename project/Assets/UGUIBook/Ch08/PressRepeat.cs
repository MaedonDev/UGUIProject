// 按住连发：按下立即触发一次，停顿 m_Delay 后开始连发，并且越按越快。数字步进器的 +/- 用它。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class PressRepeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] float m_Delay = 0.4f;          // 首次连发前的停顿
        [SerializeField] float m_Interval = 0.12f;      // 初始连发间隔
        [SerializeField] float m_MinInterval = 0.03f;   // 最快间隔
        public UnityEvent onRepeat = new UnityEvent();

        Selectable m_Selectable;
        bool m_Down;
        float m_Next, m_CurInterval;

        //#region repeat
        void Awake() => m_Selectable = GetComponent<Selectable>();

        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || (m_Selectable != null && !m_Selectable.IsInteractable())) return;
            m_Down = true;
            m_CurInterval = m_Interval;
            m_Next = Time.unscaledTime + m_Delay;
            onRepeat.Invoke();                       // 按下立刻生效一次，手感更跟手
        }

        public void OnPointerUp(PointerEventData e) => m_Down = false;
        public void OnPointerExit(PointerEventData e) => m_Down = false;   // 手指滑出按钮也停止

        void Update()
        {
            if (!m_Down) return;
            if (m_Selectable != null && !m_Selectable.IsInteractable()) { m_Down = false; return; }  // 到达上限被禁用时停下
            if (Time.unscaledTime < m_Next) return;
            onRepeat.Invoke();
            m_CurInterval = Mathf.Max(m_MinInterval, m_CurInterval * 0.85f);   // 每次缩短 15%：越按越快
            m_Next = Time.unscaledTime + m_CurInterval;
        }
        //#endregion
    }
}
