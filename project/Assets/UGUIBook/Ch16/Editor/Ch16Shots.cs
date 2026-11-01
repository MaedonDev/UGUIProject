// 第 16 章搭景：生成界面预制体（Resources/UIPanels）、自动生成绑定代码、搭示例场景并出图
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch16
{
    static class Ch16Prefabs
    {
        public const string Dir = "Assets/UGUIBook/Ch16/Resources/UIPanels";
        static bool s_Built;

        public static readonly Color Dark = UIKit.Hex("#1B1F2E");
        public static readonly Color Card = UIKit.Hex("#262B40");
        public static readonly Color Card2 = UIKit.Hex("#33395A");
        public static readonly Color Soft = UIKit.Hex("#A9B0C7");

        public static void EnsureBuilt()
        {
            if (s_Built) return;
            s_Built = true;
            if (!AssetDatabase.IsValidFolder("Assets/UGUIBook/Ch16/Resources")) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch16", "Resources");
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch16/Resources", "UIPanels");
            Save(BuildMain());
            Save(BuildBag());
            Save(BuildConfirm());
            Save(BuildToast());
            Save(BuildReward());
        }

        static void Save(GameObject go)
        {
            string path = $"{Dir}/{go.name}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            if (prefab == null) { Debug.LogError("[Ch16] 预制体保存失败 " + path); return; }
            UIBindGenerator.WriteFor(prefab, out var errors);
            foreach (var e in errors) Debug.LogError($"[Ch16] 绑定生成失败 {prefab.name}: {e}");
        }

        static RectTransform Root<T>(UILayer layer, bool fullScreen, UICacheMode cache) where T : UIPanel
        {
            var rt = UIKit.Node(null, typeof(T).Name);
            rt.Stretch();
            var p = rt.gameObject.AddComponent<T>();
            var so = new SerializedObject(p);
            so.FindProperty("m_Layer").enumValueIndex = (int)layer;
            so.FindProperty("m_FullScreen").boolValue = fullScreen;
            so.FindProperty("m_CacheMode").enumValueIndex = (int)cache;
            so.ApplyModifiedPropertiesWithoutUndo();
            return rt;
        }

        public static void SetRef(Object target, string prop, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(prop).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TextMeshProUGUI T(Transform p, string name, string text, float size, Color color, TextAlignmentOptions al,
            Vector2 anchor, Vector2 pos, Vector2 sz, bool bold = false)
        {
            var t = UIKit.Text(p, text, size, color, al, bold, name);
            t.rectTransform.Place(anchor, pos, sz);
            return t;
        }

        static Button RenameLabel(Button b, string labelName)
        {
            b.GetComponentInChildren<TextMeshProUGUI>().gameObject.name = labelName;
            return b;
        }

        static RectTransform Mask(Transform root)
        {
            var m = UIKit.Image(root, null, new Color(0.04f, 0.05f, 0.1f, 0.62f), "Mask");
            m.rectTransform.Stretch();
            return m.rectTransform;   // raycastTarget 保持 true：挡住下面界面的点击
        }

        static RectTransform Pill(Transform p, string name, string icon, Color bg, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var pill = UIKit.Image(p, "pill", bg, name).rectTransform;
            pill.Place(anchor, pos, size);
            var ic = UIKit.Icon(pill, icon, size.y * 0.8f, Color.white);
            ic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(size.y * 0.5f, 0), Vector2.one * size.y * 0.8f);
            ic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            return pill;
        }

        // ---------------- MainPanel ----------------
        static GameObject BuildMain()
        {
            var root = Root<MainPanel>(UILayer.Normal, true, UICacheMode.Permanent);
            var bg = UIKit.Image(root, "bg_landscape", Color.white, "Bg");
            bg.raycastTarget = true;   // 全屏界面的底图挡住下层点击
            bg.rectTransform.Stretch();
            var arf = bg.gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            arf.aspectRatio = 2f;
            var shade = UIKit.Image(root, "gradient_v", new Color(0.06f, 0.07f, 0.14f, 0.75f), "TopShade");
            shade.raycastTarget = false;
            var sr = shade.rectTransform;
            sr.anchorMin = new Vector2(0, 1); sr.anchorMax = new Vector2(1, 1); sr.pivot = new Vector2(0.5f, 0.5f);
            sr.sizeDelta = new Vector2(0, 260); sr.anchoredPosition = new Vector2(0, -130);
            sr.localScale = new Vector3(1, -1, 1);   // 精灵是上透明下不透明，上下翻转

            var top = UIKit.Node(root, "TopBar");
            top.anchorMin = new Vector2(0, 1); top.anchorMax = new Vector2(1, 1); top.pivot = new Vector2(0.5f, 1);
            top.sizeDelta = new Vector2(0, 170); top.anchoredPosition = Vector2.zero;
            var frame = UIKit.Image(top, "circle", Color.white, "AvatarFrame");
            frame.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -24), new Vector2(128, 128));
            frame.rectTransform.pivot = new Vector2(0, 1); frame.rectTransform.anchoredPosition = new Vector2(40, -24);
            var av = UIKit.Icon(frame.transform, "avatar_1", 116, Color.white, "Avatar");
            av.rectTransform.anchoredPosition = Vector2.zero;
            T(top, "txt_Name", "麦冬", 44, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0, 1), new Vector2(190, -34), new Vector2(360, 64), true)
                .rectTransform.pivot = new Vector2(0, 1);
            var lvPill = UIKit.Image(top, "pill", UISkin.Instance.accent, "LevelPill").rectTransform;
            lvPill.Place(new Vector2(0, 1), new Vector2(190, -102), new Vector2(150, 46)); lvPill.pivot = new Vector2(0, 1);
            lvPill.anchoredPosition = new Vector2(190, -102);
            var lv = UIKit.Text(lvPill, "Lv.12", 28, Color.white, TextAlignmentOptions.Center, true, "txt_Level");
            lv.rectTransform.Stretch();

            var gold = Pill(top, "GoldPill", "item_coin", new Color(0, 0, 0, 0.45f), new Vector2(1, 1), new Vector2(-400, -40), new Vector2(300, 76));
            gold.pivot = new Vector2(1, 1); gold.anchoredPosition = new Vector2(-400, -40);
            var gt = UIKit.Text(gold, "0", 36, Color.white, TextAlignmentOptions.MidlineRight, true, "txt_Gold");
            gt.rectTransform.Stretch(80, 0, 24, 0);
            var dia = Pill(top, "DiamondPill", "item_gem", new Color(0, 0, 0, 0.45f), new Vector2(1, 1), new Vector2(-60, -40), new Vector2(300, 76));
            dia.pivot = new Vector2(1, 1); dia.anchoredPosition = new Vector2(-60, -40);
            var dt = UIKit.Text(dia, "0", 36, Color.white, TextAlignmentOptions.MidlineRight, true, "txt_Diamond");
            dt.rectTransform.Stretch(80, 0, 24, 0);

            var side = UIKit.Node(root, "SideButtons");
            side.Place(new Vector2(1, 0.5f), new Vector2(-60, 40), new Vector2(170, 560));
            var vl = side.VLayout(24, 0, TextAnchor.MiddleCenter, false);
            Tile(side, "btn_Bag", "bag", "背包", false);
            Tile(side, "btn_Mail", "mail", "邮件", true);
            Tile(side, "btn_Settings", "gear", "设置", false);

            var battle = UIKit.Button(root, "开始战斗", UISkin.Instance.accent, 440, 130, "play", 46, "btn_Battle");
            battle.Rt().Place(new Vector2(1, 0), new Vector2(-60, 70), new Vector2(440, 130));

            var title = T(root, "Title", "麦冬的冒险", 110, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0, 0.5f), new Vector2(80, 60), new Vector2(900, 160), true);
            title.rectTransform.pivot = new Vector2(0, 0.5f);
            var hint = UIKit.Image(root, "pill", new Color(0, 0, 0, 0.5f), "Hint").rectTransform;
            hint.Place(new Vector2(0, 0), new Vector2(60, 90), new Vector2(980, 64));
            var ht = UIKit.Text(hint, "点「背包」→ 选中物品 →「出售」弹出确认框；按 Esc 逐层返回", 28, Color.white, TextAlignmentOptions.Center, false, "Text");
            ht.rectTransform.Stretch(20, 0, 20, 0);
            return root.gameObject;
        }

        static void Tile(Transform parent, string name, string icon, string label, bool dot)
        {
            var img = UIKit.Image(parent, "round_24", new Color(1, 1, 1, 0.94f), name);
            img.rectTransform.sizeDelta = new Vector2(150, 150);
            img.gameObject.AddComponent<Button>();
            var ic = UIKit.Icon(img.transform, icon, 70, UISkin.Instance.primary);
            ic.rectTransform.anchoredPosition = new Vector2(0, 16);
            var t = UIKit.Text(img.transform, label, 28, UISkin.Instance.ink, TextAlignmentOptions.Center, true, "Label");
            t.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 28), new Vector2(150, 40));
            if (dot)
            {
                var d = UIKit.Image(img.transform, "circle", UISkin.Instance.danger, "RedDot");
                d.raycastTarget = false;
                d.rectTransform.Place(new Vector2(1, 1), new Vector2(-14, -14), new Vector2(30, 30));
            }
        }

        // ---------------- BagPanel ----------------
        static GameObject BuildBag()
        {
            var root = Root<BagPanel>(UILayer.Normal, true, UICacheMode.Cache);
            var bg = UIKit.Image(root, null, Dark, "Bg");
            bg.rectTransform.Stretch();
            bg.raycastTarget = true;

            var header = UIKit.Image(root, null, Card, "Header").rectTransform;
            header.anchorMin = new Vector2(0, 1); header.anchorMax = new Vector2(1, 1); header.pivot = new Vector2(0.5f, 1);
            header.sizeDelta = new Vector2(0, 120); header.anchoredPosition = Vector2.zero;
            var back = UIKit.IconButton(header, "back", new Color(1, 1, 1, 0.1f), 84, Color.white);
            back.name = "btn_Back";
            back.Rt().Place(new Vector2(0, 0.5f), new Vector2(82, 0), new Vector2(84, 84));
            back.Rt().pivot = new Vector2(0.5f, 0.5f);
            T(header, "Title", "背包", 48, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0, 0.5f), new Vector2(150, 0), new Vector2(300, 80), true)
                .rectTransform.pivot = new Vector2(0, 0.5f);
            var gold = Pill(header, "GoldPill", "item_coin", new Color(0, 0, 0, 0.35f), new Vector2(1, 0.5f), new Vector2(-60, 0), new Vector2(300, 76));
            gold.pivot = new Vector2(1, 0.5f); gold.anchoredPosition = new Vector2(-60, 0);
            var gt = UIKit.Text(gold, "0", 36, Color.white, TextAlignmentOptions.MidlineRight, true, "txt_Gold");
            gt.rectTransform.Stretch(80, 0, 24, 0);

            // 左：格子
            var gridCard = UIKit.Panel(root, Card, 24, "GridCard").rectTransform;
            gridCard.Stretch(60, 160, 760, 80);
            T(gridCard, "Caption", "全部物品", 34, Color.white, TextAlignmentOptions.MidlineLeft, new Vector2(0, 1), new Vector2(50, -50), new Vector2(400, 60), true)
                .rectTransform.pivot = new Vector2(0, 0.5f);
            var cap = T(gridCard, "txt_Capacity", "0/40", 30, Soft, TextAlignmentOptions.MidlineRight, new Vector2(1, 1), new Vector2(-50, -50), new Vector2(300, 60));
            cap.rectTransform.pivot = new Vector2(1, 0.5f);
            var slotBgs = UIKit.Node(gridCard, "SlotBackground");
            slotBgs.Place(new Vector2(0, 1), new Vector2(50, -100), new Vector2(1000, 660)); slotBgs.pivot = new Vector2(0, 1);
            slotBgs.anchoredPosition = new Vector2(50, -100);
            ConfigGrid(slotBgs.Grid(new Vector2(150, 150), new Vector2(20, 20)));
            for (int i = 0; i < 24; i++)
            {
                var sb = UIKit.Image(slotBgs, "slot", new Color(1, 1, 1, 0.45f), "Empty");
                sb.raycastTarget = false;
            }
            var grid = UIKit.Node(gridCard, "rt_Grid");
            grid.Place(new Vector2(0, 1), new Vector2(50, -100), new Vector2(1000, 660)); grid.pivot = new Vector2(0, 1);
            grid.anchoredPosition = new Vector2(50, -100);
            ConfigGrid(grid.Grid(new Vector2(150, 150), new Vector2(20, 20)));
            BuildSlot(grid, "go_SlotTemplate");

            // 右：详情
            var detailCard = UIKit.Panel(root, Card, 24, "DetailCard").rectTransform;
            detailCard.Stretch(1200, 160, 60, 80);
            var detail = UIKit.Node(detailCard, "go_Detail"); detail.Stretch();
            var iconBg = UIKit.Image(detail, "round_24", Card2, "IconBg").rectTransform;
            iconBg.Place(new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(210, 210));
            var glow = UIKit.Image(iconBg, "circle_soft", new Color(1, 1, 1, 0.18f), "Glow"); glow.rectTransform.Stretch(-20, -20, -20, -20); glow.raycastTarget = false;
            var di = UIKit.Icon(iconBg, "item_sword", 160, Color.white, "img_DetailIcon");
            di.rectTransform.anchoredPosition = Vector2.zero;
            T(detail, "txt_DetailName", "物品名", 50, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -320), new Vector2(600, 70), true);
            T(detail, "txt_DetailPrice", "出售价格", 28, Soft, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -385), new Vector2(600, 44));
            var line = UIKit.Image(detail, null, new Color(1, 1, 1, 0.1f), "Divider");
            line.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -430), new Vector2(560, 2));
            var desc = T(detail, "txt_DetailDesc", "描述", 32, UIKit.Hex("#D8DCEA"), TextAlignmentOptions.TopLeft, new Vector2(0.5f, 1), new Vector2(0, -460), new Vector2(560, 200));
            desc.rectTransform.pivot = new Vector2(0.5f, 1); desc.rectTransform.anchoredPosition = new Vector2(0, -460);
            desc.lineSpacing = 12;
            var use = RenameLabel(UIKit.Button(detail, "使用", UISkin.Instance.primary, 250, 100, null, 38, "btn_Use"), "Text");
            use.Rt().Place(new Vector2(0.5f, 0), new Vector2(-140, 60), new Vector2(250, 100)); use.Rt().pivot = new Vector2(0.5f, 0);
            var sell = RenameLabel(UIKit.Button(detail, "出售", UISkin.Instance.accent, 250, 100, null, 38, "btn_Sell"), "Text");
            sell.Rt().Place(new Vector2(0.5f, 0), new Vector2(140, 60), new Vector2(250, 100)); sell.Rt().pivot = new Vector2(0.5f, 0);
            var empty = UIKit.Node(detailCard, "go_Empty"); empty.Stretch();
            T(empty, "Text", "选择一个物品查看详情", 34, Soft, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 80));
            return root.gameObject;
        }

        static void ConfigGrid(GridLayoutGroup g)
        {
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = 6;
            g.childAlignment = TextAnchor.UpperLeft;
        }

        static void BuildSlot(Transform parent, string name)
        {
            var img = UIKit.Image(parent, "slot", Color.white, name);
            var btn = img.gameObject.AddComponent<Button>();
            var q = UIKit.Image(img.transform, "round_16", Color.white, "Quality"); q.rectTransform.Stretch(9, 9, 9, 9); q.raycastTarget = false;
            var icon = UIKit.Icon(img.transform, "item_sword", 112, Color.white, "Icon");
            icon.rectTransform.anchoredPosition = Vector2.zero;
            var count = UIKit.Text(img.transform, "12", 30, Color.white, TextAlignmentOptions.BottomRight, true, "Count");
            count.rectTransform.Stretch(10, 10, 14, 8);
            var sel = UIKit.Image(img.transform, "frame_select", UISkin.Instance.accent, "Selected");
            sel.rectTransform.Stretch(-6, -6, -6, -6); sel.raycastTarget = false;
            var slot = img.gameObject.AddComponent<BagSlot>();
            SetRef(slot, "m_Quality", q);
            SetRef(slot, "m_Icon", icon);
            SetRef(slot, "m_Count", count);
            SetRef(slot, "m_Selected", sel.gameObject);
            SetRef(slot, "m_Button", btn);
        }

        // ---------------- ConfirmPanel ----------------
        static GameObject BuildConfirm()
        {
            var root = Root<ConfirmPanel>(UILayer.Popup, false, UICacheMode.Permanent);
            Mask(root);
            var card = UIKit.Card(root, "Card", 32);
            card.Place(0, 0, 820, 460);
            card.gameObject.AddComponent<CanvasGroup>();
            card.gameObject.AddComponent<UIPopIn>();
            T(card, "txt_Title", "提示", 44, UISkin.Instance.ink, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -64), new Vector2(720, 70), true);
            var msg = T(card, "txt_Message", "内容", 34, UISkin.Instance.ink, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(700, 170));
            msg.lineSpacing = 10;
            var buttons = UIKit.Node(card, "Buttons");
            buttons.Place(new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(720, 100)); buttons.pivot = new Vector2(0.5f, 0);
            buttons.HLayout(40);
            RenameLabel(UIKit.Button(buttons, "取消", UIKit.Hex("#E4E7EF"), 320, 100, null, 38, "btn_Cancel"), "txt_Cancel");
            RenameLabel(UIKit.Button(buttons, "确定", UISkin.Instance.primary, 320, 100, null, 38, "btn_Ok"), "txt_Ok");
            return root.gameObject;
        }

        // ---------------- ToastPanel ----------------
        static GameObject BuildToast()
        {
            var root = Root<ToastPanel>(UILayer.Toast, false, UICacheMode.Permanent);
            var cg = root.GetComponent<CanvasGroup>();
            cg.blocksRaycasts = false;     // 提示不挡点击
            cg.interactable = false;
            var list = UIKit.Node(root, "List");
            list.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(1400, 360));
            var vl = list.VLayout(14, 0, TextAnchor.LowerCenter, false);
            var item = UIKit.Image(list, "pill", new Color(0.08f, 0.09f, 0.14f, 0.88f), "go_Item");
            item.raycastTarget = false;
            item.gameObject.AddComponent<CanvasGroup>();
            var hl = item.rectTransform.HLayout(0);
            hl.padding = new RectOffset(52, 52, 20, 20);
            hl.childControlWidth = hl.childControlHeight = true;
            var fit = item.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var t = UIKit.Text(item.transform, "提示", 34, Color.white, TextAlignmentOptions.Center, false, "Text");
            t.enableWordWrapping = false;
            return root.gameObject;
        }

        // ---------------- RewardPanel ----------------
        static GameObject BuildReward()
        {
            var root = Root<RewardPanel>(UILayer.Popup, false, UICacheMode.Cache);
            Mask(root);
            var card = UIKit.Card(root, "Card", 32);
            card.Place(0, 0, 640, 600);
            card.gameObject.AddComponent<CanvasGroup>();
            card.gameObject.AddComponent<UIPopIn>();
            var ribbon = UIKit.Image(card, "round_32", UISkin.Instance.primary, "Ribbon").rectTransform;
            ribbon.Place(new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(640, 130));
            T(card, "txt_Title", "奖励", 48, Color.white, TextAlignmentOptions.Center, new Vector2(0.5f, 1), new Vector2(0, -25), new Vector2(560, 80), true);
            var glow = UIKit.Image(card, "circle_soft", new Color(1f, 0.76f, 0.35f, 0.9f), "Glow");
            glow.raycastTarget = false; glow.rectTransform.Place(0, 50, 300, 300);
            var ic = UIKit.Icon(card, "item_coin", 180, Color.white, "img_Icon");
            ic.rectTransform.anchoredPosition = new Vector2(0, 50);
            T(card, "txt_Desc", "描述", 32, UISkin.Instance.ink, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, -95), new Vector2(560, 70));
            var claim = RenameLabel(UIKit.Button(card, "领取", UISkin.Instance.accent, 360, 100, null, 40, "btn_Claim"), "txt_Claim");
            claim.Rt().Place(0, -185, 360, 96);
            T(card, "txt_Queue", "", 26, UISkin.Instance.muted, TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0, -265), new Vector2(560, 40));
            return root.gameObject;
        }
    }

    static class Ch16Shots
    {
        /// 把默认画布改造成 UIRoot：挂 UIManager、建层、放一个显示界面栈的调试标签和启动脚本
        static (UIManager ui, Ch16Bootstrap boot) Setup(Shot s)
        {
            Ch16Prefabs.EnsureBuilt();
            // 批处理里各张图共用一个进程：清掉上一张图留下的全局订阅
            EventCenter.Clear<ItemSoldEvent>();
            EventCenter.Clear<ItemUsedEvent>();
            s.Canvas.gameObject.name = "UIRoot";
            var ui = s.Canvas.gameObject.AddComponent<UIManager>();
            ui.Init();
            var label = UIKit.Image(ui.GetLayer(UILayer.Top), "pill", new Color(0, 0, 0, 0.6f), "StackLabel").rectTransform;
            label.Place(new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(600, 50));
            var hl = label.HLayout(0);
            hl.padding = new RectOffset(30, 30, 8, 8); hl.childControlWidth = hl.childControlHeight = true;
            var fit = label.gameObject.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var t = UIKit.Text(label, "界面栈：", 26, Color.white, TextAlignmentOptions.Center, false, "Text");
            t.enableWordWrapping = false;
            var boot = new GameObject("Bootstrap").AddComponent<Ch16Bootstrap>();
            Ch16Prefabs.SetRef(boot, "m_StackLabel", t);
            return (ui, boot);
        }

        static void Click(UIManager ui, string path)
        {
            var b = ui.Top.transform.Find(path).GetComponent<Button>();
            b.onClick.Invoke();
        }

        [BookShot("ch16-main", Scene = "Ch16_UIFramework")]
        static void Main(Shot s)
        {
            var (ui, boot) = Setup(s);
            s.BeforeCapture(() => boot.Boot());
        }

        [BookShot("ch16-bag")]
        static void Bag(Shot s)
        {
            var (ui, boot) = Setup(s);
            s.BeforeCapture(() =>
            {
                boot.Boot();
                ui.Open<BagPanel>().Select(9);
            });
        }

        [BookShot("ch16-confirm")]
        static void Confirm(Shot s)
        {
            var (ui, boot) = Setup(s);
            s.BeforeCapture(() =>
            {
                boot.Boot();
                ui.Open<BagPanel>().Select(0);
                Click(ui, "DetailCard/go_Detail/btn_Sell");
            });
        }

        // 左：确认出售后（Toast + 金币刷新）；右：按返回键回到主界面
        [BookShot("ch16-sold", W = 640, H = 360, Panels = 2)]
        static void Sold(Shot s)
        {
            s.Ref(1920, 1080);
            var (ui, boot) = Setup(s);
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                boot.Boot();
                ui.Open<BagPanel>().Select(0);
                Click(ui, "DetailCard/go_Detail/btn_Sell");
                Click(ui, "Card/Buttons/btn_Ok");
                if (panel == 1) ui.Back();
            });
        }

        [BookShot("ch16-layers")]
        static void Layers(Shot s)
        {
            var (ui, boot) = Setup(s);
            boot.Boot();
            ui.Open<BagPanel>().Select(5);
            Click(ui, "DetailCard/go_Detail/btn_Sell");
            UIManager.Toast("Toast 层：飘字提示不进栈");
            var bag = ui.GetLayer(UILayer.Normal).Find("BagPanel");
            s.Box((RectTransform)bag.Find("Header"), UIKit.Hex("#1E88E5"), "Normal 层 · BagPanel（全屏，下面的 MainPanel 已隐藏）", -6, true);
            s.Box((RectTransform)ui.GetLayer(UILayer.Popup).Find("ConfirmPanel/Card"), UIKit.Hex("#E5484D"), "Popup 层 · ConfirmPanel");
            var toast = (RectTransform)ui.GetLayer(UILayer.Toast).Find("ToastPanel/List").GetChild(1);
            s.Box(toast, UIKit.Hex("#2FB36A"));
            s.LabelAt(toast, new Vector2(1, 0.5f), new Vector2(120, 0), "Toast 层", UIKit.Hex("#2FB36A"));
            var stack = (RectTransform)ui.GetLayer(UILayer.Top).Find("StackLabel");
            s.Box(stack, UIKit.Hex("#FF8A3D"));
            s.LabelAt(stack, new Vector2(1, 0.5f), new Vector2(170, 0), "Top 层 · 调试标签", UIKit.Hex("#FF8A3D"));
        }

        // 一次排进三个弹窗：签到(0)、公告(0)、首充(优先级 10)。依次点“领取”
        [BookShot("ch16-queue", W = 640, H = 360, Panels = 3)]
        static void Queue(Shot s)
        {
            s.Ref(1920, 1080);
            var (ui, boot) = Setup(s);
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                boot.Boot();
                Click(ui, "SideButtons/btn_Mail");
                for (int i = 0; i < panel; i++) Click(ui, "Card/btn_Claim");
            });
        }
    }
}
