// 单选按钮组：一组普通 Button，同一时间只有一个处于“选中”样式。
// （Toggle + ToggleGroup 也能做，见第 7 章；这里演示用 Button 自己管理状态。）
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public class RadioButtonGroup : MonoBehaviour
    {
        //#region fields
        [Serializable] public class IndexEvent : UnityEvent<int> { }

        [SerializeField] Button[] m_Buttons;
        [SerializeField] int m_Index;
        [SerializeField] Color m_OnBg = new Color32(0x5B, 0x4B, 0xD6, 255), m_OffBg = Color.white;
        [SerializeField] Color m_OnText = Color.white, m_OffText = new Color32(0x1F, 0x24, 0x36, 255);
        public IndexEvent onChanged = new IndexEvent();
        //#endregion

        public int Index => m_Index;

        //#region logic
        void Awake()
        {
            for (int i = 0; i < m_Buttons.Length; i++)
            {
                int index = i;          // 关键：循环变量先拷贝到局部变量，再给闭包用
                m_Buttons[i].onClick.AddListener(() => Select(index));
            }
            Refresh();
        }

        public void Select(int index)
        {
            if (index == m_Index) return;
            m_Index = index;
            Refresh();
            onChanged.Invoke(index);
        }

        /// 只改 Image.color（选中样式），按钮自己的 Color Tint 仍然叠加在上面：悬停/按下照样有反馈
        public void Refresh()
        {
            for (int i = 0; i < m_Buttons.Length; i++)
            {
                bool on = i == m_Index;
                m_Buttons[i].image.color = on ? m_OnBg : m_OffBg;
                var label = m_Buttons[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.color = on ? m_OnText : m_OffText;
            }
        }
        //#endregion
    }
}
