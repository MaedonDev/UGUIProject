// 第 2 章配图与配套场景的搭景代码（不进书）。
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UGUIBook.Ch02
{
    static class Shots
    {
        static UISkin K => UISkin.Instance;
        static readonly Color Orange = new Color32(0xFF, 0x8A, 0x3D, 0xFF);
        static readonly Color Ink = new Color32(0x1F, 0x24, 0x36, 0xFF);

        static void Set(Object o, string prop, Object value)
        {
            var so = new SerializedObject(o);
            so.FindProperty(prop).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- 3D 小场景公用 ----------------
        static Material Mat(Shot s, string name, Color c, float gloss = 0.4f)
        {
            var m = s.MatAsset("Ch02", name, "Standard");
            m.color = c;
            m.SetFloat("_Glossiness", gloss);
            EditorUtility.SetDirty(m);
            return m;
        }

        static void Lighting()
        {
            var lg = new GameObject("Directional Light");
            var l = lg.AddComponent<Light>();
            l.type = LightType.Directional; l.intensity = 1.05f; l.shadows = LightShadows.Soft;
            lg.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.65f);
        }

        static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material m)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = name;
            go.transform.position = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        // ---------------- 2.1 三种渲染模式 ----------------
        static RectTransform DemoPanel(Transform root, float w, float h)
        {
            var card = UIKit.Card(root, "UIPanel", 24, false, Color.white);
            card.Place(0, -20, w, h);
            var t = UIKit.Text(card, "UI 面板", 64, Ink, bold: true, name: "Title");
            t.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(w - 60, 90));
            var b = UIKit.Button(card, "确定", null, 280, 96);
            b.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(280, 96));
            var bar = UIKit.Panel(root, new Color(0.12f, 0.14f, 0.24f, 0.85f), 16, "TopBar");
            bar.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(w + 200, 70));
            var bt = UIKit.Text(bar.transform, "HP 86/100    金币 1280", 32, Color.white, name: "Text");
            bt.rectTransform.Stretch();
            return card;
        }

        static GameObject ModesWorld(Shot s)
        {
            s.Cam.orthographic = false; s.Cam.fieldOfView = 50;
            s.Cam.transform.position = new Vector3(0, 1.4f, -6.2f);
            s.Cam.transform.rotation = Quaternion.Euler(9, 0, 0);
            s.Background(UIKit.Hex("#C9D3EA"));
            Lighting();
            Prim(PrimitiveType.Plane, "Ground", new Vector3(0, -0.8f, 2), new Vector3(3, 1, 3), Mat(s, "Ground", UIKit.Hex("#8E9AB8"), 0.1f));
            var cube = Prim(PrimitiveType.Cube, "Cube", new Vector3(0.2f, 0.25f, 0), Vector3.one * 1.7f, Mat(s, "Cube", Orange, 0.5f));
            cube.transform.rotation = Quaternion.Euler(18, 35, 0);
            return cube;
        }

        [BookShot("ch02-modes", W = 480, H = 320, Panels = 3)]
        static void Modes(Shot s)
        {
            ModesWorld(s);
            s.Ref(960, 640, 0.5f);
            DemoPanel(s.Root, 560, 340);
            string[] names = { "Screen Space - Overlay", "Screen Space - Camera", "World Space" };
            switch (s.Panel)
            {
                case 0:
                    // 截图流程会把 Overlay 临时转成 Camera 模式；把平面放到近裁剪面附近，效果与 Overlay 相同：UI 永远在最上面
                    s.Canvas.renderMode = RenderMode.ScreenSpaceCamera; s.Canvas.worldCamera = s.Cam; s.Canvas.planeDistance = 0.5f;
                    break;
                case 1:
                    s.Canvas.renderMode = RenderMode.ScreenSpaceCamera; s.Canvas.worldCamera = s.Cam; s.Canvas.planeDistance = 6.0f;
                    break;
                case 2:
                    s.Canvas.renderMode = RenderMode.WorldSpace; s.Canvas.worldCamera = s.Cam;
                    var rt = s.Root;
                    rt.sizeDelta = new Vector2(960, 640);
                    rt.localScale = Vector3.one * 0.0042f;
                    rt.position = new Vector3(-1.2f, 0.9f, 1.6f);
                    rt.rotation = Quaternion.Euler(0, -32, 0);
                    break;
            }
            s.Label(new Vector2(0, 280), names[s.Panel], Ink, 40);
        }

        [BookScene("Ch02_RenderModes")]
        static void ModesScene(Shot s)
        {
            ModesWorld(s);
            GameObject.Find("Cube").AddComponent<Spin>();
            s.Ref(960, 640, 0.5f);
            s.Canvas.name = "TargetCanvas";
            DemoPanel(s.Root, 560, 340);
            // 控制按钮放在另一个 Overlay 画布上，不受被切换画布的影响
            var ctl = UIKit.Canvas("ControlCanvas");
            ctl.sortingOrder = 10;
            var row = UIKit.Node(ctl.transform, "Buttons");
            row.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1500, 100));
            row.HLayout(24);
            var b1 = UIKit.Button(row, "Overlay", K.dark, 380, 90, fontSize: 32);
            var b2 = UIKit.Button(row, "Camera", K.dark, 380, 90, fontSize: 32);
            var b3 = UIKit.Button(row, "World Space", K.dark, 380, 90, fontSize: 32);
            var label = UIKit.Text(ctl.transform, "当前：ScreenSpaceOverlay", 36, Ink, bold: true, name: "ModeLabel");
            label.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 160), new Vector2(1000, 60));
            var sw = ctl.gameObject.AddComponent<RenderModeSwitcher>();
            Set(sw, "target", s.Canvas); Set(sw, "uiCamera", s.Cam);
            Set(sw, "overlayButton", b1); Set(sw, "cameraButton", b2); Set(sw, "worldButton", b3); Set(sw, "label", label);
        }

        // ---------------- 2.3 / 案例 2.1 多分辨率 HUD ----------------
        class Hud { public RectTransform refArea; public ScreenInfo info; }

        static Hud BuildHud(Shot s, float match, bool controls)
        {
            s.Ref(1920, 1080, match);
            var h = new Hud();
            var root = s.Root;
            var bg = UIKit.Image(root, "bg_landscape", Color.white, "Background");
            bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 2;

            h.refArea = UIKit.Node(root, "ReferenceArea_1920x1080");
            h.refArea.Place(0, 0, 1920, 1080);

            // 左上：头像 + 血条
            var player = UIKit.Node(root, "Player");
            player.Place(new Vector2(0, 1), new Vector2(40, -40), new Vector2(560, 150));
            var av = UIKit.Image(player, "circle", Color.white, "AvatarFrame");
            av.rectTransform.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(150, 150));
            var avi = UIKit.Image(av.transform, "avatar_1", Color.white, "Avatar");
            avi.rectTransform.Stretch(8, 8, 8, 8);
            var nm = UIKit.Text(player, "麦冬  Lv.12", 40, Color.white, TextAlignmentOptions.MidlineLeft, true, "Name");
            nm.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(175, 30), new Vector2(380, 56));
            var hpbg = UIKit.Image(player, "pill", new Color(0, 0, 0, 0.45f), "HpBar");
            hpbg.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(175, -30), new Vector2(360, 34));
            var hp = UIKit.Image(hpbg.transform, "pill", K.success, "Fill");
            hp.rectTransform.anchorMin = Vector2.zero; hp.rectTransform.anchorMax = new Vector2(0.72f, 1);
            hp.rectTransform.offsetMin = new Vector2(4, 4); hp.rectTransform.offsetMax = new Vector2(0, -4);

            // 顶部居中：关卡 + 计时
            var stage = UIKit.Image(root, "pill", new Color(0, 0, 0, 0.45f), "StagePill");
            stage.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(380, 80));
            var st = UIKit.Text(stage.transform, "第 3 关  02:15", 38, Color.white, bold: true);
            st.rectTransform.Stretch();

            // 右上：金币 + 设置
            var coin = UIKit.Image(root, "pill", new Color(0, 0, 0, 0.45f), "Coins");
            coin.rectTransform.Place(new Vector2(1, 1), new Vector2(-170, -50), new Vector2(300, 80));
            var ci = UIKit.Icon(coin.transform, "item_coin", 62, Color.white);
            ci.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(62, 62));
            var ct = UIKit.Text(coin.transform, "12,580", 38, Color.white, TextAlignmentOptions.MidlineRight, true);
            ct.rectTransform.Stretch(80, 0, 28, 0);
            var gear = UIKit.IconButton(root, "gear", new Color(0, 0, 0, 0.45f), 96);
            gear.Rt().Place(new Vector2(1, 1), new Vector2(-40, -42), new Vector2(96, 96));

            // 左下：虚拟摇杆
            var joy = UIKit.Image(root, "circle", new Color(1, 1, 1, 0.22f), "Joystick");
            joy.rectTransform.Place(new Vector2(0, 0), new Vector2(90, 70), new Vector2(300, 300));
            var knob = UIKit.Image(joy.transform, "circle", new Color(1, 1, 1, 0.75f), "Knob");
            knob.rectTransform.Place(0, 0, 130, 130);

            // 右下：攻击 + 技能
            var atk = UIKit.IconButton(root, "item_sword", K.accent, 230, Color.white);
            atk.name = "Attack";
            atk.Rt().Place(new Vector2(1, 0), new Vector2(-80, 70), new Vector2(230, 230));
            string[] sk = { "star", "heart", "gift" };
            Vector2[] sp = { new Vector2(-340, 70), new Vector2(-320, 250), new Vector2(-130, 330) };
            for (int i = 0; i < 3; i++)
            {
                var b = UIKit.IconButton(root, sk[i], new Color(0.13f, 0.15f, 0.27f, 0.75f), 120);
                b.Rt().Place(new Vector2(1, 0), sp[i], new Vector2(120, 120));
            }

            // 中间：适配信息
            var infoP = UIKit.Panel(root, new Color(0.08f, 0.09f, 0.18f, 0.72f), 24, "InfoPanel");
            infoP.rectTransform.Place(0, 60, 820, 230);
            infoP.raycastTarget = false;
            var it = UIKit.Text(infoP.transform, "", 36, Color.white, TextAlignmentOptions.Left, name: "InfoText");
            it.rectTransform.Stretch(40, 20, 30, 20);
            it.lineSpacing = 12;
            h.info = infoP.gameObject.AddComponent<ScreenInfo>();
            Set(h.info, "canvas", s.Canvas); Set(h.info, "output", it);

            if (controls)
            {
                var row = UIKit.Node(root, "MatchRow");
                row.Place(new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(760, 80));
                var lab = UIKit.Text(row, "Match", 34, Color.white, TextAlignmentOptions.MidlineLeft, true, "Label");
                lab.rectTransform.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(140, 60));
                var sl = UIKit.Slider(row, match, 600, 44);
                sl.Rt().Place(new Vector2(1, 0.5f), Vector2.zero, new Vector2(600, 44));
                var mc = row.gameObject.AddComponent<MatchControl>();
                Set(mc, "scaler", s.Scaler); Set(mc, "slider", sl);
            }
            s.BeforeCapture(() => h.info.Refresh());
            return h;
        }

        /// 配图专用：把 1920×1080 参考区域画成一个粗橙框（BeforeCapture 里加，不进场景）
        static void RefFrame(Shot s, Hud h) => s.BeforeCapture(() =>
        {
            var f = UIKit.Image(h.refArea, "round_outline_8", Orange, "Frame");
            f.rectTransform.Stretch(); f.raycastTarget = false;
            f.pixelsPerUnitMultiplier = 0.2f;   // 边框加粗 5 倍
        });

        [BookShot("ch02-aspect-169", W = 480, H = 270, Scale = 2, Scene = "Ch02_Adaptive")]
        static void A169(Shot s) { BuildHud(s, 0.5f, true); }
        [BookShot("ch02-aspect-43", W = 360, H = 270, Scale = 2)]
        static void A43(Shot s) { BuildHud(s, 0.5f, false); }
        [BookShot("ch02-aspect-219", W = 630, H = 270, Scale = 2)]
        static void A219(Shot s) { BuildHud(s, 0.5f, false); }
        [BookShot("ch02-aspect-portrait", W = 152, H = 270, Scale = 3)]
        static void APortrait(Shot s) { BuildHud(s, 0.5f, false); }

        [BookShot("ch02-match-43", W = 360, H = 270, Panels = 3, Scale = 2)]
        static void Match43(Shot s)
        {
            var h = BuildHud(s, s.Panel * 0.5f, false);
            RefFrame(s, h);
        }

        static void Match219(Shot s, float m)
        {
            var h = BuildHud(s, m, false);
            RefFrame(s, h);
        }
        [BookShot("ch02-match-219-0", W = 630, H = 270, Scale = 2)] static void M219a(Shot s) => Match219(s, 0);
        [BookShot("ch02-match-219-05", W = 630, H = 270, Scale = 2)] static void M219b(Shot s) => Match219(s, 0.5f);
        [BookShot("ch02-match-219-1", W = 630, H = 270, Scale = 2)] static void M219c(Shot s) => Match219(s, 1);

        [BookShot("ch02-expand-shrink", W = 360, H = 270, Panels = 2, Scale = 2)]
        static void ExpandShrink(Shot s)
        {
            var h = BuildHud(s, 0.5f, false);
            s.Scaler.screenMatchMode = s.Panel == 0 ? CanvasScaler.ScreenMatchMode.Expand : CanvasScaler.ScreenMatchMode.Shrink;
            RefFrame(s, h);
        }

        // ---------------- 案例 2.2 World Space 头顶名字 ----------------
        [BookShot("ch02-worldspace", W = 960, H = 540, Scene = "Ch02_WorldSpace")]
        static void WorldSpace(Shot s)
        {
            s.Background(UIKit.Hex("#BFCBE6"));
            Lighting();
            s.Cam.orthographic = false; s.Cam.fieldOfView = 45;
            var orbit = s.Cam.gameObject.AddComponent<OrbitCamera>();
            var so = new SerializedObject(orbit);
            so.FindProperty("yaw").floatValue = -18; so.FindProperty("pitch").floatValue = 16; so.FindProperty("distance").floatValue = 8.5f;
            so.ApplyModifiedPropertiesWithoutUndo();
            orbit.Apply();
            Prim(PrimitiveType.Plane, "Ground", new Vector3(0, 0, 0), new Vector3(10, 1, 10), Mat(s, "Ground", UIKit.Hex("#8E9AB8"), 0.1f));

            string[] names = { "麦冬", "白芷", "当归" };
            string[] lv = { "Lv.12", "Lv.8", "Lv.20" };
            Color[] cols = { UIKit.Hex("#5B4BD6"), UIKit.Hex("#2FB36A"), UIKit.Hex("#E5484D") };
            float[] hp = { 0.85f, 0.45f, 0.2f };
            Vector3[] pos = { new Vector3(-3f, 1, 0.5f), new Vector3(0, 1, 2.5f), new Vector3(3f, 1, 0) };
            RectTransform first = null;
            for (int i = 0; i < 3; i++)
            {
                var body = Prim(PrimitiveType.Capsule, "Hero_" + names[i], pos[i], Vector3.one, Mat(s, "Hero" + i, cols[i], 0.5f));
                var c = UIKit.Canvas("NamePlate", RenderMode.WorldSpace);
                c.worldCamera = s.Cam;
                Object.DestroyImmediate(c.GetComponent<CanvasScaler>());   // World Space 下不需要按屏幕缩放
                var rt = (RectTransform)c.transform;
                rt.SetParent(body.transform, false);
                rt.sizeDelta = new Vector2(420, 130);
                rt.localPosition = new Vector3(0, 1.6f, 0);
                rt.localScale = Vector3.one * 0.0065f;   // 1 个 UI 单位 = 0.0065 米：420 宽 ≈ 2.7 米
                var pill = UIKit.Image(rt, "pill", new Color(0.1f, 0.11f, 0.2f, 0.7f), "NameBg");
                pill.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(340, 70));
                pill.raycastTarget = false;
                var t = UIKit.Text(pill.transform, $"<size=70%><color=#FFD27A>{lv[i]}</color></size> {names[i]}", 40, Color.white, bold: true, name: "Name");
                t.rectTransform.Stretch();
                var bar = UIKit.Image(rt, "pill", new Color(0.1f, 0.11f, 0.2f, 0.75f), "HpBar");
                bar.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 10), new Vector2(320, 34));
                var fill = UIKit.Image(bar.transform, "pill", hp[i] > 0.5f ? K.success : hp[i] > 0.3f ? K.accent : K.danger, "Fill");
                fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(hp[i], 1);
                fill.rectTransform.offsetMin = new Vector2(5, 5); fill.rectTransform.offsetMax = new Vector2(-1, -5);
                var bb = c.gameObject.AddComponent<Billboard>();
                bb.Face();
                if (i == 0) first = rt;
            }
            var hint = UIKit.Text(s.Root, "按住鼠标左键拖动旋转视角，滚轮缩放：名字牌始终正对屏幕", 34, Ink, name: "Hint");
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1600, 60));
            s.BeforeCapture(() => { foreach (var b in Object.FindObjectsOfType<Billboard>()) b.Face(); });
            s.Box(first, Orange, "World Space 画布");
        }

        // ---------------- 案例 2.3 3D 物体夹在两层 UI 之间 ----------------
        [BookShot("ch02-sandwich", W = 960, H = 540, Scene = "Ch02_Sandwich")]
        static void Sandwich(Shot s)
        {
            Lighting();
            s.Cam.orthographic = false; s.Cam.fieldOfView = 40;
            s.Cam.transform.position = new Vector3(0, 0, -10);
            s.Background(Color.black);

            // 后层画布：背景与信息面板
            var back = s.Canvas; back.name = "BackCanvas";
            back.renderMode = RenderMode.ScreenSpaceCamera; back.worldCamera = s.Cam;
            back.planeDistance = 20; back.sortingOrder = 0;
            s.Ref(1920, 1080, 0.5f);
            var bg = UIKit.Image(back.transform, "bg_night", Color.white, "Background");
            bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent; fit.aspectRatio = 2;
            var info = UIKit.Card(back.transform, "StatsCard", 24, false, new Color(1, 1, 1, 0.12f));
            info.Place(new Vector2(0, 0.5f), new Vector2(80, 0), new Vector2(520, 560));
            string[] lines = { "<b>星辉宝石</b>", "攻击  +120", "暴击  +8%", "速度  +15", "<color=#FFD27A>传说 · 唯一</color>" };
            for (int i = 0; i < lines.Length; i++)
            {
                var t = UIKit.Text(info, lines[i], i == 0 ? 52 : 38, Color.white, TextAlignmentOptions.MidlineLeft, name: "Line" + i);
                t.rectTransform.Place(new Vector2(0, 1), new Vector2(50, -60 - i * 100), new Vector2(440, 70));
                t.rectTransform.pivot = new Vector2(0, 0.5f);
            }

            // 中间：3D 物体 + 粒子
            var gem = Prim(PrimitiveType.Cube, "Gem", new Vector3(0.6f, 0.2f, -3.5f), Vector3.one * 1.9f, Mat(s, "Gem", UIKit.Hex("#6FA8FF"), 0.85f));
            gem.transform.rotation = Quaternion.Euler(35, 45, 20);
            gem.AddComponent<Spin>();
            var psGo = new GameObject("Sparkles");
            psGo.transform.position = gem.transform.position;
            var ps = psGo.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 2.5f; main.startSpeed = 0.6f; main.startSize = 0.22f;
            main.startColor = new Color(1f, 0.85f, 0.45f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 40;
            var sh = ps.shape; sh.shapeType = ParticleSystemShapeType.Sphere; sh.radius = 1.8f;
            var pr = psGo.GetComponent<ParticleSystemRenderer>();
            pr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
            pr.sortingOrder = 1;   // 比后层画布大、比前层画布小

            // 前层画布：顶栏、按钮、横幅
            var front = UIKit.Canvas("FrontCanvas", RenderMode.ScreenSpaceCamera);
            front.worldCamera = s.Cam; front.planeDistance = 3; front.sortingOrder = 2;
            var fr = front.transform;
            var top = UIKit.Panel(fr, new Color(0.06f, 0.07f, 0.14f, 0.85f), 0, "TopBar");
            top.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(4000, 120));
            var back_b = UIKit.IconButton(fr, "back", Color.white, 90);
            back_b.Rt().Place(new Vector2(0, 1), new Vector2(40, -15), new Vector2(90, 90));
            var title = UIKit.Text(fr, "宝石详情", 50, Color.white, bold: true, name: "Title");
            title.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(600, 80));
            var banner = UIKit.Image(fr, "pill", new Color(1f, 0.55f, 0.24f, 0.95f), "Banner");
            banner.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(240, -170), new Vector2(560, 90));
            var bt = UIKit.Text(banner.transform, "限时 8 折", 46, Color.white, bold: true);
            bt.rectTransform.Stretch();
            var buy = UIKit.Button(fr, "购买  680", K.primary, 420, 110, "item_gem");
            buy.Rt().Place(new Vector2(1, 0), new Vector2(-80, 60), new Vector2(420, 110));

            s.BeforeCapture(() => { ps.Simulate(3f, true, true); });
            s.Label(new Vector2(-420, -380), "BackCanvas：Order 0，距离 20", Ink, 30);
            s.Label(new Vector2(-420, -440), "宝石（不透明 3D）+ 粒子：Order 1", Ink, 30);
            s.Label(new Vector2(-420, -500), "FrontCanvas：Order 2，距离 3", Ink, 30);
        }

        // ---------------- 案例 2.4 多画布与 Override Sorting ----------------
        [BookShot("ch02-sorting", W = 640, H = 400, Panels = 2, Scene = "Ch02_Sorting")]
        static void Sorting(Shot s)
        {
            s.Ref(1280, 800, 0.5f);
            s.Canvas.name = "HudCanvas";   // Sort Order 0
            var bag = UIKit.Button(s.Root, "背包", null, 260, 100, "bag");
            bag.Rt().Place(new Vector2(0, 0), new Vector2(80, 260), new Vector2(260, 100));
            // 提示气泡：挂在 HUD 的按钮下面，但自己带一个 Canvas
            var tipGo = UIKit.Node(bag.transform, "Tip");
            tipGo.Place(new Vector2(1, 1), new Vector2(-40, -10), new Vector2(380, 110));
            tipGo.pivot = new Vector2(0, 0);
            var nested = tipGo.gameObject.AddComponent<Canvas>();
            tipGo.gameObject.AddComponent<GraphicRaycaster>();
            var tipBg = UIKit.Image(tipGo, "round_16", K.accent, "Bg");
            tipBg.rectTransform.Stretch();
            var tipT = UIKit.Text(tipGo, "新获得 3 件装备！", 34, Color.white, bold: true);
            tipT.rectTransform.Stretch();

            var popup = UIKit.Canvas("PopupCanvas");
            popup.sortingOrder = 10;
            var card = UIKit.Card(popup.transform, "Notice", 24);
            card.Place(80, 30, 720, 460);
            var ct = UIKit.Text(card, "系统公告", 46, Ink, bold: true, name: "Title");
            ct.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(560, 70));
            var cb = UIKit.Text(card, "PopupCanvas 的 Sort Order = 10\nHudCanvas 的 Sort Order = 0", 32, K.muted, name: "Body");
            cb.rectTransform.Place(0, 0, 580, 140);
            var tgl = UIKit.Toggle(card, "气泡的 Override Sorting", s.Panel == 1, 520, 64);
            tgl.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(520, 64));
            var demo = popup.gameObject.AddComponent<SortingDemo>();
            Set(demo, "nestedCanvas", nested); Set(demo, "overrideToggle", tgl);
            nested.overrideSorting = s.Panel == 1;
            nested.sortingOrder = 20;
            s.Label(new Vector2(0, 350), s.Panel == 0 ? "气泡跟随 HudCanvas（Order 0）：被弹窗盖住" : "气泡 Override Sorting = 20：盖过弹窗",
                Ink, 34);
        }
    }
}
