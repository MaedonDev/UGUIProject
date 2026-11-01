// 3.4 节配套：用按钮调用 RectTransform 的几个“设置尺寸”方法，观察数值和效果。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch03
{
    public class RectOpsDemo : MonoBehaviour
    {
        [SerializeField] RectTransform target;
        [SerializeField] Button widerButton, narrowerButton, leftButton, topButton, rightButton, bottomButton;

        void Awake()
        {
            widerButton.onClick.AddListener(() => ChangeWidth(80));
            narrowerButton.onClick.AddListener(() => ChangeWidth(-80));
            leftButton.onClick.AddListener(() => Dock(RectTransform.Edge.Left));
            topButton.onClick.AddListener(() => Dock(RectTransform.Edge.Top));
            rightButton.onClick.AddListener(() => Dock(RectTransform.Edge.Right));
            bottomButton.onClick.AddListener(() => Dock(RectTransform.Edge.Bottom));
        }

        //#region ops
        public void ChangeWidth(float delta)
        {
            // 无论锚点是点还是拉伸，都把“实际宽度”设成指定值；锚点保持不变
            float w = Mathf.Max(80, target.rect.width + delta);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        }

        public void Dock(RectTransform.Edge edge)
        {
            // 贴到父节点的某条边：离边 inset 个单位、该方向上的尺寸为 size。
            // 注意它会改写锚点：贴左边时 anchorMin.x = anchorMax.x = 0
            bool horizontal = edge == RectTransform.Edge.Left || edge == RectTransform.Edge.Right;
            float size = horizontal ? 260 : 160;
            target.SetInsetAndSizeFromParentEdge(edge, 30, size);
        }
        //#endregion
    }
}
