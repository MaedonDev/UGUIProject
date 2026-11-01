// 环形菜单的一个扇区：悬停高亮、点击回调。命中区域由 RingProgress.IsRaycastLocationValid 精确裁定，
// 所以几个扇区的矩形虽然完全重叠，鼠标也只会落到它真正指着的那一块上。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(RingProgress))]
    public class RadialSector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        //#region sector
        [SerializeField] string m_Title = "背包";
        [SerializeField] Color m_Normal = new Color(0.14f, 0.16f, 0.23f, 0.92f);
        [SerializeField] Color m_Hover = new Color32(0x5B, 0x4B, 0xD6, 0xFF);
        [SerializeField] TMP_Text m_Label;     // 圆心的说明文字
        [SerializeField] Graphic m_Icon;

        public void Setup(string title, Color normal, Color hover, TMP_Text label, Graphic icon)
        {
            m_Title = title; m_Normal = normal; m_Hover = hover; m_Label = label; m_Icon = icon;
            SetHighlighted(false);
        }

        public void SetHighlighted(bool on)
        {
            GetComponent<RingProgress>().color = on ? m_Hover : m_Normal;
            if (m_Icon != null) m_Icon.rectTransform.localScale = Vector3.one * (on ? 1.25f : 1f);
            if (on && m_Label != null) m_Label.text = m_Title;
        }

        public void OnPointerEnter(PointerEventData e) => SetHighlighted(true);
        public void OnPointerExit(PointerEventData e) => SetHighlighted(false);
        public void OnPointerClick(PointerEventData e)
        {
            if (m_Label != null) m_Label.text = "打开 " + m_Title;
        }
        //#endregion
    }
}
