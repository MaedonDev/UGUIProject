// 数字跳动：金币/战力从旧值滚到新值，带千分位和一下“弹跳”。
// 用 SetCharArray 输出，自己拼字符，滚动过程中每帧零 GC。
using System;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch05
{
    public class NumberRoller : MonoBehaviour
    {
        //#region fields
        [SerializeField] TMP_Text m_Text;
        [SerializeField] float m_Duration = 0.8f;
        [SerializeField] float m_Punch = 0.18f;      // 滚动时放大多少（0 = 不弹）
        [SerializeField] string m_Prefix = "";
        //#endregion

        long m_From, m_To, m_Shown;
        float m_T = 1f;
        readonly char[] m_Buffer = new char[40];

        public long Value => m_To;

        void Awake() => SetInstant(m_To);

        //#region api
        public void SetInstant(long value)
        {
            m_From = m_To = m_Shown = value;
            m_T = 1f;
            Show(value);
            m_Text.transform.localScale = Vector3.one;
        }

        public void RollTo(long value)
        {
            m_From = m_Shown;          // 从“当前显示的值”开始，连续点击也不会跳变
            m_To = value;
            m_T = 0f;
        }

        void Update()
        {
            if (m_T >= 1f) return;
            Evaluate(Mathf.Min(1f, m_T + Time.unscaledDeltaTime / m_Duration));
        }

        /// t = 0~1 的滚动进度（拍图时也直接调它）
        public void Evaluate(float t)
        {
            m_T = t;
            double eased = 1 - Math.Pow(1 - t, 3);                       // EaseOutCubic：先快后慢
            m_Shown = m_From + (long)Math.Round((m_To - m_From) * eased);
            Show(m_Shown);
            float s = 1f + m_Punch * Mathf.Sin(t * Mathf.PI);           // 中途最大，结束时回到 1
            m_Text.transform.localScale = new Vector3(s, s, 1f);
        }
        //#endregion

        //#region format
        void Show(long value)
        {
            int len = Format(value, m_Prefix, m_Buffer);
            m_Text.SetCharArray(m_Buffer, 0, len);                      // 不产生 string，不分配内存
        }

        /// 把 value 写成 “前缀 + 千分位数字” 放进 buf，返回长度
        static int Format(long value, string prefix, char[] buf)
        {
            int n = 0;
            foreach (var c in prefix) buf[n++] = c;
            if (value < 0) { buf[n++] = '-'; value = -value; }
            int start = n, digits = 0;
            do
            {
                if (digits > 0 && digits % 3 == 0) buf[n++] = ',';
                buf[n++] = (char)('0' + value % 10);
                value /= 10; digits++;
            } while (value > 0);
            Array.Reverse(buf, start, n - start);                      // 上面是倒着写的，翻转回来
            return n;
        }
        //#endregion
    }
}
