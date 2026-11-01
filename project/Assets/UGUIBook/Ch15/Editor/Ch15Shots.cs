// 第 15 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UGUIBook.Ch15
{
    static class Ch15Shots
    {
        static UISkin K => UISkin.Instance;

        static void Bind(Object target, params (string prop, Object value)[] refs)
        {
            var so = new SerializedObject(target);
            foreach (var (p, v) in refs) so.FindProperty(p).objectReferenceValue = v;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BindArray(Object target, string prop, Object[] values)
        {
            var so = new SerializedObject(target);
            var a = so.FindProperty(prop); a.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) a.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TextMeshProUGUI TimeLabel(Shot s, float t, Color? c = null)
        {
            var lab = UIKit.Text(s.Root, $"t = {t:0.00} s", 30, c ?? K.muted);
            lab.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(400, 50));
            return lab;
        }

        // ------------------------------------------------------------------ 弹窗回弹
        [BookShot("ch15-popup", W = 300, H = 420, Panels = 4, Scene = "Ch15_Popup")]
        static void Popup(Shot s)
        {
            s.Ref(600, 840, 0.5f);
            var bg = UIKit.Image(s.Root, "bg_landscape", Color.white, "Background"); bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var open = UIKit.Button(s.Root, "打开弹窗", K.primary, 300, 90, null, 34); open.Rt().Place(0, -300, 300, 90);

            var popup = UIKit.Node(s.Root, "Popup"); popup.Stretch();
            var dimImg = UIKit.Image(popup, null, new Color(0.04f, 0.05f, 0.1f, 0.6f), "Dim"); dimImg.rectTransform.Stretch();
            var dim = dimImg.gameObject.AddComponent<CanvasGroup>();
            var panel = UIKit.Card(popup, "Panel", 24); panel.Place(0, 20, 480, 420);
            var pg = panel.gameObject.AddComponent<CanvasGroup>();
            var title = UIKit.Text(panel, "每日奖励", 44, K.ink, TextAlignmentOptions.Center, true); title.rectTransform.Place(0, 140, 400, 70);
            var gift = UIKit.Icon(panel, "gift", 130, K.accent); gift.rectTransform.anchoredPosition = new Vector2(0, 10);
            var close = UIKit.Button(panel, "收下", K.accent, 240, 84, null, 34); close.Rt().Place(0, -140, 240, 84);
            var anim = popup.gameObject.AddComponent<PopupAnim>();
            anim.Setup(dim, panel, pg);
            var demo = s.Root.gameObject.AddComponent<PopupDemo>();
            Bind(demo, ("m_Popup", anim), ("m_Open", open), ("m_Close", close));
            Bind(anim, ("m_Dim", dim), ("m_Panel", panel), ("m_PanelGroup", pg));

            float[] times = { 0.04f, 0.12f, 0.22f, 0.4f };
            float t = times[s.Panel];
            s.BeforeCapture(() => anim.BuildShow().Evaluate(t));
            TimeLabel(s, t, Color.white);
        }

        // ------------------------------------------------------------------ 列表依次入场
        [BookShot("ch15-list-enter", W = 300, H = 480, Panels = 3, Scene = "Ch15_ListEnter")]
        static void ListEnter(Shot s)
        {
            s.Ref(600, 960, 0.5f);
            s.Background(K.page);
            var head = UIKit.Text(s.Root, "任务列表", 42, K.ink, TextAlignmentOptions.Left, true);
            head.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(520, 70));
            var list = UIKit.Node(s.Root, "List"); list.Place(new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(520, 700));
            var vl = list.VLayout(18);
            string[] names = { "击败 10 只史莱姆", "收集 5 朵蓝花", "与村长对话", "强化武器一次", "完成一次副本" };
            string[] icons = { "item_sword", "item_potion_blue", "chat", "item_gem", "trophy", "gift" };
            for (int i = 0; i < names.Length; i++)
            {
                var item = UIKit.Node(list, "Item" + i); item.sizeDelta = new Vector2(520, 96);
                var body = UIKit.Card(item, "Body", 16); body.Stretch();
                var ic = UIKit.Icon(body, icons[i], 60, i == 2 || i == 4 || i == 5 ? K.primary : Color.white);
                ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0, 0.5f); ic.rectTransform.anchoredPosition = new Vector2(56, 0);
                var t = UIKit.Text(body, names[i], 30, K.ink, TextAlignmentOptions.Left); t.rectTransform.Stretch(110, 0, 20, 0);
            }
            var entrance = list.gameObject.AddComponent<ListEntrance>();
            var replay = UIKit.Button(s.Root, "重播", K.primary, 220, 80, "refresh", 30);
            replay.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(220, 80));
            var demo = s.Root.gameObject.AddComponent<ListDemo>();
            Bind(demo, ("m_List", entrance), ("m_Replay", replay));
            float[] times = { 0.12f, 0.3f, 0.55f };
            float time = times[s.Panel];
            s.BeforeCapture(() => entrance.Build().Evaluate(time));
            TimeLabel(s, time);
        }

        // ------------------------------------------------------------------ 飞金币
        [BookShot("ch15-coin-fly", W = 400, H = 420, Panels = 3, Scene = "Ch15_CoinFly")]
        static void Coin(Shot s)
        {
            s.Ref(800, 840, 0.5f);
            s.Background(K.dark);
            var top = UIKit.Panel(s.Root, new Color(1, 1, 1, 0.08f), 0, "TopBar"); top.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(800, 110));
            var coinIcon = UIKit.Icon(top.transform, "item_coin", 70, Color.white, "CoinIcon"); coinIcon.rectTransform.anchoredPosition = new Vector2(-260, 0);
            var num = UIKit.Text(top.transform, "1,280", 44, Color.white, TextAlignmentOptions.Left, true); num.rectTransform.Place(-60, 0, 300, 70);
            var roller = num.gameObject.AddComponent<NumberRoller>();
            var chest = UIKit.Icon(s.Root, "gift", 200, K.accent, "Chest"); chest.rectTransform.anchoredPosition = new Vector2(0, -120);
            var claim = UIKit.Button(s.Root, "领取 1200 金币", K.accent, 360, 90, null, 32); claim.Rt().Place(0, -280, 360, 90);
            var layer = UIKit.Node(s.Root, "FlyLayer"); layer.Stretch();
            var tpl = UIKit.Image(layer, "item_coin", Color.white, "CoinTemplate"); tpl.rectTransform.sizeDelta = new Vector2(56, 56); tpl.raycastTarget = false;
            tpl.gameObject.SetActive(false);
            var fly = s.Root.gameObject.AddComponent<CoinFly>();
            fly.Setup(tpl, layer, coinIcon.rectTransform, roller);
            Bind(fly, ("m_CoinTemplate", tpl), ("m_Layer", layer), ("m_Target", coinIcon.rectTransform), ("m_Counter", roller));
            var demo = s.Root.gameObject.AddComponent<CoinDemo>();
            Bind(demo, ("m_Fly", fly), ("m_Source", chest.rectTransform), ("m_Claim", claim));
            float[] times = { 0.25f, 0.85f, 1.1f };
            float time = times[s.Panel];
            s.BeforeCapture(() =>
            {
                roller.SetImmediate(1280);
                var from = (Vector2)layer.InverseTransformPoint(chest.rectTransform.position);
                fly.Build(from, 1200).Evaluate(time);
            });
            TimeLabel(s, time, new Color(1, 1, 1, 0.6f));
        }

        // ------------------------------------------------------------------ 屏幕转场
        [BookShot("ch15-transition", W = 400, H = 260, Panels = 3, Scene = "Ch15_Transition")]
        static void Transition(Shot s)
        {
            s.Ref(1600, 1040, 0.5f);
            var pageA = UIKit.Image(s.Root, "bg_landscape", Color.white, "PageA"); pageA.rectTransform.Stretch();
            var ta = UIKit.Text(pageA.transform, "主城", 120, Color.white, TextAlignmentOptions.Center, true); ta.rectTransform.Place(0, 220, 600, 160);
            var pageB = UIKit.Image(s.Root, "bg_night", Color.white, "PageB"); pageB.rectTransform.Stretch();
            var tb = UIKit.Text(pageB.transform, "副本", 120, Color.white, TextAlignmentOptions.Center, true); tb.rectTransform.Place(0, 220, 600, 160);
            pageB.gameObject.SetActive(false);
            var row = UIKit.Node(s.Root, "Buttons"); row.Place(0, -300, 1300, 140); row.HLayout(50);
            var b0 = UIKit.Button(row, "擦除", K.primary, 360, 130, null, 50);
            var b1 = UIKit.Button(row, "圆形", K.accent, 360, 130, null, 50);
            var b2 = UIKit.Button(row, "溶解", K.success, 360, 130, null, 50);
            var cover = UIKit.Image(s.Root, null, new Color(0.08f, 0.09f, 0.14f, 1), "Transition"); cover.rectTransform.Stretch();
            var mat = s.MatAsset("Ch15", "Transition", "UGUIBook/Ch15/Transition");
            mat.SetTexture("_NoiseTex", UIKit.Sp("noise").texture);
            mat.SetFloat("_Soft", 0.06f);
            cover.material = mat; cover.raycastTarget = false;
            var tr = cover.gameObject.AddComponent<ScreenTransition>();
            var demo = s.Root.gameObject.AddComponent<TransitionDemo>();
            Bind(demo, ("m_Transition", tr));
            BindArray(demo, "m_Buttons", new Object[] { b0, b1, b2 });
            BindArray(demo, "m_Pages", new Object[] { pageA.gameObject, pageB.gameObject });
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                tr.shape = (ScreenTransition.Shape)panel;
                tr.center = new Vector2(0.5f, 0.21f);
                tr.progress = 0.55f;
            });
        }

        // ------------------------------------------------------------------ Toast
        [BookShot("ch15-toast", W = 400, H = 420, Scene = "Ch15_Toast")]
        static void Toast(Shot s)
        {
            s.Ref(800, 840, 0.5f);
            s.Background(K.page);
            var btn = UIKit.Button(s.Root, "弹一条提示", K.primary, 320, 90, "bell", 32); btn.Rt().Place(0, 250, 320, 90);
            var anchor = UIKit.Node(s.Root, "ToastAnchor"); anchor.Place(new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(10, 10));
            var tpl = UIKit.Node(anchor, "ToastTemplate"); tpl.Place(0, 0, 520, 84);
            tpl.gameObject.AddComponent<CanvasGroup>();
            var bg = UIKit.Panel(tpl, new Color(0.1f, 0.11f, 0.16f, 0.88f), 32, "Bg"); bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var tt = UIKit.Text(tpl, "提示", 32, Color.white); tt.rectTransform.Stretch(20, 0, 20, 0);
            tpl.gameObject.SetActive(false);
            var mgr = s.Root.gameObject.AddComponent<ToastManager>();
            Bind(mgr, ("m_Template", tpl));
            var demo = s.Root.gameObject.AddComponent<ToastDemo>();
            Bind(demo, ("m_Toast", mgr), ("m_Button", btn));
            s.BeforeCapture(() =>
            {
                // 三条提示分别处在：快要淡出 / 停留中 / 刚滑入一半
                var a = mgr.Show("任务完成：初入江湖", false);
                var b = mgr.Show("背包已满", false);
                var c = mgr.Show("金币 +100", false);
                mgr.Place(a, 2.15f, 2);
                mgr.Place(b, 1.0f, 1);
                mgr.Place(c, 0.12f, 0);
            });
            s.Label(new Vector2(330, 0), "淡出中", K.muted, 24);
            s.Label(new Vector2(330, -110), "停留中", K.muted, 24);
            s.Label(new Vector2(330, -220), "滑入中", K.muted, 24);
        }

        // ------------------------------------------------------------------ 呼吸、数字滚动、抖动
        [BookShot("ch15-feedback", W = 400, H = 260, Panels = 3, Scene = "Ch15_Feedback")]
        static void Feedback(Shot s)
        {
            s.Ref(800, 520, 0.5f);
            s.Background(Color.white);
            var coin = UIKit.Icon(s.Root, "item_coin", 70, Color.white); coin.rectTransform.anchoredPosition = new Vector2(-200, 170);
            var num = UIKit.Text(s.Root, "1,280", 54, K.ink, TextAlignmentOptions.Left, true); num.rectTransform.Place(30, 170, 340, 80);
            var roller = num.gameObject.AddComponent<NumberRoller>();
            var earn = UIKit.Button(s.Root, "领取", K.accent, 280, 100, "gift", 36); earn.Rt().Place(-170, -30, 280, 100);
            earn.gameObject.AddComponent<Breathing>();
            var spend = UIKit.Button(s.Root, "购买 5000", K.primary, 280, 100, null, 36); spend.Rt().Place(170, -30, 280, 100);
            var demo = s.Root.gameObject.AddComponent<FeedbackDemo>();
            Bind(demo, ("m_Gold", roller), ("m_Earn", earn), ("m_Spend", spend), ("m_SpendRect", spend.Rt()));
            float[] times = { 0.15f, 0.45f, 0.9f };
            float t = times[s.Panel];
            s.BeforeCapture(() =>
            {
                roller.SetImmediate(1280);
                roller.RollTo(3280, 0.9f).Evaluate(t);
                earn.gameObject.GetComponent<Breathing>().Build().Evaluate(t);
                spend.Rt().Shake(18, 0.4f).Evaluate(t * 0.4f);   // 抖动只有 0.4 秒，按比例取样
            });
            var lab = UIKit.Text(s.Root, $"t = {t:0.00} s", 28, K.muted); lab.rectTransform.Place(0, -200, 400, 50);
        }

        // ------------------------------------------------------------------ Animator 的开销（实测）
        const int BenchCount = 300, BenchFrames = 300;

        /// pulse = false：一段“看起来什么也没动”的循环动画（缩放恒为 1、透明度恒为 1）
        /// pulse = true：透明度 1 → 0.6 → 1 循环（真的在变）
        static RuntimeAnimatorController IdleController(bool pulse = false)
        {
            const string dir = "Assets/UGUIBook/Ch15/Animations";
            string name = pulse ? "PulseIcon" : "IdleIcon";
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch15", "Animations");
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{dir}/{name}.controller");
            if (ctrl != null) return ctrl;
            var clip = new AnimationClip { name = name };
            foreach (var axis in new[] { "x", "y", "z" })
                clip.SetCurve("", typeof(RectTransform), "m_LocalScale." + axis, AnimationCurve.Constant(0, 1, 1));
            clip.SetCurve("", typeof(Image), "m_Color.a", pulse
                ? new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.6f), new Keyframe(1, 1))
                : AnimationCurve.Constant(0, 1, 1));
            var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, $"{dir}/{name}.anim");
            return AnimatorController.CreateAnimatorControllerAtPathWithClip($"{dir}/{name}.controller", clip);
        }

        static RectTransform BenchGrid(Transform parent, out List<Image> images, out List<Animator> animators, RuntimeAnimatorController ctrl)
        {
            var grid = UIKit.Node(parent, "Grid"); grid.Place(0, -40, 1500, 760);
            grid.Grid(new Vector2(56, 56), new Vector2(6, 6));
            images = new List<Image>(); animators = new List<Animator>();
            string[] icons = { "star", "heart", "item_coin", "item_gem", "bell", "trophy" };
            for (int i = 0; i < BenchCount; i++)
            {
                var img = UIKit.Image(grid, icons[i % icons.Length], i % 6 < 2 ? K.primary : Color.white, "Icon" + i);
                img.raycastTarget = false;
                images.Add(img);
                var an = img.gameObject.AddComponent<Animator>();
                an.runtimeAnimatorController = ctrl;
                animators.Add(an);
            }
            return grid;
        }

        [BookShot("ch15-anim-bench", W = 960, H = 440)]
        static void AnimBench(Shot s)
        {
            s.Background(Color.white);
            var ctrl = IdleController();
            var holder = UIKit.Node(s.Root, "BenchHolder"); holder.Stretch();
            var cg = holder.gameObject.AddComponent<CanvasGroup>();
            BenchGrid(holder, out var images, out var animators, ctrl);
            var graphics = new List<Graphic>(images);
            var chart = UIKit.Node(s.Root, "Chart"); chart.Stretch();
            s.BeforeCapture(() =>
            {
                string[] names = { "静止（什么都不做）", "Animator × 300（常量曲线）", "Animator × 300（透明度在变）", "代码每帧改 Image.color × 300", "代码每帧改父节点 CanvasGroup.alpha" };
                int M = names.Length;
                var ms = new double[M]; var dirty = new int[M];
                var pulseCtrl = IdleController(true);
                for (int mode = 0; mode < M; mode++)
                {
                    foreach (var a in animators)
                    {
                        a.enabled = mode == 1 || mode == 2;
                        a.runtimeAnimatorController = mode == 2 ? pulseCtrl : ctrl;
                        if (a.enabled) a.Rebind();
                    }
                    Canvas.ForceUpdateCanvases();
                    if (mode == 1)
                    {
                        var im0 = images[0]; var c0 = im0.color; c0.a = 0.3f; im0.color = c0; im0.transform.localScale = Vector3.one * 2;
                        Canvas.ForceUpdateCanvases();
                        animators[0].Update(1f / 60);
                        bool queued = !CanvasUpdateRegistry.TryRegisterCanvasElementForGraphicRebuild(im0);
                        Debug.Log($"[Ch15Probe] after Animator.Update: alpha={im0.color.a} scale={im0.transform.localScale.x} queued={queued} isPlaying={Application.isPlaying}");
                        Canvas.ForceUpdateCanvases();
                    }
                    var sw = new Stopwatch(); int dirtySum = 0;
                    for (int f = 0; f < BenchFrames + 20; f++)
                    {
                        if (f == 20) { sw.Reset(); dirtySum = 0; }
                        sw.Start();
                        if (mode == 1 || mode == 2) foreach (var a in animators) a.Update(1f / 60);
                        else if (mode == 3) { float al = 0.6f + 0.4f * Mathf.Sin(f * 0.1f); foreach (var im in images) { var c = im.color; c.a = al; im.color = c; } }
                        else if (mode == 4) cg.alpha = 0.6f + 0.4f * Mathf.Sin(f * 0.1f);
                        sw.Stop();
                        dirtySum += RebuildProbe.CountDirty(graphics);   // 不计入耗时
                        sw.Start();
                        Canvas.ForceUpdateCanvases();                       // 走一遍 CanvasUpdateRegistry.PerformUpdate
                        sw.Stop();
                    }
                    ms[mode] = sw.Elapsed.TotalMilliseconds / BenchFrames;
                    dirty[mode] = Mathf.RoundToInt(dirtySum / (float)BenchFrames);
                }
                foreach (var a in animators) a.enabled = false;
                foreach (var im in images) { var c = im.color; c.a = 1; im.color = c; }
                cg.alpha = 1;
                var sb = new StringBuilder();
                for (int i = 0; i < M; i++) sb.AppendLine($"{names[i]}\t每帧 {ms[i]:F3} ms\t标脏 {dirty[i]} / {BenchCount}");
                Debug.Log("[Ch15Bench]\n" + sb);

                // 画成条形图（数字就是这一次运行的结果）
                holder.gameObject.SetActive(false);
                double max = 0; foreach (var v in ms) max = System.Math.Max(max, v);
                Color[] cols = { K.muted, K.danger, K.danger, K.accent, K.success };
                for (int i = 0; i < M; i++)
                {
                    float y = 280 - i * 135;
                    var lab = UIKit.Text(chart, names[i], 32, K.ink, TextAlignmentOptions.Right); lab.rectTransform.Place(-560, y, 640, 60);
                    float w = (float)(ms[i] / max) * 900;
                    var bar = UIKit.Panel(chart, cols[i], 8, "Bar" + i); bar.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-220, y), new Vector2(Mathf.Max(w, 12), 64));
                    bar.rectTransform.pivot = new Vector2(0, 0.5f); bar.rectTransform.anchoredPosition = new Vector2(-220, y);
                    var val = UIKit.Text(chart, $"{ms[i]:F2} ms   标脏 {dirty[i]}", 30, K.ink, TextAlignmentOptions.Left);
                    val.rectTransform.pivot = new Vector2(0, 0.5f); val.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-200 + Mathf.Max(w, 12), y), new Vector2(420, 50));
                    val.rectTransform.pivot = new Vector2(0, 0.5f); val.rectTransform.anchoredPosition = new Vector2(-200 + Mathf.Max(w, 12), y);
                }
            });
        }

        [BookScene("Ch15_AnimatorCost")]
        static void AnimatorCostScene(Shot s)
        {
            s.Background(Color.white);
            var ctrl = IdleController();
            BenchGrid(s.Root, out _, out _, ctrl);
            var toggle = UIKit.Toggle(s.Root, "启用 300 个 Animator", true, 520, 64);
            toggle.Rt().Place(new Vector2(0.5f, 1), new Vector2(-400, -50), new Vector2(520, 64));
            var info = UIKit.Text(s.Root, "", 32, K.ink, TextAlignmentOptions.Left);
            info.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(380, -50), new Vector2(900, 60));
            var demo = s.Root.gameObject.AddComponent<AnimatorCostDemo>();
            Bind(demo, ("m_Toggle", toggle), ("m_Info", info), ("m_Grid", s.Root.Find("Grid")));
        }
    }
}
