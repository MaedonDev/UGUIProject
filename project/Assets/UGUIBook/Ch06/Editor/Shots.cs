// 第 6 章配图与配套场景（搭景代码，不进书）
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    static class Shots
    {
        static UISkin Skin => UISkin.Instance;
        static Color Ink => Skin.ink;
        static Color Muted => Skin.muted;

        static TextMeshProUGUI T(Transform p, string text, float x, float y, float size = 30, Color? col = null, float w = 600, bool bold = false)
        {
            var t = UIKit.Text(p, text, size, col ?? Ink, TextAlignmentOptions.Center, bold);
            t.rectTransform.Place(x, y, w, size * 1.6f);
            return t;
        }

        static void SetField(Object o, string field, object value)
        {
            var so = new SerializedObject(o);
            var p = so.FindProperty(field);
            switch (value)
            {
                case Object[] arr:
                    p.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
                    break;
                case Object obj: p.objectReferenceValue = obj; break;
                case float f: p.floatValue = f; break;
                case int n: p.intValue = n; break;
                case string str: p.stringValue = str; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static ColorBlock Tint(Color normal, Color hi, Color pressed, Color selected, Color disabled)
        {
            var cb = ColorBlock.defaultColorBlock;
            cb.normalColor = normal; cb.highlightedColor = hi; cb.pressedColor = pressed; cb.selectedColor = selected; cb.disabledColor = disabled;
            return cb;
        }

        // ---------------------------------------------------------------- 按钮结构
        [BookShot("ch06-structure", W = 960, H = 220)]
        static void Structure(Shot s)
        {
            var b = UIKit.Button(s.Root, "开始游戏", Skin.primary, 460, 120, "play");
            b.Rt().Place(-300, 0, 460, 120);
            s.Box(b.Rt(), null, "Button（Image + Button）", 10);
            s.Box(b.GetComponentInChildren<TextMeshProUGUI>().rectTransform, Skin.accent, "Text (TMP)", 4, true);
            T(s.Root, "Button 本身没有“画”任何东西：", 420, 60, 30, Ink, 700, true);
            T(s.Root, "底图是同物体上的 Image（Target Graphic），\n文字是子物体 Text (TMP)", 420, -20, 26, Muted, 700).rectTransform.sizeDelta = new Vector2(700, 90);
        }

        // ---------------------------------------------------------------- 五种状态 × 两种过渡
        static readonly string[] States = { "Normal", "Highlighted", "Pressed", "Selected", "Disabled" };

        [BookShot("ch06-states", W = 960, H = 440, Scene = "Ch06_ButtonStates")]
        static void StatesShot(Shot s)
        {
            Ch06Textures.Ensure();
            for (int c = 0; c < 5; c++) T(s.Root, States[c], -600 + c * 330, 340, 32, Ink, 330, true);
            T(s.Root, "Color\nTint", -850, 150, 28, Skin.primary, 200, true).rectTransform.sizeDelta = new Vector2(200, 100);
            T(s.Root, "Sprite\nSwap", -850, -150, 28, Skin.accent, 200, true).rectTransform.sizeDelta = new Vector2(200, 100);
            for (int c = 0; c < 5; c++)
            {
                float x = -600 + c * 330;
                // 第一行：Color Tint。底图用白色，颜色全由 ColorBlock 决定（Color Tint 是“乘”上去的，只能变暗不能变亮）
                var a = UIKit.Button(s.Root, "确定", Color.white, 280, 110, null, 40, "Tint_" + States[c]);
                a.Rt().Place(x, 150, 280, 110);
                a.colors = Tint(UIKit.Hex("#6C5CE7"), UIKit.Hex("#8C80FF"), UIKit.Hex("#4434B0"), UIKit.Hex("#8C80FF"), new Color(0.78f, 0.8f, 0.86f, 0.6f));   // Selected 沿用 Unity 默认习惯：与 Highlighted 同色
                a.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                // 第二行：Sprite Swap。每个状态一张图
                var b = UIKit.Button(s.Root, "确定", Color.white, 280, 110, null, 40, "Swap_" + States[c]);
                b.Rt().Place(x, -150, 280, 110);
                b.transition = Selectable.Transition.SpriteSwap;
                b.image.sprite = Ch06Textures.Sprite("btn_normal");
                b.spriteState = new SpriteState
                {
                    highlightedSprite = Ch06Textures.Sprite("btn_highlighted"),
                    pressedSprite = Ch06Textures.Sprite("btn_pressed"),
                    selectedSprite = Ch06Textures.Sprite("btn_selected"),
                    disabledSprite = Ch06Textures.Sprite("btn_disabled"),
                };
                b.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                if (c == 2) b.GetComponentInChildren<TextMeshProUGUI>().rectTransform.anchoredPosition = new Vector2(0, -3);
                if (c > 0) { s.State(a, States[c]); s.State(b, States[c]); }
                if (c == 4) { a.interactable = false; b.interactable = false; }   // 场景里这一列真的是禁用的
            }
            T(s.Root, "场景 Ch06_ButtonStates：鼠标悬停、按下、Tab 键切换选中，看两种过渡的区别", 0, -340, 26, Muted, 1700);
        }

        // ---------------------------------------------------------------- 自动导航
        [BookShot("ch06-navigation", W = 960, H = 460, Scene = "Ch06_Navigation")]
        static void Navigation(Shot s)
        {
            var pos = new (string label, Vector2 p)[]
            {
                ("设置", new Vector2(-40, 30)), ("商店", new Vector2(-40, 300)), ("邮件", new Vector2(420, 150)),
                ("好友", new Vector2(520, -220)), ("背包", new Vector2(-560, 80)), ("任务", new Vector2(-300, -300)), ("活动", new Vector2(160, -330)),
            };
            var btns = new Button[pos.Length];
            for (int i = 0; i < pos.Length; i++)
            {
                var b = UIKit.Button(s.Root, pos[i].label, i == 0 ? Skin.primary : Color.white, 240, 96, null, 36);
                b.Rt().Place(pos[i].p.x, pos[i].p.y, 240, 96);
                btns[i] = b;
            }
            var hint = T(s.Root, "Navigation = Automatic：用方向键试试从“设置”能走到哪里", 0, 0, 26, Muted, 1400);
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(1400, 44));
            s.BeforeCapture(() =>
            {
                var from = btns[0];
                var dirs = new (Selectable target, string name, Vector2 a, Vector2 b)[]
                {
                    (from.FindSelectableOnUp(), "Up", new Vector2(0.5f, 1), new Vector2(0.5f, 0)),
                    (from.FindSelectableOnDown(), "Down", new Vector2(0.5f, 0), new Vector2(0.5f, 1)),
                    (from.FindSelectableOnLeft(), "Left", new Vector2(0, 0.5f), new Vector2(1, 0.5f)),
                    (from.FindSelectableOnRight(), "Right", new Vector2(1, 0.5f), new Vector2(0, 0.5f)),
                };
                foreach (var (target, name, pa, pb) in dirs)
                {
                    if (target == null) continue;
                    var tr = (RectTransform)target.transform;
                    s.Arrow(() => s.At(from.Rt(), pa), () => s.At(tr, pb), Skin.accent, 5);
                    s.LabelAt(tr, new Vector2(0.5f, 1f), new Vector2(0, 34), name, Skin.accent, 26);
                }
            });
        }

        // ================================================================ 案例：长按连发
        [BookShot("ch06-hold", W = 960, H = 400, Scene = "Ch06_HoldRepeat")]
        static void Hold(Shot s)
        {
            var card = UIKit.Card(s.Root, "ShopItem"); card.Place(0, 20, 1000, 520);
            var icon = UIKit.Image(card, "slot", Color.white, "IconBg"); icon.rectTransform.Place(new Vector2(0, 1), new Vector2(60, -60), new Vector2(200, 200));
            var ic = UIKit.Icon(icon.transform, "item_potion_red", 150, Color.white); ic.rectTransform.Place(0, 0, 150, 150);
            var name = UIKit.Text(card, "生命药水", 48, Ink, TextAlignmentOptions.Left, true, "Name");
            name.rectTransform.Place(new Vector2(0, 1), new Vector2(300, -70), new Vector2(600, 70));
            var price = UIKit.Text(card, "单价 25 金币　·　最多买 99 个", 32, Muted, TextAlignmentOptions.Left, false, "Price");
            price.rectTransform.Place(new Vector2(0, 1), new Vector2(300, -150), new Vector2(640, 50));

            var minus = UIKit.IconButton(card, "minus", Skin.primary, 110);
            minus.Rt().Place(new Vector2(0, 0), new Vector2(60, 60), new Vector2(110, 110));
            var value = UIKit.Text(card, "12", 56, Ink, TextAlignmentOptions.Center, true, "Value");
            value.rectTransform.Place(new Vector2(0, 0), new Vector2(190, 60), new Vector2(160, 110));
            var valueBg = UIKit.Image(card, "round_16", new Color(0.93f, 0.94f, 0.97f), "ValueBg");
            valueBg.rectTransform.Place(new Vector2(0, 0), new Vector2(190, 60), new Vector2(160, 110));
            valueBg.transform.SetSiblingIndex(value.transform.GetSiblingIndex());
            var plus = UIKit.IconButton(card, "plus", Skin.primary, 110);
            plus.Rt().Place(new Vector2(0, 0), new Vector2(370, 60), new Vector2(110, 110));
            minus.gameObject.AddComponent<HoldRepeatButton>();
            plus.gameObject.AddComponent<HoldRepeatButton>();
            var total = UIKit.Text(card, "合计 300 金币", 40, Skin.accent, TextAlignmentOptions.Right, true, "Total");
            total.rectTransform.Place(new Vector2(1, 0), new Vector2(-60, 80), new Vector2(460, 70));
            var st = card.gameObject.AddComponent<QuantityStepper>();
            SetField(st, "m_Minus", minus); SetField(st, "m_Plus", plus); SetField(st, "m_Value", value); SetField(st, "m_Total", total);
            T(s.Root, "按住 + 或 - 不放：0.4 秒后开始连发，越来越快", 0, -310, 28, Muted, 1400);
            s.State(plus, "Pressed");
            s.LabelAt(plus.Rt(), new Vector2(0.5f, 1f), new Vector2(0, 40), "按住", Skin.danger, 26);
        }

        // ================================================================ 案例：双击
        [BookShot("ch06-doubleclick", W = 960, H = 400, Scene = "Ch06_DoubleClick")]
        static void DoubleClick(Shot s)
        {
            string[] items = { "item_sword", "item_potion_red", "item_shield", "item_scroll" };
            string[] names = { "破晓之剑", "生命药水", "橡木盾", "回城卷轴" };
            var cards = new Object[4]; var frames = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                var slot = UIKit.Image(s.Root, "slot", Color.white, names[i]);
                slot.rectTransform.Place(-540 + i * 360, 60, 260, 260);
                var ic = UIKit.Icon(slot.transform, items[i], 180, Color.white); ic.rectTransform.Place(0, 0, 180, 180);
                var frame = UIKit.Image(slot.transform, "frame_select", Skin.accent, "SelectFrame");
                frame.rectTransform.Stretch(-14, -14, -14, -14); frame.raycastTarget = false;
                frame.gameObject.SetActive(i == 1);
                var label = UIKit.Text(slot.transform, names[i], 30, Ink, TextAlignmentOptions.Center, true, "Name");
                label.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, -60), new Vector2(300, 50));
                cards[i] = slot.gameObject.AddComponent<DoubleClickHandler>();
                frames[i] = frame.gameObject;
            }
            var log = T(s.Root, "<color=#FF8A3D>双击：使用了 生命药水</color>", 0, -200, 34, Ink, 1200, true);
            var demo = s.Root.gameObject.AddComponent<ItemCardDemo>();
            SetField(demo, "m_Cards", cards); SetField(demo, "m_Frames", frames); SetField(demo, "m_Log", log);
            T(s.Root, "单击 = 选中（等 0.3 秒确认不是双击），双击 = 使用", 0, -300, 28, Muted, 1400);
        }

        // ================================================================ 案例：防连点
        static (Button, ButtonCooldown) CooldownButton(Transform p, string label, float x, float y, float cd)
        {
            var b = UIKit.Button(p, label, Skin.accent, 340, 110);
            b.Rt().Place(x, y, 340, 110);
            var cb = b.colors; cb.disabledColor = new Color(0.75f, 0.75f, 0.78f, 1f); b.colors = cb;
            var ov = UIKit.Image(b.transform, "round_24", new Color(0, 0, 0, 0.25f), "CooldownOverlay");
            ov.rectTransform.Stretch(); ov.raycastTarget = false;
            ov.transform.SetSiblingIndex(0);
            var c = b.gameObject.AddComponent<ButtonCooldown>();
            SetField(c, "m_Overlay", ov); SetField(c, "m_Label", b.GetComponentInChildren<TextMeshProUGUI>());
            SetField(c, "m_ReadyText", label); SetField(c, "m_Cooldown", cd);
            return (b, c);
        }

        [BookShot("ch06-cooldown", W = 960, H = 320, Scene = "Ch06_Cooldown")]
        static void Cooldown(Shot s)
        {
            var (b0, c0) = CooldownButton(s.Root, "领取奖励", -520, 50, 3f);
            var (b1, c1) = CooldownButton(s.Root, "领取奖励", 0, 50, 3f);
            var (b2, c2) = CooldownButton(s.Root, "领取奖励", 520, 50, 3f);
            T(s.Root, "可点击", -520, -70, 30, Ink, 400, true);
            T(s.Root, "刚点过：剩 2.4 秒", 0, -70, 30, Ink, 400, true);
            T(s.Root, "快结束：剩 0.6 秒", 520, -70, 30, Ink, 400, true);
            T(s.Root, "冷却期间 interactable = false，Button.Press() 直接返回，onClick 不会触发", 0, -200, 26, Muted, 1600);
            s.BeforeCapture(() =>
            {
                c0.Show(0); b1.interactable = false; c1.Show(2.4f); b2.interactable = false; c2.Show(0.6f);
            });
        }

        // ================================================================ 案例：按下缩放 + 音效
        [BookShot("ch06-press", W = 960, H = 320, Scene = "Ch06_Press")]
        static void Press(Shot s)
        {
            var bs = new PressFeedback[3];
            string[] caps = { "Normal：scale 1", "按住：scale 0.92 + 颜色变暗", "松手瞬间：回弹到 1.04 再回 1" };
            for (int i = 0; i < 3; i++)
            {
                var b = UIKit.Button(s.Root, "开始游戏", Skin.primary, 380, 120, "play");
                b.Rt().Place(-560 + i * 560, 50, 380, 120);
                bs[i] = b.gameObject.AddComponent<PressFeedback>();
                T(s.Root, caps[i], -560 + i * 560, -80, 28, Ink, 540, true);
                if (i == 1) s.State(b, "Pressed");
            }
            T(s.Root, "每次按下播放一声代码合成的“咔哒”（40 毫秒），场景里戴上耳机试试", 0, -200, 26, Muted, 1600);
            s.BeforeCapture(() => { bs[1].Apply(0.92f); bs[2].Apply(1.04f); });
        }

        // ================================================================ 案例：单选按钮组
        [BookShot("ch06-radio", W = 960, H = 340, Scene = "Ch06_Radio")]
        static void Radio(Shot s)
        {
            var card = UIKit.Card(s.Root, "Panel"); card.Place(0, 0, 1300, 560);
            var title = UIKit.Text(card, "选择难度", 44, Ink, TextAlignmentOptions.Center, true, "Title");
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(600, 70));
            var row = UIKit.Node(card, "Options"); row.Place(0, 20, 1100, 130); row.HLayout(30);
            string[] labels = { "简单", "普通", "困难" };
            var btns = new Object[3];
            Button first = null;
            for (int i = 0; i < 3; i++)
            {
                var b = UIKit.Button(row, labels[i], Color.white, 320, 120, null, 40);
                b.colors = Tint(Color.white, new Color(0.9f, 0.9f, 0.97f), new Color(0.78f, 0.78f, 0.86f), Color.white, new Color(1, 1, 1, 0.5f));
                var outline = UIKit.Image(b.transform, "round_outline_16", new Color(0.36f, 0.29f, 0.84f, 0.5f), "Outline");
                outline.rectTransform.Stretch(); outline.raycastTarget = false;
                btns[i] = b; if (i == 0) first = b;
            }
            var desc = UIKit.Text(card, "普通：推荐的游戏体验", 32, Muted, TextAlignmentOptions.Center, false, "Desc");
            desc.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(1100, 60));
            var group = card.gameObject.AddComponent<RadioButtonGroup>();
            SetField(group, "m_Buttons", btns); SetField(group, "m_Index", 1);
            group.Refresh();
            var demo = card.gameObject.AddComponent<RadioDemo>();
            SetField(demo, "m_Group", group); SetField(demo, "m_Desc", desc);
            s.State(first, "Highlighted");
            s.LabelAt(first.Rt(), new Vector2(0.5f, 0), new Vector2(0, -40), "鼠标悬停（Color Tint 叠加在选中样式之上）", Skin.accent, 22);
        }

        // ================================================================ 案例：手柄导航菜单
        [BookShot("ch06-gamepad", W = 960, H = 540, Scene = "Ch06_Gamepad")]
        static void Gamepad(Shot s)
        {
            s.Background(UIKit.Hex("#1B1F33"));
            var bg = UIKit.Image(s.Root, "bg_night", Color.white, "Background"); bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var title = T(s.Root, "星 夜 旅 人", 0, 360, 96, Color.white, 1200, true);
            var menu = UIKit.Node(s.Root, "Menu"); menu.Place(0, -80, 520, 640);
            string[] labels = { "继续游戏", "新游戏", "设置", "制作人员", "退出" };
            var items = new Selectable[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                var b = UIKit.Button(menu, labels[i], Color.white, 440, 96, null, 40);
                b.Rt().Place(0, 240 - i * 120, 440, 96);
                b.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
                // 底图是白色，透明度全交给 ColorBlock：Color Tint 会把 alpha 也乘进去
                b.colors = Tint(new Color(1, 1, 1, 0.08f), new Color(1, 1, 1, 0.18f), new Color(1, 1, 1, 0.05f), new Color(1, 1, 1, 0.26f), new Color(1, 1, 1, 0.03f));
                items[i] = b;
            }
            NavigationUtil.SetGrid(items, 1, true);          // 一列、上下循环
            var frame = UIKit.Image(menu, "frame_select", UIKit.Hex("#FFC93C"), "SelectionFrame");
            frame.raycastTarget = false;
            frame.rectTransform.Place(0, 240, 464, 120);
            var log = T(s.Root, "↑ ↓ 键或手柄摇杆选择，Enter / A 键确认", 0, -470, 30, new Color(1, 1, 1, 0.65f), 1400);
            var gm = menu.gameObject.AddComponent<GamepadMenu>();
            SetField(gm, "m_First", items[0]); SetField(gm, "m_Frame", frame.rectTransform); SetField(gm, "m_Log", log);
            s.State(items[2], "Selected");
            s.BeforeCapture(() => gm.FollowTo((RectTransform)items[2].transform, 100f));
        }

        // ================================================================ 案例：自定义 Selectable 页签
        [BookShot("ch06-tabs", W = 960, H = 400, Scene = "Ch06_Tabs")]
        static void Tabs(Shot s)
        {
            var panel = UIKit.Card(s.Root, "Window"); panel.Place(0, 30, 1500, 620);
            var bar = UIKit.Node(panel, "TabBar");
            bar.anchorMin = new Vector2(0, 1); bar.anchorMax = new Vector2(1, 1); bar.pivot = new Vector2(0.5f, 1);
            bar.offsetMin = new Vector2(30, -190); bar.offsetMax = new Vector2(-30, -20);
            var line = UIKit.Image(panel, null, new Color(0, 0, 0, 0.08f), "Divider");
            line.rectTransform.anchorMin = new Vector2(0, 1); line.rectTransform.anchorMax = new Vector2(1, 1);
            line.rectTransform.offsetMin = new Vector2(30, -193); line.rectTransform.offsetMax = new Vector2(-30, -190);
            string[] icons = { "home", "bag", "trophy", "mail", "gear" };
            string[] labels = { "首页", "背包", "成就", "邮件", "设置" };
            string[] caps = { "IsOn（当前页）", "Highlighted", "Pressed", "Selected（键盘焦点）", "Disabled" };
            var tabs = new TabButton[5]; var pages = new Object[5];
            for (int i = 0; i < 5; i++)
            {
                var root = UIKit.Node(bar, "Tab_" + labels[i]);
                root.anchorMin = new Vector2(i / 5f, 0); root.anchorMax = new Vector2((i + 1) / 5f, 1); root.offsetMin = root.offsetMax = Vector2.zero;
                var hit = root.gameObject.AddComponent<Image>(); hit.color = new Color(1, 1, 1, 0);   // 透明点击区（也作为 Target Graphic）
                var hover = UIKit.Image(root, "round_16", Color.white, "Hover"); hover.rectTransform.Stretch(14, 10, 14, 10); hover.raycastTarget = false;
                var focus = UIKit.Image(root, "round_outline_16", UIKit.Hex("#FFB02E"), "FocusRing"); focus.rectTransform.Stretch(8, 4, 8, 4); focus.raycastTarget = false;
                var ic = UIKit.Icon(root, icons[i], 70, Muted, "Icon"); ic.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(70, 70));
                var lb = UIKit.Text(root, labels[i], 32, Muted, TextAlignmentOptions.Center, true, "Label");
                lb.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(200, 50));
                var ul = UIKit.Image(root, "pill", Skin.primary, "Underline"); ul.raycastTarget = false;
                ul.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(120, 10));
                var tab = root.gameObject.AddComponent<TabButton>();
                tab.transition = Selectable.Transition.None;
                tab.targetGraphic = hit;
                SetField(tab, "m_Icon", ic); SetField(tab, "m_Label", lb); SetField(tab, "m_Underline", ul);
                SetField(tab, "m_Hover", hover); SetField(tab, "m_FocusRing", focus);
                tabs[i] = tab;
                var page = UIKit.Text(panel, $"这里是「{labels[i]}」页的内容", 44, Ink, TextAlignmentOptions.Center, false, "Page_" + labels[i]);
                page.rectTransform.Stretch(40, 220, 40, 40);
                page.gameObject.SetActive(i == 0);
                pages[i] = page.gameObject;
                T(s.Root, caps[i], -576 + i * 288, -340, 24, Skin.accent, 300, true);
            }
            var tb = panel.gameObject.AddComponent<TabBar>();
            SetField(tb, "m_Tabs", tabs); SetField(tb, "m_Pages", pages);
            tabs[0].IsOn = true;
            for (int i = 1; i < 5; i++) tabs[i].IsOn = false;
            tabs[4].interactable = false;
            s.State(tabs[1], "Highlighted"); s.State(tabs[2], "Pressed"); s.State(tabs[3], "Selected"); s.State(tabs[4], "Disabled");
        }

        // ================================================================ 闭包陷阱 + Inspector 绑定
        [BookScene("Ch06_Closure")]
        static void Closure(Shot s)
        {
            var wrong = new Object[5]; var right = new Object[5];
            T(s.Root, "for 循环里直接捕获 i（错误）", -460, 320, 34, Skin.danger, 900, true);
            T(s.Root, "先拷贝到局部变量（正确）", -460, 80, 34, Skin.success, 900, true);
            for (int i = 0; i < 5; i++)
            {
                var a = UIKit.Button(s.Root, i.ToString(), Skin.danger, 150, 100); a.Rt().Place(-760 + i * 180, 210, 150, 100); wrong[i] = a;
                var b = UIKit.Button(s.Root, i.ToString(), Skin.success, 150, 100); b.Rt().Place(-760 + i * 180, -30, 150, 100); right[i] = b;
            }
            var log = T(s.Root, "点上面的按钮", 0, -220, 40, Ink, 1600, true);
            var demo = s.Root.gameObject.AddComponent<ClosureDemo>();
            SetField(demo, "m_Wrong", wrong); SetField(demo, "m_Right", right); SetField(demo, "m_Log", log);

            // 第三种：在 Inspector 的 On Click () 列表里绑定（持久监听，会存进场景）
            var c = UIKit.Button(s.Root, "Inspector 绑定的按钮", Skin.primary, 560, 100);
            c.Rt().Place(500, 210, 560, 100);
            UnityEventTools.AddStringPersistentListener(c.onClick, demo.LogFromInspector, "这条监听存在场景文件里");
            T(s.Root, "选中右上角按钮，看 Inspector 里的 On Click () 列表", 500, 120, 24, Muted, 700);
        }
    }
}
