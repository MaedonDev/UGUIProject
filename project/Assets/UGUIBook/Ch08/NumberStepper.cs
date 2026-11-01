// 数字步进器：[-] [输入框] [+]。输入框只收整数，失焦/回车时校正到 [min, max]；按钮可长按连发。
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class NumberStepper : MonoBehaviour
    {
        [Serializable] public class IntEvent : UnityEvent<int> { }

        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Button m_Minus;
        [SerializeField] Button m_Plus;
        [SerializeField] int m_Min = 1, m_Max = 99, m_Step = 1;
        [SerializeField] int m_Value = 1;
        public IntEvent onValueChanged = new IntEvent();

        public int Value => m_Value;

        //#region bind
        void Awake()
        {
            m_Input.contentType = TMP_InputField.ContentType.IntegerNumber;
            m_Input.characterLimit = Mathf.Max(m_Min.ToString().Length, m_Max.ToString().Length);
            m_Input.onEndEdit.AddListener(OnEndEdit);
            // 按钮用 PressRepeat（按下即触发 + 长按连发），不用 onClick，否则一次点击会触发两次
            m_Minus.GetComponent<PressRepeat>().onRepeat.AddListener(() => SetValue(m_Value - m_Step));
            m_Plus.GetComponent<PressRepeat>().onRepeat.AddListener(() => SetValue(m_Value + m_Step));
        }

        void Start() => SetValue(m_Value, false, true);

        void OnEndEdit(string s)
        {
            // 空字符串、只有“-”、超出 int 范围都会解析失败：恢复成原值
            SetValue(int.TryParse(s, out int v) ? v : m_Value, true, true);
        }
        //#endregion

        //#region set
        public void SetValue(int v, bool notify = true, bool force = false)
        {
            v = Mathf.Clamp(v, m_Min, m_Max);
            bool changed = v != m_Value;
            m_Value = v;
            m_Input.SetTextWithoutNotify(v.ToString());
            // 到边界时禁用对应按钮：既是提示，也让 PressRepeat 停止连发
            m_Minus.interactable = v > m_Min;
            m_Plus.interactable = v < m_Max;
            if (notify && (changed || force)) onValueChanged.Invoke(v);
        }
        //#endregion

        public void Setup(TMP_InputField input, Button minus, Button plus, int min, int max, int value)
        {
            m_Input = input; m_Minus = minus; m_Plus = plus; m_Min = min; m_Max = max; m_Value = value;
        }
    }
}
