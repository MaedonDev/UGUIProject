using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region healthbar
    /// 血条：前景立即变化，后面的白条停顿一下再追上来，让玩家看清“这一下掉了多少”
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] Image m_Fill;        // Image.Type = Filled, Horizontal
        [SerializeField] Image m_Delay;       // 同上，颜色浅一些，放在 Fill 下面
        [SerializeField] TMP_Text m_Label;    // 可选：名字或 “820/1000”
        [SerializeField] bool m_ShowNumbers;
        [SerializeField] string m_DisplayName;
        [SerializeField] float m_DelayHold = 0.35f, m_DelaySpeed = 0.8f;
        [SerializeField] Color m_High = new Color32(0x4C, 0xD1, 0x6E, 0xFF);
        [SerializeField] Color m_Low = new Color32(0xE5, 0x48, 0x4D, 0xFF);

        float m_Ratio = 1, m_DelayRatio = 1, m_Hold;

        public float Ratio => m_Ratio;

        public void SetHealth(float current, float max, bool instant = false)
        {
            float r = max > 0 ? Mathf.Clamp01(current / max) : 0;
            if (r < m_Ratio) m_Hold = m_DelayHold;      // 掉血：白条先停一下
            m_Ratio = r;
            if (instant || m_DelayRatio < r) m_DelayRatio = r;   // 回血：白条直接跟上
            m_Fill.fillAmount = r;
            m_Fill.color = Color.Lerp(m_Low, m_High, Mathf.InverseLerp(0.2f, 0.6f, r));
            m_Delay.fillAmount = m_DelayRatio;
            if (m_Label != null)
                m_Label.text = m_ShowNumbers ? $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}" : m_DisplayName;
        }

        public void Tick(float dt)
        {
            if (m_DelayRatio <= m_Ratio) return;
            if (m_Hold > 0) { m_Hold -= dt; return; }
            m_DelayRatio = Mathf.Max(m_Ratio, m_DelayRatio - m_DelaySpeed * dt);
            m_Delay.fillAmount = m_DelayRatio;
        }

        void Update() => Tick(Time.deltaTime);
    }
    //#endregion
}
