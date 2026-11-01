// 页签栏：管理一组 TabButton 和对应的内容页
using UnityEngine;

namespace UGUIBook.Ch06
{
    public class TabBar : MonoBehaviour
    {
        [SerializeField] TabButton[] m_Tabs;
        [SerializeField] GameObject[] m_Pages;
        [SerializeField] int m_Default;

        void Start()
        {
            foreach (var t in m_Tabs) t.Bar = this;
            Select(m_Tabs[m_Default]);
        }

        public void Select(TabButton tab)
        {
            for (int i = 0; i < m_Tabs.Length; i++)
            {
                bool on = m_Tabs[i] == tab;
                m_Tabs[i].IsOn = on;
                if (i < m_Pages.Length) m_Pages[i].SetActive(on);
            }
        }
    }
}
