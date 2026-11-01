// 第 7 章配图与配套场景（搭景代码，不进书）
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
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

        /// 圆形单选框：把 UIKit.Toggle 的方框换成圆
        public static Toggle Radio(Transform p, string label, ToggleGroup g, bool on, float w = 360, float h = 72)
        {
            var tg = UIKit.Toggle(p, label, on, w, h);
            var bg = tg.transform.Find("Background").GetComponent<Image>();
            bg.sprite = Shot.Sp("circle"); bg.type = Image.Type.Simple;
            var ck = (Image)tg.graphic;
            ck.sprite = Shot.Sp("circle"); ck.type = Image.Type.Simple;
            ck.rectTransform.offsetMin = new Vector2(14, 14); ck.rectTransform.offsetMax = new Vector2(-14, -14);
            tg.group = g;
            return tg;
        }

        /// iOS 风格开关：Toggle(根) → Track(胶囊) + Knob(空节点) → Shadow + Face
        public static Toggle Switch(Transform p, bool on, float w = 132, float h = 76, Color? onColor = null, string name = "Switch")
        {
            var root = UIKit.Node(p, name);
            root.sizeDelta = new Vector2(w, h);
            var tg = root.gameObject.AddComponent<Toggle>();
            var track = UIKit.Image(root, "pill", new Color32(0xD5, 0xD9, 0xE3, 0xFF), "Track");
            track.rectTransform.Stretch(); PillFit(track, h);
            var knob = UIKit.Node(root, "Knob");
            knob.anchorMin = knob.anchorMax = new Vector2(0, 0.5f); knob.pivot = new Vector2(0, 0.5f);
            knob.sizeDelta = new Vector2(h - 12, h - 12);
            var sh = UIKit.Image(knob, "circle_soft", new Color(0, 0, 0, 0.25f), "Shadow");
            sh.rectTransform.Stretch(-6, -2, -6, -10); sh.raycastTarget = false;
            var face = UIKit.Image(knob, "circle", Color.white, "Face");
            face.rectTransform.Stretch(); face.raycastTarget = false;
            tg.targetGraphic = track;
            tg.graphic = null;
            tg.toggleTransition = Toggle.ToggleTransition.None;
            tg.isOn = on;
            var sw = root.gameObject.AddComponent<SwitchToggle>();
            sw.Setup(knob, track, onColor ?? Skin.success);
            return tg;
        }

        /// 分段选择器：一排 Toggle 共用一个 ToggleGroup，选中项是实心色块 + 白字
        public static Toggle[] Segmented(Transform p, string[] labels, int selected, float w, float h, out ToggleGroup group)
        {
            var box = UIKit.Image(p, "pill", Skin.page, "Segmented");
            box.rectTransform.sizeDelta = new Vector2(w, h); PillFit(box, h);
            group = box.gameObject.AddComponent<ToggleGroup>();
            var res = new Toggle[labels.Length];
            float cw = 1f / labels.Length;
            for (int i = 0; i < labels.Length; i++)
            {
                var item = UIKit.Node(box.transform, "Toggle_" + labels[i]);
                item.anchorMin = new Vector2(cw * i, 0); item.anchorMax = new Vector2(cw * (i + 1), 1);
                item.offsetMin = new Vector2(6, 6); item.offsetMax = new Vector2(-6, -6);
                var tg = item.gameObject.AddComponent<Toggle>();
                var bg = UIKit.Image(item, "pill", new Color(1, 1, 1, 0), "Background");
                bg.rectTransform.Stretch();
                var ck = UIKit.Image(bg.transform, "pill", Skin.primary, "Checkmark");
                ck.rectTransform.Stretch(); ck.raycastTarget = false; PillFit(ck, h - 12);
                var lb = T(item, labels[i], h * 0.4f, Skin.ink, TextAlignmentOptions.Center, true, "Label");
                lb.rectTransform.Stretch();
                tg.targetGraphic = bg; tg.graphic = ck;
                tg.group = group;
                tg.isOn = i == selected;
                item.gameObject.AddComponent<ToggleLabelColor>().Setup(lb, Color.white, Skin.ink);
                res[i] = tg;
            }
            return res;
        }

        /// UIKit.Slider 的把手宽是高度的 1.1 倍（椭圆），这里改成正圆
        public static Slider Slider(Transform p, float value, float w, float h, Color? fill = null)
        {
            var sl = UIKit.Slider(p, value, w, h, fill);
            sl.handleRect.sizeDelta = new Vector2(h, 0);
            return sl;
        }

        /// pill 精灵的九宫格边距是 32px：高度超过 64 时把 Pixels Per Unit Multiplier 调小，让两端保持半圆
        public static void PillFit(Image img, float h)
        {
            if (h > 64) img.pixelsPerUnitMultiplier = 64f / h;
        }

        public static RectTransform Card(Transform p, float x, float y, float w, float h, string name = "Card")
        {
            var c = UIKit.Card(p, name);
            c.Place(x, y, w, h);
            return c;
        }

        /// 指定锚点与轴心放置（Place 会把轴心也设成锚点，行布局时不方便）
        public static RectTransform Pin(this RectTransform rt, Vector2 anchor, Vector2 pivot, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static Button LightButton(Transform p, string label, float w, float h, float fs = 34)
        {
            var b = UIKit.Button(p, label, Skin.page, w, h, null, fs);
            return b;
        }
    }

    static class Shots
    {
        static UISkin Skin => UISkin.Instance;

        // ---------------------------------------------------------------- 7.1 Toggle 结构
        [BookShot("ch07-toggle-anatomy", W = 960, H = 400)]
        static void ToggleAnatomy(Shot s)
        {
            var card = Kit.Card(s.Root, -330, 0, 900, 560);
            var a = UIKit.Toggle(card, "开启背景音乐", true, 640, 100);
            a.Rt().Place(new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(640, 100));
            var b = UIKit.Toggle(card, "显示伤害数字", false, 640, 100);
            b.Rt().Place(new Vector2(0.5f, 0.5f), new Vector2(0, -110), new Vector2(640, 100));
            s.Box(a.Rt(), Skin.primary, "Toggle（isOn = true）");
            s.Box(b.Rt(), Skin.muted, "Toggle（isOn = false）", 6, true);

            // 右侧：拆开的三个部件
            var parts = Kit.Card(s.Root, 560, 0, 640, 560, "Parts");
            var bg = UIKit.Image(parts, "round_8", new Color(0.88f, 0.9f, 0.95f), "Background");
            bg.rectTransform.Place(-170, 140, 110, 110);
            var ck = UIKit.Image(parts, "check", Skin.primary, "Checkmark");
            ck.rectTransform.Place(-170, -10, 110, 110);
            var lb = Kit.T(parts, "Label", 48, Skin.ink, TextAlignmentOptions.Center, false, "Label");
            lb.rectTransform.Place(-170, -160, 200, 80);
            Kit.T(parts, "Background\n<size=28><color=#7A8299>targetGraphic：按状态变色</color></size>", 34, Skin.ink).rectTransform.Place(110, 140, 380, 120);
            Kit.T(parts, "Checkmark\n<size=28><color=#7A8299>graphic：isOn 控制显隐</color></size>", 34, Skin.ink).rectTransform.Place(110, -10, 380, 120);
            Kit.T(parts, "Label\n<size=28><color=#7A8299>普通文字，和 Toggle 无关</color></size>", 34, Skin.ink).rectTransform.Place(110, -160, 380, 120);
            s.Arrow(() => s.At(a.Rt(), new Vector2(1, 0.5f)) + new Vector2(30, 0), () => s.At(parts, new Vector2(0, 0.5f)) + new Vector2(-20, 0), Skin.muted);
        }

        // ---------------------------------------------------------------- 7.3 多选与单选
        [BookShot("ch07-togglegroup", W = 960, H = 460, Scene = "Ch07_ToggleGroup")]
        static void ToggleGroupShot(Shot s)
        {
            var left = Kit.Card(s.Root, -460, 60, 760, 620, "Checks");
            Kit.T(left, "多选：各自独立", 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Place(new Vector2(0, 1), new Vector2(50, -60), new Vector2(600, 70));
            string[] cs = { "自动拾取金币", "显示伤害数字", "允许组队邀请", "夜间模式" };
            bool[] con = { true, true, false, true };
            var checks = new Toggle[cs.Length];
            for (int i = 0; i < cs.Length; i++)
            {
                checks[i] = UIKit.Toggle(left, cs[i], con[i], 560, 80);
                checks[i].Rt().Place(new Vector2(0, 1), new Vector2(60, -150 - i * 110), new Vector2(560, 80));
            }

            var right = Kit.Card(s.Root, 460, 60, 760, 620, "Radios");
            Kit.T(right, "单选：同一个 ToggleGroup", 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Place(new Vector2(0, 1), new Vector2(50, -60), new Vector2(640, 70));
            var groupNode = UIKit.Node(right, "Group");
            groupNode.Stretch();
            var group = groupNode.gameObject.AddComponent<ToggleGroup>();
            string[] rs = { "简单", "普通", "困难", "噩梦" };
            for (int i = 0; i < rs.Length; i++)
            {
                var r = Kit.Radio(groupNode, rs[i], group, i == 2, 560, 80);
                r.Rt().Place(new Vector2(0, 1), new Vector2(60, -150 - i * 110), new Vector2(560, 80));
            }

            var allow = Kit.Switch(s.Root, false, 110, 64);
            allow.Rt().Place(-780, -340, 110, 64);
            Kit.T(s.Root, "allowSwitchOff", 32, Skin.ink).rectTransform.Place(-560, -340, 300, 60);
            var status = Kit.T(s.Root, "", 30, Skin.muted, TextAlignmentOptions.MidlineLeft, false, "Status");
            status.rectTransform.Place(320, -340, 1100, 60);
            var demo = s.Root.gameObject.AddComponent<ToggleGroupDemo>();
            demo.Setup(checks, group, allow, status);
            s.BeforeCapture(demo.Refresh);
        }

        // ---------------------------------------------------------------- 案例 7.1 iOS 开关
        [BookShot("ch07-switch", W = 960, H = 480, Scene = "Ch07_Switch")]
        static void SwitchShot(Shot s)
        {
            var card = Kit.Card(s.Root, -400, 0, 900, 760, "SettingsList");
            string[] names = { "无线局域网", "蓝牙", "飞行模式", "勿扰模式", "低电量模式" };
            string[] icons = { "refresh", "sound", "play", "bell", "lock" };
            Color[] ic = { Skin.primary, UIKit.Hex("#1E88E5"), Skin.accent, UIKit.Hex("#8E44AD"), Skin.success };
            bool[] on = { true, true, false, false, true };
            for (int i = 0; i < names.Length; i++)
            {
                float y = 290 - i * 145;
                var bgi = UIKit.Image(card, "round_16", ic[i], "IconBg"); bgi.rectTransform.Place(-360, y, 80, 80);
                UIKit.Icon(bgi.transform, icons[i], 48, Color.white).rectTransform.Place(0, 0, 48, 48);
                Kit.T(card, names[i], 40, Skin.ink).rectTransform.Place(-60, y, 480, 70);
                var sw = Kit.Switch(card, on[i], 132, 76, null, "Switch_" + names[i]);
                sw.Rt().Place(320, y, 132, 76);
                if (i == 4) sw.interactable = false;
                if (i < names.Length - 1)
                {
                    var line = UIKit.Image(card, null, new Color(0, 0, 0, 0.06f), "Line");
                    line.rectTransform.Place(40, y - 72, 760, 2);
                }
            }
            s.Label(new Vector2(-400, -440), "最后一行 interactable = false：Color Tint 把它染成半透明", Skin.muted, 26);

            // 右侧：同一个开关在动画进度 t = 0 / 0.5 / 1 时的样子（Evaluate 直接画出中间帧）
            var right = Kit.Card(s.Root, 520, 0, 620, 760, "Evaluate");
            Kit.T(right, "Evaluate(t)", 44, Skin.ink, TextAlignmentOptions.Center, true).rectTransform.Place(0, 290, 500, 70);
            float[] ts = { 0, 0.5f, 1 };
            for (int i = 0; i < 3; i++)
            {
                var sw = Kit.Switch(right, i == 2, 200, 112, null, "Switch_t" + i);
                sw.Rt().Place(-60, 130 - i * 190, 200, 112);
                Kit.T(right, "t = " + ts[i], 38, Skin.muted).rectTransform.Place(200, 130 - i * 190, 200, 60);
                var comp = sw.GetComponent<SwitchToggle>();
                float t = ts[i];
                s.BeforeCapture(() => comp.Evaluate(t));
            }
            s.BeforeCapture(() => { foreach (var sw in card.GetComponentsInChildren<SwitchToggle>()) sw.Evaluate(sw.GetComponent<Toggle>().isOn ? 1 : 0); });
            Kit.T(s.Root, "点击开关试试", 28, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(600, 50));
        }

        // ---------------------------------------------------------------- 案例 7.2 页签
        [BookShot("ch07-tabs", W = 960, H = 520, Scene = "Ch07_Tabs")]
        static void TabsShot(Shot s)
        {
            var card = Kit.Card(s.Root, 0, -10, 1500, 900, "TabPanel");
            var bar = UIKit.Node(card, "TabBar");
            bar.anchorMin = new Vector2(0, 1); bar.anchorMax = new Vector2(1, 1); bar.pivot = new Vector2(0.5f, 1);
            bar.anchoredPosition = new Vector2(0, -20); bar.sizeDelta = new Vector2(-80, 120);
            var group = bar.gameObject.AddComponent<ToggleGroup>();
            var line = UIKit.Image(bar, null, new Color(0, 0, 0, 0.08f), "Line");
            line.rectTransform.anchorMin = new Vector2(0, 0); line.rectTransform.anchorMax = new Vector2(1, 0);
            line.rectTransform.sizeDelta = new Vector2(0, 3); line.rectTransform.anchoredPosition = Vector2.zero;
            string[] names = { "角色", "装备", "技能", "成就" };
            var tabs = new Toggle[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                var tab = UIKit.Node(bar, "Tab_" + names[i]);
                tab.anchorMin = new Vector2(i / 4f, 0); tab.anchorMax = new Vector2((i + 1) / 4f, 1);
                tab.offsetMin = tab.offsetMax = Vector2.zero;
                var bg = UIKit.Image(tab, null, new Color(1, 1, 1, 0), "Background"); bg.rectTransform.Stretch();
                Kit.T(tab, names[i], 44, Skin.muted, TextAlignmentOptions.Center, true, "Label").rectTransform.Stretch();
                var tg = tab.gameObject.AddComponent<Toggle>();
                tg.targetGraphic = bg; tg.graphic = null; tg.group = group;
                tg.isOn = i == 1;
                tabs[i] = tg;
            }
            var ind = UIKit.Image(bar, "pill", Skin.primary, "Indicator");
            ind.rectTransform.anchorMin = ind.rectTransform.anchorMax = new Vector2(0.5f, 0); ind.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            ind.rectTransform.sizeDelta = new Vector2(160, 10); ind.rectTransform.anchoredPosition = new Vector2(0, 0);

            var pages = new GameObject[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                var page = UIKit.Node(card, "Page_" + names[i]);
                page.Stretch(60, 170, 60, 50);
                pages[i] = page.gameObject;
            }
            // 角色页
            var av = UIKit.Image(pages[0].transform, "avatar_2", Color.white, "Avatar"); av.rectTransform.Place(-480, 120, 260, 260);
            Kit.T(pages[0].transform, "艾琳 · Lv.42\n<size=34><color=#7A8299>职业：游侠    公会：晨星</color></size>", 52, Skin.ink).rectTransform.Place(150, 150, 760, 200);
            // 装备页：一排物品格
            string[] items = { "item_sword", "item_shield", "item_helmet", "item_ring", "item_bow", "item_potion_red", "item_gem", "item_key", "item_scroll", "item_coin" };
            for (int i = 0; i < items.Length; i++)
            {
                int r = i / 5, c = i % 5;
                var slot = UIKit.Image(pages[1].transform, "slot", Color.white, "Slot");
                slot.rectTransform.Place(-520 + c * 260, 130 - r * 260, 220, 220);
                UIKit.Icon(slot.transform, items[i], 150, Color.white).rectTransform.Place(0, 0, 150, 150);
            }
            Kit.T(pages[2].transform, "技能页（内容略）", 44, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(0, 0, 800, 80);
            Kit.T(pages[3].transform, "成就页（内容略）", 44, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(0, 0, 800, 80);
            for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == 1);
            tabs[1].GetComponentInChildren<TMP_Text>().color = Skin.primary;

            var tg2 = card.gameObject.AddComponent<TabGroup>();
            tg2.Setup(tabs, pages, ind.rectTransform, 1);
            s.BeforeCapture(() => tg2.Select(1));
            s.Box(bar, Skin.accent, "TabBar：ToggleGroup + 4 个 Toggle", 4, false);
            s.Box(pages[1].GetComponent<RectTransform>(), Skin.primary, "Page_装备：选中页签对应的页面", 4, true);
        }

        // ---------------------------------------------------------------- 7.4 Slider 结构
        [BookShot("ch07-slider-anatomy", W = 960, H = 420)]
        static void SliderAnatomy(Shot s)
        {
            Slider Make(float y)
            {
                var sl = Kit.Slider(s.Root, 0.62f, 1200, 64);
                sl.Rt().Place(0, y, 1200, 64);
                return sl;
            }
            var a = Make(230); var b = Make(-10); var c = Make(-250);
            var red = Skin.danger; var orange = Skin.accent; var blue = UIKit.Hex("#1E88E5"); var green = Skin.success;
            s.Box((RectTransform)a.transform.Find("Background"), red, "Background：整条底槽");
            s.Box((RectTransform)b.transform.Find("Fill Area"), orange, "Fill Area：Fill 的父节点（容器）");
            s.Box(b.fillRect, green, "Fill：anchorMax.x = 0.62（被 Slider 驱动）", 10, true);
            s.Box((RectTransform)c.transform.Find("Handle Slide Area"), blue, "Handle Slide Area：Handle 的父节点（滑动范围）");
            s.Box(c.handleRect, red, "Handle：anchorMin.x = anchorMax.x = 0.62", 8, true);
            s.Label(new Vector2(-760, 350), "value = 0.62", Skin.ink, 30);
        }

        // ---------------------------------------------------------------- 7.4 方向与填充方式
        [BookShot("ch07-slider-variants", W = 960, H = 500)]
        static void SliderVariants(Shot s)
        {
            var left = Kit.Card(s.Root, -360, 0, 1060, 900, "Horizontal");
            void Row(string title, float y, Slider sl)
            {
                Kit.T(left, title, 32, Skin.ink).rectTransform.Place(new Vector2(0, 1), new Vector2(60, y), new Vector2(900, 50));
                sl.Rt().Place(new Vector2(0.5f, 1), new Vector2(0, y - 80), new Vector2(900, 52));
            }
            var s1 = Kit.Slider(left, 0.3f, 900, 52); Row("LeftToRight，value = 0.3", -60, s1);
            var s2 = Kit.Slider(left, 0.3f, 900, 52, Skin.accent); s2.SetDirection(Slider.Direction.RightToLeft, true); Row("RightToLeft，value = 0.3（从右往左长）", -250, s2);
            var s3 = Kit.Slider(left, 7, 900, 52, Skin.success); s3.minValue = 0; s3.maxValue = 10; s3.wholeNumbers = true; s3.value = 7;
            Row("Whole Numbers，0 ~ 10，value = 7", -440, s3);
            // 刻度
            for (int i = 0; i <= 10; i++)
            {
                float x = Mathf.Lerp(-450 + 26, 450 - 26, i / 10f);
                var tick = UIKit.Image(left, null, Skin.muted, "Tick"); tick.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(x, -580), new Vector2(3, 18));
                Kit.T(left, i.ToString(), 26, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(x, -615), new Vector2(60, 40));
            }
            var s4 = Kit.Slider(left, 0.55f, 900, 52); s4.interactable = false;
            Row("Interactable = false（Disabled 状态）", -690, s4);

            var right = Kit.Card(s.Root, 600, 0, 680, 900, "Vertical");
            var v1 = Kit.Slider(right, 0.7f, 520, 52); v1.SetDirection(Slider.Direction.BottomToTop, true); v1.Rt().Place(-230, 20, 52, 520);
            var v2 = Kit.Slider(right, 0.7f, 520, 52, Skin.accent); v2.SetDirection(Slider.Direction.TopToBottom, true); v2.Rt().Place(-80, 20, 52, 520);
            Kit.T(right, "Bottom\nToTop", 24, Skin.ink, TextAlignmentOptions.Center).rectTransform.Place(-230, 340, 140, 70);
            Kit.T(right, "Top\nToBottom", 24, Skin.ink, TextAlignmentOptions.Center).rectTransform.Place(-80, 340, 140, 70);
            Kit.T(right, "value 都是 0.7", 24, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(-155, -300, 260, 40);

            // Fill 用 Filled 类型的 Image：Slider 改 fillAmount，不改锚点
            var radial = UIKit.Node(right, "Slider_Radial"); radial.Place(150, 60, 240, 240);
            var rbg = UIKit.Image(radial, "ring", new Color(0.86f, 0.88f, 0.93f), "Background"); rbg.rectTransform.Stretch();
            var fa = UIKit.Node(radial, "Fill Area"); fa.Stretch();
            var fill = UIKit.Image(fa, "ring", Skin.primary, "Fill"); fill.rectTransform.Stretch();
            fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Radial360; fill.fillOrigin = (int)Image.Origin360.Top; fill.fillClockwise = true;
            var rs = radial.gameObject.AddComponent<Slider>();
            rs.fillRect = fill.rectTransform; rs.interactable = false; rs.transition = Selectable.Transition.None;
            rs.value = 0.65f;
            Kit.T(radial, "65%", 52, Skin.ink, TextAlignmentOptions.Center, true).rectTransform.Stretch();
            Kit.T(right, "Fill 为 Filled 类型\n改 fillAmount", 26, Skin.ink, TextAlignmentOptions.Center).rectTransform.Place(150, -150, 300, 90);
        }

        // ---------------------------------------------------------------- 7.5 Scrollbar
        [BookShot("ch07-scrollbar", W = 960, H = 440)]
        static void ScrollbarShot(Shot s)
        {
            var card = Kit.Card(s.Root, -170, 0, 1440, 780, "Scrollbars");
            (string, float, float, int)[] rows =
            {
                ("size = 0.1   value = 0.5", 0.1f, 0.5f, 0),
                ("size = 0.4   value = 0.5", 0.4f, 0.5f, 0),
                ("size = 0.8   value = 0.5", 0.8f, 0.5f, 0),
                ("size = 0.4   value = 0", 0.4f, 0f, 0),
                ("size = 0.2   steps = 5   value = 0.75", 0.2f, 0.75f, 5),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                float y = 290 - i * 145;
                Kit.T(card, rows[i].Item1, 30, Skin.ink).rectTransform.Place(-420, y, 520, 50);
                var sb = UIKit.Scrollbar(card, Scrollbar.Direction.LeftToRight, 36);
                sb.Rt().Place(260, y, 760, 36);
                sb.size = rows[i].Item2; sb.numberOfSteps = rows[i].Item4; sb.value = rows[i].Item3;
                sb.handleRect.GetComponent<Image>().color = Skin.primary;
                if (rows[i].Item4 > 1)
                    for (int k = 0; k < rows[i].Item4; k++)
                    {
                        // 每一档 handle 中心所在的位置
                        float n = k / (float)(rows[i].Item4 - 1);
                        float hx = -380 + 10 + (760 - 20) * (n * (1 - 0.2f) + 0.1f);
                        Kit.T(card, (k / (float)(rows[i].Item4 - 1)).ToString("0.##"), 24, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(260 + hx, y - 48, 80, 34);
                    }
            }
            var vcard = Kit.Card(s.Root, 760, 0, 320, 780, "Vertical");
            var v = UIKit.Scrollbar(vcard, Scrollbar.Direction.BottomToTop, 36);
            v.Rt().Place(-50, 50, 36, 560); v.size = 0.3f; v.value = 1f; v.handleRect.GetComponent<Image>().color = Skin.accent;
            Kit.T(vcard, "左 BottomToTop\n右 TopToBottom\nvalue 都是 1", 24, Skin.ink, TextAlignmentOptions.Center).rectTransform.Place(0, -320, 300, 110);
            var v2 = UIKit.Scrollbar(vcard, Scrollbar.Direction.TopToBottom, 36);
            v2.Rt().Place(50, 50, 36, 560); v2.size = 0.3f; v2.value = 1f; v2.handleRect.GetComponent<Image>().color = Skin.success;
        }

        // ---------------------------------------------------------------- 案例 7.3 设置界面
        [BookShot("ch07-settings", W = 960, H = 540, Scene = "Ch07_Settings")]
        static void SettingsShot(Shot s)
        {
            s.Background(UIKit.Hex("#2B2F45"));
            var card = Kit.Card(s.Root, 0, 0, 1180, 980, "SettingsPanel");
            Kit.T(card, "设置", 56, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 0.5f), 70, -80, 400, 80);
            var close = UIKit.IconButton(card, "close", Skin.page, 76, Skin.muted); close.Rt().Pin(new Vector2(1, 1), new Vector2(1, 0.5f), -70, -80, 76, 76);

            float y0 = -205, dy = 128;
            var L = new Vector2(0, 1); var R = new Vector2(1, 1);
            var pl = new Vector2(0, 0.5f); var pr = new Vector2(1, 0.5f);
            void Label(string t, float y) => Kit.T(card, t, 38, Skin.ink).rectTransform.Pin(L, pl, 70, y, 260, 60);
            Slider VolumeRow(string t, float y, out TextMeshProUGUI val)
            {
                Label(t, y);
                var sl = Kit.Slider(card, 0, 620, 48);
                sl.minValue = 0; sl.maxValue = 100; sl.wholeNumbers = true;
                sl.Rt().Pin(L, pl, 340, y, 620, 48);
                val = Kit.T(card, "0", 38, Skin.primary, TextAlignmentOptions.MidlineRight, true, "Value");
                val.rectTransform.Pin(R, pr, -70, y, 120, 60);
                return sl;
            }
            var music = VolumeRow("音乐音量", y0, out var musicVal);
            var sfx = VolumeRow("音效音量", y0 - dy, out var sfxVal);
            Label("画质", y0 - dy * 2);
            var q = Kit.Segmented(card, new[] { "低", "中", "高", "极致" }, 2, 700, 84, out _);
            ((RectTransform)q[0].transform.parent).Pin(L, pl, 340, y0 - dy * 2, 700, 84);
            Label("震动反馈", y0 - dy * 3);
            var vib = Kit.Switch(card, true, 120, 70); vib.Rt().Pin(R, pr, -70, y0 - dy * 3, 120, 70);
            Label("推送通知", y0 - dy * 4);
            var noti = Kit.Switch(card, false, 120, 70); noti.Rt().Pin(R, pr, -70, y0 - dy * 4, 120, 70);
            var line = UIKit.Image(card, null, new Color(0, 0, 0, 0.06f), "Line"); line.rectTransform.Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), 0, 180, 1040, 2);
            var hint = Kit.T(card, "有未保存的修改", 30, Skin.accent, TextAlignmentOptions.MidlineLeft, false, "Hint");
            hint.rectTransform.Pin(Vector2.zero, pl, 70, 95, 380, 60);
            var reset = Kit.LightButton(card, "恢复默认", 250, 92); reset.Rt().Pin(new Vector2(1, 0), pr, -350, 95, 250, 92);
            var save = UIKit.Button(card, "保存", Skin.primary, 250, 92, null, 36); save.Rt().Pin(new Vector2(1, 0), pr, -70, 95, 250, 92);
            var panel = card.gameObject.AddComponent<SettingsPanel>();
            panel.Setup(music, musicVal, sfx, sfxVal, q, vib, noti, reset, save, hint);
            panel.ShowValues(80, 65, 2, true, false);
            hint.text = "";
            s.BeforeCapture(() =>
            {
                panel.ShowValues(72, 45, 2, true, false);
                hint.text = "有未保存的修改";
                foreach (var sw in card.GetComponentsInChildren<SwitchToggle>()) sw.Evaluate(sw.GetComponent<Toggle>().isOn ? 1 : 0);
                foreach (var lc in card.GetComponentsInChildren<ToggleLabelColor>()) lc.Apply(lc.GetComponent<Toggle>().isOn);
            });
        }

        // ---------------------------------------------------------------- 案例 7.4 加载进度条
        [BookShot("ch07-loading", W = 960, H = 540, Scene = "Ch07_Loading")]
        static void LoadingShot(Shot s)
        {
            var bg = UIKit.Image(s.Root, "bg_night", Color.white, "Background");
            bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 2f;
            var shade = UIKit.Image(s.Root, "gradient_v", new Color(0.05f, 0.06f, 0.12f, 0.9f), "Shade");
            shade.rectTransform.anchorMin = Vector2.zero; shade.rectTransform.anchorMax = new Vector2(1, 0); shade.rectTransform.pivot = new Vector2(0.5f, 0);
            shade.rectTransform.sizeDelta = new Vector2(0, 520); shade.rectTransform.anchoredPosition = Vector2.zero; shade.raycastTarget = false;
            var title = Kit.T(s.Root, "星夜远行", 120, Color.white, TextAlignmentOptions.Center, true, "Title");
            title.rectTransform.Place(0, 220, 1000, 160);
            title.characterSpacing = 20;

            var bar = Kit.Slider(s.Root, 0, 1600, 28, Skin.accent);
            bar.name = "ProgressBar";
            bar.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(1600, 28));
            // 进度条不需要把手：直接删掉 Handle Slide Area，Slider 只驱动 Fill
            Object.DestroyImmediate(bar.transform.Find("Handle Slide Area").gameObject);
            bar.handleRect = null;
            var bgImg = bar.transform.Find("Background").GetComponent<Image>(); bgImg.color = new Color(1, 1, 1, 0.18f);
            foreach (var n in new[] { "Background", "Fill Area" })
            {
                var r = (RectTransform)bar.transform.Find(n); r.anchorMin = new Vector2(0, 0); r.anchorMax = new Vector2(1, 1);
            }
            var fa = (RectTransform)bar.transform.Find("Fill Area"); fa.offsetMin = Vector2.zero; fa.offsetMax = Vector2.zero;
            bar.fillRect.offsetMin = Vector2.zero; bar.fillRect.offsetMax = Vector2.zero;
            var stage = Kit.T(s.Root, "", 34, new Color(1, 1, 1, 0.85f), TextAlignmentOptions.MidlineLeft, false, "Stage");
            stage.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(-400, 260), new Vector2(800, 60));
            var pct = Kit.T(s.Root, "0%", 44, Color.white, TextAlignmentOptions.MidlineRight, true, "Percent");
            pct.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(700, 264), new Vector2(200, 70));
            var tip = Kit.T(s.Root, "", 30, new Color(1, 1, 1, 0.6f), TextAlignmentOptions.Center, false, "Tip");
            tip.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1600, 50));
            var replay = UIKit.Button(s.Root, "再加载一次", Skin.accent, 300, 90, "refresh", 32);
            replay.Rt().Place(0, -20, 300, 90);
            var pb = bar.gameObject.AddComponent<ProgressBar>(); pb.Setup(pct);
            var ls = s.Root.gameObject.AddComponent<LoadingScreen>();
            ls.Setup(pb, stage, tip, replay);
            s.BeforeCapture(() => ls.Preview(2, 0.62f));
        }

        // ---------------------------------------------------------------- 案例 7.5 双滑块
        [BookShot("ch07-range", W = 960, H = 400, Scene = "Ch07_RangeSlider")]
        static void RangeShot(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1500, 640, "PriceFilter");
            Kit.T(card, "价格区间", 48, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), new Vector2(0, 0.5f), 80, -80, 400, 70);
            var label = Kit.T(card, "", 38, Skin.muted, TextAlignmentOptions.MidlineRight, false, "Summary");
            label.rectTransform.Pin(new Vector2(1, 1), new Vector2(1, 0.5f), -80, -80, 700, 70);

            var root = UIKit.Node(card, "RangeSlider"); root.Place(0, 20, 1240, 80);
            var hit = UIKit.Image(root, null, new Color(1, 1, 1, 0), "HitArea"); hit.rectTransform.Stretch(-20, -20, -20, -20);  // 透明：扩大可点击范围
            var track = UIKit.Image(root, "pill", new Color(0.86f, 0.88f, 0.93f), "Track");
            track.rectTransform.anchorMin = new Vector2(0, 0.5f); track.rectTransform.anchorMax = new Vector2(1, 0.5f); track.rectTransform.sizeDelta = new Vector2(0, 16);
            var area = UIKit.Node(root, "Slide Area"); area.Stretch(40, 0, 40, 0);
            var fill = UIKit.Image(area, "pill", Skin.primary, "Fill");
            fill.rectTransform.anchorMin = new Vector2(0, 0.5f); fill.rectTransform.anchorMax = new Vector2(1, 0.5f);
            fill.rectTransform.offsetMin = new Vector2(0, -8); fill.rectTransform.offsetMax = new Vector2(0, 8);
            RectTransform Handle(string n, out TextMeshProUGUI tag)
            {
                var h = UIKit.Node(area, n); h.sizeDelta = new Vector2(80, 0); h.anchoredPosition = Vector2.zero;
                var sh = UIKit.Image(h, "circle_soft", new Color(0, 0, 0, 0.25f), "Shadow"); sh.rectTransform.Stretch(-6, 0, -6, -10); sh.raycastTarget = false;
                var face = UIKit.Image(h, "circle", Color.white, "Face"); face.rectTransform.Stretch();
                var ring = UIKit.Image(h, "ring_thin", Skin.primary, "Ring"); ring.rectTransform.Stretch(); ring.raycastTarget = false;
                var bubble = UIKit.Image(h, "round_8", Skin.ink, "Tag"); bubble.raycastTarget = false;
                bubble.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 24), new Vector2(150, 56)); bubble.rectTransform.pivot = new Vector2(0.5f, 0);
                bubble.rectTransform.anchoredPosition = new Vector2(0, 24);
                tag = Kit.T(bubble.transform, "", 30, Color.white, TextAlignmentOptions.Center, true, "Text"); tag.rectTransform.Stretch();
                return h;
            }
            var lo = Handle("Handle Low", out var loTag);
            var hi = Handle("Handle High", out var hiTag);
            var range = root.gameObject.AddComponent<RangeSlider>();
            range.Setup(area, fill.rectTransform, lo, hi, 0, 1000, 10, 50);
            range.SetValues(200, 680, false);
            for (int i = 0; i <= 5; i++)
                Kit.T(root, "¥" + i * 200, 26, Skin.muted, TextAlignmentOptions.Center).rectTransform.Pin(new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), 40 + 1160 * i / 5f, -70, 120, 40);

            string[] ps = { "¥200 以下", "¥200 – 500", "¥500 以上", "不限" };
            Vector2[] pv = { new Vector2(0, 200), new Vector2(200, 500), new Vector2(500, 1000), new Vector2(0, 1000) };
            var btns = new Button[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                btns[i] = Kit.LightButton(card, ps[i], 260, 80, 30);
                btns[i].Rt().Place(new Vector2(0.5f, 0), new Vector2(-450 + i * 300, 90), new Vector2(260, 80));
            }
            var demo = card.gameObject.AddComponent<PriceRangeDemo>();
            demo.Setup(range, label, loTag, hiTag, btns, pv);
            demo.Refresh();
        }

        // ---------------------------------------------------------------- 案例 7.6 Slider 与输入框同步
        [BookShot("ch07-sync", W = 960, H = 400, Scene = "Ch07_SliderInput")]
        static void SyncShot(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1400, 660, "Panel");
            (string label, float min, float max, bool whole, float v, string fmt)[] rows =
            {
                ("亮度", 0, 100, true, 72, "0"),
                ("视野 FOV", 60, 110, true, 90, "0"),
                ("鼠标灵敏度", 0.1f, 5f, false, 1.8f, "0.0"),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                var r = rows[i];
                float y = 210 - i * 180;
                Kit.T(card, r.label, 38, Skin.ink).rectTransform.Place(-480, y, 300, 60);
                var sl = Kit.Slider(card, 0, 640, 48);
                sl.minValue = r.min; sl.maxValue = r.max; sl.wholeNumbers = r.whole; sl.value = r.v;
                sl.Rt().Place(40, y, 640, 48);
                var input = UIKit.Input(card, "", 190, 84, 36);
                input.Rt().Place(500, y, 190, 84);
                input.textComponent.alignment = TextAlignmentOptions.Center;
                var sync = sl.gameObject.AddComponent<SliderInputSync>();
                sync.Setup(sl, input, r.fmt);
                input.text = r.v.ToString(r.fmt);
                Kit.T(card, $"{r.min} ~ {r.max}", 24, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(40, y - 52, 300, 36);
            }
            Kit.T(card, "拖动滑块或在右侧输入数字后按回车", 28, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(900, 50));
        }
    }
}
