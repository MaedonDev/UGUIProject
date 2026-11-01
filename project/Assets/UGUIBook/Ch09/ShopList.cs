// 商城列表：数据 → 按模板生成商品卡片，挂在 ScrollRect 的 Content 下。
// Content 上的 GridLayoutGroup 负责排位置，ContentSizeFitter 负责把 Content 撑到正好装下所有卡片——
// ScrollRect 只认 Content 的尺寸，Content 不够高就滚不动。
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    public class ShopList : MonoBehaviour
    {
        [Serializable] public struct Goods { public string name; public string sprite; public int price; public string tag; }

        [SerializeField] ScrollRect m_Scroll;
        [SerializeField] RectTransform m_Template;     // 卡片模板（未激活，不在 Content 下）
        [SerializeField] TMP_Text m_Coins;
        [SerializeField] TMP_Text m_Toast;
        [SerializeField] int m_Money = 1280;
        [SerializeField] Goods[] m_Goods;

        readonly List<GameObject> m_Cards = new List<GameObject>();

        void Awake() => Build();

        //#region build
        public void Build()
        {
            if (m_Cards.Count > 0) return;
            foreach (var g in m_Goods)
            {
                var card = Instantiate(m_Template, m_Scroll.content);   // 父节点是 Content：GridLayoutGroup 会接管它的位置和尺寸
                card.gameObject.SetActive(true);
                card.name = "Goods_" + g.name;
                card.Find("Icon").GetComponent<Image>().sprite = UISkin.Sprite(g.sprite);
                card.Find("Name").GetComponent<TMP_Text>().text = g.name;
                var tag = card.Find("Tag");
                tag.gameObject.SetActive(!string.IsNullOrEmpty(g.tag));
                tag.GetComponentInChildren<TMP_Text>().text = g.tag;
                var buy = card.Find("Buy").GetComponent<Button>();
                buy.GetComponentInChildren<TMP_Text>().text = g.price.ToString();
                var goods = g;                       // 闭包要捕获循环内的副本
                buy.onClick.AddListener(() => Buy(goods));
                m_Cards.Add(card.gameObject);
            }
            RefreshCoins();
        }
        //#endregion

        void Buy(Goods g)
        {
            if (m_Money < g.price) { m_Toast.text = $"金币不足，买不起{g.name}"; return; }
            m_Money -= g.price;
            m_Toast.text = $"购买成功：{g.name}";
            RefreshCoins();
        }

        void RefreshCoins() => m_Coins.text = m_Money.ToString("N0");

        public void Setup(ScrollRect sr, RectTransform tpl, TMP_Text coins, TMP_Text toast, Goods[] goods)
        {
            m_Scroll = sr; m_Template = tpl; m_Coins = coins; m_Toast = toast; m_Goods = goods;
        }
    }
}
