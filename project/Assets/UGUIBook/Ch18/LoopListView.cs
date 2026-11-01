// 纵向循环列表：支持固定高度、可变高度（heightOf）和多种模板（templateOf，比如聊天的左右气泡）
using System;
using UnityEngine;

namespace UGUIBook.Ch18
{
    //#region list
    public class LoopListView : LoopViewBase
    {
        [SerializeField] RectTransform[] m_Templates;   // 场景里的隐藏模板
        [SerializeField] float m_DefaultHeight = 120f;
        [SerializeField] float m_Spacing = 10f;
        [SerializeField] float m_PaddingTop = 10f, m_PaddingBottom = 10f, m_PaddingLeft = 16f, m_PaddingRight = 16f;

        /// 每项高度；为空时全部用 DefaultHeight（固定高度列表）
        public Func<int, float> heightOf;
        /// 每项用第几个模板；为空时都用第 0 个
        public Func<int, int> templateOf;

        // m_Tops[i] = 第 i 项顶部到 Content 顶部的距离；m_Tops[count] = 最后一项底部。
        // 本质是高度的前缀和：可变高度列表靠它把“滚动位置 → 第几项”变成一次二分查找
        float[] m_Tops = new float[1];

        public float DefaultHeight => m_DefaultHeight;
        /// 列表项的可用宽度（聊天气泡算换行要用）
        public float ItemWidth => m_Viewport.rect.width - m_PaddingLeft - m_PaddingRight;

        protected override void OnInit()
        {
            foreach (var t in m_Templates) if (t != null) t.gameObject.SetActive(false);
        }

        protected override RectTransform TemplateFor(int index, out int type)
        {
            type = templateOf != null ? templateOf(index) : 0;
            return m_Templates[type];
        }

        public override float ItemHeight(int index) => heightOf != null ? heightOf(index) : m_DefaultHeight;
        public override float ItemTop(int index) => m_Tops[index];

        protected override void RebuildLayout() => RebuildFrom(0);

        /// 从第 start 项开始重算前缀和（追加数据时只算新增的部分）
        void RebuildFrom(int start)
        {
            if (m_Tops.Length < m_Count + 1)
            {
                // 按 2 倍扩容：聊天这种不停追加的列表不会每次都重新分配数组
                Array.Resize(ref m_Tops, Mathf.Max(m_Count + 1, m_Tops.Length * 2));
            }
            float y = start == 0 ? m_PaddingTop : m_Tops[start - 1] + ItemHeight(start - 1) + m_Spacing;
            for (int i = start; i < m_Count; i++)
            {
                m_Tops[i] = y;
                y += ItemHeight(i) + m_Spacing;
            }
            float end = m_Count > 0 ? y - m_Spacing : m_PaddingTop;
            m_Tops[m_Count] = end;
            m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, end + m_PaddingBottom);
        }

        /// 在末尾追加 added 条数据（聊天新消息）。只算新增部分，已显示的项不动
        public void Append(int added)
        {
            Init();
            int old = m_Count;
            m_Count += added;
            RebuildFrom(old);
            UpdateVisible();
        }

        /// 高度变了（比如宽度改变导致文字重新换行）：重算全部位置，可见项原地重排
        public void InvalidateHeights()
        {
            RebuildLayout();
            RelayoutVisible();
            UpdateVisible();
        }

        protected override void GetRange(float top, float bottom, out int first, out int last)
        {
            first = Mathf.Max(0, FindIndex(top) - m_Buffer);
            last = Mathf.Min(m_Count - 1, FindIndex(bottom) + m_Buffer);
        }

        /// 二分查找“顶部 <= y 的最后一项”。一万条数据最多比较 14 次
        int FindIndex(float y)
        {
            int lo = 0, hi = m_Count - 1;
            while (lo < hi)
            {
                int mid = (lo + hi + 1) >> 1;
                if (m_Tops[mid] <= y) lo = mid;
                else hi = mid - 1;
            }
            return lo;
        }

        protected override void PlaceItem(int index, RectTransform item)
        {
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(1, 1);
            item.pivot = new Vector2(0.5f, 1);
            item.sizeDelta = new Vector2(-(m_PaddingLeft + m_PaddingRight), ItemHeight(index));
            item.anchoredPosition = new Vector2((m_PaddingLeft - m_PaddingRight) * 0.5f, -m_Tops[index]);
        }
    }
    //#endregion
}
