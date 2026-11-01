// 按下反馈：按下时缩小一点、松开时带一点回弹，并播放一声“咔哒”。
// 音效用代码合成（AudioClip.Create），配套工程不需要任何音频文件。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    [RequireComponent(typeof(Selectable))]
    public class PressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        //#region fields
        [SerializeField] Transform m_Target;            // 缩放的对象，留空 = 自己
        [SerializeField] float m_PressedScale = 0.92f;
        [SerializeField] float m_Speed = 18f;           // 越大越跟手
        [SerializeField, Range(0, 1)] float m_Volume = 0.6f;
        //#endregion

        Selectable m_Selectable;
        float m_Current = 1f, m_Velocity;
        bool m_Down, m_Inside;
        static AudioClip s_Click;
        AudioSource m_Audio;

        Transform Target => m_Target != null ? m_Target : transform;

        void Awake()
        {
            m_Selectable = GetComponent<Selectable>();
            m_Audio = gameObject.AddComponent<AudioSource>();
            m_Audio.playOnAwake = false;
            if (s_Click == null) s_Click = CreateClick();
        }

        //#region events
        public void OnPointerDown(PointerEventData e)
        {
            if (!m_Selectable.IsInteractable()) return;
            m_Down = true;
            m_Audio.PlayOneShot(s_Click, m_Volume);
        }

        public void OnPointerUp(PointerEventData e) => m_Down = false;
        public void OnPointerEnter(PointerEventData e) => m_Inside = true;
        public void OnPointerExit(PointerEventData e) => m_Inside = false;

        void Update()
        {
            // 按住且指针在按钮上才保持缩小：和 Button 的 Pressed 状态一致（滑出去就弹回）
            float goal = m_Down && m_Inside ? m_PressedScale : 1f;
            // 阻尼弹簧：比 Lerp 多一点回弹，手感更“Q”
            float k = m_Speed * m_Speed, d = 2f * m_Speed * 0.55f;
            m_Velocity += (k * (goal - m_Current) - d * m_Velocity) * Time.unscaledDeltaTime;
            m_Current += m_Velocity * Time.unscaledDeltaTime;
            Apply(m_Current);
        }

        public void Apply(float scale) => Target.localScale = new Vector3(scale, scale, 1f);
        //#endregion

        //#region sound
        /// 合成一个 40 毫秒的短促“咔哒”：衰减的高频正弦 + 一点噪声
        static AudioClip CreateClick()
        {
            const int rate = 44100;
            int n = rate * 40 / 1000;
            var data = new float[n];
            var rnd = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-t * 120f);
                data[i] = env * (0.7f * Mathf.Sin(2 * Mathf.PI * 1800f * t) + 0.3f * (float)(rnd.NextDouble() * 2 - 1));
            }
            var clip = AudioClip.Create("UIClick", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
        //#endregion
    }
}
