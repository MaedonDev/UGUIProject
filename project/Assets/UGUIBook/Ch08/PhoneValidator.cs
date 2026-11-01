// 自定义输入校验器：只允许数字，最多 11 位，并自动排成“138 1234 5678”。
// 注意 TMP 的约定：走 CustomValidator 时，字符要由校验器自己插进 text，并把 pos 移到插入点之后；
// 返回值只表示“接受（非 0）/ 拒绝（0）”。这和 onValidateInput 只返回字符、由输入框负责插入不一样。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch08
{
    [CreateAssetMenu(menuName = "UGUIBook/Ch08/Phone Validator")]
    public class PhoneValidator : TMP_InputValidator
    {
        //#region validate
        public override char Validate(ref string text, ref int pos, char ch)
        {
            if (ch < '0' || ch > '9') return (char)0;
            string digits = text.Replace(" ", "");
            if (digits.Length >= 11) return (char)0;

            // 插入点前有几个数字（空格不算），新数字就插在第几位
            int before = 0;
            for (int i = 0; i < pos && i < text.Length; i++) if (text[i] != ' ') before++;
            digits = digits.Insert(before, ch.ToString());

            text = Format(digits);
            pos = IndexAfterDigit(text, before + 1);
            return ch;
        }

        /// 3-4-4 分组：13812345678 → 138 1234 5678
        public static string Format(string digits)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < digits.Length; i++)
            {
                if (i == 3 || i == 7) sb.Append(' ');
                sb.Append(digits[i]);
            }
            return sb.ToString();
        }

        /// 第 n 个数字之后的字符串下标
        static int IndexAfterDigit(string text, int n)
        {
            for (int i = 0; i < text.Length; i++)
                if (text[i] != ' ' && --n == 0) return i + 1;
            return text.Length;
        }
        //#endregion
    }
}
