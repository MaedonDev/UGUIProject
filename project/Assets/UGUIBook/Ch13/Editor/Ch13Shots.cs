// 第 13 章配图与配套场景（搭景代码，不进书）
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    static class Ch13Shots
    {
        static readonly Color Red = new Color32(0xE5, 0x48, 0x4D, 0xFF);
        static UISkin K => UISkin.Instance;

        static T Add<T>(Transform parent, string name, float x, float y, float w, float h) where T : Component
        {
            var rt = UIKit.Node(parent, name);
            rt.Place(x, y, w, h);
            return rt.gameObject.AddComponent<T>();
        }

        static WireframeView Wire(Graphic g, float line = 2.5f, float dot = 5f)
        {
            var rt = UIKit.Node(g.transform, "Wireframe");
            var w = rt.gameObject.AddComponent<WireframeView>();
            w.FitParent();
            w.color = Red;
            w.lineWidth = line; w.dotRadius = dot;
            w.raycastTarget = false;
            w.target = g;
            return w;
        }

        static TextMeshProUGUI Caption(Transform parent, string text, float y, float size = 34, Color? c = null, float w = 1200)
        {
            var t = UIKit.Text(parent, text, size, c ?? K.ink, TextAlignmentOptions.Center);
            t.rectTransform.Place(0, y, w, size * 1.6f);
            return t;
        }

        // ------------------------------------------------------------------ 13.1 顶点与三角形
        [BookShot("ch13-vertex-basics", W = 960, H = 440)]
        static void VertexBasics(Shot s)
        {
            s.Background(Color.white);
            // 左：Image（无 Sprite）= Graphic 默认的四边形
            var img = UIKit.Image(s.Root, null, new Color(0.36f, 0.29f, 0.84f, 0.25f), "Image");
            img.rectTransform.Place(-470, 10, 420, 420);
            Wire(img, 4, 9);
            Caption(s.Root, "Graphic 默认网格：4 个顶点、2 个三角形", -330, 32, null, 900).rectTransform.anchoredPosition = new Vector2(-470, -330);
            string[] lab = { "0  uv(0,0)", "1  uv(0,1)", "2  uv(1,1)", "3  uv(1,0)" };
            Vector2[] at = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };
            Vector2[] off = { new Vector2(-40, -36), new Vector2(-40, 36), new Vector2(40, 36), new Vector2(40, -36) };
            for (int i = 0; i < 4; i++) s.LabelAt(img.rectTransform, at[i], off[i], lab[i], Red, 28);
            s.Label(new Vector2(-520, 60), "三角形 A：0-1-2", K.primary, 26);
            s.Label(new Vector2(-420, -50), "三角形 B：2-3-0", K.primary, 26);

            // 右：HelloTriangle，三个顶点三种颜色
            var tri = Add<HelloTriangle>(s.Root, "HelloTriangle", 470, 0, 460, 420);
            Wire(tri, 4, 9);
            Caption(s.Root, "HelloTriangle：3 个顶点、1 个三角形，顶点色插值", -330, 32, null, 900).rectTransform.anchoredPosition = new Vector2(470, -330);
            s.LabelAt(tri.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 34), "0", Red, 28);
            s.LabelAt(tri.rectTransform, new Vector2(1, 0), new Vector2(30, -34), "1", Red, 28);
            s.LabelAt(tri.rectTransform, new Vector2(0, 0), new Vector2(-30, -34), "2", Red, 28);
        }

        // ------------------------------------------------------------------ 13.2 圆角矩形
        [BookShot("ch13-roundrect-wire", W = 640, H = 480, Panels = 3)]
        static void RoundRectWire(Shot s)
        {
            s.Background(Color.white);
            int[] segs = { 1, 3, 10 };
            var rr = Add<RoundedRect>(s.Root, "RoundedRect", 0, 40, 760, 560);
            rr.color = new Color(0.36f, 0.29f, 0.84f, 0.18f);
            rr.radius = 170; rr.segments = segs[s.Panel]; rr.feather = 1;
            Wire(rr);
            MeshProbe.Count(rr, out int v, out int t);
            Caption(s.Root, $"segments = {segs[s.Panel]}　顶点 {v} · 三角形 {t}", -340, 42);
        }

        [BookShot("ch13-roundrect-styles", W = 960, H = 540)]
        static void RoundRectStyles(Shot s)
        {
            s.Background(K.page);
            float[] radii = { 0, 16, 40, 999 };
            string[] names = { "radius 0", "radius 16", "radius 40", "radius 999（胶囊）" };
            for (int i = 0; i < 4; i++)
            {
                var rr = Add<RoundedRect>(s.Root, "Solid" + i, -660 + i * 440, 260, 340, 150);
                rr.radius = radii[i]; rr.color = i == 3 ? K.accent : K.primary;
                Caption(s.Root, names[i], 150, 30, K.muted, 400).rectTransform.anchoredPosition = new Vector2(-660 + i * 440, 150);
            }
            float[] borders = { 2, 6, 14 };
            for (int i = 0; i < 3; i++)
            {
                var rr = Add<RoundedRect>(s.Root, "Border" + i, -660 + i * 440, -40, 340, 150);
                rr.radius = 36; rr.borderWidth = borders[i]; rr.color = K.primary;
                Caption(s.Root, "borderWidth " + borders[i], -150, 30, K.muted, 400).rectTransform.anchoredPosition = new Vector2(-660 + i * 440, -150);
            }
            // 带图：把 Sprite 直接裁成圆角，不需要 Mask
            var av = Add<RoundedRect>(s.Root, "Avatar", 660, -40, 150, 150);
            av.sprite = UIKit.Sp("avatar_2"); av.radius = 36; av.color = Color.white;
            var pic = Add<RoundedRect>(s.Root, "Picture", 0, -330, 1000, 180);
            pic.sprite = UIKit.Sp("bg_landscape"); pic.radius = 48; pic.color = Color.white;
            Caption(s.Root, "sprite = bg_landscape，radius 48：不用 Mask 也能裁出圆角", -460, 30, K.muted, 1200);
            Caption(s.Root, "sprite = avatar_2", -150, 30, K.muted, 400).rectTransform.anchoredPosition = new Vector2(660, -150);
        }

        // ------------------------------------------------------------------ 13.3 圆环进度
        [BookShot("ch13-ring", W = 640, H = 480, Panels = 3)]
        static void Ring(Shot s)
        {
            s.Background(Color.white);
            if (s.Panel == 0)
            {
                var ring = Add<RingProgress>(s.Root, "Ring", 0, 40, 520, 520);
                ring.color = K.primary; ring.thickness = 44; ring.fill = 0.68f; ring.roundCaps = true;
                ring.showTrack = true; ring.trackColor = new Color(0.36f, 0.29f, 0.84f, 0.12f);
                var t = UIKit.Text(ring.transform, "68%", 110, K.ink, TextAlignmentOptions.Center, true); t.rectTransform.Stretch();
                Caption(s.Root, "底槽 + 圆头 roundCaps", -340, 40);
            }
            else if (s.Panel == 1)
            {
                var ring = Add<RingProgress>(s.Root, "Ring", 0, 40, 520, 520);
                ring.color = K.accent; ring.useGradient = true; ring.endColor = K.danger;
                ring.thickness = 70; ring.fill = 0.86f; ring.roundCaps = true; ring.startAngle = 90;
                ring.showTrack = true; ring.trackColor = new Color(0, 0, 0, 0.06f);
                var t = UIKit.Text(ring.transform, "86", 120, K.ink, TextAlignmentOptions.Center, true); t.rectTransform.Stretch();
                Caption(s.Root, "沿弧渐变 useGradient", -340, 40);
            }
            else
            {
                // 环形图：几个 RingProgress 首尾相接
                float[] parts = { 0.42f, 0.26f, 0.2f, 0.12f };
                Color[] cols = { K.primary, K.accent, K.success, new Color(0.12f, 0.53f, 0.9f) };
                float acc = 0;
                for (int i = 0; i < parts.Length; i++)
                {
                    var r = Add<RingProgress>(s.Root, "Part" + i, 0, 40, 520, 520);
                    r.color = cols[i]; r.thickness = 300; r.fill = parts[i] - 0.004f;
                    r.startAngle = 90 - acc * 360; acc += parts[i];
                }
                Caption(s.Root, "thickness ≥ 半径 = 扇形；拼起来就是饼图", -340, 40);
            }
        }

        // ------------------------------------------------------------------ 抗锯齿：低分辨率实拍放大
        [BookShot("ch13-feather", W = 640, H = 480, Panels = 2)]
        static void Feather(Shot s)
        {
            s.Background(Color.white);
            const int layer = 1; // TransparentFX：只给放大用的小相机看
            var camGo = new GameObject("TinyCamera");
            camGo.transform.position = new Vector3(5000, 0, -10);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.white;
            cam.cullingMask = 1 << layer;
            cam.allowMSAA = false; cam.allowHDR = false;   // 关掉 MSAA（工程的 QualitySettings 开了 2x），才能看到真实锯齿
            s.Cam.cullingMask &= ~(1 << layer);
            var rtex = new RenderTexture(64, 48, 24) { antiAliasing = 1, filterMode = FilterMode.Point };
            rtex.Create();
            cam.targetTexture = rtex;
            var tiny = UIKit.Canvas("TinyCanvas", RenderMode.ScreenSpaceCamera);
            var sc = tiny.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; sc.scaleFactor = 1;
            tiny.worldCamera = cam; tiny.planeDistance = 5;
            var ring = Add<RingProgress>(tiny.transform, "Ring", 0, 0, 40, 40);
            ring.color = K.primary; ring.thickness = 7; ring.fill = 0.7f; ring.roundCaps = true; ring.segments = 64;
            ring.feather = s.Panel == 0 ? 0 : 1;
            foreach (var tr in tiny.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = layer;

            var raw = UIKit.Node(s.Root, "Zoom").gameObject.AddComponent<RawImage>();
            raw.rectTransform.Place(0, 30, 1024, 768);
            raw.texture = rtex;
            Caption(s.Root, s.Panel == 0 ? "feather = 0：硬边，锯齿明显" : "feather = 1：多一圈 alpha=0 的顶点", -420, 40);
            s.BeforeCapture(() => { Canvas.ForceUpdateCanvases(); cam.Render(); });
        }

        // ------------------------------------------------------------------ 13.4 雷达图
        [BookShot("ch13-radar", W = 640, H = 480, Panels = 3)]
        static void Radar(Shot s)
        {
            s.Background(Color.white);
            string[][] labels =
            {
                new[] { "攻击", "防御", "速度", "法术", "生命" },
                new[] { "攻击", "防御", "速度", "法术", "生命", "暴击" },
                new[] { "力量", "敏捷", "智力", "体质", "感知", "魅力", "幸运", "意志" },
            };
            float[][] vals =
            {
                new[] { 0.9f, 0.55f, 0.7f, 0.35f, 0.8f },
                new[] { 0.75f, 0.9f, 0.4f, 0.6f, 0.85f, 0.5f },
                new[] { 0.6f, 0.85f, 0.95f, 0.5f, 0.7f, 0.4f, 0.65f, 0.8f },
            };
            Color[] cols = { K.primary, K.accent, K.success };
            var radar = Add<RadarChart>(s.Root, "Radar", 0, 10, 600, 600);
            radar.SetValues(vals[s.Panel]);
            var c = cols[s.Panel];
            radar.lineColor = c; radar.color = new Color(c.r, c.g, c.b, 0.3f);
            for (int i = 0; i < radar.axisCount; i++)
            {
                var t = UIKit.Text(radar.transform, labels[s.Panel][i], 34, K.ink);
                t.rectTransform.Place(new Vector2(0.5f, 0.5f), radar.AxisDir(i) * 360, new Vector2(140, 50));
            }
            Caption(s.Root, $"{radar.axisCount} 个维度", -430, 38, K.muted);
        }

        // ------------------------------------------------------------------ 13.5 折线图
        [BookShot("ch13-linechart", W = 640, H = 480, Panels = 3)]
        static void Line(Shot s)
        {
            s.Background(Color.white);
            var line = Add<LineChart>(s.Root, "LineChart", 0, 30, 1080, 600);
            line.color = K.primary;
            line.SetValues(new[] { 2.5f, 6f, 3.8f, 8.2f, 5.2f, 9.1f, 6.6f });
            line.smooth = s.Panel > 0;
            line.fillArea = s.Panel == 2;
            line.pointRadius = s.Panel == 2 ? 11 : 0;
            line.lineWidth = 7;
            string[] days = { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            for (int i = 0; i < 7; i++)
            {
                var t = UIKit.Text(s.Root, days[i], 28, K.muted);
                t.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-540 + i * 180, -310), new Vector2(120, 40));
            }
            string[] cap = { "直线连接（smooth = false）", "Catmull-Rom 平滑", "平滑 + 面积渐变 + 数据点" };
            Caption(s.Root, cap[s.Panel], -400, 40);
        }

        // ------------------------------------------------------------------ 13.6 贝塞尔连线
        static RectTransform Node(Transform parent, string title, Vector2 pos, Color c, out RectTransform inPort, out RectTransform outPort)
        {
            var card = UIKit.Card(parent, "Node_" + title, 16);
            card.Place(new Vector2(0.5f, 0.5f), pos, new Vector2(300, 170));
            var bg = card.Find("Bg").GetComponent<Image>();
            bg.raycastTarget = true;
            card.gameObject.AddComponent<DragMove>();
            var head = UIKit.Panel(card, c, 16, "Header");
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = Vector2.one; head.rectTransform.pivot = new Vector2(0.5f, 1);
            head.rectTransform.sizeDelta = new Vector2(0, 60); head.rectTransform.anchoredPosition = Vector2.zero;
            head.raycastTarget = false;
            var t = UIKit.Text(head.transform, title, 30, Color.white, TextAlignmentOptions.Center, true); t.rectTransform.Stretch();
            var body = UIKit.Text(card, "拖我", 26, K.muted); body.rectTransform.Place(0, -30, 200, 40);
            inPort = UIKit.Image(card, "circle", c, "In").rectTransform;
            inPort.anchorMin = inPort.anchorMax = new Vector2(0, 0.4f); inPort.sizeDelta = new Vector2(26, 26); inPort.anchoredPosition = Vector2.zero;
            outPort = UIKit.Image(card, "circle", c, "Out").rectTransform;
            outPort.anchorMin = outPort.anchorMax = new Vector2(1, 0.4f); outPort.sizeDelta = new Vector2(26, 26); outPort.anchoredPosition = Vector2.zero;
            inPort.GetComponent<Image>().raycastTarget = outPort.GetComponent<Image>().raycastTarget = false;
            return card;
        }

        static BezierLink Link(Transform layer, RectTransform a, RectTransform b, Color c0, Color c1)
        {
            var l = UIKit.Node(layer, "Link").gameObject.AddComponent<BezierLink>();
            l.rectTransform.Stretch();
            l.raycastTarget = false;
            l.color = c0; l.endColor = c1;
            l.from = a; l.to = b;
            return l;
        }

        [BookShot("ch13-bezier", W = 960, H = 540, Scene = "Ch13_Bezier")]
        static void Bezier(Shot s)
        {
            s.Background(K.page);
            var links = UIKit.Node(s.Root, "Links"); links.Stretch();   // 连线层放在节点下面（先画）
            var nA = Node(s.Root, "开始", new Vector2(-640, 200), K.success, out _, out var aOut);
            var nB = Node(s.Root, "对话", new Vector2(-40, 300), K.primary, out var bIn, out var bOut);
            var nC = Node(s.Root, "战斗", new Vector2(0, -230), K.danger, out var cIn, out var cOut);
            var nD = Node(s.Root, "结算", new Vector2(640, 40), K.accent, out var dIn, out _);
            var l1 = Link(links, aOut, bIn, K.success, K.primary); l1.width = 8; l1.arrowSize = 30;
            var l2 = Link(links, aOut, cIn, K.success, K.danger); l2.width = 6; l2.dash = 18;
            var l3 = Link(links, bOut, dIn, K.primary, K.accent); l3.width = 10; l3.arrowSize = 32;
            var l4 = Link(links, cOut, dIn, K.danger, K.accent); l4.width = 4;
            var hint = UIKit.Text(s.Root, "拖动节点，连线会跟着重建", 30, K.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(800, 50));
            s.LabelAt(l1.rectTransform, new Vector2(0.32f, 0.81f), Vector2.zero, "渐变 + 箭头", K.primary, 26);
            s.LabelAt(l2.rectTransform, new Vector2(0.25f, 0.42f), Vector2.zero, "虚线 dash = 18", K.ink, 26);
        }

        // ------------------------------------------------------------------ 13.7 BaseMeshEffect
        [BookShot("ch13-effects", W = 960, H = 540, Scene = "Ch13_Effects")]
        static void Effects(Shot s)
        {
            s.Background(K.page);
            // 1) 顶点渐变（Image）
            var a = UIKit.Image(s.Root, "round_32", Color.white, "Gradient_Image");
            a.rectTransform.Place(-620, 170, 360, 240);
            var ga = a.gameObject.AddComponent<VertexGradient>();
            ga.colorA = new Color(0.55f, 0.48f, 1f); ga.colorB = K.primary;
            Caption(s.Root, "VertexGradient（Image）", 0, 30, K.ink, 420).rectTransform.anchoredPosition = new Vector2(-620, 0);
            // 2) 顶点渐变（旧版 Text，渐变跨整段文字）
            var tx = UIKit.LegacyText(s.Root, "渐变标题", 110, Color.white, TextAnchor.MiddleCenter, "Gradient_Text");
            tx.rectTransform.Place(0, 170, 520, 240);
            var gt = tx.gameObject.AddComponent<VertexGradient>();
            gt.colorA = new Color(1f, 0.8f, 0.3f); gt.colorB = K.danger;
            Caption(s.Root, "VertexGradient（Text）", 0, 30, K.ink, 420);
            // 3) 倾斜按钮
            var b = UIKit.Button(s.Root, "倾斜按钮", K.accent, 360, 120);
            b.Rt().Place(620, 170, 360, 120);
            b.gameObject.AddComponent<SkewEffect>().angle = 20;
            b.GetComponentInChildren<TextMeshProUGUI>().fontStyle = FontStyles.Italic;   // TMP 不走 IMeshModifier，文字用斜体配合
            Caption(s.Root, "SkewEffect 20°", 0, 30, K.ink, 420).rectTransform.anchoredPosition = new Vector2(620, 0);
            // 4) 镜像
            var m = UIKit.Image(s.Root, "bg_landscape", Color.white, "Mirror_Image");
            m.rectTransform.Place(-330, -290, 600, 150);
            var m0 = UIKit.Image(s.Root, "bg_landscape", Color.white, "Original");
            m0.rectTransform.Place(-330, -110, 300, 150);
            Caption(s.Root, "原图", -110, 28, Color.white, 300).rectTransform.anchoredPosition = new Vector2(-330, -110);
            m.gameObject.AddComponent<MirrorEffect>();
            Caption(s.Root, "MirrorEffect：右半边是镜像出来的", -400, 30, K.ink, 700).rectTransform.anchoredPosition = new Vector2(-330, -400);
            // 5) 组合：渐变 + 倾斜 + 系统 Shadow
            var c = UIKit.Image(s.Root, "round_16", Color.white, "Combo");
            c.rectTransform.Place(420, -250, 520, 150);
            var gc = c.gameObject.AddComponent<VertexGradient>(); gc.direction = VertexGradient.Direction.Horizontal;
            gc.colorA = K.success; gc.colorB = new Color(0.12f, 0.53f, 0.9f);
            c.gameObject.AddComponent<SkewEffect>().angle = -12;
            var sh = c.gameObject.AddComponent<Shadow>(); sh.effectDistance = new Vector2(10, -10); sh.effectColor = new Color(0, 0, 0, 0.25f);
            var ct = UIKit.Text(c.transform, "多个效果按组件顺序叠加", 32, Color.white, TextAlignmentOptions.Center, true); ct.rectTransform.Stretch();
            Caption(s.Root, "渐变 → 倾斜 → Shadow", -400, 30, K.ink, 600).rectTransform.anchoredPosition = new Vector2(420, -400);
        }

        // ------------------------------------------------------------------ Outline 的代价
        [BookShot("ch13-outline-cost", W = 960, H = 380, Scene = "Ch13_OutlineCost")]
        static void OutlineCost(Shot s)
        {
            s.Background(Color.white);
            string[] names = { "无效果", "Shadow", "Outline", "Outline + Shadow" };
            for (int i = 0; i < 4; i++)
            {
                float x = -720 + i * 480;
                var t = UIKit.LegacyText(s.Root, "UI", 170, K.primary, TextAnchor.MiddleCenter, "Text_" + i);
                t.rectTransform.Place(x, 90, 400, 260);
                if (i == 1 || i == 3) { var sh = t.gameObject.AddComponent<Shadow>(); sh.effectDistance = new Vector2(8, -8); sh.effectColor = new Color(0, 0, 0, 0.35f); }
                if (i >= 2) { var o = t.gameObject.AddComponent<Outline>(); o.effectDistance = new Vector2(5, -5); o.effectColor = K.accent; }
                Wire(t, 1.6f, 3.5f).showTransparent = false;
                MeshProbe.Count(t, out int v, out int tr);
                Caption(s.Root, names[i], -110, 34, K.ink, 460).rectTransform.anchoredPosition = new Vector2(x, -110);
                Caption(s.Root, $"顶点 {v}　三角形 {tr}", -165, 30, Red, 460).rectTransform.anchoredPosition = new Vector2(x, -165);
            }
            // 场景里运行时显示实测结果
            var bench = UIKit.LegacyText(s.Root, "UGUI 描边性能测试：一段二十个字的普通文本。", 40, K.ink, TextAnchor.MiddleCenter, "BenchText");
            bench.rectTransform.Place(0, 300, 1500, 70);
            bench.gameObject.AddComponent<Shadow>();
            bench.gameObject.AddComponent<Outline>();
            var output = UIKit.Text(s.Root, "运行场景后在这里显示实测结果", 24, K.muted, TextAlignmentOptions.Top);
            output.rectTransform.Place(0, -285, 1500, 170);
            var ob = bench.gameObject.AddComponent<OutlineBench>();
            var so = new UnityEditor.SerializedObject(ob);
            so.FindProperty("m_Text").objectReferenceValue = bench;
            so.FindProperty("m_Output").objectReferenceValue = output;
            so.ApplyModifiedPropertiesWithoutUndo();
            s.BeforeCapture(() =>
            {
                // 批处理里直接跑一次基准测试，把结果写进日志（正文里的数字来自这里）
                Debug.Log("[Ch13Bench]\n" + OutlineBench.Run(bench, 5000));
                bench.gameObject.SetActive(false);
                output.gameObject.SetActive(false);
            });
        }

        // ------------------------------------------------------------------ 13.8 TMP 逐字动画
        [BookShot("ch13-tmp-anim", W = 640, H = 480, Panels = 3, Scene = "Ch13_TMPAnim")]
        static void TmpAnim(Shot s)
        {
            s.Background(K.dark);
            float[] times = { 0.2f, 0.45f, 0.9f };
            string[] texts = { "波浪 Wave 文字", "抖动 Jitter 警告", "逐字弹出 PopIn", "彩虹 Rainbow 渐变" };
            var modes = new[] { TMPVertexAnim.Mode.Wave, TMPVertexAnim.Mode.Jitter, TMPVertexAnim.Mode.PopIn, TMPVertexAnim.Mode.Rainbow };
            Color[] cols = { Color.white, new Color(1f, 0.45f, 0.45f), new Color(1f, 0.85f, 0.4f), Color.white };
            for (int i = 0; i < 4; i++)
            {
                var t = UIKit.Text(s.Root, texts[i], 68, cols[i], TextAlignmentOptions.Center, true);
                t.rectTransform.Place(0, 300 - i * 190, 1200, 140);
                var anim = t.gameObject.AddComponent<TMPVertexAnim>();
                anim.mode = modes[i];
                if (modes[i] == TMPVertexAnim.Mode.PopIn) { anim.speed = 12; anim.amplitude = 40; }
                if (modes[i] == TMPVertexAnim.Mode.Jitter) anim.amplitude = 22;
                if (modes[i] == TMPVertexAnim.Mode.Wave) anim.amplitude = 18;
                float time = times[s.Panel];
                s.BeforeCapture(() => anim.Evaluate(time));
            }
            Caption(s.Root, $"t = {times[s.Panel]:F2} s", -440, 34, new Color(1, 1, 1, 0.6f));
        }

        // ------------------------------------------------------------------ 13.9 精确命中：环形菜单
        [BookShot("ch13-radial-menu", W = 960, H = 540, Scene = "Ch13_RadialMenu")]
        static void RadialMenu(Shot s)
        {
            s.Background(K.page);
            var root = UIKit.Node(s.Root, "RadialMenu"); root.Place(-200, 0, 840, 840);
            var center = UIKit.Image(root, "circle", Color.white, "Center"); center.rectTransform.Place(0, 0, 330, 330); center.raycastTarget = false;
            var label = UIKit.Text(center.transform, "指向一个扇区", 38, K.ink, TextAlignmentOptions.Center, true); label.rectTransform.Stretch(20, 0, 20, 0);
            string[] names = { "背包", "技能", "地图", "任务", "好友", "设置" };
            string[] icons = { "bag", "star", "home", "trophy", "user", "gear" };
            RingProgress first = null;
            const float gap = 0.008f;
            for (int i = 0; i < 6; i++)
            {
                var ring = Add<RingProgress>(root, "Sector_" + names[i], 0, 0, 840, 840);
                ring.thickness = 230; ring.fill = 1f / 6 - gap; ring.startAngle = 90 + 30 - gap * 180 - i * 60; ring.segments = 120;
                ring.raycastFilledOnly = true;
                if (first == null) first = ring;
                float mid = (90 - i * 60) * Mathf.Deg2Rad;
                var ic = UIKit.Icon(root, icons[i], 84, Color.white);
                ic.rectTransform.anchoredPosition = new Vector2(Mathf.Cos(mid), Mathf.Sin(mid)) * 305;
                var sec = ring.gameObject.AddComponent<RadialSector>();
                sec.Setup(names[i], new Color(0.14f, 0.16f, 0.23f, 0.92f), K.primary, label, ic);
                if (i == 1) s.BeforeCapture(() => sec.SetHighlighted(true));
            }
            // 图标在扇区之后创建，但 Icon 的 raycastTarget = false，不会挡住扇区接收射线
            // 假光标在 BeforeCapture 里才创建：那时场景已经存盘，光标只出现在配图里
            s.BeforeCapture(() =>
            {
                var pointer = UIKit.Icon(root, "pointer", 90, K.ink, "FakeCursor");
                pointer.rectTransform.pivot = new Vector2(0.2f, 0.85f);
                pointer.rectTransform.anchoredPosition = new Vector2(330, 220);
                pointer.rectTransform.localEulerAngles = new Vector3(0, 0, 25);
            });
            s.Box(first.rectTransform, Red, "每个扇区的 RectTransform 都是整块 840×840", 4, true);
            var tip = UIKit.Text(s.Root, "6 个扇区的矩形完全重叠，\n但 IsRaycastLocationValid\n只让真正指到的那一块响应。", 34, K.ink, TextAlignmentOptions.Left);
            tip.rectTransform.Place(620, 60, 560, 300);
        }

        // ------------------------------------------------------------------ 13.9 Mask / RectMask2D 兼容
        [BookShot("ch13-maskable", W = 640, H = 480, Panels = 2)]
        static void Maskable(Shot s)
        {
            s.Background(Color.white);
            if (s.Panel == 0)
            {
                var vp = UIKit.Panel(s.Root, new Color(0.36f, 0.29f, 0.84f, 0.06f), 0, "Viewport (RectMask2D)");
                vp.rectTransform.Place(0, 30, 700, 520);
                var rm = vp.gameObject.AddComponent<RectMask2D>(); rm.softness = new Vector2Int(60, 0);
                var line = Add<LineChart>(vp.transform, "LineChart", 160, 0, 1200, 440);
                line.color = K.primary; line.smooth = true; line.pointRadius = 10;
                line.SetValues(new[] { 3f, 7f, 4f, 8.5f, 5f, 9f, 6f, 8f, 3.5f });
                var ring = Add<RingProgress>(vp.transform, "Ring", -330, 180, 260, 260);
                ring.color = K.accent; ring.thickness = 40; ring.fill = 0.8f; ring.roundCaps = true;
                s.Box(vp.rectTransform, Red, "RectMask2D  softness.x = 60", 2, true);
            }
            else
            {
                var m = UIKit.Image(s.Root, "circle", new Color(0.92f, 0.97f, 0.94f), "Mask (circle)");
                m.rectTransform.Place(0, 30, 560, 560);
                m.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var radar = Add<RadarChart>(m.transform, "Radar", 120, -60, 640, 640);
                radar.color = new Color(0.18f, 0.7f, 0.42f, 0.35f); radar.lineColor = K.success;
                s.Box(m.rectTransform, Red, "Mask + circle 精灵（模板）", 2, true);
            }
        }

        // ------------------------------------------------------------------ 配套场景：调参游乐场
        [BookScene("Ch13_Playground")]
        static void Playground(Shot s)
        {
            s.Background(K.page);
            var rr = Add<RoundedRect>(s.Root, "RoundedRect", -560, 160, 520, 360);
            rr.color = new Color(0.36f, 0.29f, 0.84f, 0.25f); rr.radius = 60; rr.segments = 6;
            var wire = Wire(rr);
            var ring = Add<RingProgress>(s.Root, "Ring", 0, 160, 360, 360);
            ring.color = K.accent; ring.thickness = 40; ring.fill = 0.6f; ring.roundCaps = true; ring.showTrack = true;
            var ringLabel = UIKit.Text(ring.transform, "60%", 70, K.ink, TextAlignmentOptions.Center, true); ringLabel.rectTransform.Stretch();
            var radar = Add<RadarChart>(s.Root, "Radar", 560, 160, 380, 380);
            radar.color = new Color(0.36f, 0.29f, 0.84f, 0.3f);
            var line = Add<LineChart>(s.Root, "LineChart", 380, -300, 860, 300);
            line.color = K.primary; line.pointRadius = 8;

            var panel = UIKit.Node(s.Root, "Controls"); panel.Place(-480, -290, 760, 380);
            panel.VLayout(18, 0, TextAnchor.UpperLeft);
            var info = UIKit.Text(panel, "", 28, K.ink, TextAlignmentOptions.Left);
            UIKit.Text(panel, "圆角半径 / 每角段数 / 圆环进度", 26, K.muted, TextAlignmentOptions.Left);
            var sRadius = UIKit.Slider(panel, 0.5f, 700, 40); sRadius.minValue = 0; sRadius.maxValue = 180; sRadius.value = 60;
            var sSeg = UIKit.Slider(panel, 0.5f, 700, 40); sSeg.minValue = 1; sSeg.maxValue = 16; sSeg.wholeNumbers = true; sSeg.value = 6;
            var sFill = UIKit.Slider(panel, 0.6f, 700, 40, K.accent);
            var row = UIKit.Node(panel, "Row"); row.sizeDelta = new Vector2(700, 70); row.HLayout(24, 0, TextAnchor.MiddleLeft);
            var tSmooth = UIKit.Toggle(row, "平滑折线", false, 220, 56);
            var tWire = UIKit.Toggle(row, "显示线框", true, 220, 56);
            var bRand = UIKit.Button(row, "随机数据", K.primary, 200, 64, null, 28);

            var ctl = s.Root.gameObject.AddComponent<Ch13Playground>();
            var so = new UnityEditor.SerializedObject(ctl);
            so.FindProperty("m_Rect").objectReferenceValue = rr;
            so.FindProperty("m_RectWire").objectReferenceValue = wire;
            so.FindProperty("m_Ring").objectReferenceValue = ring;
            so.FindProperty("m_RingLabel").objectReferenceValue = ringLabel;
            so.FindProperty("m_Radar").objectReferenceValue = radar;
            so.FindProperty("m_Line").objectReferenceValue = line;
            so.FindProperty("m_Radius").objectReferenceValue = sRadius;
            so.FindProperty("m_Segments").objectReferenceValue = sSeg;
            so.FindProperty("m_Fill").objectReferenceValue = sFill;
            so.FindProperty("m_Smooth").objectReferenceValue = tSmooth;
            so.FindProperty("m_Wire").objectReferenceValue = tWire;
            so.FindProperty("m_Random").objectReferenceValue = bRand;
            so.FindProperty("m_Info").objectReferenceValue = info;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
