// 聊天列表：可变高度 + 三种模板（别人 / 自己 / 时间）+ 新消息自动滚到底（只有在底部时才自动滚）
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region chat
    public class ChatPanel : MonoBehaviour
    {
        [SerializeField] LoopListView m_List;
        [SerializeField] TMP_Text m_Measure;          // 和气泡里的文字同字体同字号的隐藏文本，专门用来量尺寸
        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Button m_Send;
        [SerializeField] Button m_NewTip;             // “有 N 条新消息”
        [SerializeField] TMP_Text m_NewTipText;
        [SerializeField] Toggle m_AutoChat;           // 勾上后机器人每隔一会儿说一句
        [SerializeField] float m_MaxBubbleRatio = 0.68f;

        static readonly string[] s_Lines =
        {
            "有人一起打副本吗？", "来了来了", "今晚八点公会战，记得上线！",
            "这个 Boss 第二阶段会召唤小怪，先清小怪再打本体，不然很容易团灭。", "好的",
            "刚开出一把紫色猎弓，有人要吗？便宜出。", "哈哈哈哈", "等我五分钟，在做日常",
            "新版本的背包终于可以一键整理了，太方便了！", "收到",
        };
        static readonly string[] s_Names = { "小鹿", "铁匠老王", "夜莺", "阿土" };

        readonly List<ChatMessage> m_Messages = new List<ChatMessage>();
        public LoopListView List => m_List;
        int m_Unread;
        float m_BotTimer = 1.5f;
        int m_BotIndex;
        float m_LastTime = -999;
        float m_Clock = 14 * 60 + 30;   // 模拟时钟（分钟）

        void Start()
        {
            m_Send.onClick.AddListener(SendFromInput);
            m_Input.onSubmit.AddListener(_ => SendFromInput());
            m_NewTip.onClick.AddListener(() => m_List.ScrollToBottom(0.3f));
            Init();
        }

        public void Init()
        {
            m_List.heightOf = i => m_Messages[i].Height;
            m_List.templateOf = i => (int)m_Messages[i].Kind;
            m_List.onBind = (i, item) =>
            {
                var m = m_Messages[i];
                if (m.Kind == ChatKind.Time) item.GetComponentInChildren<TMP_Text>().text = m.Text;
                else item.GetComponent<ChatBubbleView>().Set(m);
            };
            m_List.Scrolled -= OnScrolled;
            m_List.Scrolled += OnScrolled;
            m_List.SetCount(m_Messages.Count);
            m_NewTip.gameObject.SetActive(false);
        }

        void SendFromInput()
        {
            string s = m_Input.text.Trim();
            if (s.Length == 0) return;
            Add(ChatKind.Me, "我", "avatar_1", s);
            m_Input.text = "";
            m_Input.ActivateInputField();   // 发送后保持焦点，可以接着打字
            m_List.ScrollToBottom(0.25f);   // 自己发的消息：无论在哪都滚到底
        }

        void Update()
        {
            m_Clock += Time.deltaTime * 0.5f;
            if (m_AutoChat == null || !m_AutoChat.isOn) return;
            m_BotTimer -= Time.deltaTime;
            if (m_BotTimer > 0) return;
            m_BotTimer = Random.Range(1.2f, 2.5f);
            BotSay();
        }

        public void BotSay()
        {
            int i = m_BotIndex++;
            Add(ChatKind.Other, s_Names[i % s_Names.Length], "avatar_" + (2 + i % 5), s_Lines[i % s_Lines.Length]);
        }

        /// 添加一条消息。在底部时自动滚到底；用户正往上翻历史时不打扰，只累加“新消息”计数
        public void Add(ChatKind kind, string sender, string avatar, string text)
        {
            bool atBottom = m_List.Count == 0 || m_List.IsAtBottom(30f);
            int added = 0;
            // 距上一条超过 5 分钟（模拟时钟）插一条时间
            if (m_Clock - m_LastTime >= 5)
            {
                m_LastTime = m_Clock;
                m_Messages.Add(MakeTime($"{(int)(m_Clock / 60) % 24:00}:{(int)m_Clock % 60:00}"));
                added++;
            }
            m_Messages.Add(Make(kind, sender, avatar, text));
            added++;
            m_List.Append(added);
            if (atBottom || kind == ChatKind.Me) m_List.ScrollToBottom(Application.isPlaying ? 0.2f : 0f);
            else { m_Unread += 1; RefreshTip(); }
        }

        ChatMessage Make(ChatKind kind, string sender, string avatar, string text)
        {
            var m = new ChatMessage { Kind = kind, Sender = sender, Avatar = avatar, Text = text };
            float maxText = m_List.ItemWidth * m_MaxBubbleRatio - ChatBubbleView.PadX * 2;
            // 先按最大宽度量：短句返回真实宽度，长句自动换行后返回多行高度
            var size = m_Measure.GetPreferredValues(text, maxText, 0);
            // 宽度多留 2 像素：量出来的宽度和真正排版时差一点点浮点误差，就可能多换一行
            m.TextSize = new Vector2(Mathf.Min(Mathf.Ceil(size.x) + 2, maxText), Mathf.Ceil(size.y));
            m.Height = ChatBubbleView.HeightOf(m);
            return m;
        }

        static ChatMessage MakeTime(string t) => new ChatMessage { Kind = ChatKind.Time, Text = t, Height = 56 };

        void OnScrolled()
        {
            if (m_Unread > 0 && m_List.IsAtBottom(30f)) { m_Unread = 0; RefreshTip(); }
        }

        void RefreshTip()
        {
            m_NewTip.gameObject.SetActive(m_Unread > 0);
            m_NewTipText.text = $"{m_Unread} 条新消息";
        }
    }
    //#endregion
}
