using System;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region mailitem
    /// 邮件列表的一项。被复用时 Set 会把每个字段都重新赋值——这是循环列表的铁律：
    /// 任何“只在某些情况下才设置”的字段（比如只在已领取时把按钮置灰）都会把上一条数据的状态带过来
    public class MailItemView : MonoBehaviour
    {
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_Title;
        [SerializeField] TMP_Text m_Sub;
        [SerializeField] Button m_Claim;
        [SerializeField] TMP_Text m_ClaimText;
        [SerializeField] Image m_Highlight;

        int m_Index;
        Action<int> m_OnClaim;

        void Awake() => m_Claim.onClick.AddListener(() => m_OnClaim?.Invoke(m_Index));

        public void Set(int index, MailData d, bool highlight, Action<int> onClaim)
        {
            m_Index = index;          // 按钮回调读的是“当前”下标，复用后自动指向新数据
            m_OnClaim = onClaim;
            m_Icon.sprite = UISkin.Sprite(d.icon);
            m_Title.text = d.title;
            m_Sub.text = d.sub;
            m_Claim.interactable = !d.claimed;
            m_ClaimText.text = d.claimed ? "已领取" : "领取";
            m_Highlight.enabled = highlight;
        }
    }

    public class MailData
    {
        public string title, sub, icon;
        public bool claimed;
    }
    //#endregion
}
