using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region entry
    /// 任务追踪里的一条任务
    public class QuestEntryView : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Title;
        [SerializeField] TMP_Text m_Objectives;
        [SerializeField] Image m_Flash;            // 进度变化时闪一下的高亮底
        [SerializeField] GameObject m_Direction;
        [SerializeField] RectTransform m_Arrow;
        [SerializeField] TMP_Text m_Distance;

        float m_FlashT;
        int m_LastMeters = -1;

        public void Set(string title, bool main, string objectives, bool done)
        {
            string tag = main ? "<color=#FFB547>[主线]</color> " : "<color=#7FB2FF>[支线]</color> ";
            m_Title.text = tag + title + (done ? "  <color=#4CD16E>可交付</color>" : "");
            m_Objectives.text = objectives;
        }

        public void SetDirection(bool show, float angle, float meters)
        {
            if (m_Direction.activeSelf != show) m_Direction.SetActive(show);
            if (!show) return;
            m_Arrow.localRotation = Quaternion.Euler(0, 0, -angle);
            int m = Mathf.RoundToInt(meters);
            if (m != m_LastMeters) { m_LastMeters = m; m_Distance.text = m + "m"; }
        }

        public void Flash() { m_FlashT = 1; ApplyFlash(); }

        public void Tick(float dt)
        {
            if (m_FlashT <= 0) return;
            m_FlashT = Mathf.Max(0, m_FlashT - dt * 1.2f);
            ApplyFlash();
        }

        void ApplyFlash()
        {
            var c = m_Flash.color;
            c.a = 0.45f * m_FlashT;
            m_Flash.color = c;
        }
    }
    //#endregion
}
