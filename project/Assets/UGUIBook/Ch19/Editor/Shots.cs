// 第 19 章配图与配套场景
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch19
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;

        // ---------------------------------------------------------------- 通用：设备“屏幕框”
        /// 在 parent 上画一个设备外框，返回里面的“屏幕”节点：屏幕的逻辑尺寸是 w×h（相当于该设备上 Canvas 的尺寸），整体缩放 scale 显示
        static RectTransform Device(RectTransform parent, float w, float h, float scale, Vector2 pos, string name = "Device")
        {
            var bezel = UIKit.Panel(parent, UIKit.Hex("#1F2436"), 32, name).rectTransform;
            bezel.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(w * scale + 16, h * scale + 16));
            var screen = UIKit.Node(bezel, "Screen");
            screen.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h));
            screen.localScale = Vector3.one * scale;
            screen.gameObject.AddComponent<RectMask2D>();
            UIKit.Panel(screen, K.page, 0, "Bg").rectTransform.Stretch();
            return screen;
        }

        static AspectLayout.RectState St(Vector2 aMin, Vector2 aMax, Vector2 offMin, Vector2 offMax) => new AspectLayout.RectState
        {
            anchorMin = aMin, anchorMax = aMax, pivot = new Vector2(0.5f, 0.5f),
            anchoredPosition = (offMin + offMax) * 0.5f, sizeDelta = offMax - offMin,
        };

        // ---------------------------------------------------------------- 横竖屏布局
        /// 一个游戏大厅：顶栏 + 横幅 + 四个入口按钮。横屏时横幅在左、按钮在右；竖屏时横幅在上、按钮在下
        static AspectLayout Lobby(RectTransform screen)
        {
            var top = UIKit.Panel(screen, Color.white, 0, "TopBar").rectTransform;
            top.anchorMin = new Vector2(0, 1); top.anchorMax = Vector2.one; top.pivot = new Vector2(0.5f, 1);
            top.sizeDelta = new Vector2(0, 120); top.anchoredPosition = Vector2.zero;
            var av = UIKit.Image(top, "avatar_1", Color.white, "Avatar"); av.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(84, 84));
            var nm = UIKit.Text(top, "麦冬  Lv.23", 38, K.ink, TextAlignmentOptions.MidlineLeft, true, "Name");
            nm.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(134, 0), new Vector2(360, 60));
            var coin = UIKit.Panel(top, UIKit.Hex("#FFF1E6"), 0, "Coins").rectTransform; coin.GetComponent<Image>().sprite = UIKit.Sp("pill");
            coin.Place(new Vector2(1, 0.5f), new Vector2(-150, 0), new Vector2(260, 72));
            UIKit.Icon(coin, "item_coin", 56, Color.white).rectTransform.Place(new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(56, 56));
            UIKit.Text(coin, "12,800", 34, K.ink, TextAlignmentOptions.MidlineRight, true).rectTransform.Stretch(70, 0, 24, 0);
            var gear = UIKit.IconButton(top, "gear", UIKit.Hex("#EEF0F6"), 84, K.ink); gear.GetComponent<RectTransform>().Place(new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(84, 84));

            // 横幅：外框裁剪 + 里面的图用 AspectRatioFitter(EnvelopeParent) 保持 2:1 不变形，多出来的部分被裁掉
            var banner = UIKit.Panel(screen, UIKit.Hex("#24283B"), 0, "Banner");
            banner.gameObject.AddComponent<RectMask2D>();
            var art = UIKit.Image(banner.transform, "bg_landscape", Color.white, "Art");
            art.raycastTarget = false;
            var arf = art.gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; arf.aspectRatio = 2f;
            var title = UIKit.Text(banner.transform, "第 3 章 · 迷雾森林", 64, Color.white, TextAlignmentOptions.BottomLeft, true, "Title");
            title.rectTransform.Stretch(40, 0, 40, 36);
            var shade = UIKit.Image(banner.transform, "gradient_v", new Color(0, 0, 0, 0.45f), "Shade");
            shade.rectTransform.Stretch(); shade.transform.SetSiblingIndex(1); shade.raycastTarget = false;

            var menu = UIKit.Node(screen, "Menu");
            var grid = menu.Grid(new Vector2(460, 130), new Vector2(24, 24));
            grid.childAlignment = TextAnchor.MiddleCenter;
            string[] labels = { "开始游戏", "商店", "背包", "设置" };
            string[] icons = { "play", "bag", "gift", "gear" };
            Color[] cols = { K.primary, K.accent, UIKit.Hex("#2FB36A"), UIKit.Hex("#7A8299") };
            for (int i = 0; i < 4; i++) UIKit.Button(menu, labels[i], cols[i], 460, 130, icons[i], 44);

            var al = screen.gameObject.AddComponent<AspectLayout>();
            al.landscapeOnly = new GameObject[0]; al.portraitOnly = new GameObject[0];
            al.entries.Add(new AspectLayout.Entry
            {
                target = banner.rectTransform,
                landscape = St(Vector2.zero, new Vector2(0.6f, 1), new Vector2(40, 40), new Vector2(-12, -150)),
                portrait = St(new Vector2(0, 0.5f), Vector2.one, new Vector2(40, 0), new Vector2(-40, -150)),
            });
            al.entries.Add(new AspectLayout.Entry
            {
                target = menu,
                landscape = St(new Vector2(0.6f, 0), Vector2.one, new Vector2(12, 40), new Vector2(-40, -150)),
                portrait = St(Vector2.zero, new Vector2(1, 0.5f), new Vector2(40, 40), new Vector2(-40, -30)),
            });
            return al;
        }

        [BookShot("ch19-aspect-land", W = 960, H = 220)]
        static void AspectLand(Shot s)
        {
            float sc = 0.27f;
            (float w, string label)[] devs = { (2520, "21:9 带鱼屏"), (1920, "16:9"), (1440, "4:3 平板横放") };
            float x = -(2520 + 1920 + 1440) * sc / 2 - 60;
            foreach (var d in devs)
            {
                float pw = d.w * sc;
                var scr = Device(s.Root, d.w, 1080, sc, new Vector2(x + pw / 2, 34), "Device_" + d.label);
                Lobby(scr).Refresh(true);
                s.Label(new Vector2(x + pw / 2, -175), d.label, K.ink, 26);
                x += pw + 60;
            }
            s.Background(Color.white);
        }

        [BookShot("ch19-aspect-portrait", W = 960, H = 540)]
        static void AspectPortrait(Shot s)
        {
            float sc = 0.43f;
            var a = Device(s.Root, 1080, 1920, sc, new Vector2(-330, 30), "Device_NoAdapt");
            var la = Lobby(a); la.Apply(AspectLayout.Mode.Landscape); la.enabled = false;
            var b = Device(s.Root, 1080, 1920, sc, new Vector2(330, 30), "Device_Adapt");
            Lobby(b).Refresh(true);
            s.Label(new Vector2(-330, -505), "未处理：横屏布局硬塞进竖屏", UIKit.Hex("#E5484D"), 28);
            s.Label(new Vector2(330, -505), "AspectLayout：自动切到竖屏布局", UIKit.Hex("#2FB36A"), 28);
            s.Background(Color.white);
        }

        [BookScene("Ch19_Responsive")]
        static void ResponsiveScene(Shot s)
        {
            var screen = UIKit.Node(s.Root, "Screen").Stretch();
            UIKit.Panel(screen, K.page, 0, "Bg").rectTransform.Stretch();
            var al = Lobby(screen);
            al.previewInEditMode = false;
            var hint = UIKit.Text(screen, "把 Game 视图切到竖屏分辨率（如 1080×1920）试试", 30, K.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 8), new Vector2(1400, 40));
            al.Refresh(true);
        }

        // ---------------------------------------------------------------- 安全区
        static readonly Vector4 InsetPortrait = new Vector4(0, 0.04f, 0, 0.068f);
        static readonly Vector4 InsetLandscape = new Vector4(0.068f, 0.055f, 0.068f, 0);

        /// 一个“背包”页面：顶栏背景向上多画一截（延伸进刘海），内容放在安全区里
        static SafeAreaPanel SafeApp(RectTransform screen, bool landscape)
        {
            var safe = UIKit.Node(screen, "SafeArea").Stretch();
            var sa = safe.gameObject.AddComponent<SafeAreaPanel>();
            var bar = UIKit.Node(safe, "TopBar");
            bar.anchorMin = new Vector2(0, 1); bar.anchorMax = Vector2.one; bar.pivot = new Vector2(0.5f, 1);
            bar.sizeDelta = new Vector2(0, 130); bar.anchoredPosition = Vector2.zero;
            var bleed = UIKit.Panel(bar, K.primary, 0, "BarBg").rectTransform;
            bleed.Stretch(-400, -400, -400, 0);            // 左、上、右各多出 400：刘海那块也是顶栏的颜色
            var back = UIKit.Icon(bar, "back", 56, Color.white); back.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(56, 56));
            var t = UIKit.Text(bar, "背包", 50, Color.white, TextAlignmentOptions.Center, true); t.rectTransform.Stretch(120, 0, 120, 0);
            var content = UIKit.Node(safe, "Content").Stretch(30, 160, 30, 170);
            content.Grid(new Vector2(170, 170), new Vector2(24, 24)).childAlignment = TextAnchor.UpperCenter;
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_gem", "item_coin", "item_apple", "item_key", "item_scroll", "item_bow", "item_helmet", "item_ring", "item_potion_blue" };
            for (int i = 0; i < (landscape ? 12 : 12); i++)
            {
                var slot = UIKit.Image(content, "slot", Color.white, "Slot");
                UIKit.Icon(slot.transform, items[i], 120, Color.white).rectTransform.anchoredPosition = Vector2.zero;
            }
            var tab = UIKit.Panel(safe, Color.white, 0, "TabBar").rectTransform;
            tab.anchorMin = Vector2.zero; tab.anchorMax = new Vector2(1, 0); tab.pivot = new Vector2(0.5f, 0);
            tab.sizeDelta = new Vector2(0, 140); tab.anchoredPosition = Vector2.zero;
            var tabBleed = UIKit.Panel(tab, Color.white, 0, "TabBleed").rectTransform; tabBleed.Stretch(-400, 0, -400, -400); tabBleed.SetAsFirstSibling();
            var row = UIKit.Node(tab, "Icons").Stretch(); row.HLayout(landscape ? 220 : 130);
            foreach (var ic in new[] { "home", "bag", "trophy", "user" })
                UIKit.Icon(row, ic, 72, ic == "bag" ? K.primary : K.muted);
            return sa;
        }

        static void Notch(RectTransform screen, bool landscape)
        {
            var n = UIKit.Panel(screen, Color.black, 0, "Notch").rectTransform;
            n.GetComponent<Image>().sprite = UIKit.Sp("pill");
            if (!landscape) n.Place(new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(330, 96));
            else n.Place(new Vector2(0, 0.5f), new Vector2(22, 0), new Vector2(96, 330));
            var h = UIKit.Panel(screen, UIKit.Hex("#1F2436"), 0, "HomeIndicator").rectTransform;
            h.GetComponent<Image>().sprite = UIKit.Sp("pill");
            h.Place(new Vector2(0.5f, 0), new Vector2(0, 22), new Vector2(landscape ? 520 : 380, 14));
        }

        [BookShot("ch19-safearea", W = 960, H = 540)]
        static void SafeArea(Shot s)
        {
            float sc = 0.4f;
            var a = Device(s.Root, 1080, 2340, sc, new Vector2(-735, 30), "Phone_NoSafe");
            SafeApp(a, false).enabled = false; Notch(a, false);
            var b = Device(s.Root, 1080, 2340, sc, new Vector2(-255, 30), "Phone_Safe");
            SafeApp(b, false).ApplyInsets(InsetPortrait); Notch(b, false);
            var c = Device(s.Root, 2340, 1080, sc, new Vector2(445, 10), "Phone_Landscape");
            var sc3 = SafeApp(c, true); sc3.ApplyInsets(InsetLandscape); Notch(c, true);
            s.Label(new Vector2(-735, -490), "未适配：顶栏被刘海挡住", UIKit.Hex("#E5484D"), 24);
            s.Label(new Vector2(-255, -490), "SafeAreaPanel", UIKit.Hex("#2FB36A"), 24);
            s.Label(new Vector2(445, -300), "横屏：左右都让开，底部让开 Home 条", UIKit.Hex("#2FB36A"), 24);
            s.Box((RectTransform)c.Find("SafeArea"), UIKit.Hex("#FF8A3D"), "安全区", 2);
            s.Background(Color.white);
        }

        [BookScene("Ch19_SafeArea")]
        static void SafeAreaScene(Shot s)
        {
            s.Ref(1080, 2340, 0.5f);
            UIKit.Panel(s.Root, K.page, 0, "Bg").rectTransform.Stretch();
            var sa = SafeApp(s.Root, false);
            sa.simulateInEditor = true;
            sa.simulatedInsets = InsetPortrait;
            var hint = UIKit.Text(s.Root, "Inspector 里取消 Simulate In Editor，再用 Device Simulator 选一台刘海屏手机试试", 28, K.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(1000, 120));
            hint.enableWordWrapping = true;
        }

        // ---------------------------------------------------------------- 多语言
        static LocFontSet EnsureFontSet()
        {
            const string path = "Assets/UGUIBook/Ch19/Resources/Ch19/LocFontSet.asset";
            var set = AssetDatabase.LoadAssetAtPath<LocFontSet>(path);
            if (set == null) { set = ScriptableObject.CreateInstance<LocFontSet>(); AssetDatabase.CreateAsset(set, path); }
            set.entries = new List<LocFontSet.Entry>
            {
                new LocFontSet.Entry { language = "zh", font = K.font, sizeScale = 1f },
                // 配套工程只带了一款字体（思源黑体已含拉丁字母）；实际项目这里换成你的英文字体
                new LocFontSet.Entry { language = "en", font = K.font, sizeScale = 1.06f },
                new LocFontSet.Entry { language = "de", font = K.font, sizeScale = 1.06f },
            };
            EditorUtility.SetDirty(set);
            AssetDatabase.SaveAssets();
            return set;
        }

        static TextMeshProUGUI LocText(Transform parent, string key, float size, Color color, TextAlignmentOptions align, bool bold = false)
        {
            var t = UIKit.Text(parent, Loc.Get(key), size, color, align, bold, "Text_" + key);
            var lt = t.gameObject.AddComponent<LocalizedText>();
            lt.Key = key;
            return t;
        }

        /// 设置面板：所有文字都挂 LocalizedText
        static RectTransform SettingsPanel(RectTransform root, float w, float h, out LanguageButtons langs)
        {
            var card = UIKit.Card(root, "Settings");
            card.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(w, h));
            var title = LocText(card, "settings.title", 52, K.ink, TextAlignmentOptions.Center, true);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(w - 80, 70));
            string[] keys = { "settings.music", "settings.sfx", "settings.notify", "settings.autosave", "settings.language" };
            for (int i = 0; i < keys.Length; i++)
            {
                float y = -140 - i * 110;
                var label = LocText(card, keys[i], 38, K.ink, TextAlignmentOptions.MidlineLeft);
                label.rectTransform.Place(new Vector2(0, 1), new Vector2(50, y), new Vector2(380, 70));
                label.enableAutoSizing = true; label.fontSizeMin = 22; label.fontSizeMax = 38;
                label.enableWordWrapping = false;
                var right = UIKit.Node(card, "Control");
                right.Place(new Vector2(1, 1), new Vector2(-50, y), new Vector2(w - 520, 70));
                right.pivot = new Vector2(1, 0.5f); right.anchoredPosition = new Vector2(-50, y - 35);
                if (i < 2) { var sl = UIKit.Slider(right, i == 0 ? 0.7f : 0.45f, w - 520, 40); sl.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; }
                else if (i < 4)
                {
                    var tg = UIKit.Toggle(right, "", i == 2, 64, 64);
                    tg.GetComponent<RectTransform>().Place(new Vector2(1, 0.5f), Vector2.zero, new Vector2(64, 64));
                    Object.DestroyImmediate(tg.transform.Find("Label").gameObject);
                }
            }
            // 语言按钮
            var row = UIKit.Node(card, "LangRow");
            row.Place(new Vector2(1, 1), new Vector2(-50, -140 - 4 * 110 - 35), new Vector2(w - 520, 70));
            row.pivot = new Vector2(1, 0.5f); row.anchoredPosition = new Vector2(-50, -140 - 4 * 110 - 35);
            row.HLayout(12, 0, TextAnchor.MiddleRight);
            langs = row.gameObject.AddComponent<LanguageButtons>();
            var items = new List<LanguageButtons.Item>();
            foreach (var (code, name) in new[] { ("zh", "中文"), ("en", "EN"), ("de", "DE") })
            {
                var b = UIKit.Button(row, name, UIKit.Hex("#E1E4EE"), 110, 64, null, 28);
                items.Add(new LanguageButtons.Item { language = code, button = b, background = b.GetComponent<Image>() });
            }
            langs.items = items.ToArray();
            // 底部按钮
            var ok = UIKit.Button(card, "OK", K.primary, 300, 96, null, 38, "Button_OK");
            ok.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(170, 40), new Vector2(300, 96));
            ReplaceWithLoc(ok, "dialog.ok");
            var cancel = UIKit.Button(card, "Cancel", UIKit.Hex("#E1E4EE"), 300, 96, null, 38, "Button_Cancel");
            cancel.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(-170, 40), new Vector2(300, 96));
            ReplaceWithLoc(cancel, "dialog.cancel");
            return card;
        }

        static void ReplaceWithLoc(Button b, string key)
        {
            var t = b.GetComponentInChildren<TextMeshProUGUI>();
            t.gameObject.AddComponent<LocalizedText>().Key = key;
        }

        static void RefreshAll(Transform root)
        {
            foreach (var lt in root.GetComponentsInChildren<LocalizedText>(true)) lt.Refresh();
            foreach (var lb in root.GetComponentsInChildren<LanguageButtons>(true))
                foreach (var it in lb.items) { it.background.color = it.language == Loc.Language ? lb.selected : lb.normal; it.button.GetComponentInChildren<TMP_Text>().color = it.language == Loc.Language ? Color.white : K.ink; }
        }

        [BookShot("ch19-loc-langs", W = 640, H = 480, Panels = 3, Scene = "Ch19_Localization")]
        static void LocLangs(Shot s)
        {
            EnsureFontSet();
            Loc.EnsureLoaded();
            Loc.SetLanguage(new[] { "zh", "en", "de" }[s.Panel]);
            SettingsPanel(s.Root, 1120, 860, out _);
            if (s.Panel == 0)
            {
                var hint = LocText(s.Root, "hint.switch", 26, K.muted, TextAlignmentOptions.Center);
                hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 2), new Vector2(1100, 40));
            }
            RefreshAll(s.Root);
            s.BeforeCapture(() => RefreshAll(s.Root));
        }

        // 同一个按钮在三种语言下，三种对付“文字变长”的方法
        [BookShot("ch19-loc-length", W = 640, H = 380, Panels = 3)]
        static void LocLength(Shot s)
        {
            string[] strategy = { "① 什么都不做：溢出", "② Auto Size：自动缩小", "③ 按钮跟着文字变宽" };
            var head = UIKit.Text(s.Root, strategy[s.Panel], 44, Color.white, TextAlignmentOptions.Center, true);
            head.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1200, 70));
            string[] langs = { "zh", "en", "de" };
            for (int i = 0; i < 3; i++)
            {
                Loc.SetLanguage(langs[i]);
                float y = 130 - i * 160;
                var tag = UIKit.Text(s.Root, langs[i].ToUpper(), 30, UIKit.Hex("#B8BED3"), TextAlignmentOptions.MidlineLeft, true);
                tag.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-520, y), new Vector2(80, 50));
                var holder = UIKit.Node(s.Root, "Holder_" + langs[i]);
                holder.Place(new Vector2(0, 0.5f), new Vector2(0, y), new Vector2(0, 120));
                holder.anchorMin = new Vector2(0.5f, 0.5f); holder.anchorMax = new Vector2(0.5f, 0.5f); holder.pivot = new Vector2(0, 0.5f);
                holder.anchoredPosition = new Vector2(-440, y); holder.sizeDelta = new Vector2(900, 120);
                var b = UIKit.Button(holder, Loc.Get("reward.claim"), K.accent, 380, 110, "gift", 40);
                var brt = b.GetComponent<RectTransform>();
                brt.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(380, 110));
                var t = b.GetComponentInChildren<TextMeshProUGUI>();
                t.enableWordWrapping = false;
                t.alignment = TextAlignmentOptions.MidlineLeft;
                t.rectTransform.offsetMin = new Vector2(104, 0);   // 给左侧图标留出位置
                if (s.Panel == 0) t.overflowMode = TextOverflowModes.Overflow;
                if (s.Panel == 1) { t.enableAutoSizing = true; t.fontSizeMin = 20; t.fontSizeMax = 40; }
                if (s.Panel == 2)
                {
                    // 水平布局 + ContentSizeFitter：按钮宽度 = 左边距 + 文字宽 + 右边距，再用 LayoutElement 保底 380
                    var hg = b.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hg.padding = new RectOffset(104, 36, 0, 0);
                    hg.childControlWidth = hg.childControlHeight = true; hg.childForceExpandWidth = false;
                    hg.childAlignment = TextAnchor.MiddleLeft;
                    foreach (Transform ch in b.transform) if (ch.GetComponent<TMP_Text>() == null) ch.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
                    var fit = b.gameObject.AddComponent<ContentSizeFitter>(); fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    b.gameObject.AddComponent<LayoutElement>().minWidth = 380;
                }
                if (s.Panel == 0 && i == 2) s.Box(brt, null, null, 4);
            }
            Loc.SetLanguage("zh");
            s.Background(UIKit.Hex("#2A3047"));   // 深色背景：溢出到按钮外的白字才看得见
        }

        [BookShot("ch19-loc-pseudo", W = 960, H = 330)]
        static void LocPseudo(Shot s)
        {
            Loc.EnsureLoaded();
            Loc.SetLanguage(Loc.Pseudo);
            var card = UIKit.Card(s.Root, "Menu"); card.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700, 560));
            string[] keys = { "menu.start", "menu.continue", "settings.notify", "menu.quit" };
            for (int i = 0; i < 4; i++)
            {
                var b = UIKit.Button(card, "", i == 0 ? K.primary : UIKit.Hex("#E1E4EE"), 380, 110, null, 36);
                b.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0.5f), new Vector2(-615 + i * 410, 120), new Vector2(380, 110));
                var t = b.GetComponentInChildren<TextMeshProUGUI>(); t.enableWordWrapping = false;
                t.gameObject.AddComponent<LocalizedText>().Key = keys[i];
            }
            var hard = UIKit.Text(card, "音量", 40, K.ink, TextAlignmentOptions.MidlineLeft, false, "HardCoded");
            hard.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-560, -90), new Vector2(300, 70));
            var miss = UIKit.Text(card, "", 40, K.ink, TextAlignmentOptions.MidlineLeft, false, "MissingKey");
            miss.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(200, -90), new Vector2(700, 70));
            miss.gameObject.AddComponent<LocalizedText>().Key = "settings.vibrate";
            RefreshAll(card);
            Loc.SetLanguage("zh");
            s.LabelAt(hard.rectTransform, new Vector2(0.5f, 0), new Vector2(0, -60), "写死的文字：没变", UIKit.Hex("#E5484D"), 26);
            s.LabelAt(miss.rectTransform, new Vector2(0.3f, 0), new Vector2(0, -60), "表里缺这个 key", UIKit.Hex("#E5484D"), 26);
            s.Background(Color.white);
        }

        // ---------------------------------------------------------------- RenderTexture 角色展示
        static Material Mat(Shot s, string name, Color c, float gloss = 0.4f, Color? emission = null)
        {
            var m = s.MatAsset("Ch19", name, "Standard");
            m.color = c;
            m.SetFloat("_Glossiness", gloss);
            if (emission.HasValue) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emission.Value); m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None; }
            return m;
        }

        static GameObject Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material m, string name, Vector3? euler = null)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            go.transform.localEulerAngles = euler ?? Vector3.zero;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        /// 用基本体拼一个小机器人，放在远离 UI 的地方（x = 100），由专用相机拍
        static (Transform model, Camera cam) Robot(Shot s)
        {
            var stage = new GameObject("ModelStage").transform;
            stage.position = new Vector3(100, 0, 0);
            var model = new GameObject("Robot").transform; model.SetParent(stage, false);
            var body = Mat(s, "RobotBody", K.primary, 0.55f);
            var white = Mat(s, "RobotWhite", new Color(0.95f, 0.95f, 0.97f), 0.6f);
            var dark = Mat(s, "RobotDark", UIKit.Hex("#1F2436"), 0.8f);
            var orange = Mat(s, "RobotOrange", K.accent, 0.5f, K.accent * 0.6f);
            var eye = Mat(s, "RobotEye", new Color(0.4f, 0.95f, 1f), 0.9f, new Color(0.3f, 0.9f, 1f));
            Part(stage, PrimitiveType.Cylinder, new Vector3(0, 0.02f, 0), new Vector3(1.5f, 0.02f, 1.5f), dark, "Platform");
            Part(model, PrimitiveType.Capsule, new Vector3(-0.2f, 0.36f, 0), new Vector3(0.24f, 0.34f, 0.24f), white, "LegL");
            Part(model, PrimitiveType.Capsule, new Vector3(0.2f, 0.36f, 0), new Vector3(0.24f, 0.34f, 0.24f), white, "LegR");
            Part(model, PrimitiveType.Capsule, new Vector3(0, 1.0f, 0), new Vector3(0.78f, 0.42f, 0.6f), body, "Body");
            Part(model, PrimitiveType.Cube, new Vector3(0, 1.05f, -0.29f), new Vector3(0.22f, 0.22f, 0.06f), orange, "Badge", new Vector3(0, 0, 45));
            Part(model, PrimitiveType.Capsule, new Vector3(-0.5f, 1.02f, 0), new Vector3(0.18f, 0.3f, 0.18f), white, "ArmL", new Vector3(0, 0, -18));
            Part(model, PrimitiveType.Capsule, new Vector3(0.5f, 1.02f, 0), new Vector3(0.18f, 0.3f, 0.18f), white, "ArmR", new Vector3(0, 0, 18));
            Part(model, PrimitiveType.Sphere, new Vector3(0, 1.68f, 0), new Vector3(0.62f, 0.56f, 0.58f), white, "Head");
            Part(model, PrimitiveType.Cube, new Vector3(0, 1.69f, -0.25f), new Vector3(0.44f, 0.18f, 0.12f), dark, "Visor");
            Part(model, PrimitiveType.Sphere, new Vector3(-0.1f, 1.69f, -0.31f), Vector3.one * 0.08f, eye, "EyeL");
            Part(model, PrimitiveType.Sphere, new Vector3(0.1f, 1.69f, -0.31f), Vector3.one * 0.08f, eye, "EyeR");
            Part(model, PrimitiveType.Cylinder, new Vector3(0, 2.02f, 0), new Vector3(0.03f, 0.1f, 0.03f), dark, "Antenna");
            Part(model, PrimitiveType.Sphere, new Vector3(0, 2.14f, 0), Vector3.one * 0.12f, orange, "AntennaTip");

            var cam = new GameObject("ModelCamera").AddComponent<Camera>();
            cam.transform.SetParent(stage, false);
            cam.transform.localPosition = new Vector3(0, 1.25f, -4.6f);
            cam.transform.LookAt(stage.position + new Vector3(0, 1.08f, 0));
            cam.fieldOfView = 30; cam.nearClipPlane = 0.1f; cam.farClipPlane = 20;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(0, 0, 0, 0);
            cam.enabled = false;            // 由 ModelViewer 在界面显示时打开
            var light = new GameObject("KeyLight").AddComponent<Light>();
            light.transform.SetParent(stage, false);
            light.type = LightType.Directional; light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(35, -35, 0);
            return (model, cam);
        }

        static ModelViewer ModelScreen(Shot s, out RawImage raw)
        {
            var (model, cam) = Robot(s);
            var left = UIKit.Panel(s.Root, UIKit.Hex("#24283B"), 0, "Stage").rectTransform;
            left.anchorMin = Vector2.zero; left.anchorMax = new Vector2(0.56f, 1); left.offsetMin = left.offsetMax = Vector2.zero;
            var glow = UIKit.Image(left, "circle_soft", new Color(0.36f, 0.29f, 0.84f, 0.55f), "Glow");
            glow.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(900, 900)); glow.raycastTarget = false;
            raw = new GameObject("ModelView", typeof(RectTransform)).AddComponent<RawImage>();
            raw.rectTransform.SetParent(left, false);
            raw.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(860, 960));
            raw.material = s.MatAsset("Ch19", "UIPremultiplied", "UGUIBook/Ch19/UIPremultiplied");
            var viewer = raw.gameObject.AddComponent<ModelViewer>();
            viewer.modelCamera = cam; viewer.target = model;
            var hint = UIKit.Text(left, "← 拖动旋转 · 滚轮缩放 →", 30, new Color(1, 1, 1, 0.6f));
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(800, 50));

            var right = UIKit.Card(s.Root, "Info");
            right.anchorMin = new Vector2(0.56f, 0); right.anchorMax = Vector2.one; right.offsetMin = new Vector2(50, 60); right.offsetMax = new Vector2(-60, -60);
            var name = UIKit.Text(right, "机甲 · 小麦", 64, K.ink, TextAlignmentOptions.TopLeft, true);
            name.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -50), new Vector2(600, 90));
            var lv = UIKit.Text(right, "Lv.23  稀有度 SR", 32, K.muted, TextAlignmentOptions.TopLeft);
            lv.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -140), new Vector2(600, 50));
            string[] stats = { "攻击", "防御", "速度", "能量" };
            float[] vals = { 0.82f, 0.55f, 0.68f, 0.4f };
            for (int i = 0; i < 4; i++)
            {
                float y = -250 - i * 110;
                var l = UIKit.Text(right, stats[i], 34, K.ink, TextAlignmentOptions.MidlineLeft);
                l.rectTransform.Place(new Vector2(0, 1), new Vector2(50, y), new Vector2(120, 60));
                var sl = UIKit.Slider(right, vals[i], 480, 36, i == 0 ? K.danger : i == 1 ? K.primary : i == 2 ? K.success : K.accent);
                sl.interactable = false;
                Object.DestroyImmediate(sl.handleRect.parent.gameObject);
                sl.GetComponent<RectTransform>().Place(new Vector2(0, 1), new Vector2(190, y - 12), new Vector2(480, 36));
            }
            var up = UIKit.Button(right, "升级", K.primary, 300, 100, "arrow_up", 40);
            up.GetComponent<RectTransform>().Place(new Vector2(0, 0), new Vector2(50, 50), new Vector2(300, 100));
            var eq = UIKit.Button(right, "装备", UIKit.Hex("#E1E4EE"), 300, 100, "item_sword", 40);
            eq.GetComponent<RectTransform>().Place(new Vector2(0, 0), new Vector2(380, 50), new Vector2(300, 100));
            return viewer;
        }

        [BookShot("ch19-model", W = 960, H = 540, Scene = "Ch19_ModelViewer")]
        static void Model(Shot s)
        {
            var viewer = ModelScreen(s, out _);
            s.BeforeCapture(() =>
            {
                viewer.Rotate(-28);
                viewer.CreateTexture();
                viewer.RenderNow();
                viewer.Rotate(28);       // 场景里保持正面朝向（场景在搭景后就存了，这里只影响配图）
            });
        }

        [BookShot("ch19-model-edge", W = 480, H = 360, Panels = 2)]
        static void ModelEdge(Shot s)
        {
            var viewer = ModelScreen(s, out var raw);
            var stage = raw.transform.parent.GetComponent<Image>();
            stage.color = UIKit.Hex("#F4E9D8");            // 浅色背景：边缘发黑最明显
            raw.transform.parent.Find("Glow").gameObject.SetActive(false);
            raw.transform.parent.Find("Text").gameObject.SetActive(false);
            raw.transform.parent.SetAsLastSibling();
            var rt = (RectTransform)raw.transform.parent;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            raw.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 960));
            if (s.Panel == 0) raw.material = null;          // UI/Default
            var lbl = UIKit.Text(rt, s.Panel == 0 ? "UI/Default：边缘发黑" : "预乘 Shader：边缘干净", 40, K.ink, TextAlignmentOptions.Center, true);
            lbl.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(900, 60));
            s.BeforeCapture(() =>
            {
                viewer.CreateTexture();
                // 放大看头部和天线：只显示 RT 的一部分
                raw.uvRect = new Rect(0.38f, 0.76f, 0.2f, 0.12f);
                raw.texture.filterMode = FilterMode.Point;       // 放大后按像素显示，边缘的暗圈更容易看清
                raw.rectTransform.sizeDelta = new Vector2(860 * 1.0f, 960 * 0.6f);
                raw.rectTransform.anchoredPosition = new Vector2(0, -40);
                viewer.RenderNow();
            });
        }

        // ---------------------------------------------------------------- 粒子与 UI 排序
        static Canvas CamCanvas(Shot s, string name, int order)
        {
            var c = UIKit.Canvas(name, RenderMode.ScreenSpaceCamera, s.W * 2, s.H * 2);
            c.worldCamera = s.Cam; c.planeDistance = 100; c.sortingOrder = order;
            return c;
        }

        [BookShot("ch19-particles", W = 640, H = 480, Panels = 3, Scene = "Ch19_ParticleSorting")]
        static void Particles(Shot s)
        {
            Object.DestroyImmediate(s.Canvas.gameObject);
            var bgC = CamCanvas(s, "BgCanvas (Order 0)", 0);
            var fgC = CamCanvas(s, "PopupCanvas (Order 10)", 10);
            var bg = (RectTransform)bgC.transform;
            UIKit.Panel(bg, UIKit.Hex("#24283B"), 0, "Bg").rectTransform.Stretch();
            var title = UIKit.Text(bg, "活动奖励", 56, Color.white, TextAlignmentOptions.Center, true);
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(800, 80));
            var chestGlow = UIKit.Image(bg, "glow_24", new Color(1, 0.75f, 0.3f, 0.5f), "ChestGlow");
            chestGlow.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-260, 20), new Vector2(420, 420));
            var chest = UIKit.Icon(bg, "gift", 260, K.accent); chest.rectTransform.anchoredPosition = new Vector2(-260, 20);

            // 粒子：挂在背景画布的宝箱旁边，Scaling Mode = Hierarchy，大小和速度按画布像素填
            var psGo = new GameObject("Sparkles", typeof(RectTransform));
            psGo.layer = LayerMask.NameToLayer("UI");
            psGo.transform.SetParent(bg, false);
            ((RectTransform)psGo.transform).anchoredPosition = new Vector2(-260, 20);
            var ps = psGo.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.duration = 2; main.loop = true; main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(160, 420);
            main.startSize = new ParticleSystem.MinMaxCurve(40, 90);
            main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
            main.startColor = new ParticleSystem.MinMaxGradient(UIKit.Hex("#FFD54A"), UIKit.Hex("#FF8A3D"));
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = 200;
            main.gravityModifier = 0;
            var em = ps.emission; em.rateOverTime = 60;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Circle; sh.radius = 60; sh.rotation = Vector3.zero;
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                      new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.1f), new GradientAlphaKey(1, 0.6f), new GradientAlphaKey(0, 1) });
            col.color = g;
            var r = psGo.GetComponent<ParticleSystemRenderer>();
            var mat = s.MatAsset("Ch19", "Sparkle", "Legacy Shaders/Particles/Alpha Blended");
            mat.mainTexture = UIKit.Sp("star").texture;
            r.sharedMaterial = mat;
            var order = psGo.AddComponent<UIParticleOrder>();
            order.offset = new[] { 5, -1, 20 }[s.Panel];
            order.Apply();

            var fg = (RectTransform)fgC.transform;
            var pop = UIKit.Card(fg, "RewardPopup"); pop.Place(new Vector2(0.5f, 0.5f), new Vector2(230, -120), new Vector2(620, 420));
            var pt = UIKit.Text(pop, "恭喜获得", 52, K.ink, TextAlignmentOptions.Center, true); pt.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(560, 70));
            var row = UIKit.Node(pop, "Items"); row.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(560, 120)); row.HLayout(20);
            foreach (var it in new[] { "item_gem", "item_coin", "item_potion_red" }) UIKit.Image(row, "slot", Color.white, "Slot").rectTransform.sizeDelta = new Vector2(120, 120);
            int k = 0; foreach (Transform sl in row) UIKit.Icon(sl, new[] { "item_gem", "item_coin", "item_potion_red" }[k++], 90, Color.white);
            var ok = UIKit.Button(pop, "收下", K.primary, 300, 96); ok.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(300, 96));

            string[] cap = { "粒子 Order 5：夹在两层画布之间", "粒子 Order -1：被背景画布挡住", "粒子 Order 20：连弹窗也盖住了" };
            var c3 = UIKit.Canvas("Caption", RenderMode.ScreenSpaceCamera, s.W * 2, s.H * 2);
            c3.worldCamera = s.Cam; c3.planeDistance = 100; c3.sortingOrder = 100;
            var capT = UIKit.Text(c3.transform, cap[s.Panel], 38, s.Panel == 0 ? UIKit.Hex("#2FB36A") : UIKit.Hex("#E5484D"), TextAlignmentOptions.Center, true, "Caption");
            capT.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(1200, 60));
            var capBg = UIKit.Panel(c3.transform, new Color(1, 1, 1, 0.92f), 0, "CaptionBg"); capBg.rectTransform.Place(new Vector2(0.5f, 0), Vector2.zero, new Vector2(1280, 88)); capBg.transform.SetAsFirstSibling();
            s.BeforeCapture(() => { ps.Simulate(1.6f, true, true); });
        }

        // ---------------------------------------------------------------- World Space
        [BookShot("ch19-worldspace", W = 960, H = 540, Scene = "Ch19_WorldSpace")]
        static void WorldSpace(Shot s)
        {
            s.Cam.orthographic = false; s.Cam.fieldOfView = 50;
            s.Cam.transform.position = new Vector3(0.2f, 1.7f, -3.6f);
            s.Cam.transform.LookAt(new Vector3(0.2f, 1.15f, 0.4f));
            s.Cam.clearFlags = CameraClearFlags.SolidColor; s.Cam.backgroundColor = UIKit.Hex("#2A3047");
            var sun = new GameObject("Directional Light").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 0.8f; sun.transform.rotation = Quaternion.Euler(45, -30, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.35f, 0.37f, 0.45f);

            var floor = Part(null, PrimitiveType.Plane, Vector3.zero, new Vector3(3, 1, 3), Mat(s, "Floor", UIKit.Hex("#5A6075"), 0.2f), "Floor");
            floor.AddComponent<MeshCollider>();
            var kiosk = Part(null, PrimitiveType.Cube, new Vector3(-0.7f, 0.9f, 0.6f), new Vector3(1.7f, 1.3f, 0.12f), Mat(s, "Kiosk", UIKit.Hex("#1F2436"), 0.7f), "Kiosk", new Vector3(0, -18, 0));
            Part(null, PrimitiveType.Cube, new Vector3(-0.7f, 0.12f, 0.6f), new Vector3(0.25f, 0.25f, 0.25f), Mat(s, "Kiosk", UIKit.Hex("#1F2436"), 0.7f), "KioskFoot", new Vector3(0, -18, 0));
            kiosk.AddComponent<BoxCollider>();

            // 世界空间画布：1400×1000 像素，缩放 0.001 → 1.4 m × 1.0 m，贴在控制台正面
            var wc = UIKit.Canvas("ConsoleCanvas (World Space)", RenderMode.WorldSpace);
            Object.DestroyImmediate(wc.GetComponent<CanvasScaler>());
            wc.worldCamera = s.Cam;              // Event Camera：没有它，这个画布收不到点击
            var wrt = (RectTransform)wc.transform;
            wrt.sizeDelta = new Vector2(1500, 1100);
            wrt.localScale = Vector3.one * 0.001f;
            wrt.position = kiosk.transform.position + kiosk.transform.rotation * new Vector3(0, 0, -0.07f);
            wrt.rotation = kiosk.transform.rotation;
            var ray = wc.GetComponent<GraphicRaycaster>(); ray.blockingObjects = GraphicRaycaster.BlockingObjects.ThreeD;
            UIKit.Panel(wrt, UIKit.Hex("#2B3150"), 24, "Screen").rectTransform.Stretch(20, 20, 20, 20);
            var t = UIKit.Text(wrt, "灯光控制台", 90, Color.white, TextAlignmentOptions.Center, true); t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1200, 120));
            var btns = new List<Button>();
            string[] names = { "暖光", "冷光", "霓虹" };
            Color[] colors = { UIKit.Hex("#FFB25B"), UIKit.Hex("#8FD3FF"), UIKit.Hex("#FF4FD8") };
            for (int i = 0; i < 3; i++)
            {
                var b = UIKit.Button(wrt, names[i], colors[i], 380, 170, null, 64);
                b.GetComponentInChildren<TMP_Text>().color = K.ink;
                b.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0.5f), new Vector2(-430 + i * 430, 90), new Vector2(380, 170));
                btns.Add(b);
            }
            var sl = UIKit.Slider(wrt, 0.6f, 1100, 70, K.accent);
            sl.minValue = 0; sl.maxValue = 3; sl.value = 1.8f;
            sl.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1100, 70));
            var status = UIKit.Text(wrt, "点按钮换颜色，拖滑块调亮度", 52, new Color(1, 1, 1, 0.75f)); status.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1300, 80));

            // 灯
            var pole = Part(null, PrimitiveType.Cylinder, new Vector3(1.3f, 0.8f, 1.0f), new Vector3(0.06f, 0.8f, 0.06f), Mat(s, "Kiosk", UIKit.Hex("#1F2436"), 0.7f), "LampPole");
            var bulbMat = Mat(s, "Bulb", colors[0], 0.9f, colors[0]);
            var bulb = Part(null, PrimitiveType.Sphere, new Vector3(1.3f, 1.7f, 1.0f), Vector3.one * 0.35f, bulbMat, "Bulb");
            var lamp = new GameObject("LampLight").AddComponent<Light>();
            lamp.type = LightType.Point; lamp.range = 4; lamp.intensity = 1.8f; lamp.color = colors[0];
            lamp.transform.position = bulb.transform.position + new Vector3(0, -0.2f, -0.4f);

            var console = kiosk.AddComponent<WorldConsole>();
            var so = new SerializedObject(console);
            so.FindProperty("m_Lamp").objectReferenceValue = lamp;
            so.FindProperty("m_Bulb").objectReferenceValue = bulb.GetComponent<Renderer>();
            var bp = so.FindProperty("m_ColorButtons"); bp.arraySize = 3;
            var cp = so.FindProperty("m_Colors"); cp.arraySize = 3;
            for (int i = 0; i < 3; i++) { bp.GetArrayElementAtIndex(i).objectReferenceValue = btns[i]; cp.GetArrayElementAtIndex(i).colorValue = colors[i]; }
            so.FindProperty("m_Intensity").objectReferenceValue = sl;
            so.FindProperty("m_Status").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 头顶名牌：World Space 小画布 + Billboard
            (string n, Vector3 p, Color c)[] npcs = { ("铁匠 老王", new Vector3(1.9f, 0, 2.6f), UIKit.Hex("#E5484D")), ("商人 小李", new Vector3(-2.2f, 0, 4.5f), K.success), ("守卫", new Vector3(0.4f, 0, 7.5f), K.primary) };
            foreach (var (n, p, c) in npcs)
            {
                var npc = Part(null, PrimitiveType.Capsule, p + new Vector3(0, 0.9f, 0), new Vector3(0.5f, 0.9f, 0.5f), Mat(s, "Npc_" + n.Split(' ')[0], c, 0.4f), "NPC_" + n);
                var nc = UIKit.Canvas("Nameplate", RenderMode.WorldSpace);
                Object.DestroyImmediate(nc.GetComponent<CanvasScaler>());
                Object.DestroyImmediate(nc.GetComponent<GraphicRaycaster>());   // 名牌不需要点击，不要 Raycaster
                var nrt = (RectTransform)nc.transform;
                nrt.SetParent(npc.transform, true);
                nrt.sizeDelta = new Vector2(420, 110);
                nrt.position = p + new Vector3(0, 2.15f, 0);
                nrt.localScale = Vector3.one * 0.0016f;
                var bgp = UIKit.Panel(nrt, new Color(0.12f, 0.14f, 0.21f, 0.82f), 0, "Bg"); bgp.sprite = UIKit.Sp("pill"); bgp.rectTransform.Stretch();
                var nt = UIKit.Text(nrt, n, 56, Color.white, TextAlignmentOptions.Center, true); nt.rectTransform.Stretch();
                var bb = nc.gameObject.AddComponent<Billboard>();
                bb.cam = s.Cam; bb.constantScreenSize = true; bb.scalePerMeter = 0.0005f;
                s.BeforeCapture(bb.Face);
            }
            var hint = UIKit.Text(s.Root, "World Space 画布：控制台上的按钮可以直接点；名牌始终朝向相机、远近大小一致", 30, Color.white);
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(1800, 50));
            s.BeforeCapture(() => console.SetColor(0));
        }
    }
}
