// 案例 2.5：用 Toggle 开关嵌套画布的 Override Sorting，看提示气泡能不能盖住另一个画布上的弹窗。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch02
{
    public class SortingDemo : MonoBehaviour
    {
        [SerializeField] Canvas nestedCanvas;   // 提示气泡上的子 Canvas
        [SerializeField] Toggle overrideToggle;
        [SerializeField] int order = 20;

        void Awake()
        {
            overrideToggle.onValueChanged.AddListener(SetOverride);
            SetOverride(overrideToggle.isOn);
        }

        //#region override
        public void SetOverride(bool on)
        {
            nestedCanvas.overrideSorting = on;  // 打开后这块 UI 不再跟随父画布的排序，自己参与全局排序
            nestedCanvas.sortingOrder = order;
        }
        //#endregion
    }
}
