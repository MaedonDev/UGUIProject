// 背包：网格循环列表 + 分类筛选 + 整理排序 + 长按拖拽交换 + 物品详情 Tips
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region bagdata
    public class BagItem
    {
        public int Id;
        public string Name, Icon, Desc;
        public int Category;   // 0 装备 1 消耗品 2 材料
        public int Quality;    // 0~4
        public int Count, Price;
    }
    //#endregion

    //#region bagview
    public class BagView : MonoBehaviour
    {
        [SerializeField] LoopGridView m_Grid;
        [SerializeField] ItemTips m_Tips;
        [SerializeField] Toggle[] m_Tabs;            // 全部 / 装备 / 消耗品 / 材料
        [SerializeField] TMP_Dropdown m_SortBy;      // 品质 / 数量 / 名称
        [SerializeField] Button m_Arrange;
        [SerializeField] TMP_Text m_Capacity;
        [SerializeField] RectTransform m_DragLayer;  // 铺满画布、在最上层
        [SerializeField] Image m_Ghost;              // 拖拽时跟手的图标（raycastTarget 必须关掉，否则挡住下面的格子收不到 Drop）
        [SerializeField] int m_ItemCount = 300;
        [SerializeField] float m_EdgeSize = 90f, m_EdgeSpeed = 900f;

        readonly List<BagItem> m_Items = new List<BagItem>();
        readonly List<int> m_View = new List<int>();     // 当前筛选结果：m_Items 的下标
        int m_Filter = -1;
        int m_Selected = -1;
        int m_DragFrom = -1;
        BagCell m_Hover;
        Vector2 m_PointerScreen;
        Camera m_PointerCam;

        public ScrollRect ScrollRect => m_Grid.ScrollRect;
        public LoopGridView Grid => m_Grid;
        public int ViewCount => m_View.Count;

        void Start()
        {
            for (int i = 0; i < m_Tabs.Length; i++)
            {
                int cat = i - 1;                                 // 闭包里用拷贝
                m_Tabs[i].onValueChanged.AddListener(on => { if (on) SetFilter(cat); });
            }
            m_Arrange.onClick.AddListener(Arrange);
            ScrollRect.onValueChanged.AddListener(_ => m_Tips.Hide());   // 一滚动就收起 Tips
            Init();
        }

        public void Init()
        {
            Generate();
            m_Grid.onBind = (i, item) =>
                item.GetComponent<BagCell>().Set(this, i, m_Items[m_View[i]], i == m_Selected);
            SetFilter(-1);
        }
        //#endregion

        //#region filter
        public void SetFilter(int category)
        {
            m_Filter = category;
            m_View.Clear();
            for (int i = 0; i < m_Items.Count; i++)
                if (category < 0 || m_Items[i].Category == category) m_View.Add(i);
            m_Selected = -1;
            m_Tips.Hide();
            m_Grid.SetCount(m_View.Count);
            m_Capacity.text = $"{m_View.Count} / {m_Items.Count}";
        }

        /// “整理”：按下拉框选择的规则重排整个背包（真实地改变物品顺序，之后拖拽也基于新顺序）
        public void Arrange()
        {
            Comparison<BagItem> cmp;
            switch (m_SortBy.value)
            {
                case 1: cmp = (a, b) => b.Count != a.Count ? b.Count.CompareTo(a.Count) : a.Id.CompareTo(b.Id); break;
                case 2: cmp = (a, b) => string.CompareOrdinal(a.Name, b.Name) != 0 ? string.CompareOrdinal(a.Name, b.Name) : a.Id.CompareTo(b.Id); break;
                default: cmp = (a, b) => b.Quality != a.Quality ? b.Quality.CompareTo(a.Quality) : a.Category != b.Category ? a.Category.CompareTo(b.Category) : a.Id.CompareTo(b.Id); break;
            }
            m_Items.Sort(cmp);   // List.Sort 不稳定，所以比较器最后用 Id 兜底，保证结果确定
            SetFilter(m_Filter);
        }
        //#endregion

        //#region select
        public void Select(int index, RectTransform cell)
        {
            m_Selected = index;
            m_Grid.Refresh();
            m_Tips.Show(m_Items[m_View[index]], cell);
        }
        //#endregion

        //#region drag
        public void BeginItemDrag(int index, PointerEventData e)
        {
            m_DragFrom = index;
            m_Tips.Hide();
            m_Ghost.sprite = UISkin.Sprite(m_Items[m_View[index]].Icon);
            m_Ghost.gameObject.SetActive(true);
            DragItem(e);
        }

        public void DragItem(PointerEventData e)
        {
            m_PointerScreen = e.position;
            m_PointerCam = e.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(m_DragLayer, e.position, e.pressEventCamera, out var local);
            m_Ghost.rectTransform.localPosition = local;
            // 高亮手指下的格子
            var go = e.pointerCurrentRaycast.gameObject;
            var cell = go != null ? go.GetComponentInParent<BagCell>() : null;
            if (cell != m_Hover)
            {
                if (m_Hover != null) m_Hover.SetDropHint(false);
                m_Hover = cell;
                if (m_Hover != null && m_Hover.Index != m_DragFrom) m_Hover.SetDropHint(true);
            }
        }

        public void DropOn(int index)
        {
            if (m_DragFrom < 0 || index == m_DragFrom) return;
            // 交换两个格子在完整背包里的位置；筛选视图里两者都在，所以视图下标不变
            int a = m_View[m_DragFrom], b = m_View[index];
            (m_Items[a], m_Items[b]) = (m_Items[b], m_Items[a]);
            m_Grid.RefreshItem(m_DragFrom);
            m_Grid.RefreshItem(index);
        }

        public void EndItemDrag()
        {
            m_DragFrom = -1;
            m_Ghost.gameObject.SetActive(false);
            if (m_Hover != null) m_Hover.SetDropHint(false);
            m_Hover = null;
        }

        void Update()
        {
            if (m_DragFrom < 0) return;
            // 拖到视口上下边缘时自动滚动，背包很长时也能把物品拖到很远的格子
            var vp = ScrollRect.viewport;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(vp, m_PointerScreen, m_PointerCam, out var p)) return;
            var r = vp.rect;
            float dir = p.y > r.yMax - m_EdgeSize ? -1 : p.y < r.yMin + m_EdgeSize ? 1 : 0;
            if (dir == 0) return;
            m_Grid.ScrollY += dir * m_EdgeSpeed * Time.unscaledDeltaTime;
            m_Grid.UpdateVisible();
        }

        /// 截图用：模拟“从 from 拖到 over 上空”的瞬间
        public void PreviewDrag(int from, int over, Vector2 ghostLocal)
        {
            m_DragFrom = from;
            m_Ghost.sprite = UISkin.Sprite(m_Items[m_View[from]].Icon);
            m_Ghost.gameObject.SetActive(true);
            m_Ghost.rectTransform.localPosition = ghostLocal;
            var cell = m_Grid.GetItem(over);
            if (cell != null) cell.GetComponent<BagCell>().SetDropHint(true);
            m_DragFrom = -1;   // 不进入 Update 的自动滚动
        }
        //#endregion

        //#region generate
        static readonly (string name, string icon, int cat, int price, string desc)[] s_Catalog =
        {
            ("长剑", "item_sword", 0, 120, "锋利的长剑。攻击 +18。"),
            ("鸢盾", "item_shield", 0, 90, "能挡住大部分正面攻击。防御 +12。"),
            ("猎弓", "item_bow", 0, 150, "射程远，适合风筝敌人。攻击 +15，射程 +20%。"),
            ("铁盔", "item_helmet", 0, 80, "防御 +6。戴上之后视野会窄一点。"),
            ("戒指", "item_ring", 0, 300, "暴击率 +3%。"),
            ("生命药水", "item_potion_red", 1, 15, "立即恢复 200 点生命值。战斗中也能使用。"),
            ("魔力药水", "item_potion_blue", 1, 15, "立即恢复 120 点魔力值。"),
            ("解毒剂", "item_potion_green", 1, 20, "解除中毒状态，并在 10 秒内免疫中毒。"),
            ("苹果", "item_apple", 1, 2, "恢复 30 点体力。"),
            ("传送卷轴", "item_scroll", 1, 60, "读完之后回到最近的城镇。"),
            ("宝石", "item_gem", 2, 200, "镶嵌材料。品质越高，加成越多。"),
            ("钥匙", "item_key", 2, 40, "地下城宝箱的钥匙，用完即消失。"),
            ("金币袋", "item_coin", 2, 100, "打开后获得一些金币。"),
        };

        void Generate()
        {
            m_Items.Clear();
            var rnd = new System.Random(18);
            string[] prefix = { "", "精制", "坚固的", "远古", "传说之" };
            for (int i = 0; i < m_ItemCount; i++)
            {
                var c = s_Catalog[rnd.Next(s_Catalog.Length)];
                int q = Math.Min(4, (int)(Math.Pow(rnd.NextDouble(), 2.2) * 5));
                m_Items.Add(new BagItem
                {
                    Id = i, Icon = c.icon, Category = c.cat, Quality = q, Desc = c.desc,
                    Name = (c.cat == 0 ? prefix[q] : "") + c.name,
                    Count = c.cat == 0 ? 1 : rnd.Next(1, 99),
                    Price = c.price * (q + 1),
                });
            }
        }
        //#endregion
    }
}
