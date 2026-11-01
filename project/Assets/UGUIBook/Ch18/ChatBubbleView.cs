using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region chatdata
    public enum ChatKind { Other, Me, Time }

    public class ChatMessage
    {
        public ChatKind Kind;
        public string Sender;
        public string Avatar;
        public string Text;
        // 由 ChatPanel 预先算好并缓存：滚动时绝不能临时去量文字，那会让每帧都做排版
        public Vector2 TextSize;
        public float Height;
    }
    //#endregion

    //#region bubble
    /// 聊天气泡（左右两个模板共用这个脚本，只是锚点镜像）。不用 LayoutGroup：
    /// 尺寸已经在数据层算好，这里直接赋值，复用时没有任何布局计算
    public class ChatBubbleView : MonoBehaviour
    {
        [SerializeField] Image m_Avatar;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] RectTransform m_Bubble;
        [SerializeField] TMP_Text m_Text;

        public const float AvatarSize = 84, Gap = 16, NameHeight = 36, PadX = 26, PadY = 16, Bottom = 8;

        public void Set(ChatMessage m)
        {
            if (m_Avatar != null) m_Avatar.sprite = UISkin.Sprite(m.Avatar);
            if (m_NameText != null) m_NameText.text = m.Sender;
            m_Text.text = m.Text;
            m_Bubble.sizeDelta = new Vector2(m.TextSize.x + PadX * 2, m.TextSize.y + PadY * 2);
        }

        /// 一条消息的总高度（名字 + 气泡 + 底部留白），头像比气泡高时以头像为准
        public static float HeightOf(ChatMessage m) =>
            Mathf.Max(AvatarSize, NameHeight + m.TextSize.y + PadY * 2) + Bottom;
    }
    //#endregion
}
