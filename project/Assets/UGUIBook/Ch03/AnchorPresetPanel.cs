// 案例 3.1 配套场景：16 个按钮 = 16 种预设，两个开关模拟 Shift / Alt，滑块改父节点宽度。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch03
{
    public class AnchorPresetPanel : MonoBehaviour
    {
        [SerializeField] RectTransform target;
        [SerializeField] Button[] presetButtons = new Button[16];   // 行：Top/Middle/Bottom/Stretch；列：Left/Center/Right/Stretch
        [SerializeField] Toggle pivotToggle;      // 模拟 Shift
        [SerializeField] Toggle positionToggle;   // 模拟 Alt
        [SerializeField] Slider parentWidth;
        [SerializeField] Button resetButton;

        Vector2 aMin, aMax, pivot, pos, size;

        void Awake()
        {
            aMin = target.anchorMin; aMax = target.anchorMax; pivot = target.pivot;
            pos = target.anchoredPosition; size = target.sizeDelta;
            for (int i = 0; i < presetButtons.Length; i++)
            {
                int index = i;   // 循环变量先拷一份再给 Lambda 用（第 6 章细讲闭包陷阱）
                presetButtons[i].onClick.AddListener(() => Apply(index));
            }
            var parent = (RectTransform)target.parent;
            parentWidth.value = parent.rect.width;
            parentWidth.onValueChanged.AddListener(w =>
                parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w));
            resetButton.onClick.AddListener(ResetTarget);
        }

        public void Apply(int index)
        {
            target.SetAnchorPreset((AnchorH)(index % 4), (AnchorV)(index / 4),
                pivotToggle.isOn, positionToggle.isOn);
        }

        void ResetTarget()
        {
            target.anchorMin = aMin; target.anchorMax = aMax; target.pivot = pivot;
            target.anchoredPosition = pos; target.sizeDelta = size;
        }
    }
}
