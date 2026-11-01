// 页签：一组 Toggle 放在同一个 ToggleGroup 里（allowSwitchOff = false 保证永远有且只有一个选中），
// 每个 Toggle 对应一个页面。选中哪个页签，就只显示哪个页面。
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class TabGroup : MonoBehaviour
    {
        [SerializeField] Toggle[] m_Tabs;
        [SerializeField] GameObject[] m_Pages;
        [SerializeField] RectTransform m_Indicator;          // 选中页签下方的指示条（可选）
        [SerializeField] Color m_LabelOn = new Color32(0x5B, 0x4B, 0xD6, 0xFF);
        [SerializeField] Color m_LabelOff = new Color32(0x7A, 0x82, 0x99, 0xFF);
        [SerializeField] int m_Default;

        public UnityEvent<int> onTabChanged = new UnityEvent<int>();
        public int Current { get; private set; } = -1;

        //#region listen
        void Awake()
        {
            for (int i = 0; i < m_Tabs.Length; i++)
            {
                int index = i;   // 必须拷一份！直接用 i 的话，所有回调拿到的都是循环结束后的 i
                m_Tabs[i].onValueChanged.AddListener(on =>
                {
                    // 切换时会收到两次回调：旧页签 false、新页签 true，只处理 true
                    if (on) Select(index);
                });
            }
        }

        void Start()
        {
            // 初始状态：不发事件地勾上默认页签，再手动刷新一次
            m_Tabs[m_Default].SetIsOnWithoutNotify(true);
            foreach (var t in m_Tabs) if (t != m_Tabs[m_Default]) t.SetIsOnWithoutNotify(false);
            Select(m_Default);
        }
        //#endregion

        //#region select
        /// 切到第 index 页（代码里也可以直接调用，例如“领取奖励后跳到背包页”）
        public void Select(int index)
        {
            if (index < 0 || index >= m_Tabs.Length) return;
            if (!m_Tabs[index].isOn) { m_Tabs[index].isOn = true; return; }   // 会再回调进来
            Current = index;
            for (int i = 0; i < m_Pages.Length; i++)
                if (m_Pages[i] != null) m_Pages[i].SetActive(i == index);
            for (int i = 0; i < m_Tabs.Length; i++)
            {
                var label = m_Tabs[i].GetComponentInChildren<TMP_Text>();
                if (label != null) label.color = i == index ? m_LabelOn : m_LabelOff;
            }
            if (m_Indicator != null)
            {
                // 指示条横向对齐到选中的页签
                var tab = (RectTransform)m_Tabs[index].transform;
                m_Indicator.position = new Vector3(tab.position.x, m_Indicator.position.y, m_Indicator.position.z);
                m_Indicator.sizeDelta = new Vector2(tab.rect.width * 0.5f, m_Indicator.sizeDelta.y);
            }
            onTabChanged.Invoke(index);
        }
        //#endregion

        public void Setup(Toggle[] tabs, GameObject[] pages, RectTransform indicator, int defaultIndex)
        {
            m_Tabs = tabs; m_Pages = pages; m_Indicator = indicator; m_Default = defaultIndex;
        }
    }
}
