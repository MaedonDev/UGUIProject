// 对话框：打字机效果（TMP maxVisibleCharacters）+ 标点停顿 + 点击跳过/继续 + 分支选项
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region data
    [Serializable]
    public class DialogueOption
    {
        public string text;
        public int next = -1;          // 跳到第几句；-1 结束对话
    }

    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        public string avatar;          // UISkin 精灵名
        [TextArea] public string text;
        public int next = -1;          // 没有选项时的下一句；-1 = 顺序下一句
        public bool end;               // 这一句说完就结束对话
        public DialogueOption[] options;
    }
    //#endregion

    //#region dialogue
    public class DialogueBox : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] TMP_Text m_Speaker;
        [SerializeField] Image m_Avatar;
        [SerializeField] TMP_Text m_Body;
        [SerializeField] GameObject m_ContinueHint;   // 一句说完后出现的小箭头
        [SerializeField] Button m_OptionTemplate;     // 选项按钮模板（隐藏）
        [SerializeField] float m_CharsPerSecond = 28f;
        [SerializeField] float m_PunctuationPause = 0.18f;
        [SerializeField] DialogueLine[] m_Lines;      // 场景里可以直接配一段对话
        [SerializeField] bool m_PlayOnStart = true;

        public event Action Finished;
        public event Action<int, int> OptionChosen;    // 句子序号、选项序号

        readonly List<Button> m_Options = new List<Button>();
        DialogueLine[] m_Script;
        int m_Index = -1;
        float m_Visible;      // 已显示的字符数（小数，便于按速度累加）
        float m_Pause;
        int m_Total;

        public bool IsTyping => m_Index >= 0 && m_Visible < m_Total;
        public int Index => m_Index;

        void Start()
        {
            m_OptionTemplate.gameObject.SetActive(false);
            if (m_PlayOnStart && m_Lines != null && m_Lines.Length > 0) Play(m_Lines);
        }

        public void Play(DialogueLine[] script, int start = 0)
        {
            m_Script = script;
            gameObject.SetActive(true);
            ShowLine(start);
        }

        public void ShowLine(int index)
        {
            if (m_Script == null || index < 0 || index >= m_Script.Length) { End(); return; }
            m_Index = index;
            var line = m_Script[index];
            m_Speaker.text = line.speaker;
            if (m_Avatar != null && !string.IsNullOrEmpty(line.avatar)) m_Avatar.sprite = UISkin.Sprite(line.avatar);
            m_Body.text = line.text;
            m_Body.maxVisibleCharacters = 0;
            m_Body.ForceMeshUpdate();                         // 立刻解析文本，才能拿到字符总数（富文本标签不算字符）
            m_Total = m_Body.textInfo.characterCount;
            m_Visible = 0;
            m_Pause = 0;
            ClearOptions();
            m_ContinueHint.SetActive(false);
        }

        void Update()
        {
            Tick(Time.unscaledDeltaTime);
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) Advance();
            for (int i = 0; i < m_Options.Count && i < 9; i++)
                if (m_Options[i].gameObject.activeSelf && Input.GetKeyDown(KeyCode.Alpha1 + i)) Choose(i);
        }

        public void Tick(float dt)
        {
            if (!IsTyping) return;
            if (m_Pause > 0) { m_Pause -= dt; return; }
            int before = (int)m_Visible;
            m_Visible = Mathf.Min(m_Total, m_Visible + dt * m_CharsPerSecond);
            int now = (int)m_Visible;
            if (now != before)
            {
                m_Body.maxVisibleCharacters = now;
                // 刚显示出的字符是标点：停顿一下，读起来更像说话
                char c = m_Body.textInfo.characterInfo[now - 1].character;
                if ("，。！？、；：…,.!?".IndexOf(c) >= 0) m_Pause = m_PunctuationPause;
            }
            if (!IsTyping) OnLineComplete();
        }

        /// 立刻显示完整句子
        public void CompleteTyping()
        {
            if (!IsTyping) return;
            m_Visible = m_Total;
            m_Body.maxVisibleCharacters = m_Total;
            OnLineComplete();
        }

        /// 显示到一半（截图、回放用）：progress 0~1
        public void SetProgress(float progress)
        {
            m_Visible = Mathf.Floor(m_Total * Mathf.Clamp01(progress));
            m_Body.maxVisibleCharacters = (int)m_Visible;
            if (!IsTyping) OnLineComplete();
        }

        public void OnPointerClick(PointerEventData e) => Advance();

        /// 点击/空格：正在打字 → 显示全句；已显示完且没有选项 → 下一句
        public void Advance()
        {
            if (m_Index < 0) return;
            if (IsTyping) { CompleteTyping(); return; }
            var line = m_Script[m_Index];
            if (line.options != null && line.options.Length > 0) return;   // 必须点选项
            if (line.end) End();
            else ShowLine(line.next >= 0 ? line.next : m_Index + 1);
        }

        void OnLineComplete()
        {
            var line = m_Script[m_Index];
            bool hasOptions = line.options != null && line.options.Length > 0;
            m_ContinueHint.SetActive(!hasOptions);
            if (!hasOptions) return;
            for (int i = 0; i < line.options.Length; i++)
            {
                if (i >= m_Options.Count)
                {
                    var b = Instantiate(m_OptionTemplate, m_OptionTemplate.transform.parent, false);
                    int captured = i;                                    // 闭包陷阱：必须拷贝循环变量
                    b.onClick.AddListener(() => Choose(captured));
                    m_Options.Add(b);
                }
                m_Options[i].gameObject.SetActive(true);
                m_Options[i].GetComponentInChildren<TMP_Text>().text = $"{i + 1}. {line.options[i].text}";
            }
        }

        void Choose(int i)
        {
            var line = m_Script[m_Index];
            if (line.options == null || i >= line.options.Length) return;
            OptionChosen?.Invoke(m_Index, i);
            ShowLine(line.options[i].next);
        }

        void ClearOptions()
        {
            foreach (var b in m_Options) b.gameObject.SetActive(false);
        }

        void End()
        {
            m_Index = -1;
            ClearOptions();
            gameObject.SetActive(false);
            Finished?.Invoke();
        }
    }
    //#endregion
}
