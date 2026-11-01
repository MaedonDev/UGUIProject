// 案例 10.4：流式布局（自动换行），标签云、技能词条、筛选条件都用它。
// 完整实现 ILayoutElement + ILayoutGroup（继承 LayoutGroup），严格遵守 UGUI 的两阶段约定：
//   CalculateLayoutInputHorizontal  报自己的 min/preferred 宽度（此时子物体的宽度输入已算好）
//   SetLayoutHorizontal             容器宽度已定 → 断行，设置子物体的 x 和宽度
//   CalculateLayoutInputVertical    子物体宽度已定（文字已能算出换行后的高度）→ 报自己的高度
//   SetLayoutVertical               设置子物体的 y 和高度
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    [AddComponentMenu("Layout/Flow Layout Group (UGUIBook)")]
    public class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField] Vector2 m_Spacing = new Vector2(16, 16);
        [Tooltip("勾选：子物体用自己的 preferred 尺寸（例如文字长度）；不勾：保持子物体原来的 sizeDelta")]
        [SerializeField] bool m_ChildControlSize = true;
        [Tooltip("勾选：每一行（最后一行除外）把多余的宽度平均分给这一行的元素，两端对齐")]
        [SerializeField] bool m_Justify = false;

        public Vector2 spacing { get => m_Spacing; set => SetProperty(ref m_Spacing, value); }
        public bool childControlSize { get => m_ChildControlSize; set => SetProperty(ref m_ChildControlSize, value); }
        public bool justify { get => m_Justify; set => SetProperty(ref m_Justify, value); }

        // 一行 = rectChildren 里的一段 [start, end)
        struct Line { public int start, end; public float width, height; }
        readonly List<Line> m_Lines = new List<Line>();
        readonly List<float> m_Widths = new List<float>();

        float ChildSize(RectTransform child, int axis) =>
            m_ChildControlSize ? LayoutUtility.GetPreferredSize(child, axis) : child.sizeDelta[axis];

        //#region calcH
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();   // 收集参与布局的子物体（跳过未激活和 ignoreLayout 的）
            float widest = 0, sum = 0;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                float w = ChildSize(rectChildren[i], 0);
                widest = Mathf.Max(widest, w);
                sum += w + (i > 0 ? m_Spacing.x : 0);
            }
            // min = 最宽的那个元素（再窄就放不下了）；preferred = 全部排成一行
            SetLayoutInputForAxis(padding.horizontal + widest, padding.horizontal + sum, -1, 0);
        }
        //#endregion

        //#region setH
        public override void SetLayoutHorizontal()
        {
            float inner = rectTransform.rect.width - padding.horizontal;
            BreakLines(inner);
            float alignX = GetAlignmentOnAxis(0);           // 0 左 / 0.5 中 / 1 右
            for (int li = 0; li < m_Lines.Count; li++)
            {
                var line = m_Lines[li];
                int n = line.end - line.start;
                float free = inner - line.width;
                bool justifyThis = m_Justify && li < m_Lines.Count - 1 && n > 1;
                float extra = justifyThis ? free / n : 0;    // 两端对齐：多余宽度平分给每个元素
                float x = padding.left + (justifyThis ? 0 : free * alignX);
                for (int i = line.start; i < line.end; i++)
                {
                    float w = m_Widths[i] + extra;
                    if (m_ChildControlSize || justifyThis) SetChildAlongAxis(rectChildren[i], 0, x, w);
                    else SetChildAlongAxis(rectChildren[i], 0, x);
                    x += w + m_Spacing.x;
                }
            }
        }

        // 贪心断行：放得下就接在本行后面，放不下就换行；单个元素比整行还宽时，压到整行宽
        void BreakLines(float inner)
        {
            m_Lines.Clear(); m_Widths.Clear();
            var line = new Line();
            for (int i = 0; i < rectChildren.Count; i++)
            {
                float w = Mathf.Min(ChildSize(rectChildren[i], 0), inner);
                m_Widths.Add(w);
                float need = line.end > line.start ? line.width + m_Spacing.x + w : w;
                if (line.end > line.start && need > inner + 0.01f)
                {
                    m_Lines.Add(line);
                    line = new Line { start = i, end = i };
                    need = w;
                }
                line.end = i + 1;
                line.width = need;
            }
            if (line.end > line.start) m_Lines.Add(line);
        }
        //#endregion

        //#region calcV
        public override void CalculateLayoutInputVertical()
        {
            // 这时所有子物体的宽度都已经设好，文字的 preferredHeight 是“按真实宽度换行后”的高度
            float total = 0;
            for (int li = 0; li < m_Lines.Count; li++)
            {
                var line = m_Lines[li];
                line.height = 0;
                for (int i = line.start; i < line.end; i++)
                    line.height = Mathf.Max(line.height, ChildSize(rectChildren[i], 1));
                m_Lines[li] = line;
                total += line.height + (li > 0 ? m_Spacing.y : 0);
            }
            float h = padding.vertical + total;
            SetLayoutInputForAxis(h, h, -1, 1);
        }
        //#endregion

        //#region setV
        public override void SetLayoutVertical()
        {
            float alignY = GetAlignmentOnAxis(1);                       // 行内：0 顶 / 0.5 中 / 1 底
            float y = GetStartOffset(1, GetTotalPreferredSize(1) - padding.vertical);  // 整块的垂直对齐
            foreach (var line in m_Lines)
            {
                for (int i = line.start; i < line.end; i++)
                {
                    var child = rectChildren[i];
                    float h = ChildSize(child, 1);
                    if (m_ChildControlSize) SetChildAlongAxis(child, 1, y + (line.height - h) * alignY, h);
                    else SetChildAlongAxis(child, 1, y + (line.height - h) * alignY);
                }
                y += line.height + m_Spacing.y;
            }
        }
        //#endregion
    }
}
