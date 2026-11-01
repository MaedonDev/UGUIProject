// 第 10 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    static class Ch10Shots
    {
        static UISkin Skin => UISkin.Instance;
        static readonly Color Line = UIKit.Hex("#C9CDE0");
        static readonly Color[] Palette = { UIKit.Hex("#5B4BD6"), UIKit.Hex("#FF8A3D"), UIKit.Hex("#2FB36A"), UIKit.Hex("#E5484D"), UIKit.Hex("#1E88E5") };

        static TextMeshProUGUI T(Transform p, string s, float size, Color? c = null, TextAlignmentOptions a = TextAlignmentOptions.Center, bool bold = false, string name = "Text")
        {
            var t = UIKit.Text(p, s, size, c ?? Skin.ink, a, bold, name);
            return t;
        }

        static Image Outline(RectTransform target, Color c)
        {
            var o = UIKit.Image(target, "round_outline_8", c, "Outline");
            o.raycastTarget = false;
            o.rectTransform.Stretch(-4, -4, -4, -4);
            var le = o.gameObject.AddComponent<LayoutElement>(); le.ignoreLayout = true;   // 别让描边参与布局
            return o;
        }

        static LayoutElement LE(Component c, float minW = -1, float prefW = -1, float flexW = -1, float minH = -1, float prefH = -1, float flexH = -1)
        {
            var le = c.gameObject.GetComponent<LayoutElement>() ?? c.gameObject.AddComponent<LayoutElement>();
            le.minWidth = minW; le.preferredWidth = prefW; le.flexibleWidth = flexW;
            le.minHeight = minH; le.preferredHeight = prefH; le.flexibleHeight = flexH;
            return le;
        }

        static HorizontalLayoutGroup H(RectTransform rt, float spacing, int l, int r, int t, int b, TextAnchor a, bool ctrlW, bool ctrlH, bool expW, bool expH)
        {
            var g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing; g.padding = new RectOffset(l, r, t, b); g.childAlignment = a;
            g.childControlWidth = ctrlW; g.childControlHeight = ctrlH; g.childForceExpandWidth = expW; g.childForceExpandHeight = expH;
            return g;
        }

        static VerticalLayoutGroup V(RectTransform rt, float spacing, int l, int r, int t, int b, TextAnchor a, bool ctrlW, bool ctrlH, bool expW, bool expH)
        {
            var g = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing; g.padding = new RectOffset(l, r, t, b); g.childAlignment = a;
            g.childControlWidth = ctrlW; g.childControlHeight = ctrlH; g.childForceExpandWidth = expW; g.childForceExpandHeight = expH;
            return g;
        }

        static void Fit(Component c, bool h, bool v)
        {
            var f = c.gameObject.AddComponent<ContentSizeFitter>();
            f.horizontalFit = h ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            f.verticalFit = v ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        }

        // ------------------------------------------------------------------ 10.3 Control Size × Force Expand
        [BookShot("ch10-combo", W = 480, H = 330, Panels = 4)]
        static void Combo(Shot s)
        {
            bool ctrl = s.Panel >= 2, exp = s.Panel % 2 == 1;
            T(s.Root, $"Control Child Size：{(ctrl ? "开" : "关")}", 40, ctrl ? Skin.primary : Skin.muted, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(900, 56));
            T(s.Root, $"Child Force Expand：{(exp ? "开" : "关")}", 40, exp ? Skin.accent : Skin.muted, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(900, 56));

            var box = UIKit.Panel(s.Root, Color.white, 16, "HorizontalLayoutGroup");
            box.rectTransform.Place(0, -50, 880, 330);
            Outline(box.rectTransform, Line);
            H(box.rectTransform, 12, 16, 16, 16, 16, TextAnchor.MiddleCenter, ctrl, ctrl, exp, exp);
            float[] pref = { 140, 200, 260 };
            var kids = new List<RectTransform>();
            for (int i = 0; i < 3; i++)
            {
                var c = UIKit.Panel(box.rectTransform, Palette[i], 16, "Child" + (i + 1));
                c.rectTransform.sizeDelta = new Vector2(120, 90);                 // “手摆”的尺寸
                LE(c, minW: 80, prefW: pref[i], minH: 60, prefH: 140);
                kids.Add(c.rectTransform);
            }
            s.BeforeCapture(() =>
            {
                foreach (var k in kids)
                {
                    var t = T(k, $"{k.rect.width:0}×{k.rect.height:0}", 26, Color.white, bold: true);
                    t.rectTransform.Stretch(); t.enableWordWrapping = false;
                }
            });
            T(s.Root, "子物体：sizeDelta 120×90，preferred 宽 140 / 200 / 260、高 140", 26, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(940, 40));
        }

        // ------------------------------------------------------------------ 10.3 min / preferred / flexible
        [BookShot("ch10-flex", W = 480, H = 300, Panels = 3)]
        static void Flex(Shot s)
        {
            float[] widths = { 230, 375, 770 };
            string[] titles = { "容器 230 < 总 min 280", "容器 375：介于 min 与 preferred", "容器 770 > 总 preferred 470" };
            float w = widths[s.Panel];
            T(s.Root, titles[s.Panel], 38, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -44), new Vector2(940, 56));
            var box = UIKit.Node(s.Root, "HorizontalLayoutGroup");
            box.Place(0, 0, w, 150);
            var frame = UIKit.Image(box, "round_outline_8", Skin.ink, "Frame"); frame.rectTransform.Stretch(-6, -6, -6, -6);
            LE(frame).ignoreLayout = true;
            H(box, 10, 0, 0, 0, 0, TextAnchor.MiddleLeft, true, true, false, true);
            string[] names = { "A", "B", "C" };
            float[,] p = { { 100, 200, 1 }, { 100, 150, 0 }, { 60, 100, 2 } };
            var kids = new List<RectTransform>();
            for (int i = 0; i < 3; i++)
            {
                var c = UIKit.Panel(box, Palette[i] * new Color(1, 1, 1, 0.9f), 8, names[i]);
                LE(c, minW: p[i, 0], prefW: p[i, 1], flexW: p[i, 2]);
                kids.Add(c.rectTransform);
            }
            s.BeforeCapture(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    var t = T(kids[i], $"{names[i]}\n{kids[i].rect.width:0}", 30, Color.white, bold: true);
                    t.rectTransform.Stretch(); t.enableWordWrapping = false;
                }
            });
            T(s.Root, "A：min 100 / pref 200 / flex 1\nB：min 100 / pref 150 / flex 0\nC：min  60 / pref 100 / flex 2   （spacing 10）", 26, Skin.muted, TextAlignmentOptions.Left)
                .rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(620, 130));
        }

        // ------------------------------------------------------------------ 10.4 Grid Layout Group
        [BookShot("ch10-grid", W = 480, H = 360, Panels = 3)]
        static void Grid(Shot s)
        {
            string[] titles = { "Flexible · UpperLeft · Horizontal", "FixedColumnCount = 2 · Vertical", "FixedRowCount = 2 · LowerRight" };
            T(s.Root, titles[s.Panel], 34, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(940, 50));
            var box = UIKit.Panel(s.Root, Color.white, 16, "Grid");
            box.rectTransform.Place(0, -36, 640, 590);
            Outline(box.rectTransform, Line);
            var g = box.rectTransform.Grid(new Vector2(122, 122), new Vector2(12, 12), 20);
            if (s.Panel == 1) { g.constraint = GridLayoutGroup.Constraint.FixedColumnCount; g.constraintCount = 2; g.startAxis = GridLayoutGroup.Axis.Vertical; g.childAlignment = TextAnchor.UpperCenter; }
            if (s.Panel == 2) { g.constraint = GridLayoutGroup.Constraint.FixedRowCount; g.constraintCount = 2; g.startCorner = GridLayoutGroup.Corner.LowerRight; g.childAlignment = TextAnchor.LowerRight; }
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_gem", "item_coin", "item_key", "item_bow" };
            for (int i = 0; i < items.Length; i++)
            {
                var slot = UIKit.Image(box.rectTransform, "slot", Color.white, "Slot" + (i + 1));
                UIKit.Icon(slot.transform, items[i], 78, Color.white).rectTransform.anchoredPosition = new Vector2(0, 6);
                var n = T(slot.transform, (i + 1).ToString(), 28, Color.white, bold: true);
                n.rectTransform.Place(new Vector2(0, 1), new Vector2(26, -24), new Vector2(40, 40));
                var nb = UIKit.Image(slot.transform, "circle", Skin.accent, "Num"); nb.rectTransform.Place(new Vector2(0, 1), new Vector2(26, -24), new Vector2(40, 40));
                nb.transform.SetSiblingIndex(n.transform.GetSiblingIndex());
            }
        }

        // ------------------------------------------------------------------ 10.6 Content Size Fitter
        [BookShot("ch10-csf", W = 960, H = 400)]
        static void Csf(Shot s)
        {
            T(s.Root, "按钮：HorizontalLayoutGroup(padding) + ContentSizeFitter(水平 Preferred)", 30, Skin.muted).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -46), new Vector2(1800, 44));
            string[] labels = { "确定", "下一步", "领取全部奖励并返回主界面" };
            float[] xs = { -620, -250, 380 };
            for (int i = 0; i < 3; i++)
            {
                var b = UIKit.Button(s.Root, labels[i], null, 200, 96);
                b.Rt().Place(new Vector2(0.5f, 0.5f), new Vector2(xs[i], 190), new Vector2(200, 96));
                var t = b.GetComponentInChildren<TextMeshProUGUI>();
                H(b.Rt(), 0, 48, 48, 18, 18, TextAnchor.MiddleCenter, true, true, false, false);
                Fit(b, true, false);
                t.enableWordWrapping = false;
            }
            T(s.Root, "提示框：VerticalLayoutGroup + ContentSizeFitter(两个方向都 Preferred) + 限宽", 30, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(1800, 44));
            var tips = new[] { "背包已满", "金币不足，是否前往商城购买？\n首充额外赠送 30% 钻石。" };
            float x = -420;
            foreach (var tip in tips)
            {
                var p = UIKit.Panel(s.Root, Skin.dark, 24, "Tooltip");
                p.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(x, -150), new Vector2(100, 100));
                p.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                V(p.rectTransform, 8, 36, 36, 24, 24, TextAnchor.UpperLeft, true, true, false, false);
                var t = T(p.transform, tip, 36, Color.white, TextAlignmentOptions.TopLeft);
                Fit(p, true, true);
                x += 760;
            }
        }

        // ------------------------------------------------------------------ 10.8 坑：布局组里的子物体挂 ContentSizeFitter
        [BookShot("ch10-fitter-pitfall", W = 480, H = 260, Panels = 2)]
        static void FitterPitfall(Shot s)
        {
            bool wrong = s.Panel == 0;
            T(s.Root, wrong ? "× 子物体挂 ContentSizeFitter，父 HLG 不控制尺寸" : "√ 父 HLG 勾 Control Child Size，子物体不挂 Fitter", 32, wrong ? Skin.danger : Skin.success, bold: true)
                .rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(940, 50));
            var row = UIKit.Panel(s.Root, Color.white, 16, "Row");
            row.rectTransform.Place(0, -30, 900, 160);
            H(row.rectTransform, 16, 20, 20, 20, 20, TextAnchor.MiddleLeft, !wrong, false, false, false);
            foreach (var l in new[] { "确定", "下一步", "返回主界面" })
            {
                var b = UIKit.Button(row.transform, l, Palette[l.Length % 3] * new Color(1, 1, 1, 0.85f), 200, 96, fontSize: 34);
                H(b.Rt(), 0, 40, 40, 16, 16, TextAnchor.MiddleCenter, true, true, false, false);
                if (wrong) Fit(b, true, false);
                b.GetComponentInChildren<TextMeshProUGUI>().enableWordWrapping = false;
            }
        }

        // ------------------------------------------------------------------ 10.6 Aspect Ratio Fitter
        [BookShot("ch10-aspect", W = 960, H = 380)]
        static void Aspect(Shot s)
        {
            var modes = new[] { AspectRatioFitter.AspectMode.WidthControlsHeight, AspectRatioFitter.AspectMode.HeightControlsWidth, AspectRatioFitter.AspectMode.FitInParent, AspectRatioFitter.AspectMode.EnvelopeParent };
            for (int i = 0; i < 4; i++)
            {
                float x = -690 + i * 460;
                var parent = UIKit.Node(s.Root, "Parent_" + modes[i]);
                parent.Place(x, 10, 300, 230);
                var bg = UIKit.Image(parent, "checker", new Color(1, 1, 1, 0.5f), "ParentBg"); bg.rectTransform.Stretch(); bg.type = Image.Type.Tiled;
                var img = UIKit.Image(parent, "bg_landscape", new Color(1, 1, 1, 0.85f), "Image");
                if (i == 0) img.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 60));
                else if (i == 1) img.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 100));
                var f = img.gameObject.AddComponent<AspectRatioFitter>();
                f.aspectMode = modes[i];   // 先设模式：模式为 None 时，编辑器下 aspectRatio 会被改成当前矩形的宽高比
                f.aspectRatio = 2f;
                var o = UIKit.Image(parent, "round_outline_8", Skin.ink, "ParentFrame"); o.rectTransform.Stretch(-4, -4, -4, -4);
                T(s.Root, modes[i].ToString(), 30, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(x, -200), new Vector2(440, 44));
                string[] notes = { "宽 240 → 高 120", "高 100 → 宽 200", "整张放进父框（留边）", "铺满父框（超出部分）" };
                T(s.Root, notes[i], 26, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(x, -244), new Vector2(440, 40));
            }
            T(s.Root, "子物体是 2:1 的风景图（aspectRatio = 2），棋盘格是父物体", 28, Skin.muted).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(1800, 44));
        }

        // ------------------------------------------------------------------ 案例 10.1 聊天气泡
        static RectTransform ChatRow(Transform parent, bool mine, string avatar, string name)
        {
            var row = UIKit.Node(parent, name);
            var h = H(row, 18, 28, 28, 0, 0, mine ? TextAnchor.UpperRight : TextAnchor.UpperLeft, true, true, false, false);
            h.reverseArrangement = mine;                       // 自己的消息：头像在右边
            var av = UIKit.Image(row, avatar, Color.white, "Avatar");
            LE(av, minW: 96, prefW: 96, minH: 96, prefH: 96);
            var bubble = UIKit.Image(row, "round_24", mine ? Skin.primary : Color.white, "Bubble");
            V(bubble.rectTransform, 0, 30, 30, 20, 22, TextAnchor.UpperLeft, true, true, false, false);
            bubble.gameObject.AddComponent<MaxSizeLayoutElement>().maxWidth = 640;
            var t = T(bubble.transform, "", 38, mine ? Color.white : Skin.ink, TextAlignmentOptions.TopLeft, name: "Text");
            t.lineSpacing = 4;
            t.enableWordWrapping = true;   // 在未激活的父节点下创建的 TMP 不会执行 Awake，默认设置没读进来，这里显式打开换行
            return row;
        }

        [BookShot("ch10-chat", W = 405, H = 720, Scene = "Ch10_Chat")]
        static void Chat(Shot s)
        {
            s.Ref(1080, 1920, 1);
            s.Background(UIKit.Hex("#E9ECF4"));
            var top = UIKit.Panel(s.Root, Color.white, 0, "TopBar"); top.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(1080, 150));
            top.rectTransform.anchorMin = new Vector2(0, 1); top.rectTransform.anchorMax = new Vector2(1, 1); top.rectTransform.sizeDelta = new Vector2(0, 150);
            T(top.transform, "小队频道（3）", 44, Skin.ink, bold: true).rectTransform.Stretch(0, 40, 0, 0);

            var sr = UIKit.ScrollView(s.Root, true, false, 1080, 1500);
            sr.Rt().Stretch(0, 150, 0, 170);
            var content = sr.content;
            V(content, 28, 0, 0, 32, 32, TextAnchor.UpperCenter, true, true, true, false);
            Fit(content, false, true);

            var templates = UIKit.Node(s.Root, "Templates"); templates.gameObject.SetActive(false);
            var left = ChatRow(templates, false, "avatar_2", "Row_Left");
            var right = ChatRow(templates, true, "avatar_1", "Row_Right");

            var bar = UIKit.Panel(s.Root, Color.white, 0, "InputBar");
            bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.anchorMax = new Vector2(1, 0); bar.rectTransform.pivot = new Vector2(0.5f, 0);
            bar.rectTransform.sizeDelta = new Vector2(0, 170); bar.rectTransform.anchoredPosition = Vector2.zero;
            H(bar.rectTransform, 20, 28, 28, 30, 40, TextAnchor.MiddleCenter, true, true, false, true);
            var input = UIKit.Input(bar.rectTransform, "说点什么…", 700, 100, 36);
            LE(input, minW: 200, prefW: 700, flexW: 1);
            var send = UIKit.Button(bar.rectTransform, "发送", null, 180, 100);
            LE(send, minW: 180, prefW: 180);

            var demo = s.Root.gameObject.AddComponent<ChatDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_Scroll").objectReferenceValue = sr;
            so.FindProperty("m_LeftTemplate").objectReferenceValue = left;
            so.FindProperty("m_RightTemplate").objectReferenceValue = right;
            so.FindProperty("m_Input").objectReferenceValue = input;
            so.FindProperty("m_Send").objectReferenceValue = send;
            so.ApplyModifiedPropertiesWithoutUndo();

            var time = T(content, "昨天 21:30", 30, Skin.muted, name: "Time"); LE(time, prefH: 44);
            demo.Add("今晚打副本吗？", false);
            demo.Add("打！不过我八点半才到家，先把装备修一下。", true);
            demo.Add("没问题。这次带上治疗药水，上次团灭就是因为大家都没药了，最后 Boss 只剩一丝血。", false);
            demo.Add("好", true);
            demo.Add("我顺便把新拿的弓也带上，攻击力比原来的高不少，试试效果。", true);
            s.BeforeCapture(demo.ScrollToBottom);
            s.Box(content.GetChild(3).Find("Bubble") as RectTransform, Skin.accent, "MaxSizeLayoutElement 限宽 640", 6, true);
        }

        // ------------------------------------------------------------------ 案例 10.2 表单
        [BookShot("ch10-form", W = 480, H = 420, Panels = 2, Scene = "Ch10_Form")]
        static void Form(Shot s)
        {
            float w = s.Panel == 0 ? 900 : 640;
            var card = UIKit.Panel(s.Root, Color.white, 24, "Form");
            card.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(w, 700));
            V(card.rectTransform, 22, 44, 44, 36, 36, TextAnchor.UpperLeft, true, true, true, false);
            Fit(card, false, true);
            var title = T(card.transform, "创建角色", 44, Skin.ink, TextAlignmentOptions.Left, true, "Title");
            RectTransform Row(string label)
            {
                var r = UIKit.Node(card.transform, "Row_" + label);
                H(r, 20, 0, 0, 0, 0, TextAnchor.MiddleLeft, true, true, false, false);
                var l = T(r, label, 34, Skin.ink, TextAlignmentOptions.MidlineRight, name: "Label");
                LE(l, minW: 120, prefW: 150, flexW: 0);
                return r;
            }
            var nameField = UIKit.Input(Row("昵称"), "2~12 个字", 400, 84, 32); LE(nameField, minW: 160, prefW: 400, flexW: 1, prefH: 84);
            var dd = UIKit.Dropdown(Row("职业"), new[] { "战士", "法师", "游侠" }, 300, 84, 32); LE(dd, minW: 160, prefW: 300, flexW: 1, prefH: 84);
            var memo = UIKit.Input(Row("签名"), "介绍一下自己（选填）", 400, 160, 32);
            memo.lineType = TMP_InputField.LineType.MultiLineNewline;
            LE(memo, minW: 160, prefW: 400, flexW: 1, prefH: 150);
            var tgRow = Row("");
            var tg = UIKit.Toggle(tgRow, "同意用户协议", true, 400, 64); LE(tg, prefW: 400, flexW: 1, prefH: 64);
            var btns = UIKit.Node(card.transform, "Buttons");
            H(btns, 20, 0, 0, 12, 0, TextAnchor.MiddleRight, true, true, false, false);
            var spacer = UIKit.Node(btns, "Spacer"); LE(spacer, flexW: 1);
            var cancel = UIKit.Button(btns, "取消", UIKit.Hex("#E6E8F0"), 200, 88); LE(cancel, minW: 140, prefW: 200, prefH: 88);
            var ok = UIKit.Button(btns, "创建", null, 200, 88); LE(ok, minW: 140, prefW: 200, prefH: 88);
            T(s.Root, $"表单宽 {w:0}", 30, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(600, 44));
        }

        // ------------------------------------------------------------------ 案例 10.3 自动列数网格
        [BookShot("ch10-autogrid", W = 480, H = 560, Panels = 3, Scene = "Ch10_AutoGrid")]
        static void AutoGrid(Shot s)
        {
            float[] widths = { 900, 660, 420 };
            float w = widths[s.Panel];
            var box = UIKit.Panel(s.Root, Color.white, 16, "Shop");
            box.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(w, 600));
            var grid = box.gameObject.AddComponent<ResponsiveGridLayout>();
            grid.padding = new RectOffset(20, 20, 20, 20); grid.spacing = new Vector2(16, 16);
            grid.minCellWidth = 180; grid.cellAspect = 0.95f;
            Fit(box, false, true);
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_potion_blue", "item_gem", "item_coin", "item_key", "item_bow", "item_helmet" };
            foreach (var it in items)
            {
                var cell = UIKit.Image(box.transform, "round_16", UIKit.Hex("#F2F3F9"), "Cell_" + it);
                var ic = UIKit.Icon(cell.transform, it, 90, Color.white); ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
                ic.rectTransform.sizeDelta = Vector2.zero; ic.rectTransform.anchorMin = new Vector2(0.2f, 0.32f); ic.rectTransform.anchorMax = new Vector2(0.8f, 0.92f);
                var price = T(cell.transform, "120", 28, Skin.accent, bold: true, name: "Price");
                price.rectTransform.anchorMin = new Vector2(0, 0); price.rectTransform.anchorMax = new Vector2(1, 0.3f); price.rectTransform.offsetMin = price.rectTransform.offsetMax = Vector2.zero;
            }
            var title = T(s.Root, $"宽 {w:0}", 34, Skin.ink, bold: true);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(900, 50));
            s.BeforeCapture(() => title.text = $"宽 {w:0} → {grid.columns} 列，格子宽 {grid.cellSize.x:0}");
        }

        // ------------------------------------------------------------------ 案例 10.4 流式布局
        [BookShot("ch10-flow", W = 480, H = 400, Panels = 2, Scene = "Ch10_Flow")]
        static void Flow(Shot s)
        {
            float w = s.Panel == 0 ? 860 : 560;
            T(s.Root, s.Panel == 0 ? "UpperLeft，宽 860" : "MiddleCenter + 两端对齐，宽 560", 34, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(940, 50));
            var box = UIKit.Panel(s.Root, Color.white, 16, "TagBox");
            box.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -96), new Vector2(w, 300));
            var flow = box.gameObject.AddComponent<FlowLayoutGroup>();
            flow.padding = new RectOffset(20, 20, 20, 20);
            flow.spacing = new Vector2(14, 14);
            if (s.Panel == 1) { flow.childAlignment = TextAnchor.MiddleCenter; flow.justify = true; }
            Fit(box, false, true);

            var templates = UIKit.Node(s.Root, "Templates"); templates.gameObject.SetActive(false);
            var tag = UIKit.Button(templates, "标签", UIKit.Hex("#EEF0F9"), 120, 64, fontSize: 30);
            tag.name = "TagTemplate";
            tag.image.sprite = UISkin.Sprite("pill");
            H(tag.Rt(), 0, 28, 28, 10, 12, TextAnchor.MiddleCenter, true, true, false, false);
            var tt = tag.GetComponentInChildren<TextMeshProUGUI>(); tt.enableWordWrapping = false; tt.fontStyle = FontStyles.Normal; tt.font = Skin.font;

            var input = UIKit.Input(s.Root, "输入新标签，回车添加", 600, 84, 30);
            input.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(Mathf.Min(w, 700), 84));

            var cloud = s.Root.gameObject.AddComponent<TagCloud>();
            var so = new SerializedObject(cloud);
            so.FindProperty("m_Container").objectReferenceValue = box.rectTransform;
            so.FindProperty("m_Template").objectReferenceValue = tag;
            so.FindProperty("m_Input").objectReferenceValue = input;
            so.ApplyModifiedPropertiesWithoutUndo();
            cloud.Populate(new[] { "动作", "角色扮演", "开放世界", "像素风", "多人联机", "Roguelike", "策略", "剧情丰富", "休闲", "二次元", "卡牌", "生存建造", "解谜", "音乐节奏" }, 1, 5, 9);
        }

        // ------------------------------------------------------------------ 案例 10.5 可折叠分组
        static FoldoutSection Section(Transform parent, string title, string[][] rows, bool expanded)
        {
            var sec = UIKit.Panel(parent, Color.white, 16, "Section_" + title);
            V(sec.rectTransform, 0, 0, 0, 0, 8, TextAnchor.UpperCenter, true, true, true, false);
            var header = UIKit.Button(sec.transform, "", new Color(1, 1, 1, 0), 900, 110, name: "Header");
            LE(header, prefH: 110);
            var ht = header.GetComponentInChildren<TextMeshProUGUI>();
            ht.text = $"{title}（{rows.Length}）"; ht.color = Skin.ink; ht.alignment = TextAlignmentOptions.MidlineLeft; ht.rectTransform.offsetMin = new Vector2(100, 0);
            var arrow = UIKit.Icon(header.transform, "arrow_right", 44, Skin.muted, "Arrow");
            arrow.rectTransform.anchorMin = arrow.rectTransform.anchorMax = new Vector2(0, 0.5f); arrow.rectTransform.anchoredPosition = new Vector2(56, 0);

            var body = UIKit.Node(sec.transform, "Body");
            body.gameObject.AddComponent<RectMask2D>();
            var le = LE(body, prefH: 0);
            var content = UIKit.Node(body, "Content");
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
            V(content, 8, 24, 24, 0, 8, TextAnchor.UpperCenter, true, true, true, false);
            Fit(content, false, true);
            foreach (var r in rows)
            {
                var item = UIKit.Image(content, "round_16", UIKit.Hex("#F3F4FA"), "Item_" + r[1]);
                H(item.rectTransform, 20, 20, 24, 12, 12, TextAnchor.MiddleLeft, true, true, false, false);
                var ic = UIKit.Icon(item.transform, r[0], 76, Color.white); LE(ic, prefW: 76, prefH: 76);
                var nm = T(item.transform, r[1], 34, Skin.ink, TextAlignmentOptions.MidlineLeft); LE(nm, flexW: 1);
                var ct = T(item.transform, r[2], 32, Skin.muted, TextAlignmentOptions.MidlineRight); LE(ct, prefW: 120);
            }
            var fs = sec.gameObject.AddComponent<FoldoutSection>();
            var so = new SerializedObject(fs);
            so.FindProperty("m_Header").objectReferenceValue = header;
            so.FindProperty("m_Arrow").objectReferenceValue = arrow.rectTransform;
            so.FindProperty("m_Body").objectReferenceValue = le;
            so.FindProperty("m_Content").objectReferenceValue = content;
            so.FindProperty("m_Expanded").boolValue = expanded;
            so.ApplyModifiedPropertiesWithoutUndo();
            return fs;
        }

        [BookShot("ch10-fold", W = 405, H = 720, Scene = "Ch10_Foldout")]
        static void Fold(Shot s)
        {
            s.Ref(1080, 1920, 1);
            T(s.Root, "背包", 52, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(900, 70));
            var sr = UIKit.ScrollView(s.Root, true, false, 1000, 1600);
            sr.Rt().Stretch(40, 150, 40, 40);
            V(sr.content, 24, 0, 0, 0, 24, TextAnchor.UpperCenter, true, true, true, false);
            Fit(sr.content, false, true);
            var a = Section(sr.content, "装备", new[] { new[] { "item_sword", "精钢长剑", "×1" }, new[] { "item_shield", "橡木盾", "×1" }, new[] { "item_helmet", "皮革头盔", "×2" }, new[] { "item_bow", "猎人短弓", "×1" } }, true);
            var b = Section(sr.content, "消耗品", new[] { new[] { "item_potion_red", "生命药水", "×12" }, new[] { "item_potion_blue", "法力药水", "×5" }, new[] { "item_apple", "苹果", "×30" } }, false);
            var c = Section(sr.content, "任务道具", new[] { new[] { "item_key", "地牢钥匙", "×1" }, new[] { "item_scroll", "古老卷轴", "×1" } }, true);
            T(s.Root, "点击分组标题展开/收起", 30, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(900, 44));
            s.BeforeCapture(() =>
            {
                a.SetProgress(1); b.SetProgress(0); c.SetProgress(0.55f);   // 第三组停在“展开到一半”的那一帧
                Canvas.ForceUpdateCanvases();
                a.SetProgress(1); b.SetProgress(0); c.SetProgress(0.55f);
            });
            s.LabelAt(c.transform as RectTransform, new Vector2(0.5f, 0), new Vector2(0, -50), "动画进行中：Body 高度 = 内容高度 × 进度", Skin.accent, 30);
        }

        // ------------------------------------------------------------------ 10.8 布局延迟
        [BookShot("ch10-delay", W = 960, H = 300, Scene = "Ch10_LayoutDelay")]
        static void Delay(Shot s)
        {
            var list = UIKit.Panel(s.Root, Color.white, 16, "List");
            list.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(-380, -50), new Vector2(560, 100));
            V(list.rectTransform, 12, 20, 20, 20, 20, TextAnchor.UpperCenter, true, true, true, false);
            Fit(list, false, true);
            var templates = UIKit.Node(s.Root, "Templates"); templates.gameObject.SetActive(false);
            var item = UIKit.Image(templates, "round_8", UIKit.Hex("#E9EBF5"), "Item");
            LE(item, prefH: 72);
            T(item.transform, "项", 30, Skin.ink).rectTransform.Stretch();
            for (int i = 0; i < 2; i++)
            {
                var it = Object.Instantiate(item, list.transform); it.gameObject.SetActive(true); it.name = "Item";
                it.GetComponentInChildren<TMP_Text>().text = $"第 {i + 1} 项";
            }
            var btn = UIKit.Button(s.Root, "添加一项", null, 320, 100, "plus");
            btn.Rt().Place(new Vector2(0.5f, 1), new Vector2(330, -70), new Vector2(320, 100));
            var report = T(s.Root, "点击“添加一项”", 34, Skin.ink, TextAlignmentOptions.Left, name: "Report");
            report.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(330, -310), new Vector2(560, 140));
            var demo = s.Root.gameObject.AddComponent<LayoutTimingDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_List").objectReferenceValue = list.rectTransform;
            so.FindProperty("m_ItemTemplate").objectReferenceValue = item.rectTransform;
            so.FindProperty("m_Report").objectReferenceValue = report;
            so.FindProperty("m_AddButton").objectReferenceValue = btn;
            so.ApplyModifiedPropertiesWithoutUndo();
            s.BeforeCapture(() => { demo.AddAndMeasure(); report.text = report.text.Replace("    ", "\n"); });
            s.Box(report.rectTransform, Skin.accent, "实测（编辑器下调用 AddAndMeasure）", 10);
        }
    }
}
