// 弹窗的打开 / 关闭动画：遮罩淡入 + 面板从 0.6 倍弹到 1 倍（OutBack 回弹）+ 面板淡入。
// 动画都由 BuildShow/BuildHide 生成 Sequence：运行时 Play，截图/调试时直接 Evaluate(t)。
using UnityEngine;

namespace UGUIBook.Ch15
{
    public class PopupAnim : MonoBehaviour
    {
        //#region fields
        [SerializeField] CanvasGroup m_Dim;        // 全屏半透明遮罩
        [SerializeField] RectTransform m_Panel;    // 弹窗本体
        [SerializeField] CanvasGroup m_PanelGroup; // 弹窗本体上的 CanvasGroup（统一控制透明度和交互）
        [SerializeField] float m_Duration = 0.35f;

        Sequence m_Current;
        //#endregion

        public void Setup(CanvasGroup dim, RectTransform panel, CanvasGroup panelGroup)
        { m_Dim = dim; m_Panel = panel; m_PanelGroup = panelGroup; }

        //#region build
        public Sequence BuildShow()
        {
            var seq = new Sequence();
            seq.Append(m_Dim.Alpha(1, m_Duration * 0.6f).From(0).SetEase(Ease.OutQuad));
            seq.Join(m_Panel.Scale(1f, m_Duration).From(Vector3.one * 0.6f).SetEase(Ease.OutBack));
            seq.Join(m_PanelGroup.Alpha(1, m_Duration * 0.5f).From(0).SetEase(Ease.OutQuad));
            return seq;
        }

        public Sequence BuildHide()
        {
            var seq = new Sequence();
            seq.Append(m_Panel.Scale(0.85f, m_Duration * 0.6f).SetEase(Ease.InBack));
            seq.Join(m_PanelGroup.Alpha(0, m_Duration * 0.6f).SetEase(Ease.InQuad));
            seq.Join(m_Dim.Alpha(0, m_Duration * 0.6f).SetDelay(m_Duration * 0.1f));
            return seq;
        }
        //#endregion

        //#region play
        public void Show()
        {
            m_Current?.Kill();                          // 打开时上一个关闭动画还没播完：直接打断
            gameObject.SetActive(true);
            m_PanelGroup.interactable = m_PanelGroup.blocksRaycasts = true;
            m_Current = BuildShow().SetUnscaled().Play();   // 暂停游戏（timeScale = 0）时弹窗也要能动
        }

        public void Hide()
        {
            m_Current?.Kill();
            m_PanelGroup.interactable = false;          // 关闭动画期间不响应点击，防止连点
            m_Current = BuildHide().SetUnscaled().OnComplete(() => gameObject.SetActive(false)).Play();
        }
        //#endregion
    }
}
