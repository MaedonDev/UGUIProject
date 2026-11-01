// 第 1 章配图与配套场景的搭景代码（不进书）。
using TMPro;
using UGUIBook.Core;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch01
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;

        static TextMeshProUGUI Hint(Transform parent, string text, float y = 50, Color? color = null)
        {
            var t = UIKit.Text(parent, text, 32, color ?? K.muted, name: "Hint");
            t.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, y), new Vector2(1600, 50));
            return t;
        }

        // ---------------- 1.2 第一个界面 ----------------
        [BookShot("ch01-hello", W = 960, H = 540, Scene = "Ch01_Hello")]
        static void Hello(Shot s)
        {
            var title = UIKit.Text(s.Root, "Hello, UGUI!", 110, K.ink, bold: true, name: "Text (TMP)");
            title.rectTransform.Place(0, 130, 1000, 160);
            var b = UIKit.Button(s.Root, "点我", null, 400, 110, name: "Button");
            b.Rt().Place(0, -110, 400, 110);
            s.Box(title.rectTransform, null, "Text (TMP)");
            s.Box(b.Rt(), new Color32(0x5B, 0x4B, 0xD6, 0xFF), "Button", 6, true);
        }

        // ---------------- 案例 1.1 / 1.3 / 1.4 主菜单 ----------------
        class Menu
        {
            public RectTransform main, settings, dialog, buttons, title, version;
            public Image bg;
            public Button start, set, quit, back, yes, no;
            public TextMeshProUGUI hint;
        }

        static Menu BuildMenu(Shot s, bool flow)
        {
            s.Ref(1920, 1080, 0.5f);
            var m = new Menu();
            var root = s.Root;
            m.bg = UIKit.Image(root, "bg_landscape", Color.white, "Background");
            m.bg.rectTransform.Stretch();
            m.bg.raycastTarget = false;
            // 背景图是 2:1，直接拉伸会变形；Envelope Parent = 保持比例、铺满父节点（多出来的部分裁到屏幕外）
            var fit = m.bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 2f;
            var shade = UIKit.Image(root, null, new Color(0.08f, 0.09f, 0.2f, 0.45f), "Shade");
            shade.rectTransform.Stretch(); shade.raycastTarget = false;

            m.main = UIKit.Node(root, "MainPage"); m.main.Stretch();
            var tg = UIKit.Node(m.main, "Title");
            tg.Place(new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1100, 260));
            m.title = tg;
            var t1 = UIKit.Text(tg, "星辰远征", 150, Color.white, bold: true, name: "TitleText");
            t1.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(1100, 180));
            t1.characterSpacing = 12;
            var t2 = UIKit.Text(tg, "STAR  EXPEDITION", 40, new Color(1, 1, 1, 0.75f), name: "SubTitle");
            t2.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(1100, 60));
            t2.characterSpacing = 20;

            m.buttons = UIKit.Node(m.main, "Buttons");
            m.buttons.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(480, 400));
            var vl = m.buttons.VLayout(30, 0, TextAnchor.MiddleCenter);
            m.start = UIKit.Button(m.buttons, "开始游戏", null, 480, 112, "play");
            m.set = UIKit.Button(m.buttons, "设置", Color.white, 480, 112, "gear");
            m.quit = UIKit.Button(m.buttons, "退出", K.dark, 480, 112, "close");
            foreach (var b in new[] { m.start, m.set, m.quit })
            {
                var le = b.gameObject.AddComponent<LayoutElement>(); le.preferredHeight = 112;
            }

            var ver = UIKit.Text(root, "v1.0.0  ·  UGUIBook", 30, new Color(1, 1, 1, 0.7f), TextAlignmentOptions.MidlineLeft, name: "Version");
            ver.rectTransform.Place(new Vector2(0, 0), new Vector2(40, 30), new Vector2(500, 50));
            m.version = ver.rectTransform;

            if (!flow) return m;

            // 设置页
            m.settings = UIKit.Node(root, "SettingsPage"); m.settings.Stretch();
            var card = UIKit.Card(m.settings, "SettingsCard");
            card.Place(0, -20, 920, 700);
            var st = UIKit.Text(card, "设置", 60, K.ink, bold: true, name: "Title");
            st.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(600, 90));
            var t_music = UIKit.Toggle(card, "背景音乐", true, 600, 76);
            t_music.Rt().Place(new Vector2(0.5f, 1), new Vector2(0, -190), new Vector2(600, 76));
            var t_sfx = UIKit.Toggle(card, "音效", true, 600, 76);
            t_sfx.Rt().Place(new Vector2(0.5f, 1), new Vector2(0, -300), new Vector2(600, 76));
            var vlab = UIKit.Text(card, "音量", 38, K.ink, TextAlignmentOptions.MidlineLeft, name: "VolumeLabel");
            vlab.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -410), new Vector2(600, 60));
            var sl = UIKit.Slider(card, 0.7f, 600, 48);
            sl.Rt().Place(new Vector2(0.5f, 1), new Vector2(0, -490), new Vector2(600, 48));
            var tip = UIKit.Text(card, "按 Esc（安卓返回键）也能返回", 30, K.muted, name: "Tip");
            tip.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(700, 50));
            m.back = UIKit.IconButton(m.settings, "back", Color.white, 110);
            m.back.Rt().Place(new Vector2(0, 1), new Vector2(60, -60), new Vector2(110, 110));

            // 退出确认框：全屏半透明遮罩 + 卡片。遮罩开着 Raycast Target，挡住后面的菜单
            m.dialog = UIKit.Node(root, "QuitDialog"); m.dialog.Stretch();
            var mask = UIKit.Image(m.dialog, null, new Color(0, 0, 0, 0.6f), "Mask");
            mask.rectTransform.Stretch();
            var dc = UIKit.Card(m.dialog, "DialogCard", 24);
            dc.Place(0, 0, 780, 440);
            var q = UIKit.Text(dc, "确定要退出游戏吗？", 50, K.ink, bold: true, name: "Message");
            q.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(700, 80));
            var q2 = UIKit.Text(dc, "未保存的进度将会丢失", 34, K.muted, name: "Detail");
            q2.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -175), new Vector2(700, 60));
            m.no = UIKit.Button(dc, "取消", UIKit.Hex("#E6E8F0"), 300, 100, name: "Button_Cancel");
            m.no.Rt().Place(new Vector2(0.5f, 0), new Vector2(-170, 50), new Vector2(300, 100));
            m.yes = UIKit.Button(dc, "退出", K.danger, 300, 100, name: "Button_Quit");
            m.yes.Rt().Place(new Vector2(0.5f, 0), new Vector2(170, 50), new Vector2(300, 100));

            m.hint = UIKit.Text(root, "", 40, Color.white, bold: true, name: "Hint");
            m.hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1400, 60));
            m.hint.transform.SetSiblingIndex(m.main.GetSiblingIndex() + 1);

            var ctl = root.gameObject.AddComponent<MenuController>();
            var so = new UnityEditor.SerializedObject(ctl);
            so.FindProperty("mainPage").objectReferenceValue = m.main.gameObject;
            so.FindProperty("settingsPage").objectReferenceValue = m.settings.gameObject;
            so.FindProperty("quitDialog").objectReferenceValue = m.dialog.gameObject;
            so.FindProperty("startButton").objectReferenceValue = m.start;
            so.FindProperty("settingsButton").objectReferenceValue = m.set;
            so.FindProperty("quitButton").objectReferenceValue = m.quit;
            so.FindProperty("backButton").objectReferenceValue = m.back;
            so.FindProperty("quitYesButton").objectReferenceValue = m.yes;
            so.FindProperty("quitNoButton").objectReferenceValue = m.no;
            so.FindProperty("hint").objectReferenceValue = m.hint;
            so.ApplyModifiedPropertiesWithoutUndo();
            return m;
        }

        [BookShot("ch01-mainmenu", W = 960, H = 540, Scene = "Ch01_MainMenu")]
        static void MainMenu(Shot s)
        {
            var m = BuildMenu(s, false);
            var red = new Color32(0xFF, 0x8A, 0x3D, 0xFF);
            s.Badge(m.title, "2", new Vector2(0.18f, 0.75f), red);
            s.Badge(m.buttons, "3", new Vector2(-0.1f, 0.86f), red);
            s.Badge(m.version, "4", new Vector2(0.66f, 0.5f), red);
            s.Badge(s.Root, "1", new Vector2(0.04f, 0.92f), red);
        }

        [BookShot("ch01-menuflow", W = 640, H = 360, Panels = 3, Scene = "Ch01_MenuFlow")]
        static void MenuFlow(Shot s)
        {
            var m = BuildMenu(s, true);
            m.settings.gameObject.SetActive(s.Panel == 1);
            m.main.gameObject.SetActive(s.Panel != 1);
            m.dialog.gameObject.SetActive(s.Panel == 2);
            string[] cap = { "① 主菜单", "② 点“设置”：切到设置页", "③ 点“退出”：弹出确认框" };
            s.Label(new Vector2(0, -470), cap[s.Panel], new Color32(0xFF, 0x8A, 0x3D, 0xFF), 44);
        }

        // ---------------- 案例 1.2 点击计数器 ----------------
        [BookShot("ch01-counter", W = 960, H = 540, Scene = "Ch01_ClickCounter")]
        static void Counter(Shot s)
        {
            ClickCounter c1 = null; ClickCounterByCode c2 = null;
            for (int i = 0; i < 2; i++)
            {
                var card = UIKit.Card(s.Root, i == 0 ? "Card_Inspector" : "Card_Code");
                card.Place(i == 0 ? -430 : 430, 40, 780, 700);
                var h = UIKit.Text(card, i == 0 ? "方式一：Inspector 里绑定" : "方式二：代码 AddListener", 46, K.ink, bold: true, name: "Header");
                h.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(720, 70));
                var num = UIKit.Text(card, "0", 200, i == 0 ? K.primary : K.accent, bold: true, name: "Count");
                num.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(600, 260));
                var b1 = UIKit.Button(card, "+1", i == 0 ? K.primary : K.accent, 300, 110, name: "Button_Add");
                b1.Rt().Place(new Vector2(0.5f, 0), new Vector2(-170, 60), new Vector2(300, 110));
                var b2 = UIKit.Button(card, i == 0 ? "+5" : "清零", UIKit.Hex("#E6E8F0"), 300, 110, name: i == 0 ? "Button_Add5" : "Button_Reset");
                b2.Rt().Place(new Vector2(0.5f, 0), new Vector2(170, 60), new Vector2(300, 110));
                if (i == 0)
                {
                    c1 = card.gameObject.AddComponent<ClickCounter>();
                    var so = new UnityEditor.SerializedObject(c1);
                    so.FindProperty("label").objectReferenceValue = num;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    // 等价于在 Inspector 的 On Click () 里点“+”、拖入 Card、选择 ClickCounter.Increment
                    UnityEventTools.AddPersistentListener(b1.onClick, c1.Increment);
                    UnityEventTools.AddIntPersistentListener(b2.onClick, c1.Add, 5);
                }
                else
                {
                    c2 = card.gameObject.AddComponent<ClickCounterByCode>();
                    var so = new UnityEditor.SerializedObject(c2);
                    so.FindProperty("addButton").objectReferenceValue = b1;
                    so.FindProperty("resetButton").objectReferenceValue = b2;
                    so.FindProperty("label").objectReferenceValue = num;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            Hint(s.Root, "点按钮试试：两种绑定方式效果完全一样", 50);
            s.BeforeCapture(() => { c1.SetCount(3); c2.SetCount(7); });
        }

        // ---------------- 案例 1.5 绘制顺序 ----------------
        [BookShot("ch01-draworder", W = 640, H = 400, Panels = 2, Scene = "Ch01_DrawOrder")]
        static void DrawOrder(Shot s)
        {
            s.Ref(1280, 800, 0.5f);
            Color[] cols = { K.danger, K.accent, K.primary };
            string[] names = { "A", "B", "C" };
            var cards = new RectTransform[3];
            var group = UIKit.Node(s.Root, "Cards"); group.Stretch();
            for (int i = 0; i < 3; i++)
            {
                var c = UIKit.Card(group, "Card_" + names[i], 24, true, cols[i]);
                c.Place(-230 + i * 170, 90 - i * 90, 380, 280);
                var t = UIKit.Text(c, names[i], 110, Color.white, bold: true, name: "Letter");
                t.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -20), new Vector2(120, 140));
                t.alignment = TextAlignmentOptions.TopLeft;
                var sub = UIKit.Text(c, $"第 {i + 1} 个子节点", 30, new Color(1, 1, 1, 0.85f), TextAlignmentOptions.MidlineLeft, name: "Order");
                sub.rectTransform.Place(new Vector2(0, 0), new Vector2(40, 30), new Vector2(320, 50));
                c.gameObject.AddComponent<BringToFront>();
                cards[i] = c;
            }
            Hint(s.Root, "点任意一张卡片，它就会被提到最前面", 30);
            if (s.Panel == 1)
            {
                cards[0].SetAsLastSibling();
                foreach (var c in cards) c.Find("Order").GetComponent<TMP_Text>().text = $"第 {c.GetSiblingIndex() + 1} 个子节点";
            }
            s.Label(new Vector2(0, 340), s.Panel == 0 ? "层级：A、B、C（C 最后画，在最上面）" : "A.SetAsLastSibling() 之后：B、C、A",
                new Color32(0x1F, 0x24, 0x36, 0xFF), 36);
        }

        // ---------------- 案例 1.6 透明图片挡住点击 ----------------
        [BookShot("ch01-blocker", W = 960, H = 540, Scene = "Ch01_Pitfalls")]
        static void Blocker(Shot s)
        {
            var leftCard = UIKit.Card(s.Root, "Stage", 24);
            leftCard.Place(-450, 30, 820, 760);
            var btn = UIKit.Button(leftCard, "领取奖励", K.accent, 440, 120, "gift");
            btn.Rt().Place(0, 80, 440, 120);
            var result = UIKit.Text(leftCard, "按钮还没被点到过", 36, K.muted, name: "Result");
            result.rectTransform.Place(0, -100, 700, 60);
            var blocker = UIKit.Image(leftCard, null, new Color(1, 1, 1, 0), "Blocker");
            blocker.rectTransform.Place(0, 20, 700, 560);   // 透明，但 Raycast Target 开着

            var panel = UIKit.Panel(s.Root, K.dark, 24, "ProbePanel");
            panel.rectTransform.Place(430, 110, 760, 520);
            var outT = UIKit.Text(panel.transform, "", 34, Color.white, TextAlignmentOptions.TopLeft, name: "ProbeText");
            outT.rectTransform.Stretch(40, 40, 30, 30);
            outT.lineSpacing = 10;
            var probe = panel.gameObject.AddComponent<PointerProbe>();
            var so = new UnityEditor.SerializedObject(probe);
            so.FindProperty("output").objectReferenceValue = outT;
            so.ApplyModifiedPropertiesWithoutUndo();

            var tgl = UIKit.Toggle(s.Root, "Blocker 的 Raycast Target", true, 700, 76);
            tgl.Rt().Place(430, -260, 700, 76);
            var sw = s.Root.gameObject.AddComponent<BlockerSwitch>();
            so = new UnityEditor.SerializedObject(sw);
            so.FindProperty("blocker").objectReferenceValue = blocker;
            so.FindProperty("raycastToggle").objectReferenceValue = tgl;
            so.FindProperty("target").objectReferenceValue = btn;
            so.FindProperty("result").objectReferenceValue = result;
            so.ApplyModifiedPropertiesWithoutUndo();
            Hint(s.Root, "鼠标移到按钮上看右侧列表；取消勾选后再点按钮", 40);

            s.BeforeCapture(() =>
            {
                var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, btn.Rt().position);
                probe.Probe(screen);
                // 画一个鼠标指针（只在配图里）
                var p = UIKit.Icon(btn.transform, "pointer", 90, K.ink, "Cursor");
                p.rectTransform.anchoredPosition = new Vector2(40, -40);
                p.rectTransform.localEulerAngles = new Vector3(0, 0, 25);
            });
            s.Box(blocker.rectTransform, null, "Blocker：完全透明，但 Raycast Target 开着");
        }
    }
}
