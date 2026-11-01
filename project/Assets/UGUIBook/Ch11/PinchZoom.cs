// 案例 11.6：双指缩放 + 单指平移（地图、大图预览）。鼠标滚轮也能缩放，方便在编辑器里测试。
// 每根手指都有自己的 PointerEventData（pointerId = fingerId），我们用字典记录“当前按着的手指”。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    public class PinchZoom : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IDragHandler, IScrollHandler
    {
        [SerializeField] RectTransform m_Content;          // 被缩放/平移的内容（地图），是本物体（视口）的子物体
        [SerializeField] float m_MinScale = 1, m_MaxScale = 4;
        [SerializeField] float m_WheelStep = 0.1f;

        readonly Dictionary<int, Vector2> m_Pointers = new Dictionary<int, Vector2>();
        RectTransform m_Viewport;
        Camera m_Cam;

        RectTransform Viewport => m_Viewport != null ? m_Viewport : (m_Viewport = (RectTransform)transform);

        //#region pointers
        public void OnPointerDown(PointerEventData e) { m_Pointers[e.pointerId] = e.position; m_Cam = e.pressEventCamera; }
        public void OnPointerUp(PointerEventData e) => m_Pointers.Remove(e.pointerId);

        // 去掉拖拽阈值：手指一动就开始收 OnDrag，双指缩放不会“卡一下”
        public void OnInitializePotentialDrag(PointerEventData e) => e.useDragThreshold = false;

        public void OnDrag(PointerEventData e)
        {
            if (!m_Pointers.ContainsKey(e.pointerId)) return;
            Vector2 prev = m_Pointers[e.pointerId];
            m_Pointers[e.pointerId] = e.position;

            if (m_Pointers.Count == 1) { Pan(prev, e.position); return; }

            // 两根手指：取另一根手指，比较“这根手指移动前后”两指距离的变化
            foreach (var kv in m_Pointers)
            {
                if (kv.Key == e.pointerId) continue;
                float before = Vector2.Distance(prev, kv.Value);
                float after = Vector2.Distance(e.position, kv.Value);
                if (before > 1f) ZoomAt((e.position + kv.Value) * 0.5f, after / before);
                break;
            }
        }
        //#endregion

        public void OnScroll(PointerEventData e) => ZoomAt(e.position, 1 + e.scrollDelta.y * m_WheelStep, e.enterEventCamera);

        //#region zoom
        /// 以屏幕上某点为中心缩放：缩放前后，该点下面的内容保持不动
        public void ZoomAt(Vector2 screenPos, float factor, Camera cam = null)
        {
            cam = cam != null ? cam : m_Cam;
            float s = Mathf.Clamp(m_Content.localScale.x * factor, m_MinScale, m_MaxScale);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Content, screenPos, cam, out var before);
            m_Content.localScale = new Vector3(s, s, 1);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_Content, screenPos, cam, out var after);
            m_Content.anchoredPosition += (after - before) * s;      // 把“漂走”的那一段补回来
            ClampInside();
        }

        /// 平移：把两个屏幕点都换算到视口本地坐标再相减，Overlay / Camera 画布、任意缩放因子都适用
        public void Pan(Vector2 fromScreen, Vector2 toScreen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, fromScreen, m_Cam, out var a);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, toScreen, m_Cam, out var b);
            m_Content.anchoredPosition += b - a;
            ClampInside();
        }
        //#endregion

        // 内容总是盖满视口（不露底）
        void ClampInside()
        {
            Vector2 size = m_Content.rect.size * m_Content.localScale.x;
            Vector2 view = Viewport.rect.size;
            Vector2 max = Vector2.Max(Vector2.zero, (size - view) * 0.5f);
            var p = m_Content.anchoredPosition;
            m_Content.anchoredPosition = new Vector2(Mathf.Clamp(p.x, -max.x, max.x), Mathf.Clamp(p.y, -max.y, max.y));
        }
    }
}
