// 数量步进器：- [数量] +，点一下加减 1，按住连发
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public class QuantityStepper : MonoBehaviour
    {
        [SerializeField] Button m_Minus, m_Plus;
        [SerializeField] TMP_Text m_Value, m_Total;
        [SerializeField] int m_Min = 1, m_Max = 99, m_UnitPrice = 25;
        int m_Count = 1;

        void Awake()
        {
            m_Minus.onClick.AddListener(() => Add(-1));
            m_Plus.onClick.AddListener(() => Add(1));
            m_Minus.GetComponent<HoldRepeatButton>().onRepeat.AddListener(() => Add(-1));
            m_Plus.GetComponent<HoldRepeatButton>().onRepeat.AddListener(() => Add(1));
            Set(m_Count);
        }

        void Add(int d) => Set(m_Count + d);

        public void Set(int value)
        {
            m_Count = Mathf.Clamp(value, m_Min, m_Max);
            m_Value.SetText("{0}", m_Count);
            m_Total.SetText("合计 {0} 金币", m_Count * m_UnitPrice);
            m_Minus.interactable = m_Count > m_Min;    // 到边界就禁用：顺便让长按自动停下
            m_Plus.interactable = m_Count < m_Max;
        }
    }
}
