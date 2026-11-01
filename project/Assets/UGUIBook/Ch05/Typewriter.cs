// 打字机效果：文字一次性排版好，只改 maxVisibleCharacters 让字符逐个出现。
// 好处：富文本标签不会被“打”出来，换行位置从头到尾不跳动，每帧也不重新排版。
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UGUIBook.Ch05
{
    public class Typewriter : MonoBehaviour
    {
        //#region fields
        [SerializeField] TMP_Text m_Text;
        [SerializeField] float m_CharsPerSecond = 24f;
        [SerializeField] float m_PunctuationPause = 0.22f;   // 逗号、句号后多停一会，读起来有节奏
        [SerializeField] bool m_UnscaledTime = true;         // 暂停游戏（timeScale = 0）时对话照样能打字
        public UnityEvent onFinished = new UnityEvent();
        //#endregion

        int m_Total, m_Visible;
        float m_Timer;
        bool m_Playing;

        public bool IsPlaying => m_Playing;
        public TMP_Text Text => m_Text;

        //#region play
        public void Play(string text)
        {
            m_Text.text = text;
            m_Text.maxVisibleCharacters = 0;
            m_Text.ForceMeshUpdate();                       // 立即排版，拿到真实的字符数（不含富文本标签）
            m_Total = m_Text.textInfo.characterCount;
            m_Visible = 0;
            m_Timer = 0f;
            m_Playing = m_Total > 0;
            if (!m_Playing) onFinished.Invoke();
        }

        /// 点击“跳过”：直接显示全部
        public void Skip()
        {
            if (!m_Playing) return;
            Show(m_Total);
            Finish();
        }
        //#endregion

        //#region tick
        void Update()
        {
            if (!m_Playing) return;
            m_Timer += m_UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float interval = 1f / Mathf.Max(1f, m_CharsPerSecond);
            int visible = m_Visible;
            while (m_Timer >= interval && visible < m_Total)    // 帧率低时一帧可能要出好几个字
            {
                m_Timer -= interval;
                char c = m_Text.textInfo.characterInfo[visible].character;
                visible++;
                if (IsPause(c)) m_Timer -= m_PunctuationPause;
            }
            if (visible != m_Visible) Show(visible);
            if (m_Visible >= m_Total) Finish();
        }

        const string PauseChars = "，。！？…,.!?";
        static bool IsPause(char c) => PauseChars.IndexOf(c) >= 0;
        //#endregion

        void Show(int count)
        {
            m_Visible = count;
            m_Text.maxVisibleCharacters = count;            // 只更新顶点可见性，不重新排版
        }

        void Finish()
        {
            m_Playing = false;
            onFinished.Invoke();
        }

        /// 拍图/预览用：直接跳到某个进度（0~1）
        public void Preview(string text, float progress)
        {
            Play(text);
            Show(Mathf.RoundToInt(m_Total * Mathf.Clamp01(progress)));
            if (progress >= 1f) m_Playing = false;
        }
    }
}
