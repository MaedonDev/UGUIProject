using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region bufficon
    public class BuffIcon : MonoBehaviour
    {
        [SerializeField] Image m_Frame;
        [SerializeField] Image m_Icon;
        [SerializeField] Image m_Mask;        // Filled Radial 360：已经过去的部分盖暗
        [SerializeField] TMP_Text m_Stacks;
        [SerializeField] TMP_Text m_Time;
        [SerializeField] CanvasGroup m_Group;

        static readonly Color s_Buff = new Color32(0x4C, 0xD1, 0x6E, 0xFF);
        static readonly Color s_Debuff = new Color32(0xE5, 0x48, 0x4D, 0xFF);
        int m_LastSeconds = -1;

        public void Setup(Sprite icon, bool debuff)
        {
            m_Icon.sprite = icon;
            m_Frame.color = debuff ? s_Debuff : s_Buff;
            m_LastSeconds = -1;
        }

        public void SetStacks(int n) => m_Stacks.text = n > 1 ? n.ToString() : "";

        public void SetTime(float left, float duration)
        {
            m_Mask.fillAmount = 1 - left / duration;
            // 只在整秒变化时改文字：避免每帧生成新字符串、触发 TMP 重建
            int s = Mathf.CeilToInt(left);
            if (s != m_LastSeconds) { m_LastSeconds = s; m_Time.text = s >= 60 ? (s / 60) + "m" : s + "s"; }
            // 最后 3 秒闪烁提醒
            m_Group.alpha = left > 3 ? 1 : 0.55f + 0.45f * Mathf.Abs(Mathf.Cos(left * Mathf.PI * 2));
        }
    }
    //#endregion
}
