// 数字滚动：金币、分数从旧值滚到新值，带千分位；滚动结束“弹”一下。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch15
{
    [RequireComponent(typeof(TMP_Text))]
    public class NumberRoller : MonoBehaviour
    {
        //#region roller
        [SerializeField] long m_Value;
        [SerializeField] string m_Format = "N0";     // N0 = 千分位、无小数
        [SerializeField] float m_Duration = 0.8f;

        TMP_Text m_Text;
        TweenBase m_Tween;
        float m_Shown;                               // 当前显示的值（可能是小数，显示时取整）

        TMP_Text text => m_Text != null ? m_Text : (m_Text = GetComponent<TMP_Text>());
        public long value => m_Value;

        public void SetImmediate(long v) { m_Tween?.Kill(); m_Value = v; m_Shown = v; Show(); }

        /// 滚到新值；返回补间，调用方可以继续 Evaluate 或挂回调
        public Tween<float> RollTo(long target, float duration = -1)
        {
            m_Tween?.Kill();
            float from = m_Shown;
            m_Value = target;
            var tw = Tw.Value(this, from, target, duration > 0 ? duration : m_Duration, v => { m_Shown = v; Show(); })
                       .SetEase(Ease.OutCubic);
            m_Tween = tw;
            return tw;
        }

        public void Add(long delta, bool punch = true)
        {
            var tw = RollTo(m_Value + delta);
            if (punch) tw.OnComplete(() => transform.PunchScale(0.18f, 0.25f).Play());
            tw.Play();
        }

        void Show() => text.text = Mathf.RoundToInt(m_Shown).ToString(m_Format);
        //#endregion
    }
}
