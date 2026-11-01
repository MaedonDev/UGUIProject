using System.Collections.Generic;
using UGUIBook.Core;

namespace UGUIBook.Ch16
{
    //#region bag
    /// 背包（Normal 层，全屏）：左边格子、右边详情，出售走确认框
    public partial class BagPanel : UIPanel
    {
        readonly List<BagSlot> m_Slots = new List<BagSlot>();
        ItemData m_Selected;

        protected override void OnCreate()
        {
            go_SlotTemplate.SetActive(false);
            btn_Back.onClick.AddListener(Close);
            btn_Sell.onClick.AddListener(RequestSell);
            btn_Use.onClick.AddListener(() => PlayerModel.Instance.Use(m_Selected));
        }

        protected override void OnOpen(object args)
        {
            var m = PlayerModel.Instance;
            AddToOpenScope(txt_Gold.BindText(m.Gold, v => v.ToString("N0")));
            // Inventory 用的是普通 C# 事件，手动登记解绑
            m.Bag.Changed += Rebuild;
            AddToOpenScope(new Disposer(() => m.Bag.Changed -= Rebuild));
            Listen<ItemUsedEvent>(e => UIManager.Toast($"使用了 {e.Item.Name}"));

            m_Selected = args as ItemData;   // 允许外部直接打开并选中某个物品
            Rebuild();
        }

        void Rebuild()
        {
            var items = PlayerModel.Instance.Bag.Items;
            if (m_Selected == null || !Contains(items, m_Selected))
                m_Selected = items.Count > 0 ? items[0] : null;
            // 格子按需补足，多余的隐藏（数量上百时请换成第 18 章的循环列表）
            while (m_Slots.Count < items.Count)
            {
                var go = Instantiate(go_SlotTemplate, rt_Grid, false);
                m_Slots.Add(go.GetComponent<BagSlot>());
            }
            for (int i = 0; i < m_Slots.Count; i++)
            {
                bool used = i < items.Count;
                m_Slots[i].gameObject.SetActive(used);
                if (used) m_Slots[i].Set(items[i], items[i] == m_Selected, OnSlotClicked);
            }
            txt_Capacity.text = $"{items.Count}/{PlayerModel.Instance.Bag.Capacity}";
            ShowDetail();
        }

        void OnSlotClicked(BagSlot slot)
        {
            m_Selected = slot.Item;
            foreach (var s in m_Slots) s.SetSelected(s.Item == m_Selected);
            ShowDetail();
        }

        void ShowDetail()
        {
            var it = m_Selected;
            go_Detail.SetActive(it != null);
            go_Empty.SetActive(it == null);
            if (it == null) return;
            img_DetailIcon.sprite = UISkin.Sprite(it.Icon);
            txt_DetailName.text = it.Name;
            txt_DetailDesc.text = it.Desc;
            txt_DetailPrice.text = $"出售价格  {it.Price} 金币　　拥有 {it.Count}";
            btn_Use.gameObject.SetActive(it.Usable);
        }

        /// 出售当前选中的物品（先弹确认框）
        public void RequestSell()
        {
            var it = m_Selected;
            if (it == null) return;
            UIManager.Confirm($"确定以 <color=#FF8A3D>{it.Price} 金币</color> 出售 1 个「{it.Name}」吗？",
                () => PlayerModel.Instance.Sell(it), title: "出售物品", okText: "出售");
        }

        static bool Contains(IReadOnlyList<ItemData> list, ItemData item)
        {
            for (int i = 0; i < list.Count; i++) if (list[i] == item) return true;
            return false;
        }

        /// 截图、测试用：直接选中第 index 个物品
        public void Select(int index)
        {
            var items = PlayerModel.Instance.Bag.Items;
            if (index >= 0 && index < items.Count) { m_Selected = items[index]; Rebuild(); }
        }
    }
    //#endregion
}
