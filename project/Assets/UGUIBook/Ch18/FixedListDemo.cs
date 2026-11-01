using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region fixeddemo
    /// 固定高度循环列表：一万封邮件 + 跳转到第 N 项 + 实时显示实例数量
    public class FixedListDemo : MonoBehaviour
    {
        [SerializeField] LoopListView m_List;
        [SerializeField] TMP_InputField m_JumpInput;
        [SerializeField] Button m_JumpButton;
        [SerializeField] TMP_Text m_Stats;
        [SerializeField] int m_Count = 10000;

        static readonly string[] s_Icons = { "item_coin", "item_gem", "item_potion_red", "item_scroll", "gift", "item_key", "mail" };
        static readonly string[] s_From = { "系统", "公会", "活动", "好友", "客服" };

        readonly List<MailData> m_Data = new List<MailData>();
        int m_Highlight = -1;

        void Start()
        {
            m_JumpButton.onClick.AddListener(Jump);
            m_JumpInput.onSubmit.AddListener(_ => Jump());
            Init();
        }

        public void Init()
        {
            m_Data.Clear();
            for (int i = 0; i < m_Count; i++)
                m_Data.Add(new MailData
                {
                    title = $"第 {i + 1} 封邮件：{s_From[i % s_From.Length]}奖励",
                    sub = $"来自{s_From[i % s_From.Length]} · 附件 {s_Icons.Length - i % 3} 件 · {i % 28 + 1} 天前",
                    icon = s_Icons[i % s_Icons.Length],
                    claimed = i % 5 == 3,
                });
            m_List.onBind = Bind;
            m_List.Scrolled -= RefreshStats;
            m_List.Scrolled += RefreshStats;
            m_List.SetCount(m_Data.Count);
        }

        void Bind(int index, RectTransform item) =>
            item.GetComponent<MailItemView>().Set(index, m_Data[index], index == m_Highlight, Claim);

        void Claim(int index)
        {
            m_Data[index].claimed = true;
            m_List.RefreshItem(index);   // 只刷新这一项
        }

        public void Jump()
        {
            if (!int.TryParse(m_JumpInput.text, out int n)) return;
            JumpTo(n - 1);
        }

        /// 跳到第 index 项并居中、高亮
        public void JumpTo(int index)
        {
            m_Highlight = Mathf.Clamp(index, 0, m_Data.Count - 1);
            m_List.ScrollTo(m_Highlight, 0.5f, 0.35f);
            m_List.Refresh();
        }

        void RefreshStats()
        {
            if (m_Stats == null) return;
            m_Stats.text = $"数据 {m_List.Count} 条 · 正在显示第 {m_List.FirstVisible + 1}~{m_List.LastVisible + 1} 项 · 实例化的 Item 共 {m_List.CreatedCount} 个";
        }
    }
    //#endregion
}
