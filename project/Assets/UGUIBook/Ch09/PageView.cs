// 分页吸附的轮播图：横向 ScrollRect + 松手后吸附到最近的一页（快速甩动则翻到下一页）+ 圆点指示 + 自动播放。
// 本组件和 ScrollRect 挂在同一个物体上：拖拽事件会同时发给两者（ExecuteEvents 对同一物体上所有实现接口的组件都调用）。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    [RequireComponent(typeof(ScrollRect))]
    public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform m_Dots;              // 圆点的父节点，子物体数 = 页数
        [SerializeField] Color m_DotOn = Color.white;
        [SerializeField] Color m_DotOff = new Color(1, 1, 1, 0.45f);
        [SerializeField] float m_FlickSpeed = 600;          // 松手速度超过它（像素/秒）就算“甩”
        [SerializeField] float m_SnapTime = 0.18f;          // 吸附动画的平滑时间
        [SerializeField] float m_AutoPlay = 4f;             // 自动翻页间隔，0 = 关闭

        ScrollRect m_Scroll;
        int m_Page;
        bool m_Dragging;
        float m_Velocity, m_Idle;
        int m_DragStartPage;

        public int PageCount => Scroll.content.childCount;
        public int Page => m_Page;
        ScrollRect Scroll => m_Scroll != null ? m_Scroll : (m_Scroll = GetComponent<ScrollRect>());

        void Awake()
        {
            m_Scroll = GetComponent<ScrollRect>();
            m_Scroll.inertia = false;              // 惯性交给我们自己的吸附动画，不然两者会打架
        }

        //#region page
        float PageToNormalized(int page) => PageCount <= 1 ? 0 : page / (float)(PageCount - 1);

        int NearestPage() => Mathf.Clamp(Mathf.RoundToInt(Scroll.horizontalNormalizedPosition * (PageCount - 1)), 0, PageCount - 1);

        public void OnBeginDrag(PointerEventData e)
        {
            m_Dragging = true;
            m_DragStartPage = NearestPage();
        }

        public void OnEndDrag(PointerEventData e)
        {
            m_Dragging = false;
            m_Idle = 0;
            float speed = e.delta.x / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);   // 最后一帧的手指速度
            int target = NearestPage();
            // 快速甩动：哪怕没拖过半页，也按方向翻一页（手指往左甩 = 看下一页）
            if (Mathf.Abs(speed) > m_FlickSpeed)
                target = Mathf.Clamp(m_DragStartPage + (speed < 0 ? 1 : -1), 0, PageCount - 1);
            GoTo(target);
        }

        public void GoTo(int page, bool instant = false)
        {
            m_Page = Mathf.Clamp(page, 0, PageCount - 1);
            if (instant) Scroll.horizontalNormalizedPosition = PageToNormalized(m_Page);
            UpdateDots();
        }

        void Update()
        {
            if (m_Dragging || PageCount == 0) return;
            // 自动播放：一段时间没人碰就翻下一页，到最后一页回到第一页
            if (m_AutoPlay > 0 && (m_Idle += Time.unscaledDeltaTime) > m_AutoPlay) { m_Idle = 0; GoTo((m_Page + 1) % PageCount); }
            float target = PageToNormalized(m_Page);
            float cur = Scroll.horizontalNormalizedPosition;
            if (Mathf.Abs(cur - target) < 0.0005f) return;
            Scroll.horizontalNormalizedPosition = Mathf.SmoothDamp(cur, target, ref m_Velocity, m_SnapTime, Mathf.Infinity, Time.unscaledDeltaTime);
        }
        //#endregion

        public void UpdateDots()
        {
            if (m_Dots == null) return;
            for (int i = 0; i < m_Dots.childCount; i++)
            {
                var dot = (RectTransform)m_Dots.GetChild(i);
                bool on = i == m_Page;
                dot.GetComponent<Image>().color = on ? m_DotOn : m_DotOff;
                dot.sizeDelta = new Vector2(on ? 44 : 16, 16);    // 当前页的圆点拉长成胶囊
            }
        }

        public void Setup(RectTransform dots) => m_Dots = dots;
    }
}
