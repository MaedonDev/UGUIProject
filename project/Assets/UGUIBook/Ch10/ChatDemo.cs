// 案例 10.1：自适应聊天气泡。
// 气泡的宽高全部交给布局系统：行(HLG) → 气泡(Image + VLG + MaxSizeLayoutElement) → 文字(TMP)。
// 本脚本只负责：克隆左右两种行模板、填文字、发完消息滚到底部。
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    public class ChatDemo : MonoBehaviour
    {
        [SerializeField] ScrollRect m_Scroll;
        [SerializeField] RectTransform m_LeftTemplate;    // 对方的消息行（隐藏的模板）
        [SerializeField] RectTransform m_RightTemplate;   // 自己的消息行
        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Button m_Send;

        static readonly string[] Replies =
        {
            "收到！", "晚上八点开黑，记得带上你的新皮肤。",
            "这一段话比较长，用来测试气泡在超过最大宽度以后会不会自动换行，而不是把整个屏幕撑满。", "好", "哈哈哈哈",
        };
        int m_ReplyIndex;

        void Awake()
        {
            m_Send.onClick.AddListener(OnSend);
            m_Input.onSubmit.AddListener(_ => OnSend());
        }

        void OnSend()
        {
            var text = m_Input.text.Trim();
            if (text.Length == 0) return;
            Add(text, true);
            m_Input.text = "";
            m_Input.ActivateInputField();
            StartCoroutine(FakeReply());
        }

        IEnumerator FakeReply()
        {
            yield return new WaitForSeconds(0.6f);
            Add(Replies[m_ReplyIndex++ % Replies.Length], false);
        }

        //#region add
        /// 加一条消息并滚到底部
        public void Add(string message, bool mine)
        {
            var row = Instantiate(mine ? m_RightTemplate : m_LeftTemplate, m_Scroll.content);
            row.gameObject.SetActive(true);
            row.GetComponentInChildren<TMP_Text>().text = message;
            ScrollToBottom();
        }

        public void ScrollToBottom()
        {
            // 新加的行要等到本帧末尾的布局重建才有尺寸，Content 的高度此刻还是旧值。
            // 直接设 verticalNormalizedPosition = 0 会停在“倒数第二条”，所以先强制把布局算完。
            Canvas.ForceUpdateCanvases();
            m_Scroll.verticalNormalizedPosition = 0;
        }
        //#endregion
    }
}
