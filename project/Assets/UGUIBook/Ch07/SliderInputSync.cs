// Slider 和输入框双向同步：拖滑块 → 输入框显示数值；输入数字回车 → 滑块跳过去。
// 关键是“谁改谁”时不要互相触发成死循环 / 打断输入，靠 SetTextWithoutNotify 和 onEndEdit 解决。
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class SliderInputSync : MonoBehaviour
    {
        [SerializeField] Slider m_Slider;
        [SerializeField] TMP_InputField m_Input;
        [SerializeField] string m_Format = "0";      // 显示格式：整数 "0"，一位小数 "0.0"

        //#region sync
        void Awake()
        {
            // 小数还是整数，交给输入框的 Content Type 先过滤一遍字符
            m_Input.contentType = m_Slider.wholeNumbers ? TMP_InputField.ContentType.IntegerNumber
                                                        : TMP_InputField.ContentType.DecimalNumber;
            m_Slider.onValueChanged.AddListener(OnSliderChanged);
            // 用 onEndEdit 而不是 onValueChanged：输入“1”准备打“120”时，不该立刻把滑块拽到 1
            m_Input.onEndEdit.AddListener(OnInputCommitted);
        }

        void Start() => OnSliderChanged(m_Slider.value);

        void OnSliderChanged(float v)
        {
            // WithoutNotify：只改显示，不触发输入框自己的 onValueChanged
            m_Input.SetTextWithoutNotify(v.ToString(m_Format, CultureInfo.CurrentCulture));
        }

        void OnInputCommitted(string s)
        {
            // 用 CurrentCulture：TMP 的 DecimalNumber 校验按当前区域的小数点放行字符（有的地区是逗号），解析要和它一致
            if (float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out float v))
                m_Slider.value = v;    // Slider 自己会 Clamp 到 [min, max]、wholeNumbers 时取整，然后回调 OnSliderChanged
            // 无论输入是否合法，都用滑块的最终值重写一遍输入框：超范围、空输入都会被“纠正”回来
            OnSliderChanged(m_Slider.value);
        }
        //#endregion

        public void Setup(Slider slider, TMP_InputField input, string format)
        {
            m_Slider = slider; m_Input = input; m_Format = format;
        }
    }
}
