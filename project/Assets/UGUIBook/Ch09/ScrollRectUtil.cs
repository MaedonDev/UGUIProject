// ScrollRect 常用小工具：算出“让某一项出现在视口指定位置”需要的 normalizedPosition。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    public static class ScrollRectUtil
    {
        //#region calc
        /// 竖向列表：让 item 出现在视口的 align 处（0 = 顶部对齐，0.5 = 居中，1 = 底部对齐），返回对应的 verticalNormalizedPosition。
        /// 原理：normalized = 1 表示 Content 顶端贴着视口顶端，0 表示底端贴底端；中间按“已经滚过的距离 / 可滚动距离”线性换算。
        public static float VerticalPositionFor(ScrollRect sr, RectTransform item, float align = 0f)
        {
            Canvas.ForceUpdateCanvases();                 // 确保 LayoutGroup / ContentSizeFitter 已经把尺寸算好
            var content = sr.content;
            var view = sr.viewport != null ? sr.viewport : (RectTransform)sr.transform;
            float hidden = content.rect.height - view.rect.height;      // 可滚动距离
            if (hidden <= 0) return 1;
            // item 的上下边换算到 Content 本地坐标，再算它离 Content 顶端有多远
            Vector3 top = content.InverseTransformPoint(item.TransformPoint(new Vector3(0, item.rect.yMax)));
            Vector3 bottom = content.InverseTransformPoint(item.TransformPoint(new Vector3(0, item.rect.yMin)));
            float itemTop = content.rect.yMax - top.y;                   // item 顶端到 Content 顶端
            float itemHeight = top.y - bottom.y;
            // 想让 item 的“align 点”对齐视口的“align 点”：Content 需要往上滚的距离
            float scrolled = itemTop + itemHeight * align - view.rect.height * align;
            return 1 - Mathf.Clamp01(scrolled / hidden);
        }
        //#endregion
    }
}
