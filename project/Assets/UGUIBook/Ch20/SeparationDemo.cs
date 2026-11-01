// 动静分离演示：每帧都在变的计时文字和转圈图标，点按钮在“与 1500 个静态图标同一 Canvas”和“独立嵌套 Canvas”之间切换。
// 配合 RenderStatsHUD 或 Profiler 的 UI 模块看 Canvas.BuildBatch 与主线程耗时的变化。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    public class SeparationDemo : MonoBehaviour
    {
        [SerializeField] RectTransform m_Dynamic;       // 动态元素的根节点
        [SerializeField] RectTransform m_StaticParent;  // 静态界面所在的画布
        [SerializeField] RectTransform m_DynamicCanvas; // 一个独立的嵌套 Canvas
        [SerializeField] TMP_Text m_Timer;
        [SerializeField] RectTransform m_Spinner;
        [SerializeField] Button m_Toggle;
        [SerializeField] TMP_Text m_ToggleLabel;

        void Awake() => m_Toggle.onClick.AddListener(Toggle);

        void Start() => Apply(false);

        void Update()
        {
            m_Timer.SetText("{0:2}", Time.time);                          // 每帧改文字：图形重建
            m_Spinner.localEulerAngles = new Vector3(0, 0, -Time.time * 180); // 每帧转动：不重建网格，但所在 Canvas 要重新合批
        }

        bool m_Separated;
        void Toggle() => Apply(!m_Separated);

        void Apply(bool separated)
        {
            m_Separated = separated;
            m_Dynamic.SetParent(separated ? m_DynamicCanvas : m_StaticParent, false);
            m_ToggleLabel.text = separated ? "当前：动静分离（点我合并）" : "当前：同一 Canvas（点我分离）";
        }
    }
}
