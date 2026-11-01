// 配套场景 Ch08_Validation：三种输入校验方式各一个输入框。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch08
{
    public class ValidationDemo : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_Age;       // ① Content Type = Integer Number（Inspector 里设好）
        [SerializeField] TMP_InputField m_Code;      // ② onValidateInput：兑换码，自动转大写
        [SerializeField] TMP_InputField m_Phone;     // ③ Input Validator = PhoneValidator 资源
        [SerializeField] TMP_Text m_Log;

        //#region code
        void Awake()
        {
            m_Code.characterLimit = 12;
            // onValidateInput 不能在 Inspector 里设置，只能用代码赋值；它的优先级高于 Content Type 的内置校验
            m_Code.onValidateInput = (text, index, c) =>
            {
                if (c >= 'a' && c <= 'z') return char.ToUpperInvariant(c);   // 小写自动转大写
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) return c;
                return '\0';                                                  // 返回 '\0' = 拒绝这个字符
            };
            m_Age.onEndEdit.AddListener(s => Log("年龄", s));
            m_Code.onEndEdit.AddListener(s => Log("兑换码", s));
            m_Phone.onEndEdit.AddListener(s => Log("手机号", s.Replace(" ", "")));
        }
        //#endregion

        void Log(string field, string value) => m_Log.text = $"onEndEdit  {field} = \"{value}\"";

        public void Setup(TMP_InputField age, TMP_InputField code, TMP_InputField phone, TMP_Text log)
        {
            m_Age = age; m_Code = code; m_Phone = phone; m_Log = log;
        }
    }
}
