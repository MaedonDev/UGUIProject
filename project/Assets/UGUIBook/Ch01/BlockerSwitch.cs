// 案例 1.6：用一个 Toggle 开关“透明挡板”的 Raycast Target，亲手验证它是不是罪魁祸首。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch01
{
    public class BlockerSwitch : MonoBehaviour
    {
        [SerializeField] Image blocker;          // 那张全透明、却挡住了点击的图
        [SerializeField] Toggle raycastToggle;   // 勾选 = 打开 Raycast Target
        [SerializeField] Button target;          // 被挡住的按钮
        [SerializeField] TMP_Text result;
        int clicks;

        void Awake()
        {
            raycastToggle.onValueChanged.AddListener(on => blocker.raycastTarget = on);
            target.onClick.AddListener(() => result.text = $"按钮被点到了 {++clicks} 次");
            raycastToggle.isOn = blocker.raycastTarget;
        }
    }
}
