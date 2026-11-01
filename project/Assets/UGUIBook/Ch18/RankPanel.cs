// 排行榜：一千名玩家的循环列表 + “我的排名”固定行。
// 我的那一行在视口里完整可见时，固定行隐藏；滚出视口上方时固定行贴在顶部，滚出下方时贴在底部
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region rank
    public class RankPanel : MonoBehaviour
    {
        [SerializeField] LoopListView m_List;
        [SerializeField] RankRowView m_Pinned;     // 列表外面的一行，与列表项同一个模板
        [SerializeField] Button m_LocateMe;
        [SerializeField] int m_Count = 1000;
        [SerializeField] int m_MyRank = 386;

        static readonly string[] s_Names = { "夜莺", "铁匠老王", "小鹿", "阿土", "青柠", "北风", "白夜", "墨鱼", "柚子", "南山" };
        readonly List<RankEntry> m_Entries = new List<RankEntry>();

        public LoopListView List => m_List;

        void Start()
        {
            m_LocateMe.onClick.AddListener(() => m_List.ScrollTo(m_MyRank - 1, 0.5f, 0.4f));
            Init();
        }

        public void Init()
        {
            m_Entries.Clear();
            var rnd = new System.Random(2026);
            int score = 98000;
            for (int i = 0; i < m_Count; i++)
            {
                score -= rnd.Next(10, 150);
                bool me = i == m_MyRank - 1;
                m_Entries.Add(new RankEntry
                {
                    Rank = i + 1, IsMe = me, Score = score, Level = 60 - i / 25,
                    Name = me ? "麦冬（我）" : s_Names[rnd.Next(s_Names.Length)] + rnd.Next(10, 99),
                    Avatar = me ? "avatar_1" : "avatar_" + rnd.Next(2, 7),
                });
            }
            m_Pinned.Set(m_Entries[m_MyRank - 1]);
            m_List.onBind = (i, item) => item.GetComponent<RankRowView>().Set(m_Entries[i]);
            m_List.Scrolled -= UpdatePinned;
            m_List.Scrolled += UpdatePinned;
            m_List.SetCount(m_Entries.Count);
        }

        void UpdatePinned()
        {
            int me = m_MyRank - 1;
            bool visible = m_List.IsFullyVisible(me);
            if (m_Pinned.gameObject.activeSelf == visible) m_Pinned.gameObject.SetActive(!visible);
            if (visible) return;
            // 我在视口上方 → 固定在顶部；在下方 → 固定在底部
            bool above = m_List.ItemTop(me) < m_List.ScrollY;
            var rt = (RectTransform)m_Pinned.transform;
            var anchor = above ? 1f : 0f;
            rt.anchorMin = new Vector2(0, anchor);
            rt.anchorMax = new Vector2(1, anchor);
            rt.pivot = new Vector2(0.5f, anchor);
            rt.anchoredPosition = Vector2.zero;
        }
    }
    //#endregion
}
