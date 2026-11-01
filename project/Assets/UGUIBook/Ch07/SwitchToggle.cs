// iOS 风格开关：本质仍是 Toggle（点击、导航、ToggleGroup、onValueChanged 全部复用），
// 只是不用 Toggle 自带的“Checkmark 淡入淡出”，改为自己驱动滑块位置和轨道颜色。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    [RequireComponent(typeof(Toggle))]
    [ExecuteAlways]
    public class SwitchToggle : MonoBehaviour
    {
        [SerializeField] RectTransform m_Knob;          // 圆形滑块
        [SerializeField] Image m_Track;                 // 胶囊形轨道
        [SerializeField] Color m_OnColor = new Color32(0x2F, 0xB3, 0x6A, 0xFF);
        [SerializeField] Color m_OffColor = new Color32(0xD5, 0xD9, 0xE3, 0xFF);
        [SerializeField] float m_Padding = 6f;          // 滑块离轨道边缘的距离
        [SerializeField] float m_Duration = 0.15f;      // 切换动画时长（秒）

        Toggle m_Toggle;
        float m_T;           // 当前动画进度：0 = 关，1 = 开

        //#region core
        void Awake()
        {
            m_Toggle = GetComponent<Toggle>();
            // Toggle 的 graphic 留空，Toggle 就不会去改 Checkmark 的透明度；视觉完全交给本组件
            m_Toggle.graphic = null;
            m_Toggle.toggleTransition = Toggle.ToggleTransition.None;
        }

        void OnEnable()
        {
            if (m_Toggle == null) m_Toggle = GetComponent<Toggle>();
            m_T = m_Toggle.isOn ? 1 : 0;      // 启用时直接跳到正确状态，不播动画
            Evaluate(m_T);
        }

        void Update()
        {
            float target = m_Toggle.isOn ? 1 : 0;
            if (!Application.isPlaying || m_Duration <= 0) { m_T = target; Evaluate(m_T); return; }
            if (Mathf.Approximately(m_T, target)) return;
            // 用 unscaledDeltaTime：游戏暂停（timeScale = 0）时设置界面的开关照样能动
            m_T = Mathf.MoveTowards(m_T, target, Time.unscaledDeltaTime / m_Duration);
            Evaluate(m_T);
        }

        /// 把开关画成进度 t（0~1）时的样子。批处理截图、编辑器预览都直接调它
        public void Evaluate(float t)
        {
            if (m_Knob == null || m_Track == null) return;
            float e = t * t * (3f - 2f * t);                       // SmoothStep 缓动
            var track = (RectTransform)m_Track.transform;
            float travel = track.rect.width - m_Knob.rect.width - m_Padding * 2;
            // 滑块锚点在轨道左侧中点，所以 x 从 padding 走到 padding + travel
            m_Knob.anchoredPosition = new Vector2(m_Padding + travel * e, 0);
            m_Track.color = Color.Lerp(m_OffColor, m_OnColor, e);
        }
        //#endregion

        /// 搭界面用：一次性把引用和颜色配好
        public void Setup(RectTransform knob, Image track, Color onColor)
        {
            m_Knob = knob; m_Track = track; m_OnColor = onColor;
            if (m_Toggle == null) m_Toggle = GetComponent<Toggle>();
            Evaluate(m_Toggle.isOn ? 1 : 0);
        }
    }
}
