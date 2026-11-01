// Toggle 只会淡入淡出一个 graphic（Checkmark）。想让“选中时文字变白”这类附加效果跟着变，
// 就挂这个小组件。分段选择器、页签都用得上。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    [RequireComponent(typeof(Toggle))]
    public class ToggleLabelColor : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Label;
        [SerializeField] Color m_On = Color.white;
        [SerializeField] Color m_Off = new Color32(0x1F, 0x24, 0x36, 0xFF);
        Toggle m_Toggle;
        bool m_Last;

        void OnEnable()
        {
            m_Toggle = GetComponent<Toggle>();
            Apply(m_Toggle.isOn);
        }

        // 不用 onValueChanged：SetIsOnWithoutNotify、ToggleGroup 静默关闭别的成员时都不会回调，
        // 每帧比较一次 bool 最省心，开销可以忽略
        void LateUpdate()
        {
            if (m_Toggle.isOn != m_Last) Apply(m_Toggle.isOn);
        }

        public void Apply(bool on)
        {
            m_Last = on;
            if (m_Label != null) m_Label.color = on ? m_On : m_Off;
        }

        public void Setup(TMP_Text label, Color on, Color off)
        {
            m_Label = label; m_On = on; m_Off = off;
            Apply(GetComponent<Toggle>().isOn);
        }
    }
}
