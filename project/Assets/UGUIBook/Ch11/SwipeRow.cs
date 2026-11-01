// 案例 11.7：竖向滚动列表里的“左滑删除”行——事件拦截与转发。
// 问题：行实现了 IDragHandler，按下时 EventSystem 沿父链找拖拽处理者，最先找到的是“行”，
//       于是上下拖也被行吃掉，ScrollRect 再也滚不动。
// 解决：在 OnBeginDrag 里看方向。横向 → 自己处理；竖向 → 把这次拖拽“转交”给 ScrollRect：
//       改写 eventData.pointerDrag，之后的 OnDrag / OnEndDrag 输入模块会直接发给 ScrollRect。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch11
{
    public class SwipeRow : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform m_Front;         // 可以左右滑的前景（盖在删除按钮上面）
        [SerializeField] Button m_Delete;
        [SerializeField] float m_OpenOffset = -220;     // 完全打开时前景的 x
        [SerializeField] ScrollRect m_Scroll;           // 留空则自动在父物体里找

        float m_Target;
        bool m_Dragging;

        void Awake()
        {
            if (m_Scroll == null) m_Scroll = GetComponentInParent<ScrollRect>();
            m_Delete.onClick.AddListener(() => Destroy(gameObject));
        }

        //#region forward
        public void OnInitializePotentialDrag(PointerEventData e)
        {
            // ScrollRect 也要在按下时收到这个事件（用来“掐停”惯性滚动），所以照转一份
            if (m_Scroll != null) ExecuteEvents.Execute(m_Scroll.gameObject, e, ExecuteEvents.initializePotentialDrag);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            Vector2 moved = e.position - e.pressPosition;            // 超过拖拽阈值时累计的位移，比单帧 delta 更可靠
            bool horizontal = Mathf.Abs(moved.x) > Mathf.Abs(moved.y);
            if (!horizontal && m_Scroll != null)
            {
                e.pointerDrag = m_Scroll.gameObject;     // 转交：后续 OnDrag/OnEndDrag 直接发给 ScrollRect
                ExecuteEvents.Execute(m_Scroll.gameObject, e, ExecuteEvents.beginDragHandler);
                return;
            }
            m_Dragging = true;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!m_Dragging) return;
            // delta 是屏幕像素：把“这一帧前后”两个屏幕点换到父物体本地坐标再相减，得到 UI 单位的位移
            var parent = (RectTransform)m_Front.parent;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position, e.pressEventCamera, out var now);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, e.position - e.delta, e.pressEventCamera, out var prev);
            float x = m_Front.anchoredPosition.x + (now.x - prev.x);
            SetOffset(Mathf.Clamp(x, m_OpenOffset * 1.15f, 0));
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!m_Dragging) return;
            m_Dragging = false;
            m_Target = m_Front.anchoredPosition.x < m_OpenOffset * 0.5f ? m_OpenOffset : 0;   // 过半就吸附打开
        }
        //#endregion

        void Update()
        {
            if (m_Dragging) return;
            float x = m_Front.anchoredPosition.x;
            if (Mathf.Approximately(x, m_Target)) return;
            SetOffset(Mathf.MoveTowards(x, m_Target, 2400 * Time.unscaledDeltaTime));
        }

        public void SetOffset(float x) => m_Front.anchoredPosition = new Vector2(x, m_Front.anchoredPosition.y);
        public void SetOpen(bool open) { m_Target = open ? m_OpenOffset : 0; SetOffset(m_Target); }
    }
}
