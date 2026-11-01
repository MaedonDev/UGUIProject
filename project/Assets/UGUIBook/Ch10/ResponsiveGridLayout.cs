// 案例 10.3：按宽度自动算列数的网格。
// GridLayoutGroup 的格子尺寸是死的，屏幕一宽右边就空出一大块。这个子类在布局的“水平阶段”
// 根据自身宽度算出能放几列，再把格子宽度拉伸到正好填满，高度按宽高比跟着变。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    [AddComponentMenu("Layout/Responsive Grid Layout (UGUIBook)")]
    public class ResponsiveGridLayout : GridLayoutGroup
    {
        [Tooltip("格子最小宽度：放不下就减少列数")]
        [SerializeField] float m_MinCellWidth = 200;
        [Tooltip("格子高 / 宽")]
        [SerializeField] float m_CellAspect = 1.25f;
        [Tooltip("最多几列，<=0 不限制")]
        [SerializeField] int m_MaxColumns = 0;

        public float minCellWidth { get => m_MinCellWidth; set { m_MinCellWidth = Mathf.Max(1, value); SetDirty(); } }
        public float cellAspect { get => m_CellAspect; set { m_CellAspect = Mathf.Max(0.01f, value); SetDirty(); } }
        public int columns => m_ConstraintCount;

        //#region fit
        void FitCells()
        {
            float width = rectTransform.rect.width - padding.horizontal;
            int cols = Mathf.Max(1, Mathf.FloorToInt((width + spacing.x) / (m_MinCellWidth + spacing.x)));
            if (m_MaxColumns > 0) cols = Mathf.Min(cols, m_MaxColumns);
            float cellW = (width - spacing.x * (cols - 1)) / cols;

            // 注意：直接写受保护字段，而不是 cellSize / constraintCount 属性。
            // 属性的 setter 会调 SetDirty()，而我们正处在布局重建当中，那样会再排一次重建（下一帧），白白多算一遍。
            m_Constraint = Constraint.FixedColumnCount;
            m_ConstraintCount = cols;
            m_CellSize = new Vector2(Mathf.Max(1, cellW), Mathf.Max(1, cellW * m_CellAspect));
        }

        // 计算阶段（自下而上）：先按当前宽度估一次，让父级拿到合理的 preferred 值
        public override void CalculateLayoutInputHorizontal()
        {
            FitCells();
            base.CalculateLayoutInputHorizontal();
        }

        // 设置阶段（自上而下）：父级已经把我的宽度定下来了，再按最终宽度算一次
        public override void SetLayoutHorizontal()
        {
            FitCells();
            base.SetLayoutHorizontal();
        }
        //#endregion
    }
}
