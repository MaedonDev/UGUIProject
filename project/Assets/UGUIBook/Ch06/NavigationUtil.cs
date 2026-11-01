// 用代码给一组 Selectable 设置显式导航（Explicit）：按网格排列，可选首尾循环。
// 自动导航（Automatic）按几何位置猜，布局不规则时常常跳错；菜单类界面建议显式指定。
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public static class NavigationUtil
    {
        //#region grid
        /// items 按行优先排列，columns 列；wrap = 到边缘后从另一侧出来
        public static void SetGrid(Selectable[] items, int columns, bool wrap)
        {
            int rows = (items.Length + columns - 1) / columns;
            Selectable At(int r, int c)
            {
                if (wrap) { r = (r + rows) % rows; c = (c + columns) % columns; }
                if (r < 0 || r >= rows || c < 0 || c >= columns) return null;
                int i = r * columns + c;
                return i < items.Length ? items[i] : null;
            }
            for (int i = 0; i < items.Length; i++)
            {
                int r = i / columns, c = i % columns;
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = At(r - 1, c),
                    selectOnDown = At(r + 1, c),
                    selectOnLeft = At(r, c - 1),
                    selectOnRight = At(r, c + 1),
                };
                items[i].navigation = nav;    // Navigation 是结构体：必须整体赋值回去才生效
            }
        }
        //#endregion
    }
}
