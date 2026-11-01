// 第 21 章配图与配套场景：工具都是在这里对着真实的 UI 跑一遍，结果画进图里 / 打印到日志（正文引用的就是这些输出）
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;

        // ---------------------------------------------------------------- 射线调试器
        static RaycastDebugger Debugger(RectTransform parent)
        {
            var c = UIKit.Canvas("RaycastDebugger (Overlay)");
            c.sortingOrder = 32000;
            Object.DestroyImmediate(c.GetComponent<GraphicRaycaster>());     // 调试面板自己不能挡射线
            var panel = UIKit.Panel(c.transform, new Color(0.08f, 0.09f, 0.14f, 0.9f), 16, "Panel");
            panel.raycastTarget = false;
            panel.rectTransform.Place(new Vector2(0, 1), new Vector2(24, -24), new Vector2(980, 300));
            var label = UIKit.Text(panel.transform, "", 26, Color.white, TextAlignmentOptions.TopLeft);
            label.rectTransform.Stretch(22, 18, 22, 14);
            label.enableWordWrapping = true;
            var hl = UIKit.Image(c.transform, "round_outline_8", K.accent, "Highlight");
            hl.raycastTarget = false;
            var dbg = c.gameObject.AddComponent<RaycastDebugger>();
            var so = new SerializedObject(dbg);
            so.FindProperty("m_Label").objectReferenceValue = label;
            so.FindProperty("m_Highlight").objectReferenceValue = hl.rectTransform;
            so.ApplyModifiedPropertiesWithoutUndo();
            return dbg;
        }

        /// 编辑模式下 EventSystem/Raycaster 都没启用，手动把每个 GraphicRaycaster 跑一遍，再按画布排序 + depth 排
        static List<RaycastResult> EditModeRaycast(Vector2 screen)
        {
            var es = Object.FindObjectOfType<EventSystem>();
            var ped = new PointerEventData(es) { position = screen };
            var hits = new List<RaycastResult>();
            foreach (var r in Object.FindObjectsOfType<GraphicRaycaster>()) r.Raycast(ped, hits);
            return hits.OrderByDescending(h => h.sortingOrder).ThenByDescending(h => h.depth).ToList();
        }

        static RectTransform ShopCard(RectTransform root, out Button buy, out Image overlay)
        {
            var card = UIKit.Card(root, "ShopCard");
            card.Place(new Vector2(0.5f, 0.5f), new Vector2(330, -40), new Vector2(980, 760));
            var title = UIKit.Text(card, "限时礼包", 56, K.ink, TextAlignmentOptions.Center, true, "Title");
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(800, 80));
            var row = UIKit.Node(card, "Items"); row.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(860, 200)); row.HLayout(30);
            foreach (var it in new[] { "item_gem", "item_coin", "item_potion_blue", "item_scroll" })
            {
                var slot = UIKit.Image(row, "slot", Color.white, "Slot_" + it); slot.rectTransform.sizeDelta = new Vector2(180, 180);
                UIKit.Icon(slot.transform, it, 130, Color.white);
            }
            var price = UIKit.Text(card, "¥ 30", 60, K.danger, TextAlignmentOptions.Center, true, "Price");
            price.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -90), new Vector2(400, 80));
            buy = UIKit.Button(card, "购买", K.primary, 420, 120, "bag", 44, "BuyButton");
            buy.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(420, 120));
            // 经典 bug：一张“提示气泡”的透明底图盖在按钮上，透明度为 0 但开着 Raycast Target
            overlay = UIKit.Image(card, null, new Color(1, 1, 1, 0), "TipsOverlay");
            overlay.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(900, 300));
            return card;
        }

        [BookShot("ch21-raycast-debug", W = 960, H = 540, Scene = "Ch21_RaycastDebug")]
        static void RaycastDebug(Shot s)
        {
            ShopCard(s.Root, out var buy, out _);
            var hint = UIKit.Text(s.Root, "按 F3 开关调试面板；“购买”点不动，看看面板说了什么", 28, K.muted, TextAlignmentOptions.MidlineLeft);
            hint.rectTransform.Place(new Vector2(0, 0), new Vector2(30, 20), new Vector2(900, 50));
            var dbg = Debugger(s.Root);
            s.BeforeCapture(() =>
            {
                s.Cam.Render();   // 先渲染一次，Graphic.depth 才有值（depth == -1 的不参与射线检测）
                var center = buy.GetComponent<RectTransform>();
                var c = new Vector3[4]; center.GetWorldCorners(c);
                var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, (c[0] + c[2]) * 0.5f + new Vector3(0.6f, 0, 0));
                var hits = EditModeRaycast(screen);
                Debug.Log("CH21 hits: " + string.Join(", ", hits.Select(h => h.gameObject.name + "(depth " + h.depth + ")")));
                dbg.Show(hits, screen, null);
            });
            s.Arrow(() => s.At(buy.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f)) + new Vector2(260, -150),
                    () => s.At(buy.GetComponent<RectTransform>(), new Vector2(0.62f, 0.45f)), K.danger);
        }

        // ---------------------------------------------------------------- Raycast Target 清理
        static RectTransform ToolDemoUI(RectTransform root)
        {
            var page = UIKit.Node(root, "SettingsPage").Stretch(40, 40, 40, 40);
            UIKit.Panel(page, Color.white, 24, "Bg").rectTransform.Stretch();
            var title = UIKit.Text(page, "设置", 52, K.ink, TextAlignmentOptions.MidlineLeft, true, "Title");
            title.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -30), new Vector2(500, 80));
            var close = UIKit.IconButton(page, "close", UIKit.Hex("#EEF0F6"), 80, K.ink); close.GetComponent<RectTransform>().Place(new Vector2(1, 1), new Vector2(-40, -30), new Vector2(80, 80));
            string[] labels = { "音乐", "音效", "震动" };
            for (int i = 0; i < 3; i++)
            {
                float y = -170 - i * 120;
                var icon = UIKit.Image(page, new[] { "music", "sound", "bell" }[i], K.primary, "Icon"); icon.rectTransform.Place(new Vector2(0, 1), new Vector2(60, y), new Vector2(60, 60));
                icon.raycastTarget = true;   // UIKit.Icon 默认关了，这里模拟“新建 Image 默认开着”
                var l = UIKit.Text(page, labels[i], 40, K.ink, TextAlignmentOptions.MidlineLeft, false, "Label"); l.raycastTarget = true;
                l.rectTransform.Place(new Vector2(0, 1), new Vector2(150, y), new Vector2(300, 60));
                if (i < 2) { var sl = UIKit.Slider(page, 0.3f + i * 0.3f, 700, 40); sl.GetComponent<RectTransform>().Place(new Vector2(1, 1), new Vector2(-80, y - 10), new Vector2(700, 40)); }
                else { var tg = UIKit.Toggle(page, "", true, 70, 70); tg.GetComponent<RectTransform>().Place(new Vector2(1, 1), new Vector2(-80, y), new Vector2(70, 70)); Object.DestroyImmediate(tg.transform.Find("Label").gameObject); }
            }
            var tip = UIKit.Panel(page, UIKit.Hex("#FFF4E8"), 16, "TipBox"); tip.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(-220, 50), new Vector2(760, 120));
            var tipT = UIKit.Text(tip.transform, "提示：设置会自动保存", 32, K.accent, TextAlignmentOptions.Center, false, "TipText"); tipT.rectTransform.Stretch(); tipT.raycastTarget = true;
            var save = UIKit.Button(page, "保存", K.primary, 300, 100); save.GetComponent<RectTransform>().Place(new Vector2(1, 0), new Vector2(-60, 60), new Vector2(300, 100));
            save.GetComponentInChildren<TMP_Text>().raycastTarget = true;   // TMP_DefaultControls 创建的按钮文字默认开着 Raycast Target
            return page;
        }

        [BookShot("ch21-raycast-clean", W = 640, H = 400, Panels = 2)]
        static void RaycastClean(Shot s)
        {
            var page = ToolDemoUI(s.Root);
            int removed = 0;
            if (s.Panel == 1) removed = RaycastTargetCleaner.Apply(page.gameObject, false);
            var on = page.GetComponentsInChildren<Graphic>(true).Where(g => g.raycastTarget).ToList();
            foreach (var g in on) s.Box(g.rectTransform, K.success, null, 2);
            s.Label(new Vector2(0, -360), s.Panel == 0 ? $"清理前：{on.Count} 个元素开着 Raycast Target" : $"清理后：{on.Count} 个（关掉了 {removed} 个）", s.Panel == 0 ? K.danger : K.success, 34);
            if (s.Panel == 1) Debug.Log("CH21 cleaner kept: " + string.Join(", ", on.Select(g => UIToolsCommon.PathOf(g.transform, page))));
            s.Background(UIKit.Hex("#EEF0F6"));
        }

        // ---------------------------------------------------------------- 规范检查 + 丢失引用（对着一个“问题很多”的预制体跑）
        const string BadPrefabPath = "Assets/UGUIBook/Ch21/Prefabs/BadPanel.prefab";

        static GameObject BuildBadPanel(Transform parent)
        {
            var root = UIKit.Image(parent, "round_16", Color.white, "Panel");
            root.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200, 800));
            UIKit.Image(root.transform, null, Color.white, "Image (1)").rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(1100, 8));
            var legacy = UIKit.LegacyText(root.transform, "旧版 Text 标题", 48, K.ink, TextAnchor.MiddleCenter, "Title");
            legacy.raycastTarget = true;
            legacy.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(800, 80));
            var desc = UIKit.Text(root.transform, "自动缩放的描述文字", 36, K.ink, TextAlignmentOptions.Center, false, "Desc");
            desc.enableAutoSizing = true; desc.raycastTarget = true;
            desc.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(800, 60));
            var icon = UIKit.Icon(root.transform, "star", 100, K.accent, "Icon"); icon.rectTransform.localScale = Vector3.one * 1.2f;
            icon.rectTransform.anchoredPosition = new Vector2(-450, 200);
            var overlay = UIKit.Image(root.transform, null, new Color(1, 1, 1, 0), "TipsOverlay"); overlay.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(1000, 200));
            var list = UIKit.Image(root.transform, null, UIKit.Hex("#EEF0F6"), "List"); list.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-300, -60), new Vector2(400, 300));
            list.gameObject.AddComponent<Mask>();
            var popup = UIKit.Node(root.transform, "Popup"); popup.Place(new Vector2(0.5f, 0.5f), new Vector2(300, -60), new Vector2(400, 300));
            popup.gameObject.AddComponent<Canvas>();
            var buy = UIKit.Button(popup, "购买", K.primary, 300, 100, null, 38, "BuyButton");
            // 层层嵌套的布局组
            Transform t = root.transform;
            for (int i = 1; i <= 4; i++)
            {
                var n = UIKit.Node(t, "Layout" + i); n.sizeDelta = new Vector2(100, 100);
                n.gameObject.AddComponent<VerticalLayoutGroup>();
                t = n;
            }
            var glow = UIKit.Image(root.transform, "glow_24", K.accent, "Glow"); glow.rectTransform.anchoredPosition = new Vector2(450, 200);
            glow.gameObject.AddComponent<Animator>();
            return root.gameObject;
        }

        /// 生成 BadPanel.prefab，并故意制造两类丢失引用：材质资源被删除、OnClick 绑定的方法改名
        static void EnsureBadPrefab()
        {
            Directory.CreateDirectory("Assets/UGUIBook/Ch21/Prefabs");
            var tmp = new GameObject("tmp");
            var go = BuildBadPanel(tmp.transform);
            go.transform.SetParent(null, false);
            Object.DestroyImmediate(tmp);
            // ① 引用一个随后会被删掉的材质
            const string matPath = "Assets/UGUIBook/Ch21/Prefabs/_TempDeleted.mat";
            var mat = new Material(Shader.Find("UI/Default"));
            AssetDatabase.CreateAsset(mat, matPath);
            go.transform.Find("Icon").GetComponent<Image>().material = mat;
            // ② OnClick 里绑定一个已经不存在的方法（模拟“方法改了名”）
            var buy = go.transform.Find("Popup/BuyButton").GetComponent<Button>();
            var so = new SerializedObject(buy);
            var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            calls.arraySize = 1;
            var call = calls.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue = buy.transform;
            call.FindPropertyRelative("m_MethodName").stringValue = "OnBuyClicked";
            call.FindPropertyRelative("m_Mode").intValue = 1;          // PersistentListenerMode.Void
            call.FindPropertyRelative("m_CallState").intValue = 2;     // UnityEventCallState.RuntimeOnly
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(go, BadPrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.DeleteAsset(matPath);
            AssetDatabase.Refresh();
        }

        static List<UIIssue> RunTools(out List<UIIssue> missing)
        {
            EnsureBadPrefab();
            var lint = new List<UIIssue>();
            var miss = new List<UIIssue>();
            UIToolsCommon.ForEachPrefab(new[] { BadPrefabPath }, (root, p) =>
            {
                lint.AddRange(UIPrefabLinter.Lint(root, p));
                MissingReferenceFinder.Check(root, p, miss);
                return false;
            });
            missing = miss;
            foreach (var i in lint) Debug.Log("CH21 LINT " + i);
            foreach (var i in miss) Debug.Log("CH21 MISSING " + i);
            return lint;
        }

        static void Console(Shot s, string title, List<UIIssue> issues, float fontSize)
        {
            s.Background(UIKit.Hex("#1F2436"));
            var t = UIKit.Text(s.Root, title, 40, Color.white, TextAlignmentOptions.MidlineLeft, true);
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -24), new Vector2(1800, 60));
            var body = UIKit.Text(s.Root, "", fontSize, UIKit.Hex("#E0DEEF"), TextAlignmentOptions.TopLeft);
            body.rectTransform.Stretch(50, 100, 50, 30);
            body.enableWordWrapping = true;
            body.richText = true;
            body.text = string.Join("\n", issues.Select(i =>
                $"<color=#FF8A3D>[{i.rule}]</color> <color=#9AA3BD>{i.objectPath}</color>  {i.message.Replace("<", "&lt;")}"));
        }

        [BookShot("ch21-lint", W = 960, H = 350)]
        static void Lint(Shot s)
        {
            var lint = RunTools(out var missing);
            Console(s, $"UI 规范检查：BadPanel.prefab 共 {lint.Count} 条（规则 {UIPrefabLinter.Rules.Count} 条）", lint, 25);
        }

        [BookShot("ch21-missing", W = 960, H = 125)]
        static void Missing(Shot s)
        {
            RunTools(out var missing);
            Console(s, $"检查丢失引用：BadPanel.prefab 共 {missing.Count} 处", missing, 28);
        }

        [BookScene("Ch21_ToolsSandbox")]
        static void Sandbox(Shot s)
        {
            if (!File.Exists(BadPrefabPath)) EnsureBadPrefab();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BadPrefabPath);
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, s.Root);
            inst.transform.localPosition = new Vector3(0, -60, 0);
            ToolDemoUI(s.Root).gameObject.SetActive(false);
            var hint = UIKit.Text(s.Root, "试试菜单：Tools > UGUIBook > UI 规范检查（当前场景） / 检查丢失引用（当前场景）；SettingsPage 默认隐藏，可用来试 Raycast Target 清理", 26, K.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(1800, 80));
            hint.enableWordWrapping = true;
        }

        // ---------------------------------------------------------------- 高清截图（透明背景）
        [BookShot("ch21-hires", W = 960, H = 400)]
        static void HiRes(Shot s)
        {
            // 待截的界面：一张带阴影的卡片，背景透明
            var src = UIKit.Canvas("PromoCanvas", RenderMode.ScreenSpaceOverlay, 1600, 900);
            var card = UIKit.Card(src.transform, "PromoCard"); card.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1100, 560));
            var title = UIKit.Text(card, "新角色 · 小麦", 80, K.ink, TextAlignmentOptions.Center, true); title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1000, 110));
            var av = UIKit.Image(card, "avatar_3", Color.white, "Avatar"); av.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(240, 240));
            var glow = UIKit.Image(card, "glow_24", new Color(1, 0.6f, 0.2f, 0.6f), "Glow"); glow.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(420, 420)); glow.transform.SetSiblingIndex(2);
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/ch21_hires_test.png"));
            var outImg = new GameObject("Result", typeof(RectTransform)).AddComponent<RawImage>();
            outImg.rectTransform.SetParent(s.Root, false);
            var checker = UIKit.Image(s.Root, "checker", new Color(0.85f, 0.87f, 0.92f), "Checker");
            checker.type = Image.Type.Tiled; checker.pixelsPerUnitMultiplier = 2;
            checker.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1280, 720));
            outImg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1280, 720));
            outImg.transform.SetAsLastSibling();
            s.BeforeCapture(() =>
            {
                HiResScreenshot.CaptureCanvas(src, 3200, 1800, true, path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(path));
                Debug.Log($"CH21 HIRES {tex.width}x{tex.height} corner alpha={tex.GetPixel(5, 5).a:0.00} center alpha={tex.GetPixel(tex.width / 2, tex.height / 2).a:0.00}");
                outImg.texture = tex;
                src.gameObject.SetActive(false);
            });
            s.Label(new Vector2(0, 370), "CaptureCanvas 输出的 3200×1800 透明 PNG（垫了棋盘格以显示透明区域）", K.ink, 30);
            s.Background(Color.white);
        }
    }
}
