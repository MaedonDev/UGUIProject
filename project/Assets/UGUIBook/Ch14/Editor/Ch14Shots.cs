// 第 14 章配图与配套场景（搭景代码，不进书）
using System;
using System.Reflection;
using System.Text;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    static class Ch14Shots
    {
        static readonly Color Red = new Color32(0xE5, 0x48, 0x4D, 0xFF);
        static UISkin K => UISkin.Instance;

        static TextMeshProUGUI Caption(Transform parent, string text, float x, float y, float size = 30, Color? c = null, float w = 460)
        {
            var t = UIKit.Text(parent, text, size, c ?? K.ink, TextAlignmentOptions.Center);
            t.rectTransform.Place(x, y, w, size * 1.6f);
            return t;
        }

        // ---------------- 材质 ----------------
        static Material Mat(Shot s, string name) => s.MatAsset("Ch14", name, "UGUIBook/Ch14/" + name);

        static Material EffectMat(Shot s, string name)
        {
            var m = Mat(s, name);
            m.SetFloat("_UseVertexParam", 1);    // 本章效果材质统一从 uv1.x 读主参数（UIParamEffect）
            return m;
        }

        static Material DissolveMat(Shot s)
        {
            var m = EffectMat(s, "Dissolve");
            m.SetTexture("_NoiseTex", UIKit.Sp("noise").texture);
            m.SetTextureScale("_NoiseTex", Vector2.one);   // 这张噪声图不是无缝的，不要平铺
            m.SetFloat("_EdgeWidth", 0.1f);
            m.SetColor("_EdgeColor", new Color(1f, 0.55f, 0.15f, 1));
            return m;
        }

        static Material DistortMat(Shot s)
        {
            var m = EffectMat(s, "Distort");
            m.SetFloat("_Aspect", 1.5f); m.SetFloat("_Amplitude", 0.04f); m.SetFloat("_Frequency", 50); m.SetFloat("_RingWidth", 0.12f);
            return m;
        }

        static Material WaveMat(Shot s)
        {
            var m = s.MatAsset("Ch14", "DistortWave", "UGUIBook/Ch14/Distort");
            m.SetFloat("_UseVertexParam", 0); m.SetFloat("_Progress", 1);
            m.SetFloat("_WaveAmp", 0.08f); m.SetFloat("_WaveFreq", 12); m.SetFloat("_WavePhase", 0);
            return m;
        }

        static UIParamEffect Param(Graphic g, float v)
        {
            var p = g.gameObject.AddComponent<UIParamEffect>();
            p.value = v;
            return p;
        }

        // ------------------------------------------------------------------ 灰度
        [BookShot("ch14-gray", W = 960, H = 420)]
        static void Gray(Shot s)
        {
            s.Background(Color.white);
            var mat = EffectMat(s, "Gray");
            mat.SetFloat("_Brightness", 0.95f);
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_gem", "item_bow", "item_helmet" };
            for (int i = 0; i < 6; i++)
            {
                float x = -700 + i * 280;
                var a = UIKit.Image(s.Root, items[i], Color.white, "Normal_" + i); a.rectTransform.Place(x, 200, 180, 180); a.preserveAspect = true;
                var b = UIKit.Image(s.Root, items[i], Color.white, "Gray_" + i); b.rectTransform.Place(x, -110, 180, 180); b.preserveAspect = true;
                b.material = mat;
                Param(b, i / 5f);
                Caption(s.Root, $"param = {i / 5f:0.0}", x, -250, 30, K.muted, 260);
            }
            Caption(s.Root, "默认材质（UI/Default）", 0, 60, 32, K.ink, 800);
            Caption(s.Root, "UGUIBook/Ch14/Gray：6 个元素共用一个材质，灰度来自各自的 uv1.x", 0, -340, 32, K.ink, 1400);
        }

        // ------------------------------------------------------------------ 流光
        [BookShot("ch14-shine", W = 960, H = 360)]
        static void Shine(Shot s)
        {
            s.Background(K.page);
            var mat = EffectMat(s, "Shine");
            mat.SetFloat("_ShineWidth", 0.12f); mat.SetFloat("_Angle", 25); mat.SetColor("_ShineColor", new Color(1, 1, 1, 0.6f));
            float[] ps = { 0.15f, 0.35f, 0.55f, 0.75f, 0.95f };
            for (int i = 0; i < 5; i++)
            {
                float x = -720 + i * 360;
                var b = UIKit.Button(s.Root, "领取奖励", K.accent, 320, 120, "gift", 34);
                b.Rt().Place(x, 60, 320, 120);
                var img = b.GetComponent<Image>();
                img.material = mat;
                Param(img, ps[i]);
                Caption(s.Root, $"progress = {ps[i]:0.00}", x, -60, 30, K.muted, 340);
            }
            Caption(s.Root, "同一按钮在扫光过程中的 5 个时刻", 0, -230, 32, K.ink, 1200);
        }

        // ------------------------------------------------------------------ 溶解
        [BookShot("ch14-dissolve", W = 960, H = 380)]
        static void Dissolve(Shot s)
        {
            s.Background(K.dark);
            var mat = DissolveMat(s);
            float[] ps = { 0f, 0.3f, 0.55f, 0.8f };
            for (int i = 0; i < 4; i++)
            {
                float x = -660 + i * 440;
                var card = UIKit.Image(s.Root, "avatar_" + (i + 2), Color.white, "Card_" + i);
                card.rectTransform.Place(x, 60, 300, 300);
                card.material = mat;
                Param(card, ps[i]);
                Caption(s.Root, $"progress = {ps[i]:0.00}", x, -150, 32, new Color(1, 1, 1, 0.75f), 400);
            }
            Caption(s.Root, "噪声图决定烧掉的顺序，_EdgeColor 是正在燃烧的边", 0, -280, 32, Color.white, 1400);
        }

        // ------------------------------------------------------------------ 扭曲
        [BookShot("ch14-distort", W = 960, H = 380)]
        static void Distort(Shot s)
        {
            s.Background(Color.white);
            var mat = DistortMat(s);
            float[] ps = { 0.1f, 0.25f, 0.45f };
            for (int i = 0; i < 3; i++)
            {
                float x = -690 + i * 460;
                var img = UIKit.Image(s.Root, "bg_landscape", Color.white, "Ripple_" + i);
                img.rectTransform.Place(x, 60, 420, 280);
                img.material = mat;
                Param(img, ps[i]);
                Caption(s.Root, $"水波 progress = {ps[i]:0.00}", x, -130, 30, K.ink, 420);
            }
            var flag = UIKit.Image(s.Root, "bg_night", Color.white, "Flag");
            flag.rectTransform.Place(690, 60, 420, 280);
            flag.material = WaveMat(s);
            Caption(s.Root, "旗帜飘动 _WaveAmp = 0.08", 690, -130, 30, K.ink, 420);
            Caption(s.Root, "都只是在片元着色器里偏移 uv，网格还是那 4 个顶点", 0, -270, 32, K.muted, 1400);
        }

        // ------------------------------------------------------------------ HSV
        [BookShot("ch14-hsv", W = 960, H = 400)]
        static void Hsv(Shot s)
        {
            s.Background(Color.white);
            var mat = Mat(s, "HSV");
            mat.SetFloat("_UseVertexParam", 1); mat.SetFloat("_Saturation", 1.1f); mat.SetFloat("_Value", 1);
            float[] hues = { 0f, 0.1f, 0.25f, 0.42f, 0.58f, 0.75f };
            for (int i = 0; i < 6; i++)
            {
                float x = -700 + i * 280;
                var p = UIKit.Image(s.Root, "item_potion_red", Color.white, "Potion_" + i); p.rectTransform.Place(x, 170, 200, 200); p.preserveAspect = true;
                p.material = mat; Param(p, hues[i] > 0.5f ? hues[i] - 1 : hues[i]);
                var g = UIKit.Image(s.Root, "item_gem", Color.white, "Gem_" + i); g.rectTransform.Place(x, -80, 180, 180); g.preserveAspect = true;
                g.material = mat; Param(g, hues[i] > 0.5f ? hues[i] - 1 : hues[i]);
                Caption(s.Root, $"hue {hues[i]:+0.00;-0.00;0}", x, -230, 30, K.muted, 260);
            }
            Caption(s.Root, "一张红药水、一张宝石，换色全靠 Shader（白色/灰色部分饱和度为 0，不受色相影响）", 0, -330, 30, K.ink, 1700);
        }

        // ------------------------------------------------------------------ Mask 的模板值（实拍读取）
        static string Desc(Material m)
        {
            if (m == null || !m.HasProperty("_Stencil")) return "—";
            return $"Ref={m.GetFloat("_Stencil")}  Comp={(CompareFunction)(int)m.GetFloat("_StencilComp")}  Op={(StencilOp)(int)m.GetFloat("_StencilOp")}  " +
                   $"Read={m.GetFloat("_StencilReadMask")}  Write={m.GetFloat("_StencilWriteMask")}";
        }

        [BookShot("ch14-stencil", W = 960, H = 540, Scene = "Ch14_Stencil")]
        static void Stencil(Shot s)
        {
            s.Background(K.page);
            var l1 = UIKit.Image(s.Root, "round_32", K.primary, "Mask1 (圆角矩形)"); l1.rectTransform.Place(-520, 0, 640, 820);
            l1.gameObject.AddComponent<Mask>();
            var bg1 = UIKit.Image(l1.transform, "stripes", new Color(1, 1, 1, 0.25f), "Stripes"); bg1.type = Image.Type.Tiled; bg1.rectTransform.Place(0, 0, 900, 1100);
            var l2 = UIKit.Image(l1.transform, "circle", K.accent, "Mask2 (圆)"); l2.rectTransform.Place(0, -60, 560, 560);
            l2.gameObject.AddComponent<Mask>();
            var bg2 = UIKit.Image(l2.transform, "checker", new Color(1, 1, 1, 0.35f), "Checker"); bg2.type = Image.Type.Tiled; bg2.rectTransform.Place(0, 0, 900, 900);
            var l3 = UIKit.Image(l2.transform, "star", Color.white, "Mask3 (星)"); l3.rectTransform.Place(140, 40, 460, 460);
            l3.gameObject.AddComponent<Mask>();
            var leaf = UIKit.Image(l3.transform, "bg_landscape", Color.white, "Leaf (风景图)"); leaf.rectTransform.Place(0, 0, 900, 450);

            var table = UIKit.Text(s.Root, "", 25, K.ink, TextAlignmentOptions.TopLeft);
            table.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(390, 0), new Vector2(1080, 900));
            table.enableWordWrapping = true;
            s.BeforeCapture(() =>
            {
                // 直接读取 UGUI 运行时生成的材质：Graphic.materialForRendering 就是最终交给 CanvasRenderer 的那个
                var unmask = typeof(Mask).GetField("m_UnmaskMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
                var sb = new StringBuilder();
                void Row(string title, Graphic g)
                {
                    sb.Append("<b>").Append(title).Append("</b>\n<color=#5B4BD6>绘制：</color>").Append(Desc(g.materialForRendering)).Append('\n');
                    var mk = g.GetComponent<Mask>();
                    if (mk != null) sb.Append("<color=#E5484D>弹出：</color>").Append(Desc((Material)unmask.GetValue(mk))).Append('\n');
                    sb.Append('\n');
                }
                Row("① Mask1 的图形（深度 0）", l1);
                Row("② 条纹（Mask1 内）", bg1);
                Row("③ Mask2 的图形（深度 1）", l2);
                Row("④ 棋盘格（Mask2 内）", bg2);
                Row("⑤ Mask3 的图形（深度 2）", l3);
                Row("⑥ 风景图（三层 Mask 内）", leaf);
                table.text = sb.ToString();
                Debug.Log("[Ch14Stencil]\n" + sb);
            });
            s.Badge(l1.rectTransform, "1", new Vector2(0.08f, 0.96f));
            s.Badge(l2.rectTransform, "3", new Vector2(0.15f, 0.85f));
            s.Badge(l3.rectTransform, "5", new Vector2(0.5f, 0.98f));
        }

        // ------------------------------------------------------------------ 自定义 Shader 与遮罩的兼容性
        [BookShot("ch14-mask-compat", W = 960, H = 480, Scene = "Ch14_MaskCompat")]
        static void MaskCompat(Shot s)
        {
            s.Background(Color.white);
            var good = EffectMat(s, "Gray");
            var bad = Mat(s, "BadNoStencil");
            for (int k = 0; k < 2; k++)
            {
                float x = k == 0 ? -470 : 470;
                Image box;
                if (k == 0)
                {
                    box = UIKit.Image(s.Root, "round_32", new Color(0.36f, 0.29f, 0.84f, 0.12f), "Mask");
                    box.gameObject.AddComponent<Mask>();
                }
                else
                {
                    box = UIKit.Image(s.Root, null, new Color(0.36f, 0.29f, 0.84f, 0.12f), "RectMask2D");
                    box.gameObject.AddComponent<RectMask2D>();
                }
                box.rectTransform.Place(x, 40, 640, 520);
                var a = UIKit.Image(box.transform, "bg_landscape", Color.white, "Good (Gray 材质)"); a.rectTransform.Place(-200, 130, 520, 260);
                a.material = good; Param(a, 0.0f);
                var b = UIKit.Image(box.transform, "bg_night", Color.white, "Bad (BadNoStencil)"); b.rectTransform.Place(200, -130, 520, 260);
                b.material = bad;
                s.Box(box.rectTransform, Red, k == 0 ? "Mask（模板）" : "RectMask2D（裁剪矩形）", 2, true);
            }
            Caption(s.Root, "左上的风景：Gray 材质（保留 Stencil / CLIP_RECT）　右下的夜景：BadNoStencil（删掉了它们）", 0, -400, 30, K.ink, 1800);
        }

        // ------------------------------------------------------------------ 新手引导挖洞
        static void GuideScene(Shot s, out GuideMask mask, out Button[] steps, out TextMeshProUGUI tip, out RectTransform hand)
        {
            s.Ref(1080, 1920, 1);
            var bg = UIKit.Image(s.Root, "bg_landscape", Color.white, "Background"); bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var top = UIKit.Panel(s.Root, new Color(0, 0, 0, 0.35f), 0, "TopBar"); top.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(1080, 150));
            var coin = UIKit.Icon(top.transform, "item_coin", 80, Color.white); coin.rectTransform.anchoredPosition = new Vector2(-420, -10);
            var ct = UIKit.Text(top.transform, "12,800", 44, Color.white, TextAlignmentOptions.Left, true); ct.rectTransform.Place(-200, -10, 300, 70);
            var title = UIKit.Text(s.Root, "冒险岛", 110, Color.white, TextAlignmentOptions.Center, true); title.rectTransform.Place(0, 450, 900, 160);
            var bStart = UIKit.Button(s.Root, "开始冒险", K.accent, 560, 150, "play", 52);
            bStart.Rt().Place(0, -260, 560, 150);
            var bar = UIKit.Node(s.Root, "BottomBar"); bar.Place(new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1000, 200));
            bar.HLayout(40);
            var bBag = UIKit.Button(bar, "背包", K.primary, 280, 140, "bag", 40);
            var bTask = UIKit.Button(bar, "任务", K.primary, 280, 140, "trophy", 40);
            var bShop = UIKit.Button(bar, "商店", K.primary, 280, 140, "gift", 40);

            // 引导层：全屏 Image + GuideHole 材质 + GuideMask
            var maskImg = UIKit.Image(s.Root, null, new Color(0.05f, 0.06f, 0.1f, 0.78f), "GuideMask");
            maskImg.rectTransform.Stretch();
            maskImg.material = Mat(s, "GuideHole");
            maskImg.raycastTarget = true;              // 必须接收射线，才能挡住洞外的点击
            mask = maskImg.gameObject.AddComponent<GuideMask>();
            var tipBox = UIKit.Panel(s.Root, Color.white, 24, "TipBox"); tipBox.rectTransform.Place(0, 120, 760, 180); tipBox.raycastTarget = false;
            tip = UIKit.Text(tipBox.transform, "", 42, K.ink, TextAlignmentOptions.Center, true); tip.rectTransform.Stretch(30, 10, 30, 10);
            hand = UIKit.Icon(s.Root, "pointer", 120, Color.white, "Hand").rectTransform;
            hand.pivot = new Vector2(0.3f, 0.9f);
            hand.localEulerAngles = new Vector3(0, 0, 20);
            steps = new[] { bStart, bBag, bShop };
        }

        [BookShot("ch14-guide", W = 300, H = 533, Panels = 3, Scene = "Ch14_Guide")]
        static void Guide(Shot s)
        {
            GuideScene(s, out var mask, out var steps, out var tip, out var hand);
            var demo = s.Root.gameObject.AddComponent<GuideDemo>();
            var so = new UnityEditor.SerializedObject(demo);
            so.FindProperty("m_Mask").objectReferenceValue = mask;
            var arr = so.FindProperty("m_Steps"); arr.arraySize = steps.Length;
            for (int i = 0; i < steps.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = steps[i];
            string[] tips = { "点击「开始冒险」进入第一关", "打开背包看看新得到的装备", "去商店买一瓶药水" };
            var ta = so.FindProperty("m_Tips"); ta.arraySize = tips.Length;
            for (int i = 0; i < tips.Length; i++) ta.GetArrayElementAtIndex(i).stringValue = tips[i];
            so.FindProperty("m_Tip").objectReferenceValue = tip;
            so.FindProperty("m_Hand").objectReferenceValue = hand;
            so.ApplyModifiedPropertiesWithoutUndo();
            float[] prog = { 0.3f, 0.7f, 1f };
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                demo.Show(panel == 2 ? 1 : 0);
                mask.progress = panel == 2 ? 1 : prog[panel];
                hand.gameObject.SetActive(panel == 2);
            });
        }

        // ------------------------------------------------------------------ 低分辨率实拍放大（看边缘锯齿）
        static Canvas Magnifier(Shot s, Vector2 pos, Vector2 size, int rtW, int rtH, out Action render)
        {
            const int layer = 1;
            var camGo = new GameObject("TinyCamera");
            camGo.transform.position = new Vector3(5000, 0, -10);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.white;
            cam.cullingMask = 1 << layer; cam.allowMSAA = false; cam.allowHDR = false;
            s.Cam.cullingMask &= ~(1 << layer);
            var rtex = new RenderTexture(rtW, rtH, 24) { antiAliasing = 1, filterMode = FilterMode.Point };
            rtex.Create();
            cam.targetTexture = rtex;
            var tiny = UIKit.Canvas("TinyCanvas", RenderMode.ScreenSpaceCamera);
            var sc = tiny.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; sc.scaleFactor = 1;
            tiny.worldCamera = cam; tiny.planeDistance = 5;
            var raw = UIKit.Node(s.Root, "Zoom").gameObject.AddComponent<RawImage>();
            raw.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, size);
            raw.texture = rtex;
            render = () =>
            {
                foreach (var tr in tiny.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = layer;
                Canvas.ForceUpdateCanvases();
                cam.Render();
            };
            return tiny;
        }

        [BookShot("ch14-avatar", W = 640, H = 480, Panels = 3)]
        static void Avatar(Shot s)
        {
            s.Background(Color.white);
            if (s.Panel < 2)
            {
                var tiny = Magnifier(s, new Vector2(0, 50), new Vector2(1024, 768), 48, 36, out var render);
                Image img;
                if (s.Panel == 0)
                {
                    var m = UIKit.Image(tiny.transform, "circle", Color.white, "Mask"); m.rectTransform.Place(0, 0, 30, 30);
                    m.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                    img = UIKit.Image(m.transform, "avatar_3", Color.white, "Avatar"); img.rectTransform.Stretch();
                }
                else
                {
                    img = UIKit.Image(tiny.transform, "avatar_3", Color.white, "Avatar"); img.rectTransform.Place(0, 0, 30, 30);
                    img.material = Mat(s, "CircleAvatar");
                    img.material.SetFloat("_Feather", 0.035f);
                    img.material.SetFloat("_BorderWidth", 0);
                }
                s.BeforeCapture(render);
                Caption(s.Root, s.Panel == 0 ? "Mask + circle：模板只有“写/不写”，硬边" : "CircleAvatar Shader：smoothstep 软边", 0, -400, 34, K.ink, 1200);
            }
            else
            {
                var mat = s.MatAsset("Ch14", "CircleAvatarBorder", "UGUIBook/Ch14/CircleAvatar");
                mat.SetFloat("_Feather", 0.01f); mat.SetFloat("_BorderWidth", 0.05f); mat.SetColor("_BorderColor", Color.white);
                var bg = UIKit.Panel(s.Root, K.dark, 0, "Bg"); bg.rectTransform.Stretch();
                for (int i = 0; i < 6; i++)
                {
                    var a = UIKit.Image(s.Root, "avatar_" + (i + 1), Color.white, "Avatar" + i);
                    a.rectTransform.Place(-380 + (i % 3) * 380, 160 - (i / 3) * 360, 280, 280);
                    a.material = mat;
                }
                Caption(s.Root, "6 个头像共用一个材质：1 个批次", 0, -400, 34, Color.white, 1200);
            }
        }

        // ------------------------------------------------------------------ 软遮罩
        /// 生成一张“实心圆 + 外圈 30% 羽化”的遮罩贴图，存成资源（场景里要引用）
        static Texture2D SoftCircleTexture()
        {
            const string path = "Assets/UGUIBook/Ch14/Materials/SoftCircleMask.asset";
            var tex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex != null) return tex;
            const int N = 128;
            tex = new Texture2D(N, N, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, name = "SoftCircleMask" };
            var px = new Color32[N * N];
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    float r = new Vector2(x + 0.5f - N / 2f, y + 0.5f - N / 2f).magnitude / (N / 2f);
                    float a = 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.7f, 1f, r));
                    px[y * N + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            tex.SetPixels32(px); tex.Apply();
            UnityEditor.AssetDatabase.CreateAsset(tex, path);
            return tex;
        }

        [BookShot("ch14-softmask", W = 960, H = 400, Scene = "Ch14_SoftMask")]
        static void Soft(Shot s)
        {
            s.Background(K.dark);
            string[] caps = { "Mask + circle：硬边", "SoftMask + 羽化圆形贴图", "RectMask2D softness = 60" };
            var softMat = Mat(s, "SoftMasked");
            for (int k = 0; k < 3; k++)
            {
                float x = -620 + k * 620;
                RectTransform holder;
                if (k == 0)
                {
                    var m = UIKit.Image(s.Root, "circle", Color.white, "Mask"); m.rectTransform.Place(x, 40, 460, 460);
                    m.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                    holder = m.rectTransform;
                }
                else if (k == 1)
                {
                    holder = UIKit.Node(s.Root, "SoftMask"); holder.Place(x, 40, 460, 460);
                    var sm = holder.gameObject.AddComponent<SoftMask>();
                    sm.maskTexture = SoftCircleTexture(); sm.maskedMaterial = softMat;
                }
                else
                {
                    holder = UIKit.Node(s.Root, "RectMask2D"); holder.Place(x, 40, 460, 460);
                    holder.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(60, 60);
                }
                var pic = UIKit.Image(holder, "bg_landscape", Color.white, "Picture"); pic.rectTransform.Place(0, 0, 920, 460);
                if (k == 1)
                {
                    // 注意：TMP 文字不能挂 SoftMaskable——SoftMasked 是普通精灵 Shader，换上后字形会变成方块
                    pic.gameObject.AddComponent<SoftMaskable>();
                    var sm = holder.GetComponent<SoftMask>();
                    s.BeforeCapture(() => sm.Refresh());
                }
                Caption(s.Root, caps[k], x, -260, 30, Color.white, 620);
            }
        }

        // ------------------------------------------------------------------ 模糊背景
        [BookShot("ch14-blur", W = 640, H = 360, Panels = 3, Scene = "Ch14_Blur")]
        static void Blur(Shot s)
        {
            // 画布改成 Screen Space - Camera：这样相机 Render() 能把 UI 拍进 RenderTexture
            s.Canvas.renderMode = RenderMode.ScreenSpaceCamera;
            s.Canvas.worldCamera = s.Cam; s.Canvas.planeDistance = 100;
            var bg = UIKit.Image(s.Root, "bg_landscape", Color.white, "Background"); bg.rectTransform.Stretch(); bg.raycastTarget = false;
            var title = UIKit.Text(s.Root, "主界面", 90, Color.white, TextAlignmentOptions.Center, true); title.rectTransform.Place(0, 260, 800, 130);
            for (int i = 0; i < 4; i++)
            {
                var it = UIKit.Card(s.Root, "Item" + i, 24); it.Place(-495 + i * 330, 10, 270, 270);
                UIKit.Icon(it, new[] { "item_sword", "item_shield", "item_potion_blue", "item_gem" }[i], 160, Color.white).rectTransform.anchoredPosition = new Vector2(0, 20);
                var t = UIKit.Text(it, new[] { "长剑", "盾牌", "药水", "宝石" }[i], 38, K.ink); t.rectTransform.Place(0, -95, 240, 60);
            }
            var open = UIKit.Button(s.Root, "打开弹窗", K.primary, 380, 110, null, 42);
            open.Rt().Place(0, -255, 380, 110);

            var popup = UIKit.Node(s.Root, "Popup"); popup.Stretch();
            var raw = UIKit.Node(popup, "BlurBackground").gameObject.AddComponent<RawImage>(); raw.rectTransform.Stretch();
            var dim = UIKit.Image(popup, null, new Color(0.05f, 0.06f, 0.12f, 0.3f), "Dim"); dim.rectTransform.Stretch();
            var dlg = UIKit.Card(popup, "Dialog", 32); dlg.Place(0, 0, 700, 500);
            var dt = UIKit.Text(dlg, "获得奖励", 60, K.ink, TextAlignmentOptions.Center, true); dt.rectTransform.Place(0, 170, 600, 90);
            var gift = UIKit.Icon(dlg, "item_gem", 170, Color.white); gift.rectTransform.anchoredPosition = new Vector2(0, 10);
            var close = UIKit.Button(dlg, "收下", K.accent, 300, 110, null, 42); close.Rt().Place(0, -170, 300, 110);

            var blur = s.Root.gameObject.AddComponent<BlurSnapshot>();
            var demo = s.Root.gameObject.AddComponent<BlurDemo>();
            var so = new UnityEditor.SerializedObject(blur);
            so.FindProperty("m_Camera").objectReferenceValue = s.Cam;
            so.FindProperty("m_Target").objectReferenceValue = raw;
            so.FindProperty("m_HideWhileCapture").objectReferenceValue = popup.gameObject;
            so.FindProperty("m_BlurMaterial").objectReferenceValue = Mat(s, "Blur");
            so.ApplyModifiedPropertiesWithoutUndo();
            var so2 = new UnityEditor.SerializedObject(demo);
            so2.FindProperty("m_Blur").objectReferenceValue = blur;
            so2.FindProperty("m_Popup").objectReferenceValue = popup.gameObject;
            so2.FindProperty("m_Open").objectReferenceValue = open;
            so2.FindProperty("m_Close").objectReferenceValue = close;
            so2.ApplyModifiedPropertiesWithoutUndo();

            int panel = s.Panel;
            popup.gameObject.SetActive(false);
            s.BeforeCapture(() =>
            {
                if (panel == 0) return;
                popup.gameObject.SetActive(true);
                dlg.gameObject.SetActive(panel == 2);
                dim.color = new Color(0.05f, 0.06f, 0.12f, panel == 2 ? 0.3f : 0f);
                blur.downsample = 4;
                blur.iterations = panel == 1 ? 1 : 4;
                blur.Capture();
            });
        }

        // ------------------------------------------------------------------ 配套场景：Shader 效果陈列
        [BookScene("Ch14_ShaderGallery")]
        static void Gallery(Shot s)
        {
            s.Background(K.page);
            var hint = UIKit.Text(s.Root, "运行场景：每个效果都由 ParamDriver 驱动 uv1.x，全部共用各自的一个材质", 30, K.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1600, 50));
            string[] items = { "item_sword", "item_shield", "item_potion_red", "item_gem" };
            // 第 1 行：灰度往返
            var gray = EffectMat(s, "Gray");
            for (int i = 0; i < 4; i++)
            {
                var img = UIKit.Image(s.Root, items[i], Color.white, "Gray_" + i); img.rectTransform.Place(-780 + i * 170, 300, 140, 140); img.preserveAspect = true;
                img.material = gray; Param(img, 0);
                img.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.PingPong, 1.5f, 0, i * 0.3f);
            }
            // 流光按钮
            var shine = EffectMat(s, "Shine");
            for (int i = 0; i < 2; i++)
            {
                var b = UIKit.Button(s.Root, i == 0 ? "领取奖励" : "立即购买", i == 0 ? K.accent : K.primary, 360, 120, "gift", 36);
                b.Rt().Place(-30 + i * 420, 300, 360, 120);
                var img = b.GetComponent<Image>(); img.material = shine; Param(img, 0);
                img.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.SweepWithPause, 0.8f, 1.2f, i * 0.6f);
            }
            // HSV
            var hsv = EffectMat(s, "HSV");
            var pot = UIKit.Image(s.Root, "item_potion_red", Color.white, "HSV_Potion"); pot.rectTransform.Place(720, 300, 180, 180); pot.preserveAspect = true;
            pot.material = hsv; Param(pot, 0);
            pot.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.Loop, 3f);
            // 溶解
            var dis = DissolveMat(s);
            for (int i = 0; i < 3; i++)
            {
                var a = UIKit.Image(s.Root, "avatar_" + (i + 1), Color.white, "Dissolve_" + i); a.rectTransform.Place(-700 + i * 260, -40, 220, 220);
                a.material = dis; Param(a, 0);
                a.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.PingPong, 1.6f, 0, i * 0.4f);
            }
            // 水波 + 旗帜
            var dst = DistortMat(s);
            var rip = UIKit.Image(s.Root, "bg_landscape", Color.white, "Ripple"); rip.rectTransform.Place(150, -40, 420, 280);
            rip.material = dst; Param(rip, 0);
            rip.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.SweepWithPause, 1.4f, 0.4f);
            var flag = UIKit.Image(s.Root, "bg_night", Color.white, "Flag"); flag.rectTransform.Place(640, -40, 420, 280);
            flag.material = WaveMat(s);
            flag.gameObject.AddComponent<FlagWave>();
            // 遮罩下的效果：证明 Shader 兼容 RectMask2D
            var vp = UIKit.Panel(s.Root, new Color(0, 0, 0, 0.06f), 0, "RectMask2D"); vp.rectTransform.Place(0, -360, 1400, 220);
            vp.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(80, 0);
            var row = UIKit.Node(vp.transform, "Row"); row.Place(0, 0, 2200, 180); row.HLayout(30);
            for (int i = 0; i < 10; i++)
            {
                var img = UIKit.Image(row, items[i % 4], Color.white, "Item" + i); img.rectTransform.sizeDelta = new Vector2(180, 180); img.preserveAspect = true;
                img.material = i % 2 == 0 ? gray : hsv; Param(img, 0);
                img.gameObject.AddComponent<ParamDriver>().Setup(ParamDriver.Mode.PingPong, 2f, 0, i * 0.2f);
            }
            row.gameObject.AddComponent<SlideBackAndForth>();
        }
    }
}
