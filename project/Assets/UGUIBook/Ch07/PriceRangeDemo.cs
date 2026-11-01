// 配套场景 Ch07_RangeSlider：区间变化时刷新文字，几个快捷按钮直接设置区间。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class PriceRangeDemo : MonoBehaviour
    {
        [SerializeField] RangeSlider m_Range;
        [SerializeField] TMP_Text m_Label;
        [SerializeField] TMP_Text m_LowTag, m_HighTag;      // 跟随滑块的小气泡
        [SerializeField] Button[] m_Presets;
        [SerializeField] Vector2[] m_PresetValues;

        void Awake()
        {
            m_Range.onValueChanged.AddListener((lo, hi) => Refresh());
            for (int i = 0; i < m_Presets.Length; i++)
            {
                var v = m_PresetValues[i];
                m_Presets[i].onClick.AddListener(() => m_Range.SetValues(v.x, v.y));
            }
        }

        void Start() => Refresh();

        public void Refresh()
        {
            m_Label.text = $"价格：<b>¥{m_Range.Low:0}</b> – <b>¥{m_Range.High:0}</b>";
            m_LowTag.text = "¥" + m_Range.Low.ToString("0");
            m_HighTag.text = "¥" + m_Range.High.ToString("0");
        }

        public void Setup(RangeSlider range, TMP_Text label, TMP_Text lowTag, TMP_Text highTag, Button[] presets, Vector2[] values)
        {
            m_Range = range; m_Label = label; m_LowTag = lowTag; m_HighTag = highTag; m_Presets = presets; m_PresetValues = values;
        }
    }
}
