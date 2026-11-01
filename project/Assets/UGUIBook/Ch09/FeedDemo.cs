// 配套场景 Ch09_PullRefresh：动态列表的“数据层”。刷新时把最新几条插到最前面，加载更多时追加到末尾。
using System.Collections;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    public class FeedDemo : MonoBehaviour
    {
        [SerializeField] PullToRefresh m_Pull;
        [SerializeField] RectTransform m_Content;
        [SerializeField] RectTransform m_Template;
        [SerializeField] RectTransform m_Footer;

        static readonly string[] kNames = { "艾琳", "布鲁诺", "卡莉", "达里安", "伊芙", "芬恩" };
        static readonly string[] kTexts =
        {
            "刚刚打通了困难模式的最终 Boss！", "求组队刷地牢，有奶妈吗？", "今天的日落好美，截图留念。",
            "新出的火抗戒指到底值不值得买？", "公会战第三名，大家辛苦了！", "钓鱼钓上来一把剑，这合理吗？",
        };
        int m_Serial, m_Pages;

        void Awake()
        {
            m_Pull.onRefresh.AddListener(() => StartCoroutine(Refresh()));
            m_Pull.onLoadMore.AddListener(() => StartCoroutine(LoadMore()));
        }

        void Start() => Fill(8);

        public void Fill(int n)
        {
            for (int i = 0; i < n; i++) Add(false);
            m_Footer.SetAsLastSibling();
        }

        void Add(bool atTop)
        {
            int k = m_Serial++;
            var item = Instantiate(m_Template, m_Content);
            item.gameObject.SetActive(true);
            item.name = "Post_" + k;
            item.Find("Avatar").GetComponent<Image>().sprite = UISkin.Sprite("avatar_" + (k % 6 + 1));
            item.Find("Name").GetComponent<TMP_Text>().text = kNames[k % kNames.Length] + (atTop ? "  <color=#FF8A3D><size=80%>新</size></color>" : "");
            item.Find("Body").GetComponent<TMP_Text>().text = kTexts[k % kTexts.Length];
            if (atTop) item.SetAsFirstSibling();
        }

        IEnumerator Refresh()
        {
            yield return new WaitForSecondsRealtime(1f);           // 模拟网络请求
            for (int i = 0; i < 2; i++) Add(true);
            m_Pull.EndRefresh();
        }

        IEnumerator LoadMore()
        {
            yield return new WaitForSecondsRealtime(0.8f);
            Fill(6);
            m_Pages++;
            m_Pull.EndLoadMore(m_Pages < 3);                        // 演示：加载 3 页后没有更多
        }

        public void Setup(PullToRefresh pull, RectTransform content, RectTransform tpl, RectTransform footer)
        {
            m_Pull = pull; m_Content = content; m_Template = tpl; m_Footer = footer;
        }
    }
}
