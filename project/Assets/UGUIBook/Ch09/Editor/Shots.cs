// 第 9 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    static class Kit
    {
        public static UISkin Skin => UISkin.Instance;

        public static TextMeshProUGUI T(Transform p, string s, float size, Color c, TextAlignmentOptions a = TextAlignmentOptions.MidlineLeft, bool bold = false, string name = "Text")
        {
            var t = UIKit.Text(p, s, size, c, a, bold, name);
            t.enableWordWrapping = false;
            return t;
        }

        public static RectTransform Pin(this RectTransform rt, Vector2 anchor, Vector2 pivot, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Card(Transform p, float x, float y, float w, float h, string name = "Card", Color? color = null)
        {
            var c = UIKit.Card(p, name, 16, true, color);
            c.Place(x, y, w, h);
            return c;
        }

        /// 竖向 Content：VerticalLayoutGroup 控制子物体高度（读 LayoutElement）+ ContentSizeFitter 把自己撑高
        public static VerticalLayoutGroup VerticalContent(RectTransform content, int pad, float spacing)
        {
            var vl = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(pad, pad, pad, pad); vl.spacing = spacing;
            vl.childControlWidth = true; vl.childForceExpandWidth = true;
            vl.childControlHeight = true; vl.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return vl;
        }

        /// 横向 Content：锚在左侧、轴心在左，宽度由 ContentSizeFitter 决定
        public static HorizontalLayoutGroup HorizontalContent(RectTransform content, int pad, float spacing)
        {
            content.anchorMin = new Vector2(0, 0); content.anchorMax = new Vector2(0, 1); content.pivot = new Vector2(0, 0.5f);
            content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
            var hl = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(pad, pad, pad, pad); hl.spacing = spacing; hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlWidth = false; hl.childForceExpandWidth = false; hl.childControlHeight = false; hl.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return hl;
        }

        public static RectTransform Row(Transform p, string label, float h, Color bg, string name = "Row")
        {
            var img = UIKit.Image(p, "round_16", bg, name);
            img.gameObject.AddComponent<LayoutElement>().preferredHeight = h;
            Kit.T(img.transform, label, 34, Skin.ink, TextAlignmentOptions.MidlineLeft, false, "Label").rectTransform.Stretch(36, 0, 20, 0);
            return img.rectTransform;
        }

        /// 截图专用：批处理下 ScrollRect 不跑 LateUpdate，滚动条的 size/value 不会跟着 normalizedPosition 更新，手动同步一次
        public static void SyncBars(ScrollRect sr)
        {
            Canvas.ForceUpdateCanvases();
            var v = sr.viewport.rect; var c = sr.content.rect;
            if (sr.verticalScrollbar) { sr.verticalScrollbar.size = Mathf.Clamp01(v.height / c.height); sr.verticalScrollbar.value = sr.verticalNormalizedPosition; }
            if (sr.horizontalScrollbar) { sr.horizontalScrollbar.size = Mathf.Clamp01(v.width / c.width); sr.horizontalScrollbar.value = sr.horizontalNormalizedPosition; }
        }

        static readonly string[] kColors = { "#5B4BD6", "#FF8A3D", "#2FB36A", "#1E88E5", "#E5484D", "#8E44AD" };
        public static Color Palette(int i) => UIKit.Hex(kColors[i % kColors.Length]);
    }

    static class Shots
    {
        static UISkin Skin => UISkin.Instance;
        static readonly Vector2 L = new Vector2(0, 0.5f), C = new Vector2(0.5f, 0.5f), R = new Vector2(1, 0.5f), Top = new Vector2(0.5f, 1);

        // ---------------------------------------------------------------- 9.1 结构
        [BookShot("ch09-anatomy", W = 640, H = 540, Panels = 2)]
        static void Anatomy(Shot s)
        {
            // 用 DefaultControls 原样创建（和菜单 UI › Scroll View 一致：Viewport 上是 Mask + Image）
            var go = DefaultControls.CreateScrollView(UIKit.LegacyRes);
            var sr = go.GetComponent<ScrollRect>();
            var root = (RectTransform)go.transform;
            root.SetParent(s.Root, false);
            root.Place(0, -10, 820, 600);
            go.GetComponent<Image>().color = Color.white;
            foreach (var sb in go.GetComponentsInChildren<Scrollbar>(true))
            {
                sb.GetComponent<Image>().color = new Color(0, 0, 0, 0.08f);
                sb.handleRect.GetComponent<Image>().color = Skin.primary;
                var r = sb.Rt();
                if (sb.direction == Scrollbar.Direction.BottomToTop) r.sizeDelta = new Vector2(28, r.sizeDelta.y); else r.sizeDelta = new Vector2(r.sizeDelta.x, 28);
            }
            var content = sr.content;
            content.sizeDelta = new Vector2(260, 0);           // Content 比 Viewport 宽 260，横向也能滚
            Kit.VerticalContent(content, 24, 16);
            for (int i = 0; i < 8; i++) Kit.Row(content, $"条目 {i + 1}", 90, Color.Lerp(Kit.Palette(i), Color.white, 0.78f), "Item_" + (i + 1));
            s.BeforeCapture(() => { sr.normalizedPosition = new Vector2(0.2f, 0.5f); Kit.SyncBars(sr); });
            var mask = sr.viewport.GetComponent<Mask>();
            if (s.Panel == 0)
            {
                // 关掉 Mask，露出 Viewport 外面的 Content
                mask.enabled = false;
                s.Box(sr.viewport, UIKit.Hex("#1E88E5"), null, 2);
                s.Box(content, Skin.success, null, 2);
                s.Box(sr.verticalScrollbar.Rt(), Skin.accent, null, 2);
                s.Box(sr.horizontalScrollbar.Rt(), Skin.accent, null, 2);
                s.Label(new Vector2(-330, 450), "Content（绿）", Skin.success, 28);
                s.Label(new Vector2(-110, 340), "Viewport（蓝）", UIKit.Hex("#1E88E5"), 28);
                s.Label(new Vector2(330, -420), "两根 Scrollbar（橙）", Skin.accent, 28);
                s.Label(new Vector2(0, -490), "关掉 Mask：Content 比 Viewport 大，多出的部分露在外面", Skin.ink, 28);
            }
            else s.Label(new Vector2(0, -490), "正常：Mask 只让 Viewport 范围内的内容显示", Skin.ink, 28);
        }

        // ---------------------------------------------------------------- 9.6 三种遮罩
        [BookShot("ch09-masks", W = 960, H = 480, Scene = "Ch09_Masks")]
        static void Masks(Shot s)
        {
            string[] titles = { "无遮罩", "Mask + 圆角图片", "RectMask2D + Softness" };
            string[] notes = { "内容溢出到外面", "按图片 Alpha 裁剪（模板缓冲）", "矩形裁剪，边缘渐隐" };
            for (int k = 0; k < 3; k++)
            {
                float x = -600 + k * 600;
                var sr = UIKit.ScrollView(s.Root, true, false, 460, 560);
                sr.name = "Scroll_" + k;
                sr.Rt().Place(x, -10, 460, 560);
                var vp = sr.viewport;
                if (k == 0) vp.GetComponent<RectMask2D>().enabled = false;
                if (k == 1)
                {
                    Object.DestroyImmediate(vp.GetComponent<RectMask2D>());
                    var img = vp.gameObject.AddComponent<Image>(); img.sprite = Shot.Sp("round_32"); img.type = Image.Type.Sliced;
                    img.pixelsPerUnitMultiplier = 0.35f;     // 圆角放大，裁剪效果更明显
                    vp.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                }
                if (k == 2) vp.GetComponent<RectMask2D>().softness = new Vector2Int(0, 90);
                if (sr.verticalScrollbar != null) { Object.DestroyImmediate(sr.verticalScrollbar.gameObject); sr.verticalScrollbar = null; }
                vp.Stretch();
                Kit.VerticalContent(sr.content, 0, 14);
                for (int i = 0; i < 9; i++)
                {
                    var c = UIKit.Image(sr.content, "round_16", Kit.Palette(i), "Card_" + i);
                    c.gameObject.AddComponent<LayoutElement>().preferredHeight = 120;
                    UIKit.Icon(c.transform, new[] { "star", "heart", "gift", "trophy", "music", "bell" }[i % 6], 64, Color.white).rectTransform.Pin(L, L, 30, 0, 64, 64);
                    Kit.T(c.transform, $"卡片 {i + 1}", 34, Color.white, TextAlignmentOptions.MidlineLeft, true).rectTransform.Stretch(120, 0, 0, 0);
                }
                var frame = UIKit.Image(s.Root, "round_outline_16", new Color(0.12f, 0.14f, 0.21f, 0.55f), "Frame_" + k);
                frame.rectTransform.Place(x, -10, 470, 570); frame.raycastTarget = false;
                var srr = sr;
                s.BeforeCapture(() => srr.verticalNormalizedPosition = 0.55f);
                // 文字放在滚动视图之后创建：层级靠后 = 后绘制，不会被左边溢出的内容盖住
                var t1 = Kit.T(s.Root, titles[k], 36, Skin.ink, TextAlignmentOptions.Center, true); t1.rectTransform.Place(x, 400, 560, 56);
                var t2 = Kit.T(s.Root, notes[k], 28, Skin.muted, TextAlignmentOptions.Center); t2.rectTransform.Place(x, -420, 560, 50);
                if (k == 0)
                {
                    foreach (var t in new[] { t1, t2 })
                    {
                        var bg = UIKit.Image(s.Root, "round_8", new Color(0.93f, 0.94f, 0.96f, 0.92f), "LabelBg");
                        bg.rectTransform.Place(x, t.rectTransform.anchoredPosition.y, 300, 60); bg.raycastTarget = false;
                        t.transform.SetAsLastSibling();
                    }
                }
            }
        }

        // ---------------------------------------------------------------- 案例 9.1 商城列表
        static readonly ShopList.Goods[] kGoods =
        {
            new ShopList.Goods { name = "精钢长剑", sprite = "item_sword", price = 480, tag = "热卖" },
            new ShopList.Goods { name = "骑士盾牌", sprite = "item_shield", price = 360 },
            new ShopList.Goods { name = "骑士头盔", sprite = "item_helmet", price = 300 },
            new ShopList.Goods { name = "猎人长弓", sprite = "item_bow", price = 520, tag = "新品" },
            new ShopList.Goods { name = "火抗戒指", sprite = "item_ring", price = 880, tag = "限时" },
            new ShopList.Goods { name = "蓝宝石", sprite = "item_gem", price = 1200 },
            new ShopList.Goods { name = "红色药水", sprite = "item_potion_red", price = 12 },
            new ShopList.Goods { name = "蓝色药水", sprite = "item_potion_blue", price = 15 },
            new ShopList.Goods { name = "绿色药水", sprite = "item_potion_green", price = 18, tag = "热卖" },
            new ShopList.Goods { name = "回城卷轴", sprite = "item_scroll", price = 30 },
            new ShopList.Goods { name = "地牢钥匙", sprite = "item_key", price = 200 },
            new ShopList.Goods { name = "金苹果", sprite = "item_apple", price = 66, tag = "新品" },
            new ShopList.Goods { name = "金币袋", sprite = "item_coin", price = 99 },
            new ShopList.Goods { name = "神秘礼盒", sprite = "gift", price = 188, tag = "限时" },
            new ShopList.Goods { name = "幸运星", sprite = "star", price = 520 },
            new ShopList.Goods { name = "心之石", sprite = "heart", price = 999 },
        };

        [BookShot("ch09-shop", W = 960, H = 540, Scene = "Ch09_Shop")]
        static void Shop(Shot s)
        {
            s.Background(UIKit.Hex("#2B2F45"));
            var panel = Kit.Card(s.Root, 0, 0, 1560, 1020, "ShopPanel");
            Kit.T(panel, "杂货铺", 52, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 60, -70, 400, 70);
            var chip = UIKit.Image(panel, "pill", Skin.page, "Coins"); chip.rectTransform.Pin(new Vector2(1, 1), R, -60, -70, 280, 76);
            UIKit.Icon(chip.transform, "item_coin", 56, Color.white).rectTransform.Pin(L, L, 14, 0, 56, 56);
            var coins = Kit.T(chip.transform, "0", 36, Skin.ink, TextAlignmentOptions.MidlineRight, true, "Value"); coins.rectTransform.Stretch(80, 0, 28, 0);

            var sr = UIKit.ScrollView(panel, true, false, 1480, 820);
            sr.Rt().Pin(Top, Top, 0, -130, 1480, 820);
            sr.scrollSensitivity = 30;     // 鼠标滚轮一格滚 30 像素（默认 1，在高分辨率画布上几乎滚不动）
            var content = sr.content; content.sizeDelta = Vector2.zero;
            var grid = content.Grid(new Vector2(334, 390), new Vector2(26, 26), 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 4;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // 卡片模板
            var tpl = UIKit.Image(panel, "round_16", Skin.page, "GoodsTemplate");
            tpl.rectTransform.sizeDelta = new Vector2(334, 390);
            UIKit.Icon(tpl.transform, "item_sword", 170, Color.white, "Icon").rectTransform.Pin(Top, Top, 0, -36, 170, 170);
            Kit.T(tpl.transform, "名称", 34, Skin.ink, TextAlignmentOptions.Center, true, "Name").rectTransform.Pin(Top, Top, 0, -222, 300, 50);
            var buy = UIKit.Button(tpl.transform, "0", Skin.accent, 230, 76, "item_coin", 32); buy.name = "Buy";
            buy.Rt().Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0), 0, 26, 230, 76);
            var tag = UIKit.Image(tpl.transform, "round_8", Skin.danger, "Tag"); tag.rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 14, -14, 92, 46);
            Kit.T(tag.transform, "热卖", 26, Color.white, TextAlignmentOptions.Center, true).rectTransform.Stretch();
            tpl.gameObject.SetActive(false);

            var toast = Kit.T(panel, "", 30, Skin.success, TextAlignmentOptions.Center, false, "Toast");
            toast.rectTransform.Pin(new Vector2(0.5f, 0), C, 0, 38, 900, 50);
            var shop = panel.gameObject.AddComponent<ShopList>();
            shop.Setup(sr, tpl.rectTransform, coins, toast, kGoods);
            s.BeforeCapture(() => { shop.Build(); Canvas.ForceUpdateCanvases(); sr.verticalNormalizedPosition = 0.62f; Kit.SyncBars(sr); });
        }

        // ---------------------------------------------------------------- 案例 9.2 滚动到第 N 项
        [BookShot("ch09-scrollto", W = 960, H = 540, Scene = "Ch09_ScrollTo")]
        static void ScrollTo(Shot s)
        {
            var left = Kit.Card(s.Root, -360, 0, 900, 1000, "ListCard");
            var sr = UIKit.ScrollView(left, true, false, 860, 960);
            sr.Rt().Stretch(20, 20, 20, 20);
            sr.scrollSensitivity = 30;
            Kit.VerticalContent(sr.content, 16, 12);
            sr.gameObject.AddComponent<SmoothScroller>();
            var tpl = UIKit.Image(left, "round_16", Skin.page, "RowTemplate");
            tpl.gameObject.AddComponent<LayoutElement>().preferredHeight = 96;
            Kit.T(tpl.transform, "第 N 项", 36, Skin.ink, TextAlignmentOptions.MidlineLeft, false, "Label").rectTransform.Stretch(40, 0, 20, 0);
            tpl.gameObject.SetActive(false);

            var right = Kit.Card(s.Root, 560, 0, 640, 1000, "Controls");
            Kit.T(right, "跳转到第几项", 36, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 50, -80, 500, 56);
            var input = UIKit.Input(right, "1～100", 540, 96, 38); input.Rt().Pin(Top, C, 0, -170, 540, 96);
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            Kit.T(right, "对齐方式", 36, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 50, -290, 500, 56);
            var align = UIKit.Dropdown(right, new List<string> { "顶部对齐", "居中", "底部对齐" }, 540, 96, 36);
            align.Rt().Pin(Top, C, 0, -380, 540, 96);
            var go = UIKit.Button(right, "跳 转", Skin.primary, 540, 100, null, 38); go.Rt().Pin(Top, C, 0, -520, 540, 100);
            var top = UIKit.Button(right, "回到顶部", Skin.page, 255, 96, "arrow_up", 32); top.Rt().Pin(Top, C, -142, -650, 255, 96);
            var bottom = UIKit.Button(right, "滚到底部", Skin.page, 255, 96, "arrow_down", 32); bottom.Rt().Pin(Top, C, 142, -650, 255, 96);

            var demo = left.gameObject.AddComponent<ScrollToDemo>();
            demo.Setup(sr, tpl.rectTransform, input, align, go, top, bottom);
            s.BeforeCapture(() =>
            {
                demo.Build();
                input.text = "37";
                align.value = 1;
                demo.Jump(37, true);
                Kit.SyncBars(sr);
            });
        }

        // ---------------------------------------------------------------- 案例 9.3 轮播图
        [BookShot("ch09-carousel", W = 640, H = 420, Panels = 2, Scene = "Ch09_Carousel")]
        static void Carousel(Shot s)
        {
            var sr = UIKit.ScrollView(s.Root, false, true, 1120, 520);
            sr.name = "Carousel";
            sr.Rt().Place(0, 40, 1120, 520);
            sr.movementType = ScrollRect.MovementType.Elastic;
            if (sr.horizontalScrollbar != null) { Object.DestroyImmediate(sr.horizontalScrollbar.gameObject); sr.horizontalScrollbar = null; }
            sr.viewport.Stretch();
            Kit.HorizontalContent(sr.content, 0, 0);
            (string sprite, Color color, string icon, string title, string sub)[] pages =
            {
                ("bg_landscape", Color.white, null, "夏日远征", "新地图「翡翠山谷」现已开放"),
                ("bg_night", Color.white, null, "星夜祭典", "限时活动 · 登录领取祭典烟花"),
                (null, Skin.primary, "gift", "新手礼包", "七日签到，免费领取史诗装备"),
                (null, Skin.accent, "trophy", "公会战", "本周六晚八点 · 报名开启"),
            };
            for (int i = 0; i < pages.Length; i++)
            {
                var p = pages[i];
                var page = UIKit.Image(sr.content, p.sprite, p.color, "Page_" + i);
                page.rectTransform.sizeDelta = new Vector2(1120, 520);
                if (p.sprite != null) page.preserveAspect = false;
                if (p.icon != null) UIKit.Icon(page.transform, p.icon, 280, new Color(1, 1, 1, 0.25f)).rectTransform.Pin(R, R, -80, 0, 280, 280);
                var shade = UIKit.Image(page.transform, "gradient_h", new Color(0, 0, 0, 0.45f), "Shade"); shade.rectTransform.Stretch(); shade.raycastTarget = false;
                shade.transform.localScale = new Vector3(-1, 1, 1);
                Kit.T(page.transform, p.title, 76, Color.white, TextAlignmentOptions.MidlineLeft, true, "Title").rectTransform.Pin(L, L, 80, 50, 900, 100);
                Kit.T(page.transform, p.sub, 36, new Color(1, 1, 1, 0.9f), TextAlignmentOptions.MidlineLeft, false, "Sub").rectTransform.Pin(L, L, 80, -40, 900, 60);
            }
            var dots = UIKit.Node(sr.transform, "Dots");
            dots.Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0), 0, 30, 400, 16);
            var hl = dots.HLayout(14); hl.childAlignment = TextAnchor.MiddleCenter;
            for (int i = 0; i < pages.Length; i++)
            {
                var d = UIKit.Image(dots, "pill", Color.white, "Dot_" + i);
                d.rectTransform.sizeDelta = new Vector2(16, 16); d.raycastTarget = false;
            }
            var pv = sr.gameObject.AddComponent<PageView>();
            pv.Setup(dots);
            pv.GoTo(0, false);
            if (s.Panel == 0)
            {
                s.BeforeCapture(() => pv.GoTo(1, true));
                s.Label(new Vector2(0, -330), "松手后吸附：正好停在第 2 页", Skin.ink, 30);
            }
            else
            {
                s.BeforeCapture(() => { pv.GoTo(1, true); sr.horizontalNormalizedPosition = 0.52f; });
                s.Label(new Vector2(0, -330), "拖动中：介于第 2、3 页之间", Skin.ink, 30);
            }
            Kit.T(s.Root, "左右拖动或快速甩动；4 秒无操作自动翻页", 26, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(0, -390, 1000, 40);
        }

        // ---------------------------------------------------------------- 案例 9.4 横竖嵌套
        [BookShot("ch09-nested", W = 960, H = 540, Scene = "Ch09_Nested")]
        static void Nested(Shot s)
        {
            var card = Kit.Card(s.Root, -180, 0, 1300, 1020, "StorePage");
            var outer = UIKit.ScrollView(card, true, false, 1260, 980);
            outer.name = "Outer (ScrollRect 竖向)";
            outer.Rt().Stretch(20, 20, 20, 20);
            outer.scrollSensitivity = 30;
            Kit.VerticalContent(outer.content, 20, 26);
            string[] sections = { "热门推荐", "新品上架", "武器", "防具", "消耗品", "收藏品" };
            string[] items = { "item_sword", "item_shield", "item_helmet", "item_ring", "item_bow", "item_gem", "item_potion_red", "item_key", "item_scroll", "item_apple" };
            var rows = new List<NestedScrollRect>();
            for (int k = 0; k < sections.Length; k++)
            {
                var sec = UIKit.Node(outer.content, "Section_" + sections[k]);
                sec.gameObject.AddComponent<LayoutElement>().preferredHeight = 330;
                Kit.T(sec, sections[k], 38, Skin.ink, TextAlignmentOptions.MidlineLeft, true, "Title").rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 10, 0, 400, 56);

                var row = UIKit.Node(sec, "Row (NestedScrollRect 横向)");
                row.anchorMin = new Vector2(0, 0); row.anchorMax = new Vector2(1, 0); row.pivot = new Vector2(0.5f, 0);
                row.sizeDelta = new Vector2(0, 260); row.anchoredPosition = Vector2.zero;
                var hit = row.gameObject.AddComponent<Image>(); hit.color = new Color(1, 1, 1, 0);   // 空白处也能接住拖拽
                var nsr = row.gameObject.AddComponent<NestedScrollRect>();
                nsr.horizontal = true; nsr.vertical = false; nsr.scrollSensitivity = 30;
                var vp = UIKit.Node(row, "Viewport"); vp.Stretch(); vp.gameObject.AddComponent<RectMask2D>();
                var content = UIKit.Node(vp, "Content");
                Kit.HorizontalContent(content, 6, 22);
                nsr.viewport = vp; nsr.content = content;
                for (int i = 0; i < 10; i++)
                {
                    var c = UIKit.Image(content, "round_16", Color.Lerp(Kit.Palette(i + k), Color.white, 0.8f), "Item_" + i);
                    c.rectTransform.sizeDelta = new Vector2(210, 240);
                    UIKit.Icon(c.transform, items[(i + k * 3) % items.Length], 130, Color.white).rectTransform.Pin(Top, Top, 0, -24, 130, 130);
                    Kit.T(c.transform, "¥" + (i * 37 + k * 11 + 20), 32, Skin.ink, TextAlignmentOptions.Center, true).rectTransform.Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0), 0, 18, 200, 50);
                }
                rows.Add(nsr);
            }
            s.BeforeCapture(() =>
            {
                outer.verticalNormalizedPosition = 0.78f;
                rows[1].horizontalNormalizedPosition = 0.45f;
                Kit.SyncBars(outer);
            });
            var note = Kit.Card(s.Root, 720, 0, 420, 1020, "Notes");
            Kit.T(note, "竖着拖：\n整页上下滚", 34, Skin.ink, TextAlignmentOptions.TopLeft, true).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 40, -60, 360, 120);
            UIKit.Icon(note, "arrow_up", 70, Skin.primary).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 40, -200, 70, 70);
            UIKit.Icon(note, "arrow_down", 70, Skin.primary).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 130, -200, 70, 70);
            Kit.T(note, "横着拖：\n只滚这一排", 34, Skin.ink, TextAlignmentOptions.TopLeft, true).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 40, -360, 360, 120);
            UIKit.Icon(note, "arrow_left", 70, Skin.accent).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 40, -500, 70, 70);
            UIKit.Icon(note, "arrow_right", 70, Skin.accent).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 130, -500, 70, 70);
            Kit.T(note, "斜着拖：开始拖动那一刻看哪个方向分量大，整次拖拽都交给那一层", 28, Skin.muted, TextAlignmentOptions.TopLeft).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 40, -640, 340, 300);
            note.GetComponentsInChildren<TextMeshProUGUI>()[2].enableWordWrapping = true;
        }

        // ---------------------------------------------------------------- 案例 9.5 下拉刷新 / 上拉加载
        [BookShot("ch09-pullrefresh", W = 640, H = 540, Panels = 2, Scene = "Ch09_PullRefresh")]
        static void PullRefresh(Shot s)
        {
            var phone = Kit.Card(s.Root, 0, 30, 900, 960, "FeedPanel", Skin.page);
            var head = UIKit.Image(phone, "round_16", Color.white, "Header");
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = new Vector2(1, 1); head.rectTransform.pivot = new Vector2(0.5f, 1);
            head.rectTransform.sizeDelta = new Vector2(0, 110); head.rectTransform.anchoredPosition = Vector2.zero;
            Kit.T(head.transform, "好友动态", 40, Skin.ink, TextAlignmentOptions.Center, true).rectTransform.Stretch();

            var sr = UIKit.ScrollView(phone, true, false, 900, 900);
            sr.Rt().Stretch(0, 112, 0, 0);
            if (sr.verticalScrollbar != null) { Object.DestroyImmediate(sr.verticalScrollbar.gameObject); sr.verticalScrollbar = null; }
            sr.viewport.Stretch();
            sr.scrollSensitivity = 30;
            var content = sr.content;
            Kit.VerticalContent(content, 20, 16);

            // 下拉提示：Content 的子物体，但不参与布局；锚在 Content 顶端、轴心在底边 → 平时位于 Content 上方（视口外）
            var header = UIKit.Node(content, "RefreshHeader");
            header.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            header.anchorMin = new Vector2(0, 1); header.anchorMax = new Vector2(1, 1); header.pivot = new Vector2(0.5f, 0);
            header.sizeDelta = new Vector2(0, 140); header.anchoredPosition = Vector2.zero;
            var arrow = UIKit.Icon(header, "arrow_down", 48, Skin.primary); arrow.rectTransform.Place(-150, 0, 48, 48);
            var htext = Kit.T(header, "下拉刷新", 32, Skin.muted, TextAlignmentOptions.MidlineLeft, false, "Label"); htext.rectTransform.Place(30, 0, 280, 50);

            var tpl = UIKit.Image(phone, "round_16", Color.white, "PostTemplate");
            tpl.gameObject.AddComponent<LayoutElement>().preferredHeight = 170;
            UIKit.Image(tpl.transform, "avatar_1", Color.white, "Avatar").rectTransform.Pin(L, L, 30, 0, 110, 110);
            Kit.T(tpl.transform, "名字", 34, Skin.ink, TextAlignmentOptions.MidlineLeft, true, "Name").rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 165, -26, 620, 50);
            Kit.T(tpl.transform, "内容", 30, Skin.muted, TextAlignmentOptions.MidlineLeft, false, "Body").rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 1), 165, -88, 660, 50);
            tpl.gameObject.SetActive(false);

            var footer = Kit.T(content, "上拉加载更多", 30, Skin.muted, TextAlignmentOptions.Center, false, "Footer");
            footer.gameObject.AddComponent<LayoutElement>().preferredHeight = 90;

            var pull = sr.gameObject.AddComponent<PullToRefresh>();
            pull.Setup(header, htext, arrow.rectTransform, footer);
            var feed = phone.gameObject.AddComponent<FeedDemo>();
            feed.Setup(pull, content, tpl.rectTransform, footer.rectTransform);
            if (s.Panel == 0)
                s.BeforeCapture(() =>
                {
                    feed.Fill(8);
                    Canvas.ForceUpdateCanvases();
                    content.anchoredPosition = new Vector2(0, -170);      // 模拟手指把列表往下拉了 170 像素
                    pull.SetState(PullToRefresh.State.Ready);
                });
            else
                s.BeforeCapture(() =>
                {
                    feed.Fill(8);
                    Canvas.ForceUpdateCanvases();
                    sr.verticalNormalizedPosition = 0;
                    content.anchoredPosition += new Vector2(0, 60);       // 再往上多拉一点（Elastic 越界）
                    footer.text = "正在加载更多…";
                });
            s.Label(new Vector2(0, -505), s.Panel == 0 ? "下拉超过阈值：提示“松开立即刷新”" : "拉到底部：自动加载下一页", Skin.ink, 28);
        }
    }
}
