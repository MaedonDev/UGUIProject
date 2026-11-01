// 示例用的数据层（Model）：玩家货币 + 背包。界面只读它、订阅它，不在界面里存“真数据”。
using System;
using System.Collections.Generic;

namespace UGUIBook.Ch16
{
    //#region model
    public class ItemData
    {
        public int Id;
        public string Name;
        public string Icon;       // UISkin 里的精灵名
        public string Desc;
        public int Price;         // 出售单价
        public int Count;
        public int Quality;       // 0 白 1 绿 2 蓝 3 紫
        public bool Usable;
    }

    public struct ItemSoldEvent { public ItemData Item; public int Gold; }
    public struct ItemUsedEvent { public ItemData Item; }

    public class Inventory
    {
        readonly List<ItemData> m_Items = new List<ItemData>();
        public IReadOnlyList<ItemData> Items => m_Items;
        public int Capacity = 40;
        public event Action Changed;

        public void Add(ItemData item)
        {
            m_Items.Add(item);
            Changed?.Invoke();
        }

        /// 扣数量，扣光了就移出背包
        public bool Remove(ItemData item, int count = 1)
        {
            if (item == null || item.Count < count || !m_Items.Contains(item)) return false;
            item.Count -= count;
            if (item.Count == 0) m_Items.Remove(item);
            Changed?.Invoke();
            return true;
        }
    }

    public class PlayerModel
    {
        public static PlayerModel Instance { get; private set; } = CreateDemo();

        public readonly Observable<string> Name = new Observable<string>("麦冬");
        public readonly Observable<int> Level = new Observable<int>(1);
        public readonly Observable<int> Gold = new Observable<int>();
        public readonly Observable<int> Diamond = new Observable<int>();
        public readonly Inventory Bag = new Inventory();

        public bool Sell(ItemData item)
        {
            if (!Bag.Remove(item)) return false;
            Gold.Value += item.Price;
            EventCenter.Publish(new ItemSoldEvent { Item = item, Gold = item.Price });
            return true;
        }

        public bool Use(ItemData item)
        {
            if (item == null || !item.Usable || !Bag.Remove(item)) return false;
            EventCenter.Publish(new ItemUsedEvent { Item = item });
            return true;
        }

        public static void ResetDemo() => Instance = CreateDemo();

        static PlayerModel CreateDemo()
        {
            var m = new PlayerModel();
            m.Level.Value = 12;
            m.Gold.Value = 2680;
            m.Diamond.Value = 120;
            int id = 1;
            void Add(string name, string icon, int q, int count, int price, bool usable, string desc) =>
                m.Bag.Add(new ItemData { Id = id++, Name = name, Icon = icon, Quality = q, Count = count, Price = price, Usable = usable, Desc = desc });
            Add("铁剑", "item_sword", 1, 1, 120, false, "铁匠铺的标准货，攻击 +12。卖掉也能换点零花钱。");
            Add("橡木盾", "item_shield", 1, 1, 90, false, "结实的橡木盾牌，防御 +8。");
            Add("生命药水", "item_potion_red", 0, 12, 15, true, "立即恢复 200 点生命值。");
            Add("魔力药水", "item_potion_blue", 0, 8, 15, true, "立即恢复 120 点魔力值。");
            Add("解毒剂", "item_potion_green", 0, 3, 20, true, "解除中毒状态。");
            Add("蓝宝石", "item_gem", 2, 2, 300, false, "镶嵌材料，闪着幽幽的蓝光。");
            Add("苹果", "item_apple", 0, 20, 2, true, "脆甜多汁，恢复 30 点体力。");
            Add("旧钥匙", "item_key", 2, 1, 50, false, "不知道能打开哪扇门。");
            Add("传送卷轴", "item_scroll", 2, 5, 80, true, "读完后回到主城。");
            Add("猎弓", "item_bow", 3, 1, 680, false, "精灵工匠的作品，攻击 +35，射程远。");
            Add("铁盔", "item_helmet", 1, 1, 110, false, "防御 +6。");
            Add("红宝石戒指", "item_ring", 3, 1, 1200, false, "暴击率 +5%。传说是某位公主的遗物。");
            return m;
        }
    }
    //#endregion
}
