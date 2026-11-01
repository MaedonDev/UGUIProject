// 运行时驱动 UIParamEffect.value：循环 / 往返 / 间歇扫光。配套场景里所有动起来的 Shader 效果都靠它。
using UnityEngine;

namespace UGUIBook.Ch14
{
    [RequireComponent(typeof(UIParamEffect))]
    public class ParamDriver : MonoBehaviour
    {
        public enum Mode { Loop, PingPong, SweepWithPause }
        [SerializeField] Mode m_Mode = Mode.Loop;
        [SerializeField] float m_Duration = 1.2f;   // 0→1 用时
        [SerializeField] float m_Pause = 1.0f;      // SweepWithPause：两次扫光之间的停顿
        [SerializeField] float m_Offset = 0;        // 相位偏移，让同一排元素错开

        public void Setup(Mode mode, float duration, float pause = 1, float offset = 0)
        {
            m_Mode = mode; m_Duration = duration; m_Pause = pause; m_Offset = offset;
        }

        UIParamEffect m_Effect;
        void Awake() => m_Effect = GetComponent<UIParamEffect>();

        void Update()
        {
            float t = Time.unscaledTime + m_Offset;
            switch (m_Mode)
            {
                case Mode.Loop: m_Effect.value = Mathf.Repeat(t / m_Duration, 1f); break;
                case Mode.PingPong: m_Effect.value = Mathf.PingPong(t / m_Duration, 1f); break;
                case Mode.SweepWithPause:
                    float cycle = m_Duration + m_Pause, local = Mathf.Repeat(t, cycle);
                    m_Effect.value = Mathf.Clamp01(local / m_Duration);
                    break;
            }
        }
    }
}
