// 第 3 章配图与配套场景的搭景代码（不进书）。
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch03
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;
        static readonly Color Orange = new Color32(0xFF, 0x8A, 0x3D, 0xFF);
        static readonly Color Ink = new Color32(0x1F, 0x24, 0x36, 0xFF);
        static readonly Color Blue = new Color32(0x1E, 0x88, 0xE5, 0xFF);
        static readonly Color Line = UIKit.Hex("#C9CDE0");

        static void Set(Object o, string prop, Object value)
        {
            var so = new SerializedObject(o);
            so.FindProperty(prop).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArr(Object o, string prop, Object[] values)
        {
            var so = new SerializedObject(o);
            var p = so.FindProperty(prop);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// 浅色“父节点”框：浅底 + 描边
        static RectTransform ParentBox(Transform parent, string name, Color? fill = null)
        {
            var bg = UIKit.Image(parent, "round_8", fill ?? UIKit.Hex("#E4E7F3"), name);
            var ol = UIKit.Image(bg.transform, "round_outline_8", Line, "Outline");
            ol.rectTransform.Stretch(); ol.raycastTarget = false;
            return bg.rectTransform;
        }

        static RectTransform Target(Transform parent, string label = "Target", Color? c = null)
        {
            var img = UIKit.Image(parent, "round_8", c ?? new Color(0.36f, 0.29f, 0.84f, 0.9f), "Target");
            var t = UIKit.Text(img.transform, label, 30, Color.white, bold: true, name: "Label");
            t.rectTransform.Stretch();
            return img.rectTransform;
        }

        static (TextMeshProUGUI text, RectProbe probe) Probe(Transform parent, RectTransform target, float fontSize = 24)
        {
            var p = UIKit.Panel(parent, K.dark, 16, "RectProbe");
            var t = UIKit.Text(p.transform, "", fontSize, Color.white, TextAlignmentOptions.TopLeft, name: "Values");
            t.rectTransform.Stretch(26, 22, 16, 16);
            t.lineSpacing = 18;
            var probe = p.gameObject.AddComponent<RectProbe>();
            Set(probe, "target", target); Set(probe, "output", t);
            return (t, probe);
        }

        // ---------------- 3.1 RectTransform 解剖 ----------------
        [BookShot("ch03-anatomy", W = 640, H = 400, Panels = 2, Scene = "Ch03_RectProbe")]
        static void Anatomy(Shot s)
        {
            s.Ref(1280, 800, 0.5f);
            var parent = ParentBox(s.Root, "Parent");
            parent.Place(-250, -20, 640, 560);
            var t = Target(parent);
            if (s.Panel == 0)
            {
                t.anchorMin = t.anchorMax = new Vector2(0.5f, 0.5f); t.pivot = new Vector2(0.5f, 0.5f);
                t.anchoredPosition = new Vector2(70, 60); t.sizeDelta = new Vector2(300, 180);
            }
            else
            {
                t.anchorMin = new Vector2(0.15f, 0.2f); t.anchorMax = new Vector2(0.8f, 0.8f); t.pivot = new Vector2(0.5f, 0.5f);
                t.offsetMin = new Vector2(60, 50); t.offsetMax = new Vector2(-50, -45);
            }
            var (_, probe) = Probe(s.Root, t, 21);
            probe.GetComponent<RectTransform>().Place(360, -20, 480, 560);
            s.BeforeCapture(probe.Refresh);
            s.Anchors(t);
            s.Label(new Vector2(0, 340), s.Panel == 0 ? "锚点重合（一个点）：sizeDelta 就是尺寸" : "锚点分开（一个矩形）：用 offsetMin / offsetMax 描述四边",
                Ink, 30);
            if (s.Panel == 0)
            {
                s.Arrow(() => s.At(parent, new Vector2(0.5f, 0.5f)), () => s.At(t, new Vector2(0.5f, 0.5f)), Orange, 5);
                s.LabelAt(parent, new Vector2(0.5f, 0.5f), new Vector2(-30, -50), "anchoredPosition (70, 60)", Orange, 24);
            }
            else
            {
                s.Arrow(() => s.At(parent, new Vector2(0.15f, 0.2f)), () => s.At(t, Vector2.zero), Orange, 5);
                s.LabelAt(parent, new Vector2(0.15f, 0.2f), new Vector2(60, -38), "offsetMin (60, 50)", Orange, 24);
                s.Arrow(() => s.At(parent, new Vector2(0.8f, 0.8f)), () => s.At(t, Vector2.one), Orange, 5);
                s.LabelAt(parent, new Vector2(0.8f, 0.8f), new Vector2(-40, 36), "offsetMax (-50, -45)", Orange, 24);
            }
        }

        // ---------------- 3.2 十六种锚点预设 ----------------
        static string[] HN = { "left", "center", "right", "stretch" };
        static string[] VN = { "top", "middle", "bottom", "stretch" };

        [BookShot("ch03-presets", W = 960, H = 600)]
        static void Presets(Shot s)
        {
            float cw = 330, ch = 196, gx = 40, gy = 40;
            float x0 = -(cw * 4 + gx * 3) / 2 + cw / 2 + 60, y0 = (ch * 4 + gy * 3) / 2 - ch / 2 - 30;
            for (int v = 0; v < 4; v++)
                for (int h = 0; h < 4; h++)
                {
                    var p = ParentBox(s.Root, $"Parent_{HN[h]}_{VN[v]}");
                    p.Place(x0 + h * (cw + gx), y0 - v * (ch + gy), cw, ch);
                    var t = Target(p, "", new Color(0.36f, 0.29f, 0.84f, 0.85f));
                    t.Place(0, 0, 110, 60);
                    t.SetAnchorPreset((AnchorH)h, (AnchorV)v, true, true);
                    // 拉伸方向留一点边，好看出是“撑满”
                    var omin = t.offsetMin; var omax = t.offsetMax;
                    if (h == 3) { omin.x = 22; omax.x = -22; }
                    if (v == 3) { omin.y = 22; omax.y = -22; }
                    t.offsetMin = omin; t.offsetMax = omax;
                    s.Anchors(t, null, false);
                }
            for (int h = 0; h < 4; h++)
                s.Label(new Vector2(x0 + h * (cw + gx), y0 + ch / 2 + 36), HN[h], Ink, 30);
            for (int v = 0; v < 4; v++)
                s.Label(new Vector2(x0 - cw / 2 - 105, y0 - v * (ch + gy)), VN[v], Ink, 30);
        }

        // ---------------- 案例 3.1 代码版锚点预设 ----------------
        [BookShot("ch03-anchorcode", W = 960, H = 600, Scene = "Ch03_AnchorPresets")]
        static void AnchorCode(Shot s)
        {
            // 左：4×4 预设按钮，每个按钮里用同一个函数画一个小示意
            var grid = UIKit.Node(s.Root, "PresetGrid");
            grid.Place(-640, 40, 560, 560);
            var buttons = new Button[16];
            for (int v = 0; v < 4; v++)
                for (int h = 0; h < 4; h++)
                {
                    var b = UIKit.Button(grid, "", Color.white, 120, 120, name: $"Preset_{HN[h]}_{VN[v]}");
                    b.Rt().Place(new Vector2(0, 1), new Vector2(40 + h * 132, -40 - v * 132), new Vector2(120, 120));
                    b.Rt().pivot = new Vector2(0, 1);
                    Object.DestroyImmediate(b.GetComponentInChildren<TextMeshProUGUI>().gameObject);
                    var frame = UIKit.Image(b.transform, "round_outline_8", K.muted, "Frame");
                    frame.raycastTarget = false;
                    frame.rectTransform.Place(0, 0, 76, 76);
                    var mini = UIKit.Image(frame.transform, null, K.danger, "Mini");
                    mini.raycastTarget = false;
                    mini.rectTransform.Place(0, 0, 26, 26);
                    mini.rectTransform.SetAnchorPreset((AnchorH)h, (AnchorV)v, true, true);
                    var o1 = mini.rectTransform.offsetMin; var o2 = mini.rectTransform.offsetMax;
                    if (h == 3) { o1.x = 8; o2.x = -8; }
                    if (v == 3) { o1.y = 8; o2.y = -8; }
                    mini.rectTransform.offsetMin = o1; mini.rectTransform.offsetMax = o2;
                    buttons[v * 4 + h] = b;
                }
            var gt = UIKit.Text(s.Root, "点按钮 = 选预设", 36, Ink, bold: true, name: "GridTitle");
            gt.rectTransform.Place(-640, 360, 560, 60);

            var parent = ParentBox(s.Root, "Parent");
            parent.Place(new Vector2(0.5f, 0.5f), new Vector2(330, 150), new Vector2(900, 420));
            var target = Target(parent);
            target.Place(-80, 30, 260, 130);

            var t1 = UIKit.Toggle(s.Root, "Shift：同时设置轴心", true, 520, 64);
            t1.Rt().Place(-30, -170, 520, 64);
            var t2 = UIKit.Toggle(s.Root, "Alt：同时设置位置", true, 520, 64);
            t2.Rt().Place(-30, -250, 520, 64);
            var sl = UIKit.Slider(s.Root, 0.5f, 400, 40);
            sl.minValue = 400; sl.maxValue = 1100; sl.value = 900;
            sl.Rt().Place(60, -340, 340, 40);
            var sll = UIKit.Text(s.Root, "父节点宽度", 30, Ink, TextAlignmentOptions.MidlineLeft, name: "WidthLabel");
            sll.rectTransform.Place(-200, -340, 180, 50);
            var reset = UIKit.Button(s.Root, "复位", UIKit.Hex("#E6E8F0"), 200, 80, fontSize: 32);
            reset.Rt().Place(-170, -450, 200, 80);
            var (_, probe) = Probe(s.Root, target, 22);
            probe.GetComponent<RectTransform>().Place(600, -300, 600, 400);

            var panel = s.Root.gameObject.AddComponent<AnchorPresetPanel>();
            Set(panel, "target", target); SetArr(panel, "presetButtons", buttons);
            Set(panel, "pivotToggle", t1); Set(panel, "positionToggle", t2); Set(panel, "parentWidth", sl); Set(panel, "resetButton", reset);

            s.BeforeCapture(() =>
            {
                target.SetAnchorPreset(AnchorH.Stretch, AnchorV.Top, true, true);
                probe.Refresh();
            });
            s.Box(buttons[3].Rt(), null, null, 4);
            s.Anchors(target);
        }

        // ---------------- 3.3 固定尺寸 vs 拉伸 ----------------
        [BookShot("ch03-fixed-stretch", W = 640, H = 360, Panels = 2)]
        static void FixedStretch(Shot s)
        {
            s.Ref(1280, 720, 0.5f);
            float w = s.Panel == 0 ? 640 : 1100;
            var p = ParentBox(s.Root, "Window", Color.white);
            p.Place(0, -30, w, 520);
            var bar = UIKit.Image(p, "round_8", K.primary, "TitleBar");
            bar.rectTransform.anchorMin = new Vector2(0, 1); bar.rectTransform.anchorMax = new Vector2(1, 1);
            bar.rectTransform.pivot = new Vector2(0.5f, 1);
            bar.rectTransform.offsetMin = new Vector2(20, -100); bar.rectTransform.offsetMax = new Vector2(-20, -20);
            var bt = UIKit.Text(bar.transform, "标题栏：横向拉伸", 30, Color.white, TextAlignmentOptions.MidlineLeft, true);
            bt.rectTransform.Stretch(30, 0, 100, 0);
            var close = UIKit.Icon(bar.transform, "close", 44, Color.white, "Close");
            close.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(44, 44));
            var btn = UIKit.Button(p, "固定 300×100", K.accent, 300, 100, fontSize: 32);
            btn.Rt().Place(0, -10, 300, 100);
            var input = UIKit.Image(p, "round_8", UIKit.Hex("#E6E8F0"), "Footer");
            input.rectTransform.anchorMin = Vector2.zero; input.rectTransform.anchorMax = new Vector2(1, 0);
            input.rectTransform.pivot = new Vector2(0.5f, 0);
            input.rectTransform.offsetMin = new Vector2(20, 20); input.rectTransform.offsetMax = new Vector2(-20, 100);
            var it = UIKit.Text(input.transform, "底栏：横向拉伸，左右各留 20", 28, K.muted);
            it.rectTransform.Stretch();
            s.Anchors(bar.rectTransform, null, false);
            s.Anchors(btn.Rt(), null, false);
            s.Label(new Vector2(0, 310), $"父节点宽 {w:0}", Ink, 32);
        }

        // ---------------- 3.1 轴心 ----------------
        [BookShot("ch03-pivot", W = 420, H = 360, Panels = 3)]
        static void Pivot(Shot s)
        {
            s.Ref(840, 720, 0.5f);
            Vector2[] pv = { Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.one };
            var p = ParentBox(s.Root, "Parent");
            p.Place(0, -20, 760, 600);
            var cross1 = UIKit.Image(p, null, Line, "CrossH"); cross1.rectTransform.Place(0, 0, 760, 3);
            var cross2 = UIKit.Image(p, null, Line, "CrossV"); cross2.rectTransform.Place(0, 0, 3, 600);
            var ghost = UIKit.Image(p, "round_outline_8", new Color(0.36f, 0.29f, 0.84f, 0.5f), "Ghost");
            ghost.rectTransform.pivot = pv[s.Panel]; ghost.rectTransform.anchoredPosition = Vector2.zero; ghost.rectTransform.sizeDelta = new Vector2(240, 140);
            var t = Target(p);
            t.pivot = pv[s.Panel]; t.anchoredPosition = Vector2.zero; t.sizeDelta = new Vector2(240, 140);
            t.localEulerAngles = new Vector3(0, 0, 30);
            s.Anchors(t);
            s.Label(new Vector2(0, 320), $"pivot = ({pv[s.Panel].x:0.#}, {pv[s.Panel].y:0.#})", Ink, 34);
        }

        // ---------------- 3.4 用代码设置尺寸 ----------------
        [BookShot("ch03-rectops", W = 640, H = 400, Panels = 2, Scene = "Ch03_RectOps")]
        static void RectOps(Shot s)
        {
            s.Ref(1280, 800, 0.5f);
            var parent = ParentBox(s.Root, "Parent");
            parent.Place(-170, 50, 820, 520);
            var t = Target(parent);
            t.Place(0, 0, 300, 160);
            var (_, probe) = Probe(s.Root, t, 20);
            probe.GetComponent<RectTransform>().Place(440, 50, 380, 520);
            string[] labels = { "加宽", "变窄", "贴左", "贴上", "贴右", "贴下" };
            var row = UIKit.Node(s.Root, "Buttons");
            row.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1200, 80));
            row.HLayout(16);
            var bs = new Button[6];
            for (int i = 0; i < 6; i++) bs[i] = UIKit.Button(row, labels[i], i < 2 ? K.accent : K.primary, 180, 72, fontSize: 30);
            var demo = s.Root.gameObject.AddComponent<RectOpsDemo>();
            Set(demo, "target", t);
            Set(demo, "widerButton", bs[0]); Set(demo, "narrowerButton", bs[1]); Set(demo, "leftButton", bs[2]);
            Set(demo, "topButton", bs[3]); Set(demo, "rightButton", bs[4]); Set(demo, "bottomButton", bs[5]);
            s.BeforeCapture(() =>
            {
                demo.Dock(RectTransform.Edge.Left);
                if (s.Panel == 1) demo.Dock(RectTransform.Edge.Top);
                probe.Refresh();
            });
            s.Anchors(t);
            s.Label(new Vector2(0, 360), s.Panel == 0 ? "Dock(Left)：贴左边 30，宽 260" : "再 Dock(Top)：贴上边 30，高 160", Ink, 30);
        }

        // ---------------- 案例 3.2 App 布局 ----------------
        static void AppLayout(Shot s, bool annotate)
        {
            s.Ref(1080, 1920, 0);
            var safe = UIKit.Node(s.Root, "SafeArea"); safe.Stretch();
            safe.gameObject.AddComponent<SafeArea>();

            // 顶栏：上边横向拉伸，高 180
            var top = UIKit.Image(safe, null, K.primary, "TopBar");
            top.rectTransform.anchorMin = new Vector2(0, 1); top.rectTransform.anchorMax = Vector2.one;
            top.rectTransform.pivot = new Vector2(0.5f, 1);
            top.rectTransform.anchoredPosition = Vector2.zero; top.rectTransform.sizeDelta = new Vector2(0, 180);
            var av = UIKit.Image(top.transform, "avatar_2", Color.white, "Avatar");
            av.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(110, 110));
            var title = UIKit.Text(top.transform, "每日任务", 56, Color.white, bold: true, name: "Title");
            title.rectTransform.Stretch(200, 0, 200, 0);
            var bell = UIKit.Icon(top.transform, "bell", 72, Color.white, "Bell");
            bell.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(-50, 0), new Vector2(72, 72));
            var dot = UIKit.Image(bell.transform, "circle", K.danger, "RedDot");
            dot.rectTransform.sizeDelta = new Vector2(30, 30);
            var cb = dot.gameObject.AddComponent<CornerBadge>(); cb.inset = new Vector2(4, 4); cb.Apply();

            // 底栏：下边横向拉伸，高 170；四个页签用锚点四等分
            var tab = UIKit.Image(safe, null, Color.white, "TabBar");
            tab.rectTransform.anchorMin = Vector2.zero; tab.rectTransform.anchorMax = new Vector2(1, 0);
            tab.rectTransform.pivot = new Vector2(0.5f, 0);
            tab.rectTransform.anchoredPosition = Vector2.zero; tab.rectTransform.sizeDelta = new Vector2(0, 170);
            string[] icons = { "home", "bag", "chat", "user" }; string[] names = { "首页", "背包", "消息", "我的" };
            for (int i = 0; i < 4; i++)
            {
                var cell = UIKit.Node(tab.transform, "Tab_" + names[i]);
                cell.anchorMin = new Vector2(i / 4f, 0); cell.anchorMax = new Vector2((i + 1) / 4f, 1);
                cell.offsetMin = cell.offsetMax = Vector2.zero;
                var col = i == 0 ? K.primary : K.muted;
                var ic = UIKit.Icon(cell, icons[i], 64, col);
                ic.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 22), new Vector2(64, 64));
                var tl = UIKit.Text(cell, names[i], 30, col, name: "Label");
                tl.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -42), new Vector2(200, 44));
            }

            // 内容区：四边拉伸，上下让出顶栏和底栏
            var content = UIKit.Node(safe, "Content");
            content.anchorMin = Vector2.zero; content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(0, 170); content.offsetMax = new Vector2(0, -180);
            content.gameObject.AddComponent<RectMask2D>();
            string[] tasks = { "登录游戏", "完成 3 场对战", "强化任意装备", "赠送好友体力", "参加公会活动", "观看一段广告", "分享战绩" };
            string[] ticons = { "star", "trophy", "item_sword", "heart", "user", "play", "gift" };
            for (int i = 0; i < tasks.Length; i++)
            {
                var card = UIKit.Card(content, "Task_" + i, 24);
                card.anchorMin = new Vector2(0, 1); card.anchorMax = Vector2.one; card.pivot = new Vector2(0.5f, 1);
                card.anchoredPosition = new Vector2(0, -40 - i * 230); card.sizeDelta = new Vector2(-80, 200);
                var icBg = UIKit.Image(card, "round_24", UIKit.Hex("#ECEAFB"), "IconBg");
                icBg.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(36, 0), new Vector2(128, 128));
                var ic = UIKit.Icon(icBg.transform, ticons[i], 76, ticons[i].StartsWith("item") ? Color.white : K.primary);
                ic.rectTransform.anchoredPosition = Vector2.zero;
                var tt = UIKit.Text(card, tasks[i], 44, Ink, TextAlignmentOptions.MidlineLeft, true, "Title");
                tt.rectTransform.anchorMin = new Vector2(0, 0.5f); tt.rectTransform.anchorMax = new Vector2(1, 1);
                tt.rectTransform.offsetMin = new Vector2(196, 0); tt.rectTransform.offsetMax = new Vector2(-240, -24);
                var st = UIKit.Text(card, $"奖励：金币 ×{(i + 1) * 50}", 32, K.muted, TextAlignmentOptions.MidlineLeft, name: "Reward");
                st.rectTransform.anchorMin = new Vector2(0, 0); st.rectTransform.anchorMax = new Vector2(1, 0.5f);
                st.rectTransform.offsetMin = new Vector2(196, 24); st.rectTransform.offsetMax = new Vector2(-240, 0);
                var go = UIKit.Button(card, i < 2 ? "领取" : "前往", i < 2 ? K.accent : K.primary, 180, 84, fontSize: 34);
                go.Rt().Place(new Vector2(1, 0.5f), new Vector2(-36, 0), new Vector2(180, 84));
            }
            var fab = UIKit.IconButton(content, "plus", K.accent, 150);
            fab.Rt().Place(new Vector2(1, 0), new Vector2(-50, 50), new Vector2(150, 150));

            if (annotate)
            {
                s.Badge(top.rectTransform, "1", new Vector2(0.5f, 0.0f), Orange);
                s.Badge(content, "2", new Vector2(0.5f, 0.5f), Orange);
                s.Badge(tab.rectTransform, "3", new Vector2(0.5f, 1f), Orange);
            }
        }

        [BookShot("ch03-app-916", W = 405, H = 720, Scene = "Ch03_AppLayout")]
        static void App916(Shot s) => AppLayout(s, true);
        [BookShot("ch03-app-1995", W = 405, H = 877)]
        static void App1995(Shot s) => AppLayout(s, false);
        [BookShot("ch03-app-34", W = 540, H = 720)]
        static void App34(Shot s) => AppLayout(s, false);

        // ---------------- 案例 3.3 Tooltip ----------------
        static readonly (string sprite, string text)[] Items =
        {
            ("item_sword", "<b>精钢长剑</b>\n攻击 +35\n<color=#9AA3BD>普通的铁匠铺出品，胜在结实。</color>"),
            ("item_shield", "<b>橡木盾</b>\n防御 +20"),
            ("item_potion_red", "<b>生命药水</b>\n立即恢复 200 点生命"),
            ("item_potion_blue", "<b>法力药水</b>\n立即恢复 120 点法力"),
            ("item_gem", "<b>星辉宝石</b>\n可镶嵌在武器上\n<color=#FFD27A>传说品质</color>"),
            ("item_coin", "<b>金币</b>\n通用货币"),
            ("item_apple", "<b>苹果</b>\n恢复 20 点体力"),
            ("item_key", "<b>古老的钥匙</b>\n似乎能打开某扇门……"),
            ("item_scroll", "<b>回城卷轴</b>\n读条 5 秒后回到主城"),
            ("item_bow", "<b>猎弓</b>\n攻击 +28  射程 +2"),
            ("item_helmet", "<b>铁盔</b>\n防御 +12"),
            ("item_ring", "<b>守护戒指</b>\n全抗性 +8%\n<color=#9AA3BD>戒圈内侧刻着一行小字。</color>"),
            ("item_potion_green", "<b>解毒药水</b>\n解除中毒状态"),
        };

        [BookShot("ch03-tooltip", W = 640, H = 400, Panels = 2, Scene = "Ch03_Tooltip")]
        static void TooltipShot(Shot s)
        {
            s.Ref(1280, 800, 0.5f);
            var hint = UIKit.Text(s.Root, "把鼠标移到物品上（试试最右下角那格）", 30, K.muted, name: "Hint");
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1000, 50));
            RectTransform center = null, corner = null;
            int cols = 7, rows = 4; float cell = 140, gap = 22;
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    int i = r * cols + c;
                    var slot = UIKit.Image(s.Root, "slot", Color.white, "Slot_" + i);
                    // 最后一格贴着屏幕右下角，专门用来测试“翻转 + 夹紧”
                    bool last = r == rows - 1 && c == cols - 1;
                    if (last) slot.rectTransform.Place(new Vector2(1, 0), new Vector2(-12, 12), new Vector2(cell, cell));
                    else slot.rectTransform.Place(-((cols - 1) * (cell + gap)) / 2 + c * (cell + gap) - 40, 200 - r * (cell + gap), cell, cell);
                    var it = Items[i % Items.Length];
                    if (i % 5 == 3 && !last) continue;   // 留几个空格子
                    var icon = UIKit.Icon(slot.transform, it.sprite, 100, Color.white);
                    icon.rectTransform.anchoredPosition = Vector2.zero;
                    var trig = slot.gameObject.AddComponent<TooltipTrigger>();
                    trig.message = it.text;
                    if (i == 9) center = slot.rectTransform;
                    if (last) corner = slot.rectTransform;
                }
            var panel = UIKit.Panel(s.Root, new Color(0.1f, 0.11f, 0.2f, 1f), 16, "TooltipPanel");
            panel.raycastTarget = false;   // 关键：提示框不能挡射线，否则会盖住鼠标下的格子、导致反复显示/隐藏
            var txt = UIKit.Text(panel.transform, "", 28, Color.white, TextAlignmentOptions.TopLeft, name: "Text");
            txt.rectTransform.Stretch(24, 24, 24, 24);
            txt.lineSpacing = 8;
            var tip = s.Root.gameObject.AddComponent<Tooltip>();
            Set(tip, "panel", panel.rectTransform); Set(tip, "text", txt);

            s.BeforeCapture(() =>
            {
                var target = s.Panel == 0 ? center : corner;
                var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, target.TransformPoint(target.rect.center));
                tip.Show(target.GetComponent<TooltipTrigger>().message);
                Canvas.ForceUpdateCanvases();
                tip.MoveTo(screen);
                var cursor = UIKit.Icon(target, "pointer", 60, Ink, "Cursor");
                cursor.rectTransform.anchorMin = cursor.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                cursor.rectTransform.anchoredPosition = new Vector2(16, -22);
                cursor.rectTransform.localEulerAngles = new Vector3(0, 0, 25);
            });
        }

        // ---------------- 案例 3.4 安全区 ----------------
        [BookShot("ch03-safearea", W = 600, H = 277, Panels = 2, Scale = 2, Scene = "Ch03_SafeArea")]
        static void SafeAreaShot(Shot s)
        {
            s.Ref(1920, 1080, 1f);
            var bg = UIKit.Image(s.Root, "bg_landscape", Color.white, "Background");
            bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 2;

            var safe = UIKit.Node(s.Root, "SafeArea"); safe.Stretch();
            var sa = safe.gameObject.AddComponent<SafeArea>();
            var hint = UIKit.Text(safe, "用 Game 视图的 Simulator 选一台刘海屏手机试试", 34, Color.white, name: "Hint");
            hint.rectTransform.Place(0, 60, 1200, 60);
            var av = UIKit.Image(safe, "circle", Color.white, "AvatarFrame");
            av.rectTransform.Place(new Vector2(0, 1), new Vector2(30, -30), new Vector2(140, 140));
            var avi = UIKit.Image(av.transform, "avatar_1", Color.white, "Avatar"); avi.rectTransform.Stretch(8, 8, 8, 8);
            var nm = UIKit.Text(safe, "麦冬  Lv.12", 40, Color.white, TextAlignmentOptions.MidlineLeft, true, "Name");
            nm.rectTransform.Place(new Vector2(0, 1), new Vector2(190, -70), new Vector2(380, 60));
            var gear = UIKit.IconButton(safe, "gear", new Color(0, 0, 0, 0.45f), 110);
            gear.Rt().Place(new Vector2(1, 1), new Vector2(-30, -30), new Vector2(110, 110));
            var joy = UIKit.Image(safe, "circle", new Color(1, 1, 1, 0.25f), "Joystick");
            joy.rectTransform.Place(new Vector2(0, 0), new Vector2(40, 40), new Vector2(300, 300));
            var knob = UIKit.Image(joy.transform, "circle", new Color(1, 1, 1, 0.8f), "Knob"); knob.rectTransform.Place(0, 0, 130, 130);
            var atk = UIKit.IconButton(safe, "item_sword", K.accent, 230, Color.white);
            atk.Rt().Place(new Vector2(1, 0), new Vector2(-40, 40), new Vector2(230, 230));

            if (s.Panel == 1)
            {
                var so = new SerializedObject(sa);
                so.FindProperty("simulate").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
                sa.Apply(true);
            }
            s.BeforeCapture(() =>
            {
                // 配图专用的“手机外观”：刘海、底部横条、不安全区域（不进场景）
                var dev = UIKit.Canvas("DeviceFrame", RenderMode.ScreenSpaceCamera, 1920, 1080, 1f);
                dev.worldCamera = s.Cam; dev.planeDistance = 40; dev.sortingOrder = 100;
                var dr = dev.transform;
                if (s.Panel == 1)
                {
                    foreach (var (a0, a1) in new[] { (new Vector2(0, 0), new Vector2(0.056f, 1)), (new Vector2(0.944f, 0), new Vector2(1, 1)), (new Vector2(0.056f, 0), new Vector2(0.944f, 0.054f)) })
                    {
                        var z = UIKit.Image(dr, null, new Color(0.9f, 0.28f, 0.3f, 0.35f), "Unsafe");
                        z.rectTransform.anchorMin = a0; z.rectTransform.anchorMax = a1; z.rectTransform.offsetMin = z.rectTransform.offsetMax = Vector2.zero;
                    }
                }
                var notch = UIKit.Image(dr, "pill", Color.black, "Notch");
                notch.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(-30, 0), new Vector2(110, 420));
                var home = UIKit.Image(dr, "pill", new Color(1, 1, 1, 0.85f), "HomeIndicator");
                home.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(560, 14));
            });
            s.Label(new Vector2(0, -90), s.Panel == 0 ? "不处理：头像、摇杆钻进刘海和圆角里" : "SafeArea：内容收进安全区（红色为不安全区域）", Ink, 40);
        }

        // ---------------- 案例 3.5 角标 ----------------
        [BookShot("ch03-badges", W = 960, H = 540, Scene = "Ch03_Badges")]
        static void Badges(Shot s)
        {
            var box = ParentBox(s.Root, "Box", Color.white);
            box.Place(-470, 40, 460, 380);
            for (int i = 0; i < 9; i++)
            {
                var d = UIKit.Image(box, "circle", K.danger, "Badge_" + (BadgeCorner)i);
                d.rectTransform.sizeDelta = new Vector2(56, 56);
                var b = d.gameObject.AddComponent<CornerBadge>(); b.corner = (BadgeCorner)i; b.inset = Vector2.zero; b.Apply();
                var t = UIKit.Text(d.transform, (i + 1).ToString(), 30, Color.white, bold: true); t.rectTransform.Stretch();
            }
            var slider = UIKit.Slider(s.Root, 0.65f, 460, 40);
            slider.Rt().Place(-470, -300, 460, 40);
            var sl = UIKit.Text(s.Root, "拖动改变方框大小", 30, K.muted, name: "SliderLabel");
            sl.rectTransform.Place(-470, -360, 460, 50);
            var rs = s.Root.gameObject.AddComponent<ResizeBySlider>();
            Set(rs, "target", box); Set(rs, "slider", slider);

            // 右侧：常见用法
            var mail = UIKit.IconButton(s.Root, "mail", K.primary, 150);
            mail.Rt().Place(130, 180, 150, 150);
            var dot = UIKit.Image(mail.transform, "circle", K.danger, "RedDot");
            dot.rectTransform.sizeDelta = new Vector2(40, 40);
            var b1 = dot.gameObject.AddComponent<CornerBadge>(); b1.inset = new Vector2(22, 22); b1.Apply();

            var bag = UIKit.IconButton(s.Root, "bag", K.primary, 150);
            bag.Rt().Place(400, 180, 150, 150);
            var num = UIKit.Image(bag.transform, "pill", K.danger, "Count");
            num.rectTransform.sizeDelta = new Vector2(84, 50);
            var b2 = num.gameObject.AddComponent<CornerBadge>(); b2.inset = new Vector2(10, 10); b2.Apply();
            var nt = UIKit.Text(num.transform, "99+", 30, Color.white, bold: true); nt.rectTransform.Stretch();

            var avf = UIKit.Image(s.Root, "round_24", Color.white, "AvatarCard");
            avf.rectTransform.Place(680, 180, 170, 170);
            var avi = UIKit.Image(avf.transform, "avatar_3", Color.white, "Avatar"); avi.rectTransform.Stretch(10, 10, 10, 10);
            var nw = UIKit.Image(avf.transform, "round_8", K.accent, "New");
            nw.rectTransform.sizeDelta = new Vector2(90, 46);
            var b3 = nw.gameObject.AddComponent<CornerBadge>(); b3.corner = BadgeCorner.TopLeft; b3.inset = new Vector2(30, 6); b3.Apply();
            var nwt = UIKit.Text(nw.transform, "NEW", 28, Color.white, bold: true); nwt.rectTransform.Stretch();

            var task = UIKit.Button(s.Root, "每日任务", Color.white, 640, 120, "trophy");
            task.Rt().Place(400, -60, 640, 120);
            var tdot = UIKit.Image(task.transform, "circle", K.danger, "RedDot");
            tdot.rectTransform.sizeDelta = new Vector2(30, 30);
            var b4 = tdot.gameObject.AddComponent<CornerBadge>(); b4.corner = BadgeCorner.Right; b4.inset = new Vector2(50, 0); b4.Apply();

            var cap = UIKit.Text(s.Root, "红点、数字（探出）、NEW、行尾红点：全是 CornerBadge", 28, K.muted, name: "Caption");
            cap.rectTransform.Place(400, -220, 900, 50);
        }

        // ---------------- 案例 3.6 坐标转换 ----------------
        [BookShot("ch03-coords", W = 960, H = 540, Scene = "Ch03_Coords")]
        static void Coords(Shot s)
        {
            var area = UIKit.Image(s.Root, "round_8", Color.white, "ClickArea");
            area.rectTransform.anchorMin = area.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            area.rectTransform.pivot = Vector2.zero;   // 故意把轴心放在左下角：本地坐标从左下角算起
            area.rectTransform.sizeDelta = new Vector2(1200, 780);
            area.rectTransform.anchoredPosition = new Vector2(-860, -400);
            var grid = UIKit.Image(area.transform, "checker", new Color(0.36f, 0.29f, 0.84f, 0.14f), "Grid");
            grid.type = Image.Type.Tiled; grid.rectTransform.Stretch(); grid.raycastTarget = false;

            var tpl = UIKit.Node(area.transform, "MarkerTemplate");
            tpl.sizeDelta = new Vector2(34, 34);
            var dotImg = UIKit.Image(tpl, "circle", K.accent, "Dot"); dotImg.rectTransform.Stretch(); dotImg.raycastTarget = false;
            var lab = UIKit.Text(tpl, "(0, 0)", 28, Ink, TextAlignmentOptions.MidlineLeft, true, "Coord");
            lab.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(12, 0), new Vector2(220, 40));
            lab.rectTransform.pivot = new Vector2(0, 0.5f);
            tpl.gameObject.SetActive(false);

            var info = UIKit.Panel(s.Root, K.dark, 16, "InfoPanel");
            info.rectTransform.Place(640, 160, 520, 260);
            var it = UIKit.Text(info.transform, "点击左侧区域", 32, Color.white, TextAlignmentOptions.TopLeft, name: "Info");
            it.rectTransform.Stretch(30, 30, 20, 20); it.lineSpacing = 16;
            var hint = UIKit.Text(s.Root, "点击白色区域任意位置放置标记。\n区域的轴心在左下角，\n所以本地坐标都是正数。", 30, K.muted, TextAlignmentOptions.TopLeft, name: "Hint");
            hint.rectTransform.Place(640, -140, 520, 200);

            var cm = area.gameObject.AddComponent<ClickMarker>();
            Set(cm, "area", area.rectTransform); Set(cm, "markerTemplate", tpl); Set(cm, "info", it);
            s.BeforeCapture(() =>
            {
                foreach (var p in new[] { new Vector2(180, 600), new Vector2(560, 300), new Vector2(900, 120) })
                {
                    var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, area.rectTransform.TransformPoint(p));
                    cm.Mark(screen, s.Cam);
                }
            });
            s.Anchors(area.rectTransform);
            s.LabelAt(area.rectTransform, Vector2.zero, new Vector2(130, 36), "轴心 = 原点", Blue, 26);
        }
    }
}
