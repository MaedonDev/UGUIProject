// 第 12 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    static class Ch12Shots
    {
        static UISkin Skin => UISkin.Instance;

        static TextMeshProUGUI T(Transform p, string s, float size, Color? c = null, TextAlignmentOptions a = TextAlignmentOptions.Center, bool bold = false, string name = "Text")
            => UIKit.Text(p, s, size, c ?? Skin.ink, a, bold, name);

        static void Set(Object o, string prop, Object v)
        {
            var so = new SerializedObject(o);
            so.FindProperty(prop).objectReferenceValue = v;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CameraCanvas(Shot s)
        {
            s.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            s.Canvas.worldCamera = s.Cam;
            s.Canvas.planeDistance = 100;
        }

        // ------------------------------------------------------------------ 12.5 网格：各种 Image / Text 生成的真实三角形
        [BookShot("ch12-mesh", W = 960, H = 400)]
        static void Mesh(Shot s)
        {
            var specs = new (string label, System.Func<Transform, Graphic> make)[]
            {
                ("Simple", p => { var i = UIKit.Image(p, "round_16", UIKit.Hex("#C9C2F2"), "Img"); i.type = Image.Type.Simple; i.rectTransform.sizeDelta = new Vector2(200, 200); return i; }),
                ("Sliced", p => { var i = UIKit.Image(p, "round_24", UIKit.Hex("#C9C2F2"), "Img"); i.rectTransform.sizeDelta = new Vector2(240, 170); return i; }),
                ("Sliced 不填中心", p => { var i = UIKit.Image(p, "round_24", UIKit.Hex("#C9C2F2"), "Img"); i.fillCenter = false; i.rectTransform.sizeDelta = new Vector2(240, 170); return i; }),
                ("Tiled", p => { var i = UIKit.Image(p, "circle_small", UIKit.Hex("#C9C2F2"), "Img"); i.type = Image.Type.Tiled; i.rectTransform.sizeDelta = new Vector2(200, 200); return i; }),
                ("Filled Radial360 0.62", p => { var i = UIKit.Image(p, "circle", UIKit.Hex("#C9C2F2"), "Img"); i.type = Image.Type.Filled; i.fillMethod = Image.FillMethod.Radial360; i.fillAmount = 0.62f; i.rectTransform.sizeDelta = new Vector2(200, 200); return i; }),
                ("Text（旧版）", p => { var t = UIKit.LegacyText(p, "UI网格", 64, Skin.ink); t.rectTransform.sizeDelta = new Vector2(240, 120); return t; }),
            };
            var stats = new List<MeshStats>();
            var overlays = new List<WireOverlay>();
            var labels = new List<TextMeshProUGUI>();
            for (int i = 0; i < specs.Length; i++)
            {
                float x = -800 + i * 320;
                var cell = UIKit.Node(s.Root, "Cell_" + specs[i].label); cell.Place(x, 40, 300, 300);
                var g = specs[i].make(cell);
                g.raycastTarget = false;
                stats.Add(g.gameObject.AddComponent<MeshStats>());
                var wire = UIKit.Node(cell, "Wire").gameObject.AddComponent<WireOverlay>();
                wire.rectTransform.Stretch(); wire.color = Skin.danger; wire.raycastTarget = false;
                wire.source = stats[i];
                overlays.Add(wire);
                var l = T(s.Root, specs[i].label, 28, Skin.ink, bold: true);
                l.rectTransform.Place(x, -170, 310, 44);
                var n = T(s.Root, "", 26, Skin.muted); n.rectTransform.Place(x, -214, 310, 40);
                labels.Add(n);
            }
            s.BeforeCapture(() =>
            {
                for (int i = 0; i < stats.Count; i++)
                {
                    labels[i].text = $"顶点 {stats[i].vertexCount} · 三角形 {stats[i].triangleCount}";
                    overlays[i].SetVerticesDirty();
                }
            });
            T(s.Root, "红线 = 该 Graphic 经 VertexHelper 生成的真实三角形（MeshStats 记录，WireOverlay 绘制）", 28, Skin.muted)
                .rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -34), new Vector2(1800, 44));
        }

        // ------------------------------------------------------------------ 12.7 重建实验台
        [BookShot("ch12-rebuildlab", W = 960, H = 540, Scene = "Ch12_RebuildLab")]
        static void RebuildLabShot(Shot s)
        {
            CameraCanvas(s);
            var imgRoot = UIKit.Node(s.Root, "Images"); imgRoot.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(800, 800));
            imgRoot.gameObject.AddComponent<CanvasGroup>();
            for (int y = 0; y < 20; y++)
                for (int x = 0; x < 20; x++)
                {
                    var n = UIKit.Node(imgRoot, $"Probe_{x}_{y}");
                    n.Place(new Vector2(0, 1), new Vector2(x * 40 + 2, -y * 40 - 2), new Vector2(36, 36));
                    var pi = n.gameObject.AddComponent<ProbeImage>();
                    pi.sprite = UISkin.Sprite("round_8"); pi.type = Image.Type.Sliced;
                    pi.color = Color.HSVToRGB((x + y) / 40f, 0.45f, 0.95f);
                    pi.raycastTarget = false;
                }
            var legacy = UIKit.Node(s.Root, "LegacyTexts"); legacy.Place(new Vector2(0, 0.5f), new Vector2(870, 0), new Vector2(220, 960));
            for (int i = 0; i < 40; i++)
            {
                var t = UIKit.LegacyText(legacy, $"得分 {100 + i}", 18, Skin.ink, TextAnchor.MiddleLeft);
                t.rectTransform.Place(new Vector2(0, 1), new Vector2(0, -i * 24), new Vector2(220, 24));
                t.rectTransform.pivot = new Vector2(0, 1);
                t.verticalOverflow = VerticalWrapMode.Overflow;   // 旧版 Text 放不下一行时默认什么都不画
            }
            var tmpRoot = UIKit.Node(s.Root, "TmpList"); tmpRoot.Place(new Vector2(0, 0.5f), new Vector2(1110, 0), new Vector2(240, 960));
            var vl = tmpRoot.VLayout(0); vl.childControlHeight = true; vl.childForceExpandHeight = false;
            for (int i = 0; i < 40; i++)
            {
                var t = T(tmpRoot, $"第 {i} 行：胜利", 18, Skin.ink, TextAlignmentOptions.MidlineLeft);
                t.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
            }
            var panel = UIKit.Panel(s.Root, Color.white, 16, "Controls");
            panel.rectTransform.Place(new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(520, 1040));
            var buttons = UIKit.Node(panel.transform, "Buttons"); buttons.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(480, 640));
            var bl = buttons.VLayout(6); bl.childControlHeight = true;
            foreach (var name in RebuildLab.OpNames)
            {
                var b = UIKit.Button(buttons, name, UIKit.Hex("#EEF0F9"), 480, 40, fontSize: 22);
                b.gameObject.AddComponent<LayoutElement>().preferredHeight = 40;
                b.GetComponentInChildren<TextMeshProUGUI>().font = Skin.font;
            }
            var cont = UIKit.Toggle(panel.transform, "每帧持续执行", false, 300, 48);
            cont.Rt().Place(new Vector2(0, 1), new Vector2(20, -680), new Vector2(300, 48)); cont.Rt().pivot = new Vector2(0, 1);
            var bench = UIKit.Button(panel.transform, "跑一遍基准", null, 170, 52, fontSize: 24);
            bench.Rt().Place(new Vector2(1, 1), new Vector2(-20, -678), new Vector2(170, 52)); bench.Rt().pivot = new Vector2(1, 1);
            var readout = T(panel.transform, "点上面的按钮执行一次修改；勾选“每帧持续执行”看稳定数值", 22, Skin.ink, TextAlignmentOptions.TopLeft, name: "Readout");
            readout.rectTransform.Stretch(20, 750, 20, 20);
            var lab = s.Root.gameObject.AddComponent<RebuildLab>();
            Set(lab, "m_ImageRoot", imgRoot); Set(lab, "m_LegacyRoot", legacy); Set(lab, "m_TmpRoot", tmpRoot);
            Set(lab, "m_AltMaterial", s.MatAsset("Ch12", "UIDefaultCopy", "UI/Default"));
            Set(lab, "m_Readout", readout); Set(lab, "m_ButtonRoot", buttons); Set(lab, "m_Continuous", cont); Set(lab, "m_Benchmark", bench);
        }

        // ------------------------------------------------------------------ 12.6 合批实验台
        static RectTransform Case(RectTransform root, string name)
        {
            var c = UIKit.Node(root, name); c.Stretch(0, 90, 0, 90);
            return c;
        }

        static Image Icon(Transform p, string sprite, Vector2 pos, float size = 60, string name = "Icon")
        {
            var i = UIKit.Image(p, sprite, Color.white, name);
            i.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
            if (i.type == Image.Type.Sliced) i.type = Image.Type.Simple;
            if (sprite == "round_16") i.color = UIKit.Hex("#5B4BD6"); else if (sprite == "circle") i.color = UIKit.Hex("#FF8A3D");   // 顶点色不影响合批
            i.raycastTarget = false;
            return i;
        }

        static void Card(Transform p, Vector2 pos, int i)
        {
            var bg = UIKit.Image(p, "round_16", Color.white, "Card" + i);
            bg.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(140, 190)); bg.raycastTarget = false;
            var ol = UIKit.Image(bg.transform, "round_outline_16", UIKit.Hex("#C9CDE0"), "Outline"); ol.rectTransform.Stretch(); ol.raycastTarget = false;
            Icon(bg.transform, "item_gem", new Vector2(0, 26), 80);
            T(bg.transform, "卡 " + (i + 1), 26, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 34), new Vector2(130, 36));
        }

        static readonly string[] BatchCaseNames =
        {
            "空画布（基线）", "20 张同一精灵，不重叠", "两种纹理交替，不重叠", "两种纹理交替，依次重叠", "同精灵，中间一张换了材质",
            "同精灵，中间一张 z = -10", "列表 10 项：底图+图标+文字（图标同一张）", "列表 10 项：图标 10 张不同纹理",
            "两组图标之间夹一张全屏透明 Image（cull 关）", "同上，cullTransparentMesh 开", "20 张同一精灵，分到 2 个子 Canvas",
            "6 张卡牌叠放（盖住上一张的字）", "6 张卡牌错开（不重叠）",
        };

        [BookShot("ch12-batchlab", W = 480, H = 300, Panels = 4, Scene = "Ch12_BatchLab")]
        static void BatchLabShot(Shot s)
        {
            CameraCanvas(s);
            var root = UIKit.Node(s.Root, "Cases"); root.Stretch();
            var alt = s.MatAsset("Ch12", "UIDefaultCopy", "UI/Default");
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_potion_blue", "item_gem", "item_coin", "item_key", "item_bow", "item_helmet", "item_ring" };
            Vector2 G(int i) => new Vector2(-405 + (i % 10) * 90, i < 10 ? 60 : -60);
            for (int c = 0; c < BatchCaseNames.Length; c++)
            {
                var r = Case(root, BatchCaseNames[c]);
                switch (c)
                {
                    case 1: for (int i = 0; i < 20; i++) Icon(r, "round_16", G(i)); break;
                    case 2: for (int i = 0; i < 20; i++) Icon(r, i % 2 == 0 ? "round_16" : "circle", G(i)); break;
                    case 3: for (int i = 0; i < 20; i++) Icon(r, i % 2 == 0 ? "round_16" : "circle", new Vector2(-400 + (i % 10) * 40, i < 10 ? 60 : -60), 70); break;
                    case 4: for (int i = 0; i < 20; i++) { var im = Icon(r, "round_16", G(i)); if (i == 10) im.material = alt; } break;
                    case 5: for (int i = 0; i < 20; i++) { var im = Icon(r, "round_16", G(i)); if (i == 10) im.rectTransform.localPosition += new Vector3(0, 0, -10); } break;
                    case 6:
                    case 7:
                        for (int i = 0; i < 10; i++)
                        {
                            var bg = UIKit.Image(r, "round_16", Color.white, "Item" + i);
                            bg.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(i < 5 ? -220 : 220, 150 - (i % 5) * 75), new Vector2(400, 66)); bg.raycastTarget = false;
                            Icon(bg.transform, c == 6 ? "item_sword" : items[i], new Vector2(-160, 0), 54);
                            T(bg.transform, "物品 " + (i + 1), 28, Skin.ink, TextAlignmentOptions.MidlineLeft).rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(300, 50));
                        }
                        break;
                    case 8:
                    case 9:
                        for (int i = 0; i < 10; i++) Icon(r, "round_16", G(i));
                        var blocker = UIKit.Image(r, null, new Color(0, 0, 0, 0), "TransparentBlocker"); blocker.rectTransform.Stretch();
                        Debug.Log("[Ch12] CanvasRenderer.cullTransparentMesh 默认值 = " + blocker.canvasRenderer.cullTransparentMesh);
                        blocker.canvasRenderer.cullTransparentMesh = c == 9;   // 新建的 CanvasRenderer 默认是 true，这里显式区分两种情况
                        for (int i = 10; i < 20; i++) Icon(r, "round_16", G(i));
                        break;
                    case 10:
                        for (int k = 0; k < 2; k++)
                        {
                            var sub = UIKit.Node(r, "SubCanvas" + k); sub.Stretch();
                            sub.gameObject.AddComponent<Canvas>();
                            for (int i = k * 10; i < k * 10 + 10; i++) Icon(sub, "round_16", G(i));
                        }
                        break;
                    case 11: for (int i = 0; i < 6; i++) Card(r, new Vector2(-180 + i * 70, 0), i); break;
                    case 12: for (int i = 0; i < 6; i++) Card(r, new Vector2(-375 + i * 150, 0), i); break;
                }
            }
            var title = T(s.Root, "", 30, Skin.ink, TextAlignmentOptions.TopLeft, true, "Title");
            title.rectTransform.Place(new Vector2(0, 1), new Vector2(24, -18), new Vector2(900, 44)); title.rectTransform.pivot = new Vector2(0, 1);
            var hud = T(s.Root, "Batches -", 26, Skin.primary, TextAlignmentOptions.TopLeft, true, "Hud");
            hud.rectTransform.Place(new Vector2(0, 1), new Vector2(24, -58), new Vector2(900, 40)); hud.rectTransform.pivot = new Vector2(0, 1);
            var prev = UIKit.Button(s.Root, "上一个", UIKit.Hex("#E6E8F0"), 160, 60, fontSize: 26); prev.Rt().Place(new Vector2(0.5f, 0), new Vector2(-260, 16), new Vector2(160, 60));
            var next = UIKit.Button(s.Root, "下一个", UIKit.Hex("#E6E8F0"), 160, 60, fontSize: 26); next.Rt().Place(new Vector2(0.5f, 0), new Vector2(-80, 16), new Vector2(160, 60));
            var all = UIKit.Button(s.Root, "测量全部", null, 200, 60, fontSize: 26); all.Rt().Place(new Vector2(0.5f, 0), new Vector2(130, 16), new Vector2(200, 60));
            var lab = s.Root.gameObject.AddComponent<BatchLab>();
            Set(lab, "m_Cases", root); Set(lab, "m_Title", title); Set(lab, "m_Hud", hud);
            Set(lab, "m_Prev", prev); Set(lab, "m_Next", next); Set(lab, "m_MeasureAll", all);

            // 配图：四格分别显示 4 种摆法，并标出实测数值（来自 Ch12LabRunner 写的结果文件）
            int[] show = { 2, 3, 6, 11 };
            int idx = show[s.Panel];
            for (int i = 0; i < root.childCount; i++) root.GetChild(i).gameObject.SetActive(i == idx);
            title.text = BatchCaseNames[idx];
            hud.text = MeasuredLine(BatchCaseNames[idx]);
            if (s.Panel != 0) { prev.gameObject.SetActive(false); next.gameObject.SetActive(false); all.gameObject.SetActive(false); }
            else s.BeforeCapture(() => { prev.gameObject.SetActive(false); next.gameObject.SetActive(false); all.gameObject.SetActive(false); });
        }

        static string MeasuredLine(string caseName)
        {
            const string path = "Assets/UGUIBook/Ch12/Data/batch_results.txt";
            if (!File.Exists(path)) return "（尚未实测）";
            foreach (var line in File.ReadAllLines(path))
            {
                var c = line.Split('\t');
                if (c.Length >= 5 && c[0] == caseName) return $"实测：Batches {c[1]}（减去空画布基线后 {c[4]}）";
            }
            return "";
        }
    }
}
