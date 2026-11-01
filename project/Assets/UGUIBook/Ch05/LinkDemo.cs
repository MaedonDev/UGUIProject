// 链接演示：点物品名弹出说明卡片，点网址只打印日志
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch05
{
    public class LinkDemo : MonoBehaviour
    {
        [SerializeField] LinkHandler m_Handler;
        [SerializeField] RectTransform m_Tip;
        [SerializeField] TMP_Text m_TipText;
        [SerializeField] TMP_Text m_Log;

        void Awake()
        {
            m_Handler.onLinkClick.AddListener(OnLink);
            m_Tip.gameObject.SetActive(false);
        }

        //#region onlink
        void OnLink(string id, Vector2 screenPos)
        {
            m_Log.text = "点击了链接：" + id;
            // 约定 id 的格式为 “类型:参数”，按类型分发
            var parts = id.Split(':');
            switch (parts[0])
            {
                case "item": ShowTip(parts[1], screenPos); break;
                case "url":
                    Debug.Log("打开网页 " + parts[1]);        // 真机上用 Application.OpenURL(parts[1])
                    m_Tip.gameObject.SetActive(false);
                    break;
                default: m_Tip.gameObject.SetActive(false); break;
            }
        }
        //#endregion

        public void ShowTip(string item, Vector2 screenPos)
        {
            m_TipText.text = item switch
            {
                "sword" => "<b><color=#FF8A3D>破晓之剑</color></b>\n攻击 +120\n" +
                           "<size=80%><color=#7A8299>传说中在黎明时铸成的剑</color></size>",
                "potion" => "<b><color=#E5484D>生命药水</color></b>\n立即恢复 500 点生命\n" +
                            "<size=80%><color=#7A8299>可叠加 99 个</color></size>",
                _ => item,
            };
            var parent = (RectTransform)m_Tip.parent;
            var canvas = parent.GetComponentInParent<Canvas>();
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPos, cam, out var local);
            m_Tip.anchoredPosition = local + new Vector2(0, -40);   // 卡片轴心在顶边：出现在链接下方
            m_Tip.gameObject.SetActive(true);
        }
    }
}
