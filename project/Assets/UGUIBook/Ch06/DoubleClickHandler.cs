// 区分单击和双击：第一次点击先不急着响应，等过了双击时间窗还没有第二次点击，才算单击。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch06
{
    public class DoubleClickHandler : MonoBehaviour, IPointerClickHandler
    {
        //#region main
        [SerializeField] float m_Window = 0.3f;          // 双击时间窗（StandaloneInputModule 内部也是 0.3 秒）
        [SerializeField] bool m_DelaySingle = true;      // true：确认不是双击才发单击；false：单击立即发，双击时再补发双击
        public UnityEvent onSingleClick = new UnityEvent();
        public UnityEvent onDoubleClick = new UnityEvent();

        float m_LastClick = -1f;
        bool m_Pending;

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left) return;
            float now = Time.unscaledTime;
            if (m_LastClick >= 0f && now - m_LastClick <= m_Window)
            {
                m_Pending = false;                       // 第二下来了：取消待定的单击
                m_LastClick = -1f;                       // 防止三连击被算成两次双击
                onDoubleClick.Invoke();
                return;
            }
            m_LastClick = now;
            if (m_DelaySingle) m_Pending = true;
            else onSingleClick.Invoke();
        }

        void Update()
        {
            if (m_Pending && Time.unscaledTime - m_LastClick > m_Window)
            {
                m_Pending = false;
                onSingleClick.Invoke();
            }
        }
        //#endregion
    }
}
