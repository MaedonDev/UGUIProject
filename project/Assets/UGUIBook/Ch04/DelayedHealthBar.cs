// 带“延迟白条”的血条：受击时红条立刻缩短，白条停顿一下再追上去，玩家能直观看到这一下掉了多少血。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    public class DelayedHealthBar : MonoBehaviour
    {
        //#region fields
        [SerializeField] Image m_Fill;            // 当前血量：Filled / Horizontal / Origin = Left
        [SerializeField] Image m_Delay;           // 延迟白条：同样设置，层级在 m_Fill 之前（被它盖住）
        [SerializeField] TMP_Text m_Label;
        [SerializeField] float m_Max = 1000f;
        [SerializeField] float m_HoldTime = 0.45f;     // 受击后白条先停多久
        [SerializeField] float m_DrainPerSecond = 0.6f; // 白条每秒追赶“满血量”的多少比例
        [SerializeField] Gradient m_Colors = DefaultColors();
        //#endregion

        float m_Value, m_DelayValue, m_Hold;
        bool m_Inited;

        public float Value => m_Value;
        public float Max => m_Max;

        void Awake() { if (!m_Inited) SetInstant(m_Max); }

        //#region logic
        public void SetInstant(float value)
        {
            m_Inited = true;
            m_Value = m_DelayValue = Mathf.Clamp(value, 0f, m_Max);
            m_Hold = 0f;
            Refresh();
        }

        public void Damage(float amount)
        {
            m_Value = Mathf.Max(0f, m_Value - amount);
            m_Hold = m_HoldTime;                       // 连续受击会不断重置停顿，白条攒住整段伤害
            Refresh();
        }

        public void Heal(float amount)
        {
            m_Value = Mathf.Min(m_Max, m_Value + amount);
            if (m_DelayValue < m_Value) m_DelayValue = m_Value;   // 回血时白条不能落在红条后面
            Refresh();
        }

        void Update() => Tick(Time.deltaTime);

        /// 推进 dt 秒（公开出来，拍图或单元测试时可以手动“快进”）
        public void Tick(float dt)
        {
            if (m_DelayValue <= m_Value) return;
            if (m_Hold > 0f) { m_Hold -= dt; return; }
            m_DelayValue = Mathf.MoveTowards(m_DelayValue, m_Value, m_DrainPerSecond * m_Max * dt);
            Refresh();
        }

        void Refresh()
        {
            float r = m_Value / m_Max;
            m_Fill.fillAmount = r;                     // 只改 fillAmount：只重建这一个 Image 的网格
            m_Fill.color = m_Colors.Evaluate(r);
            m_Delay.fillAmount = m_DelayValue / m_Max;
            if (m_Label != null) m_Label.SetText("{0} / {1}", Mathf.CeilToInt(m_Value), m_Max);
        }
        //#endregion

        static Gradient DefaultColors()
        {
            // Fixed 模式：分段变色（≤30% 红，≤60% 橙，其余绿），不会出现两色混合的“脏”中间色
            var g = new Gradient { mode = GradientMode.Fixed };
            g.SetKeys(new[]
                {
                    new GradientColorKey(new Color32(0xE5, 0x48, 0x4D, 255), 0.3f),
                    new GradientColorKey(new Color32(0xFF, 0xA3, 0x1A, 255), 0.6f),
                    new GradientColorKey(new Color32(0x2F, 0xB3, 0x6A, 255), 1f),
                },
                new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) });
            return g;
        }
    }
}
