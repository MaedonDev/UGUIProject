// 防连点：按钮点击后进入冷却，期间 interactable = false，遮罩从满到空，结束后恢复。
// 适合“购买”“领取”“发送”这类不能重复提交的按钮。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    [RequireComponent(typeof(Button))]
    public class ButtonCooldown : MonoBehaviour
    {
        //#region main
        [SerializeField] float m_Cooldown = 3f;
        [SerializeField] Image m_Overlay;        // 圆角半透明遮罩（Sliced），用锚点控制宽度
        [SerializeField] TMP_Text m_Label;
        [SerializeField] string m_ReadyText = "领取";

        Button m_Button;
        float m_EndTime = -1f;

        void Awake()
        {
            m_Button = GetComponent<Button>();
            // 冷却逻辑必须排在其他监听之后也没关系：Press() 在调用 onClick 之前已经检查过 IsInteractable()
            m_Button.onClick.AddListener(StartCooldown);
            Show(0f);
        }

        public void StartCooldown()
        {
            m_EndTime = Time.unscaledTime + m_Cooldown;
            m_Button.interactable = false;           // 关键一步：之后的点击在 Button.Press() 里就被拦掉
        }

        void Update()
        {
            if (m_EndTime < 0f) return;
            float remain = m_EndTime - Time.unscaledTime;
            if (remain <= 0f) { m_EndTime = -1f; m_Button.interactable = true; remain = 0f; }
            Show(remain);
        }

        /// 显示剩余时间（拍图时也直接调用）
        public void Show(float remain)
        {
            // 不用 Filled：Filled 不支持九宫格，圆角会被压扁。改锚点 = 遮罩宽度按比例缩短，圆角保持不变
            m_Overlay.rectTransform.anchorMax = new Vector2(remain / m_Cooldown, 1f);
            m_Overlay.enabled = remain > 0f;
            if (remain > 0f) m_Label.SetText("{0:1} 秒", remain);
            else m_Label.text = m_ReadyText;
        }
        //#endregion
    }
}
