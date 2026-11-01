// 只读进度条：用 Slider 做外观（Fill 自动按 value 伸缩），但关掉交互，并让显示值平滑追赶目标值。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    [RequireComponent(typeof(Slider))]
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Percent;          // “62%”
        [SerializeField] float m_Speed = 1.5f;        // 每秒最多前进多少（0~1），防止进度瞬间跳满
        Slider m_Slider;
        float m_Target;

        public float Target => m_Target;
        public float Shown => Bar.normalizedValue;
        Slider Bar => m_Slider != null ? m_Slider : (m_Slider = GetComponent<Slider>());

        //#region readonly
        void Awake()
        {
            // 只读：不可交互、不参与导航、不挡射线。interactable = false 会让 Selectable 进入 Disabled 状态，
            // 所以过渡方式也一并关掉，否则进度条会被染成灰色
            Bar.interactable = false;
            Bar.transition = Selectable.Transition.None;
            Bar.navigation = new Navigation { mode = Navigation.Mode.None };
            foreach (var g in GetComponentsInChildren<Graphic>()) g.raycastTarget = false;
        }
        //#endregion

        //#region progress
        /// 设置目标进度（0~1）。instant = true 时立即显示到位（重置、截图时用）
        public void SetProgress(float p, bool instant = false)
        {
            m_Target = Mathf.Clamp01(p);
            if (instant) Show(m_Target);
        }

        void Update()
        {
            if (Mathf.Approximately(Shown, m_Target)) return;
            // 只往前追：加载进度不应该倒退（重置请用 instant）
            Show(Mathf.MoveTowards(Shown, m_Target, m_Speed * Time.unscaledDeltaTime));
        }

        void Show(float v)
        {
            Bar.SetValueWithoutNotify(Mathf.Lerp(Bar.minValue, Bar.maxValue, v));
            if (m_Percent != null) m_Percent.text = Mathf.FloorToInt(v * 100) + "%";   // Floor：没加载完不显示 100%
        }
        //#endregion

        public void Setup(TMP_Text percent) => m_Percent = percent;
    }
}
