// 第 20 章所有实验的定义。每个实验 = Setup（搭界面）+ Tick（每帧做的改动）。
// 测量由 PerfLab 统一完成；实验自己要额外记录的数（比如射线检测耗时）用 c.Add / c.Set。
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    /// 实验里搭界面用的小工具（不依赖编辑器，Play 模式下即时创建）
    public static class LabUI
    {
        public static Sprite Sp(string name) => UISkin.Sprite(name);

        public static RectTransform Node(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        public static Image Img(Transform parent, Sprite sp, float x, float y, float w, float h, Color? col = null, string name = "Image")
        {
            var rt = Node(parent, name);
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sp;
            if (sp != null && sp.border != Vector4.zero) img.type = Image.Type.Sliced;
            img.color = col ?? Color.white;
            return img;
        }

        public static TextMeshProUGUI Txt(Transform parent, string text, float x, float y, float w, float h, float size = 28, string name = "Text")
        {
            var rt = Node(parent, name);
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = UISkin.Instance.font;
            t.fontSize = size;
            t.color = UISkin.Instance.ink;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
            t.text = text;
            return t;
        }

        /// 铺满屏幕的一面“图标墙”：count 个小图片，模拟一个元素很多的静态界面
        public static RectTransform Wall(Transform parent, int count, Sprite sp, float cell = 32, bool raycast = true)
        {
            var wall = Node(parent, "Wall");
            int cols = Mathf.FloorToInt(1840 / cell);
            for (int i = 0; i < count; i++)
            {
                float x = -920 + cell * 0.5f + (i % cols) * cell, y = 520 - cell * 0.5f - (i / cols) * cell;
                var img = Img(wall, sp, x, y, cell - 4, cell - 4, Color.HSVToRGB(i % 37 / 37f, 0.35f, 0.95f), "Tile");
                img.raycastTarget = raycast;
            }
            return wall;
        }

        /// 列表项：底板 + 图标 + 名字 + 价格（绝对定位，不用布局组）
        public static RectTransform Row(Transform parent, Sprite bg, Sprite icon, string label, string price, float x, float y, float w = 520)
        {
            var row = Node(parent, "Row");
            row.anchoredPosition = new Vector2(x, y); row.sizeDelta = new Vector2(w, 80);
            var b = Img(row, bg, 0, 0, w, 80, Color.white, "Bg"); b.raycastTarget = false;
            var ic = Img(row, icon, -w / 2 + 50, 0, 60, 60, Color.white, "Icon"); ic.raycastTarget = false;
            Txt(row, label, -w / 2 + 100 + 130, 12, 260, 36, 26, "Name");
            var p = Txt(row, price, w / 2 - 80, 0, 120, 36, 24, "Price"); p.alignment = TextAlignmentOptions.MidlineRight;
            return row;
        }
    }

    public static class PerfLabCases
    {
        class Lambda : LabCase
        {
            public Action<LabContext> OnSetup;
            public Action<LabContext, int> OnTick;
            public override void Setup(LabContext c) => OnSetup(c);
            public override void Tick(LabContext c, int f) => OnTick?.Invoke(c, f);
        }

        static LabCase Case(string group, string id, string title, Action<LabContext> setup, Action<LabContext, int> tick = null, int frames = 60) =>
            new Lambda { Group = group, Id = id, Title = title, OnSetup = setup, OnTick = tick, Frames = frames };

        static Sprite Sp(string n) => LabUI.Sp(n);
        static readonly string[] Items = { "item_sword", "item_shield", "item_potion_red", "item_gem", "item_coin", "item_apple" };

        public static IEnumerable<LabCase> All(PerfLab lab)
        {
            Sprite Atlas(string n) => lab.atlasSprites.FirstOrDefault(s => s != null && s.name == n);

            // ---------------- 1. 重建与动静分离 ----------------
            //#region rebuild
            // 静态部分：1500 个图标 + 40 个文字；动态部分：每帧改一个元素。
            // separate = true 时，动态元素放进一个嵌套 Canvas（动静分离）
            Transform Screen(LabContext c, bool separate)
            {
                LabUI.Wall(c.Root, 1500, Sp("star"));
                for (int i = 0; i < 40; i++)
                    LabUI.Txt(c.Root, "Lv." + i, -860 + (i % 10) * 190, 300 - (i / 10) * 160, 160, 40, 26);
                if (!separate) return c.Root;
                var dyn = LabUI.Node(c.Root, "Dynamic");
                dyn.gameObject.AddComponent<Canvas>();    // 嵌套 Canvas：自己单独合批，变化不会让外层重新合批
                return dyn;
            }

            yield return Case("rebuild", "rb_static", "静态界面，什么都不改", c => Screen(c, false));
            foreach (var sep in new[] { false, true })
            {
                string s = sep ? "sep" : "same", t = sep ? "（动静分离）" : "（同一 Canvas）";
                Image img = null; TextMeshProUGUI txt = null;
                yield return Case("rebuild", "rb_color_" + s, "每帧改 1 个图片颜色" + t,
                    c => img = LabUI.Img(Screen(c, sep), Sp("circle"), 0, -200, 120, 120),
                    (c, f) => img.color = Color.HSVToRGB(f % 60 / 60f, 0.6f, 1f));
                yield return Case("rebuild", "rb_move_" + s, "每帧移动 1 个图片" + t,
                    c => img = LabUI.Img(Screen(c, sep), Sp("circle"), 0, -200, 120, 120),
                    (c, f) => img.rectTransform.anchoredPosition = new Vector2(Mathf.Sin(f * 0.2f) * 300, -200));
                yield return Case("rebuild", "rb_text_" + s, "每帧改 1 个文字" + t,
                    c => txt = LabUI.Txt(Screen(c, sep), "0", 0, -200, 300, 60, 48),
                    (c, f) => txt.SetText("{0}", f));
            }
            //#endregion

            // ---------------- 2. 布局嵌套 ----------------
            //#region layout
            TextMeshProUGUI[] descs = null;
            yield return Case("layout", "ly_nested", "60 行列表：布局组嵌套 + ContentSizeFitter，每帧改一行文字", c =>
            {
                var list = LabUI.Node(c.Root, "List");
                list.anchorMin = list.anchorMax = list.pivot = new Vector2(0.5f, 1);
                list.anchoredPosition = new Vector2(0, -20); list.sizeDelta = new Vector2(900, 0);
                var vg = list.gameObject.AddComponent<VerticalLayoutGroup>();
                vg.spacing = 4; vg.childControlWidth = vg.childControlHeight = true; vg.childForceExpandHeight = false;
                list.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                descs = new TextMeshProUGUI[60];
                for (int i = 0; i < 60; i++)
                {
                    var row = LabUI.Img(list, Sp("round_16"), 0, 0, 0, 0, Color.white, "Row").rectTransform;
                    var hg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
                    hg.padding = new RectOffset(12, 12, 6, 6); hg.spacing = 12;
                    hg.childControlWidth = hg.childControlHeight = true; hg.childForceExpandWidth = false;
                    var icon = LabUI.Img(row, Sp(Items[i % 6]), 0, 0, 0, 0);
                    var le = icon.gameObject.AddComponent<LayoutElement>(); le.preferredWidth = le.preferredHeight = 48;
                    var col = LabUI.Node(row, "Col");
                    var cv = col.gameObject.AddComponent<VerticalLayoutGroup>();
                    cv.childControlWidth = cv.childControlHeight = true; cv.childForceExpandHeight = false;
                    col.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
                    LabUI.Txt(col, "道具 " + i, 0, 0, 0, 0, 24, "Name");
                    descs[i] = LabUI.Txt(col, "这是一段描述文字", 0, 0, 0, 0, 18, "Desc");
                    var btn = LabUI.Img(row, Sp("pill"), 0, 0, 0, 0, UISkin.Instance.primary, "Btn");
                    var be = btn.gameObject.AddComponent<LayoutElement>(); be.preferredWidth = 110; be.preferredHeight = 40;
                }
            }, (c, f) => descs[f % 60].SetText("描述已更新 {0}", f));

            yield return Case("layout", "ly_flat", "60 行列表：锚点直接定位（无布局组），每帧改一行文字", c =>
            {
                descs = new TextMeshProUGUI[60];
                for (int i = 0; i < 60; i++)
                {
                    float y = 520 - 30 - i * 64;
                    var row = LabUI.Img(c.Root, Sp("round_16"), 0, y, 900, 60, Color.white, "Row").rectTransform;
                    LabUI.Img(row, Sp(Items[i % 6]), -450 + 36, 0, 48, 48);
                    LabUI.Txt(row, "道具 " + i, -100, 12, 600, 30, 24, "Name");
                    descs[i] = LabUI.Txt(row, "这是一段描述文字", -100, -14, 600, 24, 18, "Desc");
                    LabUI.Img(row, Sp("pill"), 450 - 70, 0, 110, 40, UISkin.Instance.primary, "Btn");
                }
            }, (c, f) => descs[f % 60].SetText("描述已更新 {0}", f));
            //#endregion

            // ---------------- 3. 隐藏界面的五种方法 ----------------
            RectTransform popup = null;
            foreach (HideMethod m in Enum.GetValues(typeof(HideMethod)))
            {
                yield return Case("hide", "hd_toggle_" + m, HideName(m) + "：每帧切换显示/隐藏",
                    c => popup = Popup(c),
                    (c, f) => { bool show = f % 2 == 1; SetVisible(popup, m, show); c.Tag = show ? "show" : "hide"; });
                yield return Case("hide", "hd_hidden_" + m, HideName(m) + "：隐藏后保持不动",
                    c => { popup = Popup(c); SetVisible(popup, m, false); });
            }
            yield return Case("hide", "hd_hidden_CanvasGroup_nocull", "CanvasGroup.alpha：隐藏后保持不动（关闭 Cull Transparent Mesh）", c =>
            {
                popup = Popup(c);
                foreach (var r in popup.GetComponentsInChildren<CanvasRenderer>()) r.cullTransparentMesh = false;
                SetVisible(popup, HideMethod.CanvasGroup, false);
            });
            yield return Case("hide", "hd_visible", "弹窗显示、保持不动（对照）", c => Popup(c));

            // ---------------- 4. 合批 ----------------
            yield return Case("batch", "bt_empty", "空画布（基线）", c => { }, null, 30);
            yield return Case("batch", "bt_one", "1 个图片", c => LabUI.Img(c.Root, Sp("star"), 0, 0, 100, 100), null, 30);
            //#region batch-overlap
            // 100 个图标，star/heart 两种贴图交替；grid = 互不重叠，overlap = 每个都压住前一个
            void Icons(LabContext c, bool overlap, Func<int, Sprite> sprite)
            {
                for (int i = 0; i < 100; i++)
                {
                    float x = overlap ? -800 + i * 16 : -900 + (i % 20) * 90;
                    float y = overlap ? 300 - i * 6 : 400 - (i / 20) * 90;
                    LabUI.Img(c.Root, sprite(i), x, y, 80, 80);
                }
            }
            yield return Case("batch", "bt_grid_2tex", "100 图标·两张贴图交替·互不重叠",
                c => Icons(c, false, i => Sp(i % 2 == 0 ? "star" : "heart")), null, 30);
            yield return Case("batch", "bt_overlap_2tex", "100 图标·两张贴图交替·依次重叠",
                c => Icons(c, true, i => Sp(i % 2 == 0 ? "star" : "heart")), null, 30);
            yield return Case("batch", "bt_overlap_atlas", "100 图标·同一图集·依次重叠",
                c => Icons(c, true, i => Atlas(i % 2 == 0 ? "star" : "heart")), null, 30);
            //#endregion
            yield return Case("batch", "bt_overlap_1tex", "100 图标·同一张贴图·依次重叠",
                c => Icons(c, true, i => Sp("star")), null, 30);
            yield return Case("batch", "bt_list_sep", "12 行商店列表·各自独立贴图",
                c => { for (int i = 0; i < 12; i++) LabUI.Row(c.Root, Sp("round_16"), Sp(Items[i % 6]), "道具 " + i, "120", 0, 470 - i * 86); }, null, 30);
            yield return Case("batch", "bt_list_atlas", "12 行商店列表·底板与图标在同一图集",
                c => { for (int i = 0; i < 12; i++) LabUI.Row(c.Root, Atlas("round_16"), Atlas(Items[i % 6]), "道具 " + i, "120", 0, 470 - i * 86); }, null, 30);
            //#region batch-z
            yield return Case("batch", "bt_z0", "100 个同贴图图标·Z = 0",
                c => Icons(c, false, i => Sp("star")), null, 30);
            yield return Case("batch", "bt_znot0", "100 个同贴图图标·一半 Z ≠ 0",
                c =>
                {
                    Icons(c, false, i => Sp("star"));
                    int k = 0;
                    foreach (RectTransform t in c.Root) if (k++ % 2 == 0) t.localPosition += new Vector3(0, 0, -5);
                }, null, 30);
            yield return Case("batch", "bt_material", "100 个同贴图图标·每 10 个夹一个不同材质",
                c =>
                {
                    Icons(c, false, i => Sp("star"));
                    var other = new Material(Graphic.defaultGraphicMaterial) { name = "OtherMat" };
                    int k = 0;
                    foreach (RectTransform t in c.Root) if (k++ % 10 == 5) t.GetComponent<Image>().material = other;
                }, null, 30);
            yield return Case("batch", "bt_material_overlap", "100 个同贴图图标·依次重叠·每 10 个夹一个不同材质",
                c =>
                {
                    Icons(c, true, i => Sp("star"));
                    var other = new Material(Graphic.defaultGraphicMaterial) { name = "OtherMat" };
                    int k = 0;
                    foreach (RectTransform t in c.Root) if (k++ % 10 == 5) t.GetComponent<Image>().material = other;
                }, null, 30);
            //#endregion
            //#region batch-mask
            foreach (var mode in new[] { "none", "mask", "rect" })
            {
                yield return Case("batch", "bt_clip_" + mode, "4 个滚动列表·" + (mode == "none" ? "不裁剪" : mode == "mask" ? "Mask" : "RectMask2D"), c =>
                {
                    for (int v = 0; v < 4; v++)
                    {
                        var bg = LabUI.Img(c.Root, Atlas("round_16"), -690 + v * 460, 0, 420, 700, new Color(0.93f, 0.94f, 0.97f), "Viewport");
                        var vp = bg.rectTransform;
                        if (mode == "mask") vp.gameObject.AddComponent<Mask>();
                        else if (mode == "rect") vp.gameObject.AddComponent<RectMask2D>();
                        for (int i = 0; i < 12; i++)   // 12 行 × 86 = 1032 高，比视口高，下面几行在视口外
                            LabUI.Row(vp, Atlas("round_16"), Atlas(Items[(i + v) % 6]), "道具 " + i, "99", 0, 300 - i * 86, 390);
                    }
                }, null, 30);
            }
            foreach (var mode in new[] { "none", "mask", "rect" })
            {
                yield return Case("batch", "bt_clip1_" + mode, "1 个滚动列表·" + (mode == "none" ? "不裁剪" : mode == "mask" ? "Mask" : "RectMask2D"), c =>
                {
                    var vp = LabUI.Img(c.Root, Atlas("round_16"), 0, 0, 420, 700, new Color(0.93f, 0.94f, 0.97f), "Viewport").rectTransform;
                    if (mode == "mask") vp.gameObject.AddComponent<Mask>();
                    else if (mode == "rect") vp.gameObject.AddComponent<RectMask2D>();
                    for (int i = 0; i < 12; i++)
                        LabUI.Row(vp, Atlas("round_16"), Atlas(Items[i % 6]), "道具 " + i, "99", 0, 300 - i * 86, 390);
                }, null, 30);
            }
            //#endregion
            //#region batch-click
            foreach (var mode in new[] { "alpha0", "alpha0_nocull", "empty" })
            {
                bool empty = mode == "empty";
                yield return Case("batch", "bt_click_" + mode, empty ? "全屏点击区：EmptyRaycast"
                    : mode == "alpha0" ? "全屏点击区：透明度 0 的 Image" : "全屏点击区：透明度 0 的 Image（关闭 Cull Transparent Mesh）", c =>
                    {
                        LabUI.Wall(c.Root, 200, Sp("star"), 60);
                        var area = LabUI.Node(c.Root, "ClickArea");
                        area.anchorMin = Vector2.zero; area.anchorMax = Vector2.one; area.sizeDelta = Vector2.zero;
                        if (empty) area.gameObject.AddComponent<EmptyRaycast>();
                        else area.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0);
                        // CanvasRenderer.cullTransparentMesh（Inspector 里的 Cull Transparent Mesh）：顶点全透明时不提交绘制
                        if (mode == "alpha0_nocull") area.GetComponent<CanvasRenderer>().cullTransparentMesh = false;
                    }, (c, f) => c.Add("hits_clickarea", TopHit(new Vector2(960, 540)) == "ClickArea" ? 1 : 0), 30);
            }
            //#endregion

            // ---------------- 5. 射线检测 ----------------
            //#region raycast
            foreach (int targets in new[] { 2000, 200, 20 })
            {
                yield return Case("raycast", "rc_" + targets, $"2000 个图片，其中 {targets} 个开着 Raycast Target", c =>
                {
                    var wall = LabUI.Wall(c.Root, 2000, Sp("star"), 32);
                    int k = 0;
                    foreach (var g in wall.GetComponentsInChildren<Graphic>()) g.raycastTarget = k++ < targets;
                }, (c, f) =>
                {
                    var es = EventSystem.current;
                    var ped = new PointerEventData(es);
                    var sw = Stopwatch.StartNew();
                    for (int i = 0; i < 100; i++)
                    {
                        ped.position = new Vector2(100 + i * 17 % 1700, 100 + i * 31 % 880);
                        s_Hits.Clear();
                        es.RaycastAll(ped, s_Hits);
                    }
                    c.Add("raycast_us", sw.Elapsed.TotalMilliseconds * 1000 / 100);
                });
            }
            //#endregion

            // ---------------- 6. TextMeshPro ----------------
            //#region tmp-settext
            TextMeshProUGUI[] labels = null;
            void Labels(LabContext c, int n)
            {
                labels = new TextMeshProUGUI[n];
                for (int i = 0; i < n; i++) labels[i] = LabUI.Txt(c.Root, "金币：0", -800 + (i % 5) * 360, 400 - (i / 5) * 90, 340, 60, 32);
            }
            yield return Case("tmp", "tm_concat", "30 个文字每帧更新：text = \"金币：\" + n", c => Labels(c, 30), (c, f) =>
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < labels.Length; i++) labels[i].text = "金币：" + (f * 7 + i);
                c.Add("tick_ms", sw.Elapsed.TotalMilliseconds);
            });
            yield return Case("tmp", "tm_settext", "30 个文字每帧更新：SetText(\"金币：{0}\", n)", c => Labels(c, 30), (c, f) =>
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < labels.Length; i++) labels[i].SetText("金币：{0}", f * 7 + i);
                c.Add("tick_ms", sw.Elapsed.TotalMilliseconds);
            });
            //#endregion
            //#region tmp-same
            const string same = "金币：100";
            yield return Case("tmp", "tm_same_text", "30 个文字每帧赋相同的值：text = 同一字符串", c => Labels(c, 30),
                (c, f) => { for (int i = 0; i < labels.Length; i++) labels[i].text = same; });
            yield return Case("tmp", "tm_same_settext", "30 个文字每帧赋相同的值：SetText(\"金币：{0}\", 100)", c => Labels(c, 30),
                (c, f) => { for (int i = 0; i < labels.Length; i++) labels[i].SetText("金币：{0}", 100); });
            //#endregion
            //#region tmp-autosize
            foreach (var auto in new[] { false, true })
            {
                yield return Case("tmp", auto ? "tm_autosize_on" : "tm_autosize_off", "12 段文字每帧改一段·Auto Size " + (auto ? "开" : "关"), c =>
                {
                    Labels(c, 12);
                    foreach (var t in labels)
                    {
                        t.rectTransform.sizeDelta = new Vector2(340, 80);
                        t.enableAutoSizing = auto; t.fontSizeMin = 12; t.fontSizeMax = 40;
                        t.text = "这是一段会随窗口大小自动缩放的说明文字，用来测试 Auto Size 的代价。";
                    }
                }, (c, f) => labels[f % 12].SetText("这是一段会随窗口大小自动缩放的说明文字，第 {0} 次更新。", f));
            }
            //#endregion
            //#region tmp-glyph
            yield return Case("tmp", "tm_first_glyph", "动态字体：首次出现的字 vs 已缓存的字", c =>
            {
                // 现建一个空的动态字体资源，保证这些字都是“第一次见”
                var fresh = TMP_FontAsset.CreateFontAsset(UISkin.Instance.legacyFont);
                fresh.name = "FreshDynamic";
                string chars = new string(Qianziwen.Distinct().ToArray());
                var a = LabUI.Txt(c.Root, chars, 0, 200, 1800, 400, 28); a.font = fresh; a.enableWordWrapping = true;
                var sw = Stopwatch.StartNew();
                a.ForceMeshUpdate();                       // 第一次：要把每个字光栅化成 SDF 写进图集
                c.Set("first_ms", sw.Elapsed.TotalMilliseconds);
                var b = LabUI.Txt(c.Root, chars, 0, -250, 1800, 400, 28); b.font = fresh; b.enableWordWrapping = true;
                sw.Restart();
                b.ForceMeshUpdate();                       // 第二次：字形已在图集里，只做排版
                c.Set("cached_ms", sw.Elapsed.TotalMilliseconds);
                c.Set("chars", chars.Length);
                c.Set("glyphs_in_atlas", fresh.characterTable.Count);
            }, null, 5);
            //#endregion

            // ---------------- 7. 对象池 ----------------
            //#region pool
            foreach (var pooled in new[] { false, true })
            {
                RectTransform template = null, list = null;
                UIPool<RectTransform> pool = null;
                var live = new List<RectTransform>();
                yield return Case("pool", pooled ? "pl_pool" : "pl_instantiate", pooled ? "每帧换 10 行：对象池取还" : "每帧换 10 行：Destroy + Instantiate", c =>
                {
                    live.Clear();
                    template = LabUI.Row(c.Root, Sp("round_16"), Sp("item_gem"), "道具", "0", 0, 0);
                    template.gameObject.SetActive(false);
                    list = LabUI.Node(c.Root, "List");
                    var poolRoot = LabUI.Node(c.Root, "Pool"); poolRoot.gameObject.SetActive(false);
                    pool = new UIPool<RectTransform>(template, poolRoot, 10);
                }, (c, f) =>
                {
                    var sw = Stopwatch.StartNew();
                    foreach (var r in live) { if (pooled) pool.Release(r); else UnityEngine.Object.Destroy(r.gameObject); }
                    live.Clear();
                    for (int i = 0; i < 10; i++)
                    {
                        var r = pooled ? pool.Get(list) : UnityEngine.Object.Instantiate(template, list, false);
                        if (!pooled) r.gameObject.SetActive(true);
                        r.anchoredPosition = new Vector2(0, 400 - i * 86);
                        r.Find("Price").GetComponent<TMP_Text>().SetText("{0}", f + i);
                        live.Add(r);
                    }
                    c.Add("tick_ms", sw.Elapsed.TotalMilliseconds);
                });
            }
            //#endregion

            // ---------------- 8. Animator ----------------
            //#region anim
            foreach (var mode in new[] { "none", "animator", "pulse" })
            {
                var ctrl = mode == "pulse" ? lab.pulseController : lab.idleController;
                bool withAnimator = mode != "none";
                yield return Case("anim", "an_" + mode, mode == "none" ? "40 个图标不挂 Animator"
                    : mode == "animator" ? "40 个图标挂 Animator（动画曲线是常量，看起来不动）" : "40 个图标挂 Animator（透明度在 1 和 0.9 之间呼吸）", c =>
                    {
                        LabUI.Wall(c.Root, 1000, Sp("star"));
                        for (int i = 0; i < 40; i++)
                        {
                            var img = LabUI.Img(c.Root, Sp("gift"), -800 + (i % 10) * 180, 200 - (i / 10) * 140, 100, 100, UISkin.Instance.accent, "Gift");
                            if (withAnimator && ctrl != null)
                                img.gameObject.AddComponent<Animator>().runtimeAnimatorController = ctrl;
                        }
                    });
            }
            //#endregion

            // ---------------- 9. 全屏界面与 3D 相机 ----------------
            //#region world
            foreach (var renderWorld in new[] { true, false })
            {
                yield return Case("world", renderWorld ? "wd_on" : "wd_off",
                    renderWorld ? "全屏界面打开，3D 相机仍在渲染" : "全屏界面打开，3D 相机已关闭", c =>
                    {
                        c.RenderWorld = renderWorld;
                        var mat = new Material(Shader.Find("Standard"));
                        var root = new GameObject("World3D");
                        c.Extra.Add(root);
                        for (int i = 0; i < 2000; i++)
                        {
                            // 球体 515 个顶点，超过动态合批上限（300），每个球都是一次 Draw Call，接近真实场景
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            UnityEngine.Object.Destroy(cube.GetComponent<Collider>());
                            cube.transform.SetParent(root.transform);
                            cube.transform.position = new Vector3(i % 50 - 25, (i / 50) % 40 - 20, 20 + i / 2000f);
                            cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
                        }
                        // 一个盖满屏幕的不透明界面：3D 画面被完全挡住
                        var full = LabUI.Img(c.Root, null, 0, 0, 0, 0, UISkin.Instance.page, "FullScreenPanel");
                        full.rectTransform.anchorMin = Vector2.zero; full.rectTransform.anchorMax = Vector2.one;
                        for (int i = 0; i < 12; i++) LabUI.Row(c.Root, Sp("round_16"), Sp(Items[i % 6]), "道具 " + i, "1", 0, 470 - i * 86);
                    });
            }
            //#endregion
        }

        static readonly List<RaycastResult> s_Hits = new List<RaycastResult>();

        static string TopHit(Vector2 pos)
        {
            var es = EventSystem.current;
            s_Hits.Clear();
            es.RaycastAll(new PointerEventData(es) { position = pos }, s_Hits);
            return s_Hits.Count > 0 ? s_Hits[0].gameObject.name : "";
        }

        //#region hide-methods
        public enum HideMethod { SetActive, CanvasGroup, CanvasEnabled, MoveAway, ScaleZero }

        public static void SetVisible(RectTransform panel, HideMethod m, bool visible)
        {
            switch (m)
            {
                case HideMethod.SetActive:
                    panel.gameObject.SetActive(visible);
                    break;
                case HideMethod.CanvasGroup:
                    var g = panel.GetComponent<CanvasGroup>();
                    g.alpha = visible ? 1 : 0;
                    g.blocksRaycasts = g.interactable = visible;
                    break;
                case HideMethod.CanvasEnabled:
                    panel.GetComponent<Canvas>().enabled = visible;
                    panel.GetComponent<GraphicRaycaster>().enabled = visible;
                    break;
                case HideMethod.MoveAway:
                    panel.anchoredPosition = visible ? Vector2.zero : new Vector2(100000, 0);
                    break;
                case HideMethod.ScaleZero:
                    panel.localScale = visible ? Vector3.one : Vector3.zero;
                    break;
            }
        }
        //#endregion

        static string HideName(HideMethod m) => m switch
        {
            HideMethod.SetActive => "SetActive",
            HideMethod.CanvasGroup => "CanvasGroup.alpha",
            HideMethod.CanvasEnabled => "Canvas.enabled",
            HideMethod.MoveAway => "移出屏幕",
            _ => "缩放为 0",
        };

        /// 一个有 400 个图形的弹窗（自带嵌套 Canvas + GraphicRaycaster + CanvasGroup，五种隐藏方法都能用），下面垫一面 800 个图标的墙
        static RectTransform Popup(LabContext c)
        {
            LabUI.Wall(c.Root, 800, Sp("star"));
            var p = LabUI.Node(c.Root, "Popup");
            p.sizeDelta = new Vector2(1500, 960);
            p.gameObject.AddComponent<Canvas>();
            p.gameObject.AddComponent<GraphicRaycaster>();
            p.gameObject.AddComponent<CanvasGroup>();
            var bg = LabUI.Img(p, Sp("round_24"), 0, 0, 1500, 960, Color.white, "Bg");
            for (int i = 0; i < 100; i++)
            {
                float x = -555 + (i % 4) * 370, y = 440 - (i / 4) * 36;
                var cell = LabUI.Img(p, Sp("round_8"), x, y, 360, 32, new Color(0.93f, 0.94f, 0.97f), "Cell").rectTransform;
                LabUI.Img(cell, Sp(Items[i % 6]), -160, 0, 28, 28);
                LabUI.Txt(cell, "道具 " + i, 0, 0, 200, 30, 20);
                LabUI.Img(cell, Sp("circle"), 160, 0, 20, 20, UISkin.Instance.danger, "Dot");
            }
            return p;
        }

        // 《千字文》前 400 字：用来测“很多新字第一次出现”的代价（各字基本不重复）
        const string Qianziwen =
            "天地玄黄宇宙洪荒日月盈昃辰宿列张寒来暑往秋收冬藏闰余成岁律吕调阳云腾致雨露结为霜金生丽水玉出昆冈剑号巨阙珠称夜光果珍李柰菜重芥姜" +
            "海咸河淡鳞潜羽翔龙师火帝鸟官人皇始制文字乃服衣裳推位让国有虞陶唐吊民伐罪周发殷汤坐朝问道垂拱平章爱育黎首臣伏戎羌遐迩一体率宾归王" +
            "鸣凤在竹白驹食场化被草木赖及万方盖此身发四大五常恭惟鞠养岂敢毁伤女慕贞洁男效才良知过必改得能莫忘罔谈彼短靡恃己长信使可覆器欲难量" +
            "墨悲丝染诗赞羔羊景行维贤克念作圣德建名立形端表正空谷传声虚堂习听祸因恶积福缘善庆尺璧非宝寸阴是竞资父事君曰严与敬孝当竭力忠则尽命" +
            "临深履薄夙兴温凊似兰斯馨如松之盛川流不息渊澄取映容止若思言辞安定笃初诚美慎终宜令荣业所基籍甚无竟学优登仕摄职从政存以甘棠去而益咏" +
            "乐殊贵贱礼别尊卑上和下睦夫唱妇随外受傅训入奉母仪诸姑伯叔犹子比儿孔怀兄弟同气连枝交友投分切磨箴规仁慈隐恻造次弗离节义廉退颠沛匪亏";
    }
}
