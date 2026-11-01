// 案例 3.5：把红点、数字角标、“NEW”标签钉在父节点的九个位置之一，父节点变大变小都跟着走。
using UnityEngine;

namespace UGUIBook.Ch03
{
    public enum BadgeCorner { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }

    //#region badge
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class CornerBadge : MonoBehaviour
    {
        public BadgeCorner corner = BadgeCorner.TopRight;
        [Tooltip("向父节点内部收进去多少（正数往里，负数往外探出）")]
        public Vector2 inset = new Vector2(8, 8);

        void OnEnable() => Apply();
        void OnValidate() => Apply();   // Inspector 里改了参数立刻生效

        public void Apply()
        {
            int i = (int)corner;
            // 九宫格位置 → 锚点：列 0/0.5/1，行 1/0.5/0（Top 在上）
            var a = new Vector2((i % 3) * 0.5f, 1f - (i / 3) * 0.5f);
            var rt = (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = a;
            rt.pivot = new Vector2(0.5f, 0.5f);   // 轴心在角标中心：角标中心正好压在锚点上
            // (0.5 - a) * 2 是“指向父节点中心”的方向：右上角是 (-1, -1)，正中是 (0, 0)
            rt.anchoredPosition = Vector2.Scale((new Vector2(0.5f, 0.5f) - a) * 2f, inset);
        }
    }
    //#endregion
}
