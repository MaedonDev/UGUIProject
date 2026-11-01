// 循环列表（虚拟化列表）的公共部分：只为“看得见的那几项”创建 GameObject，滚动时把移出视口的项回收、
// 换上新数据再放到新位置。数据有一万条，场景里也只有十几个 Item。
// 子类只需要回答三个问题：每一项放在哪、多高；给定可见区间，该显示哪些下标；Content 一共多高。
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region base
    [RequireComponent(typeof(ScrollRect))]
    public abstract class LoopViewBase : MonoBehaviour
    {
        [Tooltip("视口上下各多准备几项（列表）/几行（网格），快速滑动时不露白")]
        [SerializeField] protected int m_Buffer = 1;

        /// 把第 index 条数据显示到 item 上（item 是复用来的，务必把所有字段都重新赋值）
        public Action<int, RectTransform> onBind;
        /// 可见区间变化后触发（排行榜的固定行、聊天的“新消息”提示用它）
        public event Action Scrolled;

        protected ScrollRect m_Scroll;
        protected RectTransform m_Content;
        protected RectTransform m_Viewport;
        protected int m_Count;

        readonly Dictionary<int, RectTransform> m_Active = new Dictionary<int, RectTransform>();
        readonly Dictionary<RectTransform, int> m_TypeOf = new Dictionary<RectTransform, int>();
        readonly Dictionary<int, Stack<RectTransform>> m_Pools = new Dictionary<int, Stack<RectTransform>>();
        readonly List<int> m_Temp = new List<int>();
        bool m_Inited;
        float m_AnimFrom, m_AnimTo, m_AnimT = 1, m_AnimDuration;

        public int Count => m_Count;
        public int ActiveCount => m_Active.Count;
        /// 一共 Instantiate 过多少个 Item（判断复用是否生效的关键指标）
        public int CreatedCount { get; private set; }
        public int FirstVisible { get; private set; }
        public int LastVisible { get; private set; } = -1;
        public ScrollRect ScrollRect { get { Init(); return m_Scroll; } }

        // ---------- 子类实现 ----------
        protected abstract RectTransform TemplateFor(int index, out int type);
        protected abstract void RebuildLayout();                       // 重新计算每项位置与 Content 尺寸
        protected abstract void GetRange(float top, float bottom, out int first, out int last);
        protected abstract void PlaceItem(int index, RectTransform item);
        public abstract float ItemTop(int index);                      // 第 index 项顶部到 Content 顶部的距离
        public abstract float ItemHeight(int index);

        protected virtual void Awake() => Init();

        protected virtual void OnEnable()
        {
            Init();
            m_Scroll.onValueChanged.AddListener(OnScroll);
        }

        protected virtual void OnDisable() => m_Scroll.onValueChanged.RemoveListener(OnScroll);

        public void Init()
        {
            if (m_Inited) return;
            m_Inited = true;
            m_Scroll = GetComponent<ScrollRect>();
            m_Content = m_Scroll.content;
            m_Viewport = m_Scroll.viewport != null ? m_Scroll.viewport : (RectTransform)transform;
            // Content 顶对齐、轴心在上：anchoredPosition.y 就是“已经往下滚了多少”
            m_Content.anchorMin = new Vector2(0, 1);
            m_Content.anchorMax = new Vector2(1, 1);
            m_Content.pivot = new Vector2(0.5f, 1);
            OnInit();
        }

        protected virtual void OnInit() { }

        void OnScroll(Vector2 _) => UpdateVisible();
        //#endregion

        //#region setcount
        /// 设置数据条数。keepPosition=false 时回到顶部
        public void SetCount(int count, bool keepPosition = false)
        {
            Init();
            m_Count = Mathf.Max(0, count);
            RebuildLayout();
            RecycleAll();
            ScrollY = keepPosition ? ScrollY : 0;
            UpdateVisible();
        }

        /// 数据内容变了（条数、高度不变）：重新绑定所有可见项
        public void Refresh()
        {
            foreach (var kv in m_Active) onBind?.Invoke(kv.Key, kv.Value);
        }

        /// 某一项的数据变了
        public void RefreshItem(int index)
        {
            if (m_Active.TryGetValue(index, out var rt)) onBind?.Invoke(index, rt);
        }

        public RectTransform GetItem(int index) => m_Active.TryGetValue(index, out var rt) ? rt : null;
        //#endregion

        //#region update
        public void UpdateVisible()
        {
            Init();
            float top = ScrollY, bottom = top + m_Viewport.rect.height;
            int first = 0, last = -1;
            if (m_Count > 0) GetRange(top, bottom, out first, out last);

            // 1) 先把区间外的项放回池子（先不隐藏，下面马上可能就被复用）
            m_Temp.Clear();
            foreach (var kv in m_Active) if (kv.Key < first || kv.Key > last) m_Temp.Add(kv.Key);
            foreach (int i in m_Temp)
            {
                var rt = m_Active[i];
                m_Active.Remove(i);
                Pool(m_TypeOf[rt]).Push(rt);
            }
            // 2) 区间内缺的项：优先从池子里取
            for (int i = first; i <= last; i++)
            {
                if (m_Active.ContainsKey(i)) continue;
                var tpl = TemplateFor(i, out int type);
                var pool = Pool(type);
                RectTransform rt;
                if (pool.Count > 0) rt = pool.Pop();
                else
                {
                    rt = Instantiate(tpl, m_Content, false);
                    m_TypeOf[rt] = type;
                    CreatedCount++;
                }
                if (!rt.gameObject.activeSelf) rt.gameObject.SetActive(true);
                PlaceItem(i, rt);
                onBind?.Invoke(i, rt);
                m_Active[i] = rt;
            }
            // 3) 这一轮没被复用的才真正隐藏：避免“刚隐藏又显示”带来的 OnDisable/OnEnable 和重建
            foreach (var pool in m_Pools.Values)
                foreach (var rt in pool)
                    if (rt.gameObject.activeSelf) rt.gameObject.SetActive(false);

            FirstVisible = first;
            LastVisible = last;
            Scrolled?.Invoke();
        }

        Stack<RectTransform> Pool(int type)
        {
            if (!m_Pools.TryGetValue(type, out var s)) m_Pools[type] = s = new Stack<RectTransform>();
            return s;
        }

        protected void RecycleAll()
        {
            foreach (var kv in m_Active) Pool(m_TypeOf[kv.Value]).Push(kv.Value);
            m_Active.Clear();
        }

        /// 布局变了（高度、列数）：可见项重新摆放，不重新创建
        protected void RelayoutVisible()
        {
            foreach (var kv in m_Active) PlaceItem(kv.Key, kv.Value);
        }
        //#endregion

        //#region scroll
        /// 已经向下滚动的距离（0 = 顶部）
        public float ScrollY
        {
            get => m_Content.anchoredPosition.y;
            set
            {
                float max = Mathf.Max(0, m_Content.rect.height - m_Viewport.rect.height);
                m_Content.anchoredPosition = new Vector2(m_Content.anchoredPosition.x, Mathf.Clamp(value, 0, max));
            }
        }

        public float MaxScrollY => Mathf.Max(0, m_Content.rect.height - m_Viewport.rect.height);
        public bool IsAtBottom(float tolerance = 4f) => ScrollY >= MaxScrollY - tolerance;

        /// 跳到第 index 项。align：0 = 顶部对齐，0.5 = 居中，1 = 底部对齐；duration>0 时平滑滚动（运行时）
        public void ScrollTo(int index, float align = 0f, float duration = 0f)
        {
            Init();
            if (m_Count == 0) return;
            index = Mathf.Clamp(index, 0, m_Count - 1);
            float target = ItemTop(index) - (m_Viewport.rect.height - ItemHeight(index)) * align;
            target = Mathf.Clamp(target, 0, MaxScrollY);
            m_Scroll.StopMovement();   // 停掉惯性，否则下一帧又被惯性带走
            if (duration <= 0 || !Application.isPlaying)
            {
                m_AnimT = 1;
                ScrollY = target;
                UpdateVisible();
                return;
            }
            m_AnimFrom = ScrollY;
            m_AnimTo = target;
            m_AnimDuration = duration;
            m_AnimT = 0;
        }

        public void ScrollToBottom(float duration = 0f) => ScrollTo(m_Count - 1, 1f, duration);

        /// 第 index 项是否完整地在视口里
        public bool IsFullyVisible(int index)
        {
            float top = ItemTop(index), y = ScrollY;
            return top >= y - 0.5f && top + ItemHeight(index) <= y + m_Viewport.rect.height + 0.5f;
        }

        protected virtual void Update()
        {
            if (m_AnimT >= 1) return;
            // 用户一碰屏幕（ScrollRect 有速度了）就放弃动画，把控制权还给手指
            if (m_Scroll.velocity.sqrMagnitude > 1f && m_AnimT > 0.05f) { m_AnimT = 1; return; }
            m_AnimT = Mathf.Min(1, m_AnimT + Time.unscaledDeltaTime / m_AnimDuration);
            float e = 1 - Mathf.Pow(1 - m_AnimT, 3);
            ScrollY = Mathf.Lerp(m_AnimFrom, m_AnimTo, e);
            UpdateVisible();
        }
    }
    //#endregion
}
