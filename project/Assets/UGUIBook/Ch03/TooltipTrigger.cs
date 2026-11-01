// 案例 3.3：挂在任何能接收射线的 UI 上（有 Raycast Target 的 Graphic），鼠标移入显示提示。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch03
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea] public string message = "提示文字";

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Tooltip.Instance != null) Tooltip.Instance.Show(message);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Tooltip.Instance != null) Tooltip.Instance.Hide();
        }

        void OnDisable() => OnPointerExit(null);   // 物体被隐藏时也要收起提示，否则提示框会“卡”在屏幕上
    }
}
