// 网格循环列表：按“行”虚拟化。列数可以固定，也可以按视口宽度自动计算
using UnityEngine;

namespace UGUIBook.Ch18
{
    //#region grid
    public class LoopGridView : LoopViewBase
    {
        [SerializeField] RectTransform m_Template;
        [SerializeField] Vector2 m_CellSize = new Vector2(150, 150);
        [SerializeField] Vector2 m_Spacing = new Vector2(16, 16);
        [SerializeField] float m_PaddingTop = 16f, m_PaddingBottom = 16f, m_PaddingLeft = 16f, m_PaddingRight = 16f;
        [Tooltip("0 = 按视口宽度自动算列数")]
        [SerializeField] int m_Columns;

        int m_Cols = 1;
        float m_OffsetX;      // 自动列数时让整个网格水平居中
        float m_LastWidth = -1;

        public int Columns => m_Cols;
        float RowHeight => m_CellSize.y + m_Spacing.y;

        protected override void OnInit() => m_Template.gameObject.SetActive(false);

        protected override RectTransform TemplateFor(int index, out int type)
        {
            type = 0;
            return m_Template;
        }

        protected override void RebuildLayout()
        {
            float width = m_Viewport.rect.width - m_PaddingLeft - m_PaddingRight;
            m_LastWidth = m_Viewport.rect.width;
            m_Cols = m_Columns > 0 ? m_Columns : Mathf.Max(1, Mathf.FloorToInt((width + m_Spacing.x) / (m_CellSize.x + m_Spacing.x)));
            float used = m_Cols * m_CellSize.x + (m_Cols - 1) * m_Spacing.x;
            m_OffsetX = Mathf.Max(0, (width - used) * 0.5f);
            int rows = (m_Count + m_Cols - 1) / m_Cols;
            float h = m_PaddingTop + m_PaddingBottom + (rows > 0 ? rows * RowHeight - m_Spacing.y : 0);
            m_Content.sizeDelta = new Vector2(m_Content.sizeDelta.x, h);
        }

        public override float ItemTop(int index) => m_PaddingTop + (index / m_Cols) * RowHeight;
        public override float ItemHeight(int index) => m_CellSize.y;

        protected override void GetRange(float top, float bottom, out int first, out int last)
        {
            int firstRow = Mathf.FloorToInt((top - m_PaddingTop) / RowHeight) - m_Buffer;
            int lastRow = Mathf.FloorToInt((bottom - m_PaddingTop) / RowHeight) + m_Buffer;
            first = Mathf.Max(0, firstRow * m_Cols);
            last = Mathf.Min(m_Count - 1, (lastRow + 1) * m_Cols - 1);
        }

        protected override void PlaceItem(int index, RectTransform item)
        {
            int row = index / m_Cols, col = index % m_Cols;
            item.anchorMin = item.anchorMax = new Vector2(0, 1);
            item.pivot = new Vector2(0, 1);
            item.sizeDelta = m_CellSize;
            item.anchoredPosition = new Vector2(m_PaddingLeft + m_OffsetX + col * (m_CellSize.x + m_Spacing.x), -ItemTop(index));
        }

        /// 视口宽度变了（横竖屏切换、窗口拉伸）：重新算列数
        void OnRectTransformDimensionsChange()
        {
            if (m_Content == null || Mathf.Approximately(m_Viewport.rect.width, m_LastWidth)) return;
            RebuildLayout();
            RelayoutVisible();
            UpdateVisible();
        }
    }
    //#endregion
}
