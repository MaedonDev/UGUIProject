// 案例 11.1：可拖拽窗口。挂在窗口的标题栏上（标题栏要有 Raycast Target 打开的 Image）。
// 按下：窗口置顶；拖动：窗口跟着指针走，并且不会被拖出父物体的范围。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] RectTransform m_Window;          // 要移动的窗口（标题栏的某个父物体）
        [SerializeField] bool m_ClampToParent = true;

        RectTransform m_Parent;
        Vector2 m_Offset;                                  // 按下点 → 窗口轴心 的偏移（父物体本地坐标）

        void Awake() => m_Parent = (RectTransform)m_Window.parent;

        //#region drag
        public void OnPointerDown(PointerEventData e)
        {
            m_Window.SetAsLastSibling();                   // 同一父物体下，越靠后越晚画、越在上面
        }

        public void OnBeginDrag(PointerEventData e)
        {
            // 屏幕坐标 → 父物体本地坐标。Overlay 画布 pressEventCamera 为 null，Camera 画布是画布相机，两种都对
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Parent, e.position, e.pressEventCamera, out var local))
                m_Offset = (Vector2)m_Window.localPosition - local;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Parent, e.position, e.pressEventCamera, out var local))
                return;
            Vector2 pos = local + m_Offset;
            if (m_ClampToParent) pos = Clamp(pos);
            m_Window.localPosition = pos;
        }
        //#endregion

        //#region clamp
        // localPosition 是“轴心”在父物体里的位置；窗口的左边 = 轴心 - 宽 × pivot.x，右边 = 轴心 + 宽 × (1 - pivot.x)
        Vector2 Clamp(Vector2 pos)
        {
            Rect p = m_Parent.rect;
            Vector2 size = m_Window.rect.size * (Vector2)m_Window.localScale;
            Vector2 pivot = m_Window.pivot;
            pos.x = Mathf.Clamp(pos.x, p.xMin + size.x * pivot.x, p.xMax - size.x * (1 - pivot.x));
            pos.y = Mathf.Clamp(pos.y, p.yMin + size.y * pivot.y, p.yMax - size.y * (1 - pivot.y));
            return pos;
        }
        //#endregion
    }
}
