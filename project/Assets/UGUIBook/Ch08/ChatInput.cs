// 聊天输入：回车发送、发送后清空并保持焦点、新消息自动滚到底。
// 气泡尺寸这里用 TMP 的 GetPreferredValues 手算（自动布局版本见第 10 章，大量消息的虚拟化列表见第 18 章）。
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class ChatInput : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Button m_Send;
        [SerializeField] ScrollRect m_Scroll;
        [SerializeField] RectTransform m_MineTemplate;     // “我”的气泡模板（右侧，未激活）
        [SerializeField] RectTransform m_OtherTemplate;    // 对方的气泡模板（左侧，未激活）
        [SerializeField] float m_MaxBubbleWidth = 620;
        [SerializeField] Vector2 m_Padding = new Vector2(28, 18);
        [SerializeField] bool m_AutoReply = true;           // 演示用：对方自动回一句

        //#region submit
        void Awake()
        {
            // onSubmit：在输入框里按回车（LineType 不是 MultiLineNewline 时）才触发；
            // 失焦不会触发，按 Esc 取消也不会触发——这正是“发送”需要的语义
            m_Input.onSubmit.AddListener(_ => Send());
            m_Send.onClick.AddListener(Send);
        }

        public void Send()
        {
            string msg = m_Input.text.Trim();
            if (msg.Length > 0)
            {
                AddMessage(msg, true);
                m_Input.text = "";
                if (m_AutoReply) StartCoroutine(Reply(msg));
            }
            // 回车会让输入框退出编辑状态（DeactivateInputField），聊天里通常希望能接着打字，所以重新激活
            m_Input.ActivateInputField();
        }
        //#endregion

        IEnumerator Reply(string msg)
        {
            yield return new WaitForSecondsRealtime(0.8f);
            AddMessage(msg.Length > 12 ? "收到，说得有点长，我慢慢看～" : $"你说的是“{msg}”吗？", false);
        }

        //#region bubble
        public void AddMessage(string msg, bool mine)
        {
            var item = Instantiate(mine ? m_MineTemplate : m_OtherTemplate, m_Scroll.content);
            item.gameObject.SetActive(true);
            var bubble = (RectTransform)item.Find("Bubble");
            var text = bubble.GetComponentInChildren<TMP_Text>();
            text.text = msg;
            // 先算单行宽度；超过最大宽度就按最大宽度折行，再算折行后的高度
            float w = Mathf.Min(text.GetPreferredValues(msg).x, m_MaxBubbleWidth);
            float h = text.GetPreferredValues(msg, w, 0).y;
            bubble.sizeDelta = new Vector2(w + m_Padding.x * 2, h + m_Padding.y * 2);
            // Content 上的 VerticalLayoutGroup 读 LayoutElement 决定这一行占多高
            item.GetComponent<LayoutElement>().preferredHeight = Mathf.Max(bubble.sizeDelta.y, 90);
            ScrollToBottom();
        }

        public void ScrollToBottom()
        {
            // Content 的高度由 ContentSizeFitter 在下一次布局重建时才更新；先强制重建，再设滚动位置
            Canvas.ForceUpdateCanvases();
            m_Scroll.verticalNormalizedPosition = 0;
        }
        //#endregion

        public void Setup(TMP_InputField input, Button send, ScrollRect scroll, RectTransform mine, RectTransform other)
        {
            m_Input = input; m_Send = send; m_Scroll = scroll; m_MineTemplate = mine; m_OtherTemplate = other;
        }
    }
}
