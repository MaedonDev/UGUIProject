// 第 20 章配图与配套场景。
// 图表里的数字全部来自 Ch20/Results/perflab_results.txt（由 Ch20_PerfLab 场景在 Play 模式下实测生成）；
// Overdraw 图在这里现场渲染、逐像素统计。
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;

        // ---------------------------------------------------------------- 读实验结果
        static Dictionary<string, Dictionary<string, double>> s_Results;
        static string s_Env;

        static Dictionary<string, Dictionary<string, double>> Results()
        {
            if (s_Results != null) return s_Results;
            s_Results = new Dictionary<string, Dictionary<string, double>>();
            var path = Path.Combine(Application.dataPath, "UGUIBook/Ch20/Results/perflab_results.txt");
            if (!File.Exists(path)) throw new System.Exception("先运行 UGUIBook › Ch20 › Run Perf Lab 生成 " + path);
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("# env ")) { s_Env = line.Substring(6); continue; }
                var p = line.Split('|');
                if (p.Length < 4) continue;
                var d = new Dictionary<string, double>();
                foreach (var kv in p[3].Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries))
                {
                    var x = kv.Split('=');
                    d[x[0]] = double.Parse(x[1], CultureInfo.InvariantCulture);
                }
                s_Results[p[1]] = d;
            }
            return s_Results;
        }

        static double R(string id, string metric) => Results()[id][metric];

        // ---------------------------------------------------------------- 条形图
        struct Bar { public string label; public double v; public Color c; public string text; }
        static Bar B(string label, double v, Color c, string text) => new Bar { label = label, v = v, c = c, text = text };

        static void Chart(Shot s, string title, string unit, Bar[] bars, double max, float labelW = 640, float rowH = 64)
        {
            s.Background(Color.white);
            var t = UIKit.Text(s.Root, title, 40, K.ink, TextAlignmentOptions.MidlineLeft, true, "Title");
            t.rectTransform.Place(new Vector2(0, 1), new Vector2(60, -24), new Vector2(1800, 60));
            float top = -110, barMax = s.W * 2 - labelW - 340;
            for (int i = 0; i < bars.Length; i++)
            {
                var b = bars[i];
                float y = top - i * rowH;
                var l = UIKit.Text(s.Root, b.label, 28, K.ink, TextAlignmentOptions.MidlineRight, false, "Label");
                l.rectTransform.Place(new Vector2(0, 1), new Vector2(40, y), new Vector2(labelW, rowH - 8));
                l.rectTransform.pivot = new Vector2(0, 1);
                var track = UIKit.Panel(s.Root, UIKit.Hex("#F1F2F7"), 8, "Track").rectTransform;
                track.Place(new Vector2(0, 1), new Vector2(labelW + 70, y - 10), new Vector2(barMax, rowH - 26));
                float w = Mathf.Max(6, (float)(b.v / max) * barMax);
                var bar = UIKit.Panel(s.Root, b.c, 8, "Bar").rectTransform;
                bar.Place(new Vector2(0, 1), new Vector2(labelW + 70, y - 10), new Vector2(w, rowH - 26));
                var v = UIKit.Text(s.Root, b.text, 26, K.ink, TextAlignmentOptions.MidlineLeft, true, "Value");
                v.rectTransform.Place(new Vector2(0, 1), new Vector2(labelW + 84 + w, y), new Vector2(320, rowH - 8));
            }
            var env = UIKit.Text(s.Root, "作者环境：" + EnvShort() + "　单位：" + unit, 22, K.muted, TextAlignmentOptions.MidlineLeft);
            env.rectTransform.Place(new Vector2(0, 0), new Vector2(60, 14), new Vector2(1800, 40));
        }

        static string EnvShort() { Results(); return "Apple M5 / macOS / Unity 2022.3.62f3 / Metal / 编辑器 Play 模式"; }

        static string Ms(double v) => v < 0.1 ? v.ToString("0.000") + " ms" : v.ToString("0.00") + " ms";

        // ---------------------------------------------------------------- 1. 动静分离
        [BookShot("ch20-rebuild-chart", W = 960, H = 320)]
        static void RebuildChart(Shot s)
        {
            Color same = K.danger, sep = K.success, gray = K.muted;
            string[] ids = { "rb_static", "rb_color_same", "rb_color_sep", "rb_move_same", "rb_move_sep", "rb_text_same", "rb_text_sep" };
            string[] labels = { "静态界面（对照）", "改颜色 · 同一 Canvas", "改颜色 · 动静分离", "移动 · 同一 Canvas", "移动 · 动静分离", "改文字 · 同一 Canvas", "改文字 · 动静分离" };
            var bars = ids.Select((id, i) => B(labels[i], R(id, "render_ms"), i == 0 ? gray : i % 2 == 1 ? same : sep, Ms(R(id, "render_ms")))).ToArray();
            Chart(s, "1500 个静态图标的画布里每帧改 1 个元素：UI 相机每帧渲染耗时（含 Canvas 合批）", "毫秒/帧（60 帧平均）", bars, 2.0, 520, 62);
        }

        [BookShot("ch20-layout-chart", W = 960, H = 175)]
        static void LayoutChart(Shot s)
        {
            Chart(s, "60 行列表每帧改一行文字：C# 重建耗时（Canvas.ForceUpdateCanvases）", "毫秒/帧", new[]
            {
                B("布局组嵌套 + ContentSizeFitter", R("ly_nested", "rebuild_ms"), K.danger, Ms(R("ly_nested", "rebuild_ms"))),
                B("锚点直接定位", R("ly_flat", "rebuild_ms"), K.success, Ms(R("ly_flat", "rebuild_ms"))),
            }, 7, 520, 62);
        }

        [BookShot("ch20-hide-chart", W = 960, H = 340)]
        static void HideChart(Shot s)
        {
            string[] m = { "SetActive", "CanvasGroup", "CanvasEnabled", "MoveAway", "ScaleZero" };
            string[] n = { "SetActive", "CanvasGroup.alpha", "Canvas.enabled", "移出屏幕", "缩放为 0" };
            var bars = new List<Bar>();
            for (int i = 0; i < 5; i++)
            {
                double show = R("hd_toggle_" + m[i], "rebuild_ms@show") + R("hd_toggle_" + m[i], "render_ms@show");
                double hide = R("hd_toggle_" + m[i], "rebuild_ms@hide") + R("hd_toggle_" + m[i], "render_ms@hide");
                bars.Add(B(n[i] + " · 显示", show, K.primary, Ms(show)));
                bars.Add(B(n[i] + " · 隐藏", hide, UIKit.Hex("#A79BF0"), Ms(hide)));
            }
            Chart(s, "400 个图形的弹窗切换一次的主线程耗时（C# 重建 + UI 渲染）", "毫秒/次（30 次平均）", bars.ToArray(), 12, 420, 52);
        }

        [BookShot("ch20-raycast-chart", W = 960, H = 205)]
        static void RaycastChart(Shot s)
        {
            Chart(s, "2000 个图片，一次 EventSystem.RaycastAll 的耗时", "微秒/次（100 次 × 60 帧平均）", new[]
            {
                B("2000 个开着 Raycast Target", R("rc_2000", "raycast_us"), K.danger, R("rc_2000", "raycast_us").ToString("0") + " μs"),
                B("200 个开着", R("rc_200", "raycast_us"), K.accent, R("rc_200", "raycast_us").ToString("0.0") + " μs"),
                B("20 个开着", R("rc_20", "raycast_us"), K.success, R("rc_20", "raycast_us").ToString("0.0") + " μs"),
            }, 700, 520, 62);
        }

        [BookShot("ch20-pool-chart", W = 960, H = 175)]
        static void PoolChart(Shot s)
        {
            Chart(s, "每帧换 10 行列表项：托管内存分配（GC Allocated In Frame）", "KB/帧", new[]
            {
                B("Destroy + Instantiate", R("pl_instantiate", "gc_kb"), K.danger, R("pl_instantiate", "gc_kb").ToString("0") + " KB"),
                B("对象池取还（UIPool）", R("pl_pool", "gc_kb"), K.success, R("pl_pool", "gc_kb").ToString("0.0") + " KB"),
            }, 380, 520, 62);
        }

        // ---------------------------------------------------------------- 2. 合批：三种摆法
        static Sprite AtlasSprite(string n) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Ch20Assets.AtlasDir}/{n}.png");

        [BookShot("ch20-batch-overlap", W = 640, H = 400, Panels = 3)]
        static void BatchOverlap(Shot s)
        {
            Ch20Assets.EnsureAtlas();
            s.Ref(1920, 1200);
            s.Background(Color.white);
            bool overlap = s.Panel > 0;
            for (int i = 0; i < 100; i++)
            {
                float x = overlap ? -800 + i * 16 : -900 + (i % 20) * 90;
                float y = overlap ? 330 - i * 6 : 380 - (i / 20) * 90;
                var name = i % 2 == 0 ? "star" : "heart";
                var img = UIKit.Image(s.Root, null, i % 2 == 0 ? K.accent : K.danger, "Icon_" + name);
                img.sprite = s.Panel == 2 ? AtlasSprite(name) : UIKit.Sp(name);
                img.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(80, 80));
            }
            string[] ids = { "bt_grid_2tex", "bt_overlap_2tex", "bt_overlap_atlas" };
            string[] cap = { "两张贴图 · 互不重叠", "两张贴图 · 依次重叠", "同一图集 · 依次重叠" };
            int batches = (int)R(ids[s.Panel], "batches") - (int)R("bt_empty", "batches");
            var c = UIKit.Text(s.Root, cap[s.Panel], 56, K.ink, TextAlignmentOptions.Center, true);
            c.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1800, 80));
            var b = UIKit.Text(s.Root, $"实测 UI 批次：{batches}", 64, batches > 10 ? K.danger : K.success, TextAlignmentOptions.Center, true);
            b.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1800, 90));
        }

        // ---------------------------------------------------------------- 3. Overdraw 热力图
        /// 一个“背包”全屏界面开在主城界面上面。optimized = 打开全屏界面时关掉下面的主城、去掉大面积的装饰光晕、格子底板与边框合成一张图
        static RectTransform BagScreen(RectTransform root, bool optimized)
        {
            // 主城界面（被背包完全挡住）
            var city = UIKit.Node(root, "CityScreen").Stretch();
            UIKit.Image(city, "bg_landscape", Color.white, "CityBg").rectTransform.Stretch();
            for (int i = 0; i < 8; i++)
            {
                var b = UIKit.Button(city, "建筑 " + i, K.primary, 360, 120);
                b.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0.5f), new Vector2(-600 + (i % 4) * 400, 200 - (i / 4) * 300), new Vector2(360, 120));
            }
            if (optimized) city.gameObject.SetActive(false);

            // 背包界面
            var bag = UIKit.Node(root, "BagScreen").Stretch();
            UIKit.Panel(bag, UIKit.Hex("#2B3150"), 0, "Bg").rectTransform.Stretch();
            if (!optimized)
            {
                // 大面积装饰：一张几乎全透明的光晕图、一层半透明暗角
                var glow = UIKit.Image(bag, "glow_24", new Color(0.36f, 0.29f, 0.84f, 0.5f), "DecoGlow");
                glow.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1800, 1800));
                var vignette = UIKit.Image(bag, "gradient_v", new Color(0, 0, 0, 0.3f), "Vignette");
                vignette.rectTransform.Stretch();
            }
            var title = UIKit.Text(bag, "背包", 60, Color.white, TextAlignmentOptions.Center, true, "Title");
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(600, 90));
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_gem", "item_coin", "item_apple", "item_key", "item_scroll" };
            for (int i = 0; i < 24; i++)
            {
                var pos = new Vector2(-750 + (i % 8) * 214, 230 - (i / 8) * 214);
                if (!optimized)
                {
                    // 每个格子四层：阴影 + 底板 + 边框 + 图标
                    var sh = UIKit.Image(bag, "shadow_16", new Color(0, 0, 0, 0.35f), "SlotShadow");
                    sh.rectTransform.Place(new Vector2(0.5f, 0.5f), pos + new Vector2(0, -8), new Vector2(230, 230));
                    UIKit.Image(bag, "round_16", UIKit.Hex("#3A4166"), "SlotBg").rectTransform.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(190, 190));
                    UIKit.Image(bag, "round_outline_16", UIKit.Hex("#5B4BD6"), "SlotFrame").rectTransform.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(190, 190));
                }
                else UIKit.Image(bag, "slot", Color.white, "Slot").rectTransform.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(190, 190));
                UIKit.Icon(bag, items[i % 8], 140, Color.white).rectTransform.anchoredPosition = pos;
            }
            return bag;
        }

        static readonly Color32[] Heat =
        {
            new Color32(12, 14, 24, 255), new Color32(40, 70, 160, 255), new Color32(40, 170, 120, 255),
            new Color32(230, 210, 60, 255), new Color32(255, 140, 50, 255), new Color32(230, 60, 60, 255), new Color32(190, 50, 170, 255),
            new Color32(255, 255, 255, 255),
        };

        //#region measure-overdraw
        /// 把当前画面用 Overdraw 材质渲染一遍，逐像素数“被画了几层”。返回每像素层数
        static int[] MeasureOverdraw(Shot s, int w, int h)
        {
            var mat = s.MatAsset("Ch20", "Overdraw", "UGUIBook/Ch20/Overdraw");
            mat.SetColor("_Step", new Color(16 / 255f, 16 / 255f, 16 / 255f, 1));   // 每层加 16，最多数到 15 层
            foreach (var g in Object.FindObjectsOfType<Graphic>())
            {
                if (g is TMP_Text t) t.fontSharedMaterial = mat; else g.material = mat;
            }
            Canvas.ForceUpdateCanvases();
            var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var prevTarget = s.Cam.targetTexture; var prevBg = s.Cam.backgroundColor;
            s.Cam.targetTexture = rt; s.Cam.backgroundColor = Color.black;
            s.Cam.Render();
            var prevActive = RenderTexture.active; RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            RenderTexture.active = prevActive;
            s.Cam.targetTexture = prevTarget; s.Cam.backgroundColor = prevBg;
            RenderTexture.ReleaseTemporary(rt);
            var px = tex.GetPixels32();
            Object.DestroyImmediate(tex);
            return px.Select(p => Mathf.RoundToInt(p.r / 16f)).ToArray();
        }
        //#endregion

        [BookShot("ch20-overdraw", W = 640, H = 360, Panels = 3)]
        static void Overdraw(Shot s)
        {
            bool optimized = s.Panel == 2;
            s.Ref(1920, 1080);
            BagScreen(s.Root, optimized);
            if (s.Panel == 0)
            {
                s.Label(new Vector2(0, -470), "看到的画面（两种做法看起来几乎一样）", K.ink, 48);
                return;
            }
            s.BeforeCapture(() =>
            {
                int w = 960, h = 540;
                var layers = MeasureOverdraw(s, w, h);
                double avg = layers.Average();
                double p4 = layers.Count(x => x >= 4) / (double)layers.Length;
                int max = layers.Max();
                Debug.Log($"OVERDRAW {(optimized ? "optimized" : "original")} avg={avg:0.00} p4={p4:P1} max={max}");
                var heat = new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                heat.SetPixels32(layers.Select(n => Heat[Mathf.Min(n, Heat.Length - 1)]).ToArray());
                heat.Apply();
                foreach (var c in Object.FindObjectsOfType<Canvas>()) if (c.isRootCanvas) c.gameObject.SetActive(false);
                var hc = UIKit.Canvas("Heatmap", RenderMode.ScreenSpaceCamera, 1920, 1080);
                hc.worldCamera = s.Cam; hc.planeDistance = 100;
                var raw = new GameObject("Heat", typeof(RectTransform)).AddComponent<RawImage>();
                raw.rectTransform.SetParent(hc.transform, false); raw.rectTransform.Stretch(); raw.texture = heat;
                var legend = UIKit.Panel(hc.transform, new Color(1, 1, 1, 0.94f), 16, "Legend").rectTransform;
                legend.Place(new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(1860, 130));
                var txt = UIKit.Text(legend, $"{(optimized ? "优化后" : "优化前")}：平均 {avg:0.00} 层　≥4 层的像素 {p4:P0}　最多 {max} 层", 50, K.ink, TextAlignmentOptions.Center, true);
                txt.rectTransform.Stretch();
                Canvas.ForceUpdateCanvases();
            });
        }

        [BookShot("ch20-overdraw-legend", W = 640, H = 60, Png = true)]
        static void OverdrawLegend(Shot s)
        {
            s.Background(UIKit.Hex("#E0DEEF"));   // 浅灰底：白色的 “7+” 色块才看得见
            var row = UIKit.Node(s.Root, "Legend").Stretch(); row.HLayout(10);
            string[] names = { "0 层", "1", "2", "3", "4", "5", "6", "7+" };
            for (int i = 0; i < Heat.Length; i++)
            {
                var sw = UIKit.Panel(row, Heat[i], 8, "Swatch"); sw.rectTransform.sizeDelta = new Vector2(140, 80);
                var t = UIKit.Text(sw.transform, names[i], 34, i >= 3 && i < 6 || i == 7 ? K.ink : Color.white, TextAlignmentOptions.Center, true); t.rectTransform.Stretch();
            }
        }

        // ---------------------------------------------------------------- 配套场景
        [BookScene("Ch20_PerfLab")]
        static void PerfLabScene(Shot s)
        {
            Object.DestroyImmediate(s.Canvas.gameObject); Object.DestroyImmediate(s.Cam.gameObject);
            Object.DestroyImmediate(Object.FindObjectOfType<EventSystem>().gameObject);
            PerfLabRunner.Build();
        }

        static TextMeshProUGUI HudText(RectTransform root, Vector2 pos)
        {
            var hud = UIKit.Panel(root, new Color(0.12f, 0.14f, 0.21f, 0.88f), 16, "StatsHUD").rectTransform;
            hud.Place(new Vector2(0, 1), pos, new Vector2(820, 130));
            var t = UIKit.Text(hud, "Play 后显示 Batches / BuildBatch / GC", 26, Color.white, TextAlignmentOptions.TopLeft);
            t.rectTransform.Stretch(20, 14, 20, 10);
            var comp = hud.gameObject.AddComponent<RenderStatsHUD>();
            var so = new SerializedObject(comp); so.FindProperty("m_Label").objectReferenceValue = t; so.ApplyModifiedPropertiesWithoutUndo();
            return t;
        }

        [BookScene("Ch20_Overdraw")]
        static void OverdrawScene(Shot s)
        {
            BagScreen(s.Root, false);
            var back = UIKit.Canvas("OverdrawBackdrop"); back.sortingOrder = -1000;
            var black = UIKit.Panel(back.transform, Color.black, 0, "Black"); black.rectTransform.Stretch(); black.raycastTarget = false;
            back.gameObject.SetActive(false);
            var view = new GameObject("OverdrawView").AddComponent<OverdrawView>();
            view.overdrawMaterial = s.MatAsset("Ch20", "OverdrawView", "UGUIBook/Ch20/Overdraw");
            view.overdrawMaterial.SetColor("_Step", new Color(0.12f, 0.06f, 0.02f, 1));
            view.blackBackdrop = back.gameObject;
            var hint = UIKit.Text(s.Root, "按 F2 切换 Overdraw 视图：越亮 = 同一像素被画的层数越多", 30, Color.white);
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(1600, 50));
        }

        [BookScene("Ch20_DynamicStatic")]
        static void DynamicStaticScene(Shot s)
        {
            var wall = LabUI.Wall(s.Root, 1500, UIKit.Sp("star"), 32, false);
            var dynCanvas = UIKit.Node(s.Root, "DynamicCanvas").Stretch();
            dynCanvas.gameObject.AddComponent<Canvas>();
            dynCanvas.gameObject.AddComponent<GraphicRaycaster>();
            var dyn = UIKit.Node(s.Root, "Dynamic").Stretch();
            var card = UIKit.Panel(dyn, new Color(0.12f, 0.14f, 0.21f, 0.92f), 24, "TimerCard").rectTransform;
            card.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(620, 260));
            var timer = UIKit.Text(card, "0.00", 110, Color.white, TextAlignmentOptions.Center, true, "Timer");
            timer.rectTransform.Stretch(160, 0, 20, 0);
            var spin = UIKit.Icon(card, "refresh", 140, K.accent); spin.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(140, 140));
            spin.rectTransform.pivot = new Vector2(0.5f, 0.5f); spin.rectTransform.anchoredPosition = new Vector2(110, 0);
            var btn = UIKit.Button(s.Root, "切换", K.primary, 760, 110, null, 36);
            btn.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(760, 110));
            HudText(s.Root, new Vector2(40, -40));
            var demo = new GameObject("SeparationDemo").AddComponent<SeparationDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_Dynamic").objectReferenceValue = dyn;
            so.FindProperty("m_StaticParent").objectReferenceValue = s.Root;
            so.FindProperty("m_DynamicCanvas").objectReferenceValue = dynCanvas;
            so.FindProperty("m_Timer").objectReferenceValue = timer;
            so.FindProperty("m_Spinner").objectReferenceValue = spin.rectTransform;
            so.FindProperty("m_Toggle").objectReferenceValue = btn;
            so.FindProperty("m_ToggleLabel").objectReferenceValue = btn.GetComponentInChildren<TMP_Text>();
            so.ApplyModifiedPropertiesWithoutUndo();
            wall.SetAsFirstSibling();
        }

        [BookScene("Ch20_HideMethods")]
        static void HideMethodsScene(Shot s)
        {
            LabUI.Wall(s.Root, 800, UIKit.Sp("star"), 32, false);
            var p = UIKit.Node(s.Root, "Popup");
            p.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1500, 760));
            p.gameObject.AddComponent<Canvas>(); p.gameObject.AddComponent<GraphicRaycaster>(); p.gameObject.AddComponent<CanvasGroup>();
            UIKit.Panel(p, Color.white, 24, "Bg").rectTransform.Stretch();
            for (int i = 0; i < 80; i++)
            {
                var cell = UIKit.Panel(p, UIKit.Hex("#EEF0F6"), 8, "Cell").rectTransform;
                cell.Place(new Vector2(0.5f, 0.5f), new Vector2(-555 + (i % 4) * 370, 340 - (i / 4) * 36), new Vector2(360, 32));
                UIKit.Icon(cell, "item_gem", 28, Color.white).rectTransform.anchoredPosition = new Vector2(-160, 0);
                UIKit.Text(cell, "道具 " + i, 20, K.ink).rectTransform.sizeDelta = new Vector2(200, 30);
            }
            var bar = UIKit.Node(s.Root, "Methods"); bar.Place(new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(1800, 90)); bar.HLayout(16);
            var buttons = new List<Button>();
            foreach (var n in new[] { "SetActive", "CanvasGroup", "Canvas.enabled", "移出屏幕", "缩放为 0" })
                buttons.Add(UIKit.Button(bar, n, UIKit.Hex("#E1E4EE"), 300, 80, null, 28));
            var toggle = UIKit.Button(s.Root, "显示 / 隐藏", K.primary, 400, 96, null, 34);
            toggle.GetComponent<RectTransform>().Place(new Vector2(0.5f, 0), new Vector2(-300, 30), new Vector2(400, 96));
            var status = UIKit.Text(s.Root, "", 28, K.ink, TextAlignmentOptions.MidlineLeft);
            status.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(380, 30), new Vector2(1000, 96));
            HudText(s.Root, new Vector2(40, -20));
            var demo = new GameObject("HideMethodsDemo").AddComponent<HideMethodsDemo>();
            var so = new SerializedObject(demo);
            so.FindProperty("m_Panel").objectReferenceValue = p;
            var arr = so.FindProperty("m_MethodButtons"); arr.arraySize = buttons.Count;
            for (int i = 0; i < buttons.Count; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
            so.FindProperty("m_ToggleButton").objectReferenceValue = toggle;
            so.FindProperty("m_Status").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
