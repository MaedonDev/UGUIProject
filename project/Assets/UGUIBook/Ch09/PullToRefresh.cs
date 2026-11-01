// 下拉刷新 + 上拉加载更多。依赖 ScrollRect 的 Elastic 模式：拉过头时 Content 会离开边界，松手后弹回。
// 本组件挂在 ScrollRect 同一物体上，收到同一次拖拽的 OnEndDrag，以此判断“松手时拉了多远”。
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    [RequireComponent(typeof(ScrollRect))]
    public class PullToRefresh : MonoBehaviour, IEndDragHandler
    {
        public enum State { Idle, Pulling, Ready, Refreshing }

        [SerializeField] RectTransform m_Header;      // Content 顶端外侧的提示条（LayoutElement.ignoreLayout）
        [SerializeField] TMP_Text m_HeaderText;
        [SerializeField] RectTransform m_Arrow;
        [SerializeField] TMP_Text m_FooterText;       // Content 最后一个子物体：加载更多提示
        [SerializeField] float m_Threshold = 140;     // 下拉超过多少像素松手才刷新

        public UnityEvent onRefresh = new UnityEvent();       // 业务方在这里重新拉数据，完成后调用 EndRefresh()
        public UnityEvent onLoadMore = new UnityEvent();      // 完成后调用 EndLoadMore(hasMore)

        ScrollRect m_Scroll;
        VerticalLayoutGroup m_Layout;
        int m_BasePadding;
        bool m_Loading, m_NoMore;
        public State Current { get; private set; }

        void Awake()
        {
            m_Scroll = GetComponent<ScrollRect>();
            m_Scroll.movementType = ScrollRect.MovementType.Elastic;
            m_Layout = m_Scroll.content.GetComponent<VerticalLayoutGroup>();
            m_BasePadding = m_Layout.padding.top;
            m_Scroll.onValueChanged.AddListener(_ => OnScrolled());
        }

        //#region pull
        /// 当前下拉距离：Content 轴心在顶部，anchoredPosition.y < 0 说明顶端被拉到了视口顶端以下
        float Pull => -m_Scroll.content.anchoredPosition.y;

        void OnScrolled()
        {
            if (Current == State.Refreshing) return;
            if (Pull > 0) SetState(Pull >= m_Threshold ? State.Ready : State.Pulling);
            else if (Current != State.Idle) SetState(State.Idle);

            // 上拉加载：Content 底端进入视口（normalized ≤ 0）且没在加载
            if (!m_Loading && !m_NoMore && m_Scroll.verticalNormalizedPosition <= 0.001f && m_Scroll.content.rect.height > m_Scroll.viewport.rect.height)
            {
                m_Loading = true;
                m_FooterText.text = "正在加载更多…";
                onLoadMore.Invoke();
            }
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (Current != State.Ready) return;
            SetState(State.Refreshing);
            // 刷新期间要让提示条留在视口里：给 Content 顶部加一段内边距，把提示条“装进” Content，
            // 同时把 Content 往上挪同样的距离，画面不跳；之后 Elastic 回弹会把它平滑地拉到位
            m_Layout.padding.top = m_BasePadding + (int)m_Header.rect.height;
            m_Header.pivot = new Vector2(0.5f, 1);
            Nudge(m_Header.rect.height);
            onRefresh.Invoke();
        }

        public void EndRefresh()
        {
            m_Layout.padding.top = m_BasePadding;
            m_Header.pivot = new Vector2(0.5f, 0);
            Nudge(-m_Header.rect.height);       // 内容区变矮，Content 往下挪回同样距离，然后由回弹收起提示条
            SetState(State.Idle);
            m_NoMore = false;
            m_FooterText.text = "上拉加载更多";
        }

        void Nudge(float dy)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Scroll.content);
            var p = m_Scroll.content.anchoredPosition;
            m_Scroll.content.anchoredPosition = new Vector2(p.x, p.y + dy);
        }
        //#endregion

        public void EndLoadMore(bool hasMore)
        {
            m_Loading = false;
            m_NoMore = !hasMore;
            m_FooterText.text = hasMore ? "上拉加载更多" : "—— 没有更多了 ——";
            m_FooterText.transform.SetAsLastSibling();          // 新加的条目插在提示前面
        }

        //#region state
        public void SetState(State s)
        {
            Current = s;
            m_Arrow.gameObject.SetActive(s != State.Refreshing);
            switch (s)
            {
                case State.Pulling: m_HeaderText.text = "下拉刷新"; m_Arrow.localEulerAngles = Vector3.zero; break;
                case State.Ready: m_HeaderText.text = "松开立即刷新"; m_Arrow.localEulerAngles = new Vector3(0, 0, 180); break;
                case State.Refreshing: m_HeaderText.text = "正在刷新…"; break;
                default: m_HeaderText.text = "下拉刷新"; m_Arrow.localEulerAngles = Vector3.zero; break;
            }
        }
        //#endregion

        public void Setup(RectTransform header, TMP_Text headerText, RectTransform arrow, TMP_Text footer)
        {
            m_Header = header; m_HeaderText = headerText; m_Arrow = arrow; m_FooterText = footer;
        }
    }
}
