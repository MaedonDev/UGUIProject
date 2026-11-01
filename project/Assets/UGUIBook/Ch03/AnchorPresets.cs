// 案例 3.1：用代码实现 Inspector 里的锚点预设面板（含 Shift = 同时设轴心、Alt = 同时设位置）。
using UnityEngine;

namespace UGUIBook.Ch03
{
    public enum AnchorH { Left, Center, Right, Stretch }
    public enum AnchorV { Top, Middle, Bottom, Stretch }

    public static class AnchorPresets
    {
        //#region preset
        /// 设置锚点预设。默认保持元素在屏幕上的位置和大小不变（和编辑器里直接点预设一样）；
        /// setPivot：轴心也移到对应位置（相当于按住 Shift）；setPosition：把元素贴到对应的边或铺满（相当于按住 Alt）
        public static void SetAnchorPreset(this RectTransform rt, AnchorH h, AnchorV v,
            bool setPivot = false, bool setPosition = false)
        {
            var parent = (RectTransform)rt.parent;
            Rect pr = parent.rect;

            // 1. 记下元素四条边在父节点本地坐标里的位置（这是“看得见的矩形”，和锚点怎么设无关）
            Vector2 min = pr.min + Vector2.Scale(rt.anchorMin, pr.size) + rt.offsetMin;
            Vector2 max = pr.min + Vector2.Scale(rt.anchorMax, pr.size) + rt.offsetMax;
            Vector2 size = max - min;

            // 2. 预设对应的新锚点：点锚点 min == max，拉伸时 0 → 1
            float x0 = h == AnchorH.Center ? 0.5f : h == AnchorH.Right ? 1f : 0f;
            float y0 = v == AnchorV.Middle ? 0.5f : v == AnchorV.Top ? 1f : 0f;
            var aMin = new Vector2(x0, y0);
            var aMax = new Vector2(h == AnchorH.Stretch ? 1f : x0, v == AnchorV.Stretch ? 1f : y0);

            if (setPivot)
                rt.pivot = new Vector2(h == AnchorH.Stretch ? 0.5f : x0, v == AnchorV.Stretch ? 0.5f : y0);

            if (setPosition)
            {
                // 固定轴：贴到锚点所在的边（或居中）；拉伸轴：铺满父节点
                if (h == AnchorH.Stretch) { min.x = pr.xMin; max.x = pr.xMax; }
                else { min.x = pr.xMin + x0 * pr.width - x0 * size.x; max.x = min.x + size.x; }
                if (v == AnchorV.Stretch) { min.y = pr.yMin; max.y = pr.yMax; }
                else { min.y = pr.yMin + y0 * pr.height - y0 * size.y; max.y = min.y + size.y; }
            }

            // 3. 换锚点，再用 offset 把四条边“钉回”原处：offset = 边的位置 − 锚点的位置
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = min - (pr.min + Vector2.Scale(aMin, pr.size));
            rt.offsetMax = max - (pr.min + Vector2.Scale(aMax, pr.size));
        }
        //#endregion
    }
}
