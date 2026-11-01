// 五种隐藏界面方法的对比演示：选一种方法，然后反复点“显示/隐藏”，看 HUD 上的耗时与 Batches。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    public class HideMethodsDemo : MonoBehaviour
    {
        [SerializeField] RectTransform m_Panel;          // 带 Canvas + GraphicRaycaster + CanvasGroup 的弹窗
        [SerializeField] Button[] m_MethodButtons;       // 顺序与 PerfLabCases.HideMethod 枚举一致
        [SerializeField] Button m_ToggleButton;
        [SerializeField] TMP_Text m_Status;

        PerfLabCases.HideMethod m_Method;
        bool m_Visible = true;

        void Awake()
        {
            for (int i = 0; i < m_MethodButtons.Length; i++)
            {
                var m = (PerfLabCases.HideMethod)i;
                m_MethodButtons[i].onClick.AddListener(() => Select(m));
            }
            m_ToggleButton.onClick.AddListener(Toggle);
            Refresh();
        }

        void Select(PerfLabCases.HideMethod m)
        {
            // 换方法前先用当前方法把弹窗恢复显示，避免几种“隐藏”状态叠在一起
            PerfLabCases.SetVisible(m_Panel, m_Method, true);
            m_Visible = true;
            m_Method = m;
            Refresh();
        }

        void Toggle()
        {
            m_Visible = !m_Visible;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            PerfLabCases.SetVisible(m_Panel, m_Method, m_Visible);
            Canvas.ForceUpdateCanvases();            // 把本次切换引起的重建立刻做掉，计入耗时
            m_Status.text = $"方法：{m_Method}    {(m_Visible ? "显示" : "隐藏")}耗时 {sw.Elapsed.TotalMilliseconds:0.000} ms（C# 重建部分）";
        }

        void Refresh() => m_Status.text = $"方法：{m_Method}    点“显示/隐藏”开始测";
    }
}
