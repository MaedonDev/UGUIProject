// 第 5 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch05
{
    static class Shots
    {
        const string FontDir = "Assets/UGUIBook/Ch05/Fonts";
        static UISkin Skin => UISkin.Instance;
        static Color Ink => Skin.ink;
        static Color Muted => Skin.muted;

        static TextMeshProUGUI T(Transform p, string text, float x, float y, float size = 30, Color? col = null, float w = 600, bool bold = false)
        {
            var t = UIKit.Text(p, text, size, col ?? Ink, TextAlignmentOptions.Center, bold);
            t.rectTransform.Place(x, y, w, size * 1.6f);
            return t;
        }

        static Image Frame(Transform p, float x, float y, float w, float h, Color? c = null)
        {
            var f = UIKit.Image(p, "round_outline_8", c ?? new Color(0.48f, 0.51f, 0.6f, 0.7f), "Frame");
            f.raycastTarget = false;
            f.rectTransform.Place(x, y, w, h);
            return f;
        }

        static void SetField(Object o, string field, object value)
        {
            var so = new SerializedObject(o);
            var p = so.FindProperty(field);
            switch (value)
            {
                case Object obj: p.objectReferenceValue = obj; break;
                case float f: p.floatValue = f; break;
                case string str: p.stringValue = str; break;
                case string[] arr:
                    p.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++) p.GetArrayElementAtIndex(i).stringValue = arr[i];
                    break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------- 本章字体资源
        const string TitleChars = "限时活动开启胜利失败挑战成功新纪录永字八法描边粗细 VICTORYDEFEATAgabcdefghijklmnopqrstuvwxyz0123456789!?.:·";

        static TMP_FontAsset Font(string name, int sampling, int padding, int atlas, string chars)
        {
            if (!AssetDatabase.IsValidFolder(FontDir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch05", "Fonts");
            string path = $"{FontDir}/{name}.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa != null) return fa;
            var ttf = AssetDatabase.LoadAssetAtPath<Font>("Assets/UGUIBook/_Core/Fonts/NotoSansSC-Bold.ttf");
            return FontAssetTool.CreateStatic(ttf, chars, path, sampling, padding, atlas);
        }

        static TMP_FontAsset TitleFont()
        {
            var fa = Font("TitleBold SDF", 72, 8, 1024, TitleChars);
            if (fa.fallbackFontAssetTable == null || fa.fallbackFontAssetTable.Count == 0)
            {
                // 静态字体缺字时，回退到全书的动态粗体
                fa.fallbackFontAssetTable = new List<TMP_FontAsset> { Skin.fontBold };
                EditorUtility.SetDirty(fa);
                AssetDatabase.SaveAssets();
            }
            return fa;
        }

        // ---------------------------------------------------------------- 旧版 Text vs TMP 放大
        [BookShot("ch05-legacy", W = 960, H = 300)]
        static void Legacy(Shot s)
        {
            var lg = UIKit.LegacyText(s.Root, "字 Ag", 24, Ink);
            lg.rectTransform.Place(-470, 70, 140, 40);
            lg.rectTransform.localScale = Vector3.one * 5;
            var tmp = UIKit.Text(s.Root, "字 Ag", 24, Ink);
            tmp.rectTransform.Place(470, 70, 140, 40);
            tmp.rectTransform.localScale = Vector3.one * 5;
            T(s.Root, "旧版 Text：字号 24，Scale ×5", -470, -110, 32, Ink, 800, true);
            T(s.Root, "按 24 号栅格化成位图，再被拉大 → 发糊", -470, -165, 26, Muted, 800);
            T(s.Root, "TextMeshPro：字号 24，Scale ×5", 470, -110, 32, Ink, 800, true);
            T(s.Root, "距离场在片元着色器里逐像素重算边缘 → 依然锐利", 470, -165, 26, Muted, 800);
        }

        // ---------------------------------------------------------------- SDF 图集与单字放大
        static Texture2D Readable(Texture2D src)
        {
            try { src.GetPixel(0, 0); return src; }
            catch
            {
                var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(src, rt);
                var prev = RenderTexture.active; RenderTexture.active = rt;
                var t = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, true);
                t.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0); t.Apply();
                RenderTexture.active = prev; RenderTexture.ReleaseTemporary(rt);
                return t;
            }
        }

        [BookShot("ch05-atlas", W = 960, H = 440)]
        static void Atlas(Shot s)
        {
            s.Background(UIKit.Hex("#F4F5F9"));
            var fa = TitleFont();
            var src = Readable(fa.atlasTexture);
            int w = src.width, h = src.height;
            // 1) 整张图集（Alpha8 → 灰度显示）
            var gray = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var px = src.GetPixels();
            for (int i = 0; i < px.Length; i++) { float a = px[i].a; px[i] = new Color(a, a, a, 1); }
            gray.SetPixels(px); gray.Apply(); s.Temp.Add(gray);
            var raw = UIKit.Node(s.Root, "Atlas").gameObject.AddComponent<RawImage>();
            raw.texture = gray; raw.rectTransform.Place(-640, 30, 600, 600);
            T(s.Root, $"图集 {w}×{h}（Static，{fa.characterTable.Count} 个字）", -640, -320, 28, Ink, 700, true);

            // 2) “胜” 字放大三种看法
            var g = fa.characterLookupTable['胜'].glyph.glyphRect;
            int pad = fa.atlasPadding;
            int gx = g.x - pad, gy = g.y - pad, gw = g.width + pad * 2, gh = g.height + pad * 2;
            string[] titles = { "距离场原始值", "取 0.5 等值线 = 字形", "0.5±0.12 之间染色 = 描边" };
            for (int k = 0; k < 3; k++)
            {
                var t = new Texture2D(gw, gh, TextureFormat.RGBA32, false) { filterMode = k == 0 ? FilterMode.Point : FilterMode.Bilinear };
                for (int y = 0; y < gh; y++)
                    for (int x = 0; x < gw; x++)
                    {
                        float a = src.GetPixel(gx + x, gy + y).a;
                        Color c;
                        if (k == 0) c = new Color(a, a, a, 1);
                        else if (k == 1) c = a >= 0.5f ? (Color)Skin.ink : Color.white;
                        else c = a >= 0.62f ? new Color(1f, 0.8f, 0.3f) : a >= 0.38f ? (Color)Skin.primary : Color.white;
                        t.SetPixel(x, y, c);
                    }
                t.Apply(); s.Temp.Add(t);
                var r = UIKit.Node(s.Root, "Glyph" + k).gameObject.AddComponent<RawImage>();
                r.texture = t;
                float x0 = -80 + k * 380;
                float size = 340f, aspect = (float)gw / gh;
                r.rectTransform.Place(x0 + 60, 60, size * Mathf.Min(1, aspect), size / Mathf.Max(1, aspect));
                T(s.Root, titles[k], x0 + 60, -170, 26, Ink, 380, true);
            }
            T(s.Root, $"“胜”字（Sampling {fa.faceInfo.pointSize}，Padding {pad}）：右边两张是 CPU 按同一规则对距离场取阈值的结果", 300, -260, 22, Muted, 1150);
            if (src != fa.atlasTexture) s.Temp.Add(src);
        }

        // ---------------------------------------------------------------- Sampling / Padding 对比
        [BookShot("ch05-params", W = 960, H = 400)]
        static void Params(Shot s)
        {
            var s16 = Font("Sample16 SDF", 16, 2, 256, "永Ag描边");
            var s90 = Font("Sample90 SDF", 90, 9, 512, "永Ag描边");
            var p2 = Font("Pad2 SDF", 64, 2, 256, "永Ag描边");
            var p10 = Font("Pad10 SDF", 64, 10, 512, "永Ag描边");
            var dark = UIKit.Hex("#1F2436");
            void Show(TMP_FontAsset f, float x, float y, string label, string sub, float outline = 0)
            {
                var t = UIKit.Text(s.Root, "永Ag", 180, dark);
                t.font = f;
                if (outline > 0) t.fontSharedMaterial = TextPresets.Create(s, f, "Outline", m => TextPresets.Outline(m, Skin.accent, outline));
                t.rectTransform.Place(x, y, 600, 230);
                T(s.Root, label, x, y - 160, 30, Ink, 700, true);
                T(s.Root, sub, x, y - 205, 24, Muted, 800);
            }
            Show(s16, -720, 250, "Sampling 16", "采样太小：拐角被抹圆");
            Show(s90, -240, 250, "Sampling 90", "细节完整");
            Show(p2, 240, 250, "Padding 2 + 描边 0.3", "距离场范围太窄，描边被压得很细", 0.3f);
            Show(p10, 720, 250, "Padding 10 + 描边 0.3", "同样的参数，描边够厚", 0.3f);
            var line = UIKit.Image(s.Root, null, new Color(0, 0, 0, 0.1f), "Divider"); line.rectTransform.Place(0, 190, 4, 380);
            T(s.Root, "四个字体资源都只含“永Ag描边”几个字，显示字号都是 180", 0, -220, 26, Muted, 1600);
            T(s.Root, "左：Sampling Point Size 决定距离场的精度；右：Padding 决定距离场能向外延伸多远（描边/阴影的上限）", 0, -270, 24, Muted, 1800);
        }

        // ---------------------------------------------------------------- Fallback
        [BookShot("ch05-fallback", W = 480, H = 260, Panels = 2)]
        static void Fallback(Shot s)
        {
            // 两格分开渲染：第 0 格临时清空 Fallback 列表，第 1 格由 TitleFont() 恢复
            var title = TitleFont();
            if (s.Panel == 0) title.fallbackFontAssetTable = new List<TMP_FontAsset>();
            string text = "限时活动<color=#E5484D>今晚</color>开启";
            var a = UIKit.Text(s.Root, text, 96, Ink); a.font = title; a.rectTransform.Place(0, 120, 900, 140);
            if (s.Panel == 0)
            {
                T(s.Root, "Fallback 列表为空", 0, -40, 34, Ink, 900, true);
                T(s.Root, "“今晚”不在静态字体里，最后由 TMP Settings 的\n默认字体（Regular）顶上：笔画明显变细", 0, -140, 28, Muted, 900).rectTransform.sizeDelta = new Vector2(900, 100);
            }
            else
            {
                T(s.Root, "Fallback = NotoSansSC-Bold SDF", 0, -40, 34, Ink, 900, true);
                T(s.Root, "缺字由同字重的动态字体补齐，\n看不出差别", 0, -140, 28, Muted, 900).rectTransform.sizeDelta = new Vector2(900, 100);
            }
        }

        // ---------------------------------------------------------------- 对齐与换行
        const string Para = "TextMeshPro 的排版选项都在 Inspector 的 Main Settings 里：对齐、换行、溢出、自动字号。";

        [BookShot("ch05-wrap", W = 960, H = 400)]
        static void Wrap(Shot s)
        {
            var cases = new (string label, TextAlignmentOptions align, bool wrap)[]
            {
                ("Left + 自动换行", TextAlignmentOptions.TopLeft, true),
                ("Center", TextAlignmentOptions.Top, true),
                ("Justified（两端对齐）", TextAlignmentOptions.TopJustified, true),
                ("关闭 Wrapping：一行到底", TextAlignmentOptions.TopLeft, false),
            };
            for (int i = 0; i < 4; i++)
            {
                float x = -705 + i * 470;
                Frame(s.Root, x, 60, 400, 260);
                var t = UIKit.Text(s.Root, Para, 32, Ink, cases[i].align);
                t.enableWordWrapping = cases[i].wrap;
                t.rectTransform.Place(x, 60, 380, 240);
                T(s.Root, cases[i].label, x, -130, 28, Ink, 460, true);
            }
            T(s.Root, "灰框 = 文本框（RectTransform）的范围", 0, -260, 24, Muted, 1600);
        }

        // ---------------------------------------------------------------- 溢出模式
        [BookShot("ch05-overflow", W = 960, H = 520)]
        static void Overflow(Shot s)
        {
            const string txt = "第一行文字比较长会自动换行，第二行继续，第三行已经超出文本框的高度了，第四行更是看不到。";
            var modes = new[] { TextOverflowModes.Overflow, TextOverflowModes.Ellipsis, TextOverflowModes.Truncate, TextOverflowModes.Masking, TextOverflowModes.Page, TextOverflowModes.Linked };
            string[] subs = { "超出部分照样画出来", "末尾变成 …", "整行裁掉", "本身不裁，这里靠父节点 RectMask2D", "分页，这里显示第 2 页", "多出的字流到下一个框" };
            TextMeshProUGUI linkedTarget = null;
            for (int i = 0; i < 6; i++)
            {
                int col = i % 3, row = i / 3;
                float x = -620 + col * 620, y = 290 - row * 520;
                Frame(s.Root, x, y, 440, 130, Skin.primary);
                var t = UIKit.Text(s.Root, txt, 32, Ink, TextAlignmentOptions.TopLeft);
                t.rectTransform.Place(x, y, 420, 110);
                t.overflowMode = modes[i];
                if (modes[i] == TextOverflowModes.Page) t.pageToDisplay = 2;
                if (modes[i] == TextOverflowModes.Masking)
                {
                    // TMP 3.0.7 的 UGUI 版本里 Masking 不会自己裁剪，要靠父节点的 RectMask2D
                    var clip = UIKit.Node(s.Root, "RectMask2D"); clip.Place(x, y, 440, 130);
                    clip.gameObject.AddComponent<RectMask2D>().softness = new Vector2Int(0, 24);
                    t.transform.SetParent(clip, true);
                }
                if (modes[i] == TextOverflowModes.Linked)
                {
                    t.rectTransform.Place(x - 115, y, 190, 110);
                    Frame(s.Root, x + 115, y, 210, 130, Skin.accent);
                    linkedTarget = UIKit.Text(s.Root, "", 32, Ink, TextAlignmentOptions.TopLeft, false, "LinkedText");
                    linkedTarget.rectTransform.Place(x + 115, y, 190, 110);
                    linkedTarget.overflowMode = TextOverflowModes.Ellipsis;
                    t.linkedTextComponent = linkedTarget;
                }
                T(s.Root, modes[i].ToString(), x, y - 185, 32, Ink, 500, true);
                T(s.Root, subs[i], x, y - 235, 26, Muted, 560);
            }
        }

        // ---------------------------------------------------------------- Auto Size
        [BookShot("ch05-autosize", W = 960, H = 260)]
        static void AutoSize(Shot s)
        {
            float[] widths = { 440, 240, 170, 260 };
            string[] texts = { "立即购买", "立即购买", "立即购买", "限时特惠立即购买" };
            var labels = new TextMeshProUGUI[4];
            var bodies = new TextMeshProUGUI[4];
            float x = -735;
            for (int i = 0; i < 4; i++)
            {
                var b = UIKit.Button(s.Root, texts[i], Skin.accent, widths[i], 110, null, 48);
                b.Rt().Place(x + widths[i] / 2, 50, widths[i], 110);
                var t = b.GetComponentInChildren<TextMeshProUGUI>();
                t.enableAutoSizing = true; t.fontSizeMin = 18; t.fontSizeMax = 48;
                t.enableWordWrapping = false;                     // 按钮文字一般不换行：只缩字号
                t.rectTransform.Stretch(16, 8, 16, 8);
                bodies[i] = t;
                labels[i] = T(s.Root, "", x + widths[i] / 2, -60, 28, Ink, 360, true);
                x += widths[i] + 120;
            }
            T(s.Root, "Auto Size：Min 18 / Max 48，按钮内边距 16，不换行。数字是排版后实际用的字号", 0, -170, 26, Muted, 1700);
            s.BeforeCapture(() =>
            {
                for (int i = 0; i < 4; i++) { bodies[i].ForceMeshUpdate(); labels[i].text = $"宽 {widths[i]:0} → {bodies[i].fontSize:0.#} 号"; }
            });
        }

        // ---------------------------------------------------------------- 间距与边距
        [BookShot("ch05-spacing", W = 960, H = 400)]
        static void Spacing(Shot s)
        {
            const string txt = "山不在高，有仙则名。\n水不在深，有龙则灵。";
            for (int i = 0; i < 4; i++)
            {
                float x = -705 + i * 470;
                Frame(s.Root, x, 60, 420, 260);
                var t = UIKit.Text(s.Root, txt, 34, Ink, TextAlignmentOptions.TopLeft);
                t.rectTransform.Place(x, 60, 420, 260);
                string label = "默认";
                if (i == 1) { t.characterSpacing = 18; label = "Character Spacing = 18"; }
                if (i == 2) { t.lineSpacing = 60; label = "Line Spacing = 60"; }
                if (i == 3)
                {
                    t.margin = new Vector4(40, 40, 40, 0); label = "Margins 左/上/右 = 40";
                    var inner = UIKit.Image(s.Root, null, new Color(0.36f, 0.29f, 0.84f, 0.08f), "MarginArea");
                    inner.rectTransform.Place(x, 40, 340, 220);
                    inner.transform.SetSiblingIndex(t.transform.GetSiblingIndex());
                }
                T(s.Root, label, x, -130, 28, Ink, 460, true);
            }
            T(s.Root, "间距单位是 em 的 1/100（相对字号）；Margins 单位是像素，在 RectTransform 内再缩进", 0, -250, 24, Muted, 1700);
        }

        // ---------------------------------------------------------------- 富文本
        static void EnsureGradientPreset()
        {
            const string dir = "Assets/UGUIBook/Ch05/Resources/Color Gradient Presets";
            if (!AssetDatabase.IsValidFolder("Assets/UGUIBook/Ch05/Resources")) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch05", "Resources");
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch05/Resources", "Color Gradient Presets");
            var path = dir + "/Fire.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_ColorGradient>(path) != null) return;
            var g = ScriptableObject.CreateInstance<TMP_ColorGradient>();
            g.colorMode = ColorMode.VerticalGradient;
            g.topLeft = g.topRight = new Color32(0xFF, 0xD2, 0x3F, 255);
            g.bottomLeft = g.bottomRight = new Color32(0xE5, 0x48, 0x4D, 255);
            AssetDatabase.CreateAsset(g, path);
            AssetDatabase.SaveAssets();
        }

        [BookShot("ch05-richtext", W = 960, H = 640)]
        static void RichText(Shot s)
        {
            EnsureGradientPreset();
            var emoji = EmojiSpriteAssetBuilder.Ensure();
            var rows = new[]
            {
                "<b>粗体</b> <i>斜体</i> <u>下划线</u> <s>删除线</s>",
                "<color=#E5484D>红色</color> <color=#5B4BD6>紫色</color> <alpha=#66>半透明（alpha 没有闭合标签）",
                "<size=150%>大</size>字<size=70%>小</size>字",
                "<mark=#FFB02E66>高亮标记</mark> <cspace=0.3em>加大字距</cspace>",
                "H<sub>2</sub>O  x<sup>2</sup>  <voffset=0.5em>上浮</voffset>",
                "<gradient=\"Fire\">渐变色 Fire</gradient>",
                "<sprite name=\"smile\"> 表情 <sprite name=\"coin\">x99",
                "<link=\"item:1\"><color=#FF8A3D>[可点击]</color></link> 需要配合脚本",
            };
            var bg = UIKit.Card(s.Root, "Card"); bg.Place(0, 0, 1820, 1220);
            T(s.Root, "源文本（Text 框里写的）", -440, 560, 28, Muted, 800, true);
            T(s.Root, "显示效果", 460, 560, 28, Muted, 800, true);
            for (int i = 0; i < rows.Length; i++)
            {
                float y = 470 - i * 130;
                var src = UIKit.Text(s.Root, "<noparse>" + rows[i] + "</noparse>", 28, Skin.primary, TextAlignmentOptions.MidlineLeft);
                src.rectTransform.Place(-440, y, 860, 110);
                var dst = UIKit.Text(s.Root, rows[i], 44, Ink, TextAlignmentOptions.MidlineLeft);
                dst.spriteAsset = emoji;
                dst.rectTransform.Place(500, y, 720, 110);
                if (i > 0)
                {
                    var line = UIKit.Image(s.Root, null, new Color(0, 0, 0, 0.06f), "Line");
                    line.rectTransform.Place(0, y + 65, 1720, 3);
                }
            }
        }

        // ---------------------------------------------------------------- 材质预设
        static Dictionary<string, Material> Presets(Shot s, TMP_FontAsset f)
        {
            var dict = new Dictionary<string, Material>
            {
                ["Outline"] = TextPresets.Create(s, f, "Outline", m => TextPresets.Outline(m, UIKit.Hex("#1F2436"), 0.25f)),
                ["Shadow"] = TextPresets.Create(s, f, "Shadow", m => TextPresets.Underlay(m, new Color(0, 0, 0, 0.8f), 0.6f, -0.6f, 0f, 0f)),
                ["SoftShadow"] = TextPresets.Create(s, f, "SoftShadow", m => TextPresets.Underlay(m, new Color(0, 0, 0, 0.7f), 0.3f, -0.5f, 0.1f, 0.5f)),
                ["Glow"] = TextPresets.Create(s, f, "Glow", m => TextPresets.Glow(m, new Color(1f, 0.7f, 0.2f, 1f), 1f, 0.3f)),
                ["Title"] = TextPresets.Create(s, f, "Title", m =>
                {
                    TextPresets.Outline(m, UIKit.Hex("#5A2A0A"), 0.22f);
                    TextPresets.Underlay(m, new Color(0, 0, 0, 0.6f), 0.4f, -0.8f, 0.2f, 0.2f);
                }),
            };
            AssetDatabase.SaveAssets();
            return dict;
        }

        [BookShot("ch05-presets", W = 960, H = 460)]
        static void PresetShot(Shot s)
        {
            s.Background(UIKit.Hex("#5C6390"));
            var f = Skin.fontBold;
            var p = Presets(s, f);
            var items = new (string label, Material mat, bool grad)[]
            {
                ("默认材质", f.material, false), ("Outline 描边", p["Outline"], false), ("Underlay 硬投影", p["Shadow"], false),
                ("Underlay 软阴影", p["SoftShadow"], false), ("Glow 发光", p["Glow"], false), ("描边 + 投影 + 顶点渐变", p["Title"], true),
            };
            for (int i = 0; i < items.Length; i++)
            {
                int col = i % 3, row = i / 3;
                float x = -620 + col * 620, y = 240 - row * 420;
                var t = UIKit.Text(s.Root, "胜利 WIN", 110, Color.white, TextAlignmentOptions.Center, true);
                t.fontSharedMaterial = items[i].mat;
                t.rectTransform.Place(x, y, 600, 170);
                if (items[i].grad)
                {
                    t.enableVertexGradient = true;
                    t.colorGradient = new VertexGradient(UIKit.Hex("#FFF3B0"), UIKit.Hex("#FFF3B0"), UIKit.Hex("#FF9F1C"), UIKit.Hex("#FF9F1C"));
                }
                T(s.Root, items[i].label, x, y - 130, 30, Color.white, 600, true);
            }
            T(s.Root, "同一个字体资源 NotoSansSC-Bold SDF，换 5 个材质预设（Ch05/Materials）", 0, -420, 26, new Color(1, 1, 1, 0.75f), 1600);
        }

        // ================================================================ 案例 5.2 标题
        [BookShot("ch05-title", W = 960, H = 540, Scene = "Ch05_Title")]
        static void Title(Shot s)
        {
            s.Background(UIKit.Hex("#1B1F33"));
            var f = Skin.fontBold;
            var p = Presets(s, f);
            var glow = UIKit.Image(s.Root, "circle_soft", new Color(1f, 0.75f, 0.3f, 0.35f), "Glow");
            glow.rectTransform.Place(0, 160, 1300, 700);
            var title = UIKit.Text(s.Root, "胜 利", 230, Color.white, TextAlignmentOptions.Center, true, "Title");
            title.fontSharedMaterial = p["Title"];
            title.enableVertexGradient = true;
            title.colorGradient = new VertexGradient(UIKit.Hex("#FFF6C2"), UIKit.Hex("#FFF6C2"), UIKit.Hex("#FF9F1C"), UIKit.Hex("#FF9F1C"));
            title.characterSpacing = 10;
            title.rectTransform.Place(0, 170, 1200, 320);
            var sub = UIKit.Text(s.Root, "挑战成功 · 新纪录", 64, UIKit.Hex("#FFE7A3"), TextAlignmentOptions.Center, true, "Subtitle");
            sub.fontSharedMaterial = p["Glow"];
            sub.rectTransform.Place(0, -30, 1200, 110);
            var stars = UIKit.Node(s.Root, "Stars"); stars.Place(0, -170, 600, 140); stars.HLayout(30);
            for (int i = 0; i < 3; i++)
            {
                var st = UIKit.Icon(stars, "star", i == 1 ? 140 : 110, UIKit.Hex("#FFC93C"));
            }
            var info = UIKit.Text(s.Root, "用时 01:42　　连击 x38　　评价 S", 40, UIKit.Hex("#C9CDE8"), TextAlignmentOptions.Center, true, "Info");   // 材质预设来自粗体图集，字体也必须是粗体
            info.fontSharedMaterial = p["SoftShadow"];
            info.rectTransform.Place(0, -300, 1400, 70);
            var btn = UIKit.Button(s.Root, "继 续", Skin.accent, 380, 110);
            btn.Rt().Place(0, -430, 380, 110);
        }

        // ================================================================ 案例 5.3 图文混排
        static RectTransform Bubble(Transform parent, string avatar, string text, bool right, float y, TMP_SpriteAsset emoji)
        {
            var row = UIKit.Node(parent, "Msg"); row.Place(0, y, 1500, 160);
            var av = UIKit.Image(row, avatar, Color.white, "Avatar");
            av.rectTransform.Place(new Vector2(right ? 1 : 0, 1), new Vector2(right ? -50 : 50, 0), new Vector2(110, 110));
            av.rectTransform.pivot = new Vector2(right ? 1 : 0, 1);
            av.rectTransform.anchoredPosition = new Vector2(right ? -40 : 40, 0);
            var bub = UIKit.Image(row, "round_24", right ? UIKit.Hex("#95EC69") : Color.white, "Bubble");
            var rt = bub.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(right ? 1 : 0, 1);
            rt.anchoredPosition = new Vector2(right ? -180 : 180, 0);
            var t = UIKit.Text(rt, text, 40, Ink, TextAlignmentOptions.MidlineLeft, false, "Text");
            t.spriteAsset = emoji;
            t.enableWordWrapping = true;
            t.rectTransform.Stretch(30, 18, 30, 18);
            var pref = t.GetPreferredValues(text, 980, 1000);
            rt.sizeDelta = new Vector2(Mathf.Min(pref.x, 980) + 60, pref.y + 36);
            return row;
        }

        [BookShot("ch05-emoji", W = 960, H = 600, Scene = "Ch05_Emoji")]
        static void Emoji(Shot s)
        {
            s.Background(UIKit.Hex("#EDEDED"));
            var emoji = EmojiSpriteAssetBuilder.Ensure();
            Bubble(s.Root, "avatar_1", "今天副本掉了 <sprite name=\"gem\"> 和 <sprite name=\"coin\">x999，运气爆棚 <sprite name=\"laugh\">", false, 470, emoji);
            Bubble(s.Root, "avatar_2", "<sprite name=\"cry\"> 我刷了一晚上啥也没出", true, 290, emoji);
            Bubble(s.Root, "avatar_3", "恭喜恭喜 <sprite name=\"love\"><sprite name=\"heart\"><sprite name=\"heart\"> 送你一瓶 <sprite name=\"potion\">", false, 110, emoji);
            var all = UIKit.Card(s.Root, "Palette"); all.Place(0, -330, 1700, 300);
            string line = "";
            foreach (var n in new[] { "smile", "laugh", "cry", "angry", "cool", "love", "wow", "sleepy" }) line += $"<sprite name=\"{n}\">";
            line += "\n";
            foreach (var n in new[] { "coin", "gem", "heart", "star", "potion", "sword", "trophy", "gift" }) line += $"<sprite name=\"{n}\">";
            var pal = UIKit.Text(all, line, 76, Ink, TextAlignmentOptions.Center); pal.spriteAsset = emoji;
            pal.characterSpacing = 30; pal.lineSpacing = 10;
            pal.rectTransform.Stretch(20, 10, 20, 10);
            T(s.Root, "Ch05/Sprites/Emoji.asset：16 个精灵（8 个代码画的表情 + 8 个物品图标）", 0, -530, 24, Muted, 1600);
        }

        // ================================================================ 案例 5.4 可点击链接
        [BookShot("ch05-link", W = 960, H = 460, Scene = "Ch05_Link")]
        static void Link(Shot s)
        {
            s.Background(UIKit.Hex("#1F2436"));
            var panel = UIKit.Image(s.Root, "round_16", new Color(1, 1, 1, 0.06f), "ChatPanel");
            panel.rectTransform.Place(0, 90, 1700, 560);
            string msg =
                "<color=#FFB547>[系统]</color> 恭喜 <link=\"player:麦冬\"><color=#8C9CFF><u>麦冬</u></color></link> 在深渊副本中获得了 " +
                "<link=\"item:sword\"><color=#FF8A3D>[破晓之剑]</color></link>！\n" +
                "<color=#FFB547>[公会]</color> 今晚 8 点开团，记得带上 <link=\"item:potion\"><color=#FF6B6B>[生命药水]</color></link>。\n" +
                "<color=#9AA3B5>更新公告请看 <link=\"url:https://example.com/notes\"><u>这里</u></link>。</color>";
            var text = UIKit.Text(panel.transform, msg, 42, Color.white, TextAlignmentOptions.TopLeft, false, "Messages");
            text.rectTransform.Stretch(50, 50, 50, 50);
            text.lineSpacing = 30;
            text.raycastTarget = true;                                  // 关键：不勾就收不到点击
            var handler = text.gameObject.AddComponent<LinkHandler>();

            var tip = UIKit.Card(s.Root, "ItemTip"); tip.sizeDelta = new Vector2(460, 230);
            tip.anchorMin = tip.anchorMax = new Vector2(0.5f, 0.5f); tip.pivot = new Vector2(0.5f, 1);
            var tipText = UIKit.Text(tip, "", 34, Ink, TextAlignmentOptions.TopLeft, false, "TipText");
            tipText.rectTransform.Stretch(30, 24, 30, 24);
            var log = T(s.Root, "把鼠标移到彩色文字上，点击试试", 0, -340, 30, new Color(1, 1, 1, 0.6f), 1400);
            var demo = s.Root.gameObject.AddComponent<LinkDemo>();
            SetField(demo, "m_Handler", handler); SetField(demo, "m_Tip", tip); SetField(demo, "m_TipText", tipText); SetField(demo, "m_Log", log);

            s.BeforeCapture(() =>
            {
                tip.gameObject.SetActive(true);
                text.ForceMeshUpdate();
                // 模拟“悬停在第 2 个链接上并点击”：取链接第一个字的位置
                var li = text.textInfo.linkInfo[1];
                var ci = text.textInfo.characterInfo[li.linkTextfirstCharacterIndex + 3];
                var world = text.transform.TransformPoint((ci.bottomLeft + ci.topRight) * 0.5f);
                var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, world);
                var parent = (RectTransform)tip.parent;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, s.Cam, out var local);
                tip.anchoredPosition = local + new Vector2(0, -40);
                tipText.text = "<b><color=#FF8A3D>破晓之剑</color></b>\n攻击 +120\n<size=80%><color=#7A8299>传说中在黎明时铸成的剑</color></size>";
                log.text = "点击了链接：item:sword";
            });
            s.BeforeCapture(() => { handler.SetHover(1); });
        }

        // ================================================================ 案例 5.5 打字机
        const string Line1 = "勇者，你终于来了。北边的<color=#FF8A3D>黑森林</color>里出现了奇怪的光，村里的猎人都不敢靠近……";

        [BookShot("ch05-typewriter", W = 640, H = 300, Panels = 3, Scene = "Ch05_Typewriter")]
        static void TypewriterShot(Shot s)
        {
            s.Background(UIKit.Hex("#40466E"));
            var box = UIKit.Image(s.Root, "round_24", new Color(0.08f, 0.09f, 0.16f, 0.92f), "DialogBox");
            box.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(1220, 360));
            var av = UIKit.Image(box.transform, "avatar_5", Color.white, "Avatar");
            av.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -40), new Vector2(150, 150));
            var nameT = UIKit.Text(box.transform, "老村长", 40, UIKit.Hex("#FFC93C"), TextAlignmentOptions.TopLeft, true, "Name");
            nameT.rectTransform.Place(new Vector2(0, 1), new Vector2(220, -36), new Vector2(600, 60));
            var body = UIKit.Text(box.transform, "", 40, Color.white, TextAlignmentOptions.TopLeft, false, "Body");
            body.rectTransform.anchorMin = Vector2.zero; body.rectTransform.anchorMax = Vector2.one;
            body.rectTransform.offsetMin = new Vector2(220, 40); body.rectTransform.offsetMax = new Vector2(-50, -110);
            body.lineSpacing = 12;
            var arrowRoot = UIKit.Node(box.transform, "NextArrowRoot");
            arrowRoot.Place(new Vector2(1, 0), new Vector2(-60, 50), new Vector2(50, 50));
            var arrow = UIKit.Icon(arrowRoot, "arrow_down", 46, UIKit.Hex("#FFC93C"), "NextArrow");
            arrow.rectTransform.Place(0, 0, 46, 46);
            var tw = box.gameObject.AddComponent<Typewriter>();
            SetField(tw, "m_Text", body);
            var dlg = box.gameObject.AddComponent<DialogBox>();
            SetField(dlg, "m_Writer", tw); SetField(dlg, "m_NextArrow", arrow.gameObject);
            SetField(dlg, "m_Lines", new[] { Line1, "听说那光每隔<color=#FF8A3D>七天</color>出现一次，今晚正好是第七天。", "带上这把钥匙 <b>[古老的钥匙]</b>，它也许能打开森林深处的门。" });
            T(s.Root, "点击对话框：打字中 = 跳过，打完 = 下一句", 0, 220, 30, new Color(1, 1, 1, 0.7f), 1200);
            float[] prog = { 0.25f, 0.6f, 1f };
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                tw.Preview(Line1, prog[panel]);
                arrow.gameObject.SetActive(panel == 2);
            });
        }

        // ================================================================ 案例 5.6 数字跳动
        static NumberRoller Counter(Transform parent, string icon, Color iconColor, string label, float y, string prefix, Color color, out Button button, string btnText)
        {
            var card = UIKit.Card(parent, label); card.Place(0, y, 1100, 200);
            var ic = UIKit.Icon(card, icon, 120, iconColor, "Icon"); ic.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(120, 120));
            var lb = UIKit.Text(card, label, 30, Skin.muted, TextAlignmentOptions.Left, false, "Label");
            lb.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(200, 45), new Vector2(380, 50));
            lb.alignment = TextAlignmentOptions.MidlineLeft;
            var num = UIKit.Text(card, "0", 72, color, TextAlignmentOptions.Left, true, "Number");
            num.rectTransform.pivot = new Vector2(0, 0.5f);
            num.rectTransform.anchorMin = num.rectTransform.anchorMax = new Vector2(0, 0.5f);
            num.rectTransform.anchoredPosition = new Vector2(200, -25); num.rectTransform.sizeDelta = new Vector2(560, 100);
            num.alignment = TextAlignmentOptions.MidlineLeft;
            var roller = card.gameObject.AddComponent<NumberRoller>();
            SetField(roller, "m_Text", num); SetField(roller, "m_Prefix", prefix);
            button = UIKit.Button(card, btnText, Skin.accent, 230, 96, null, 34);
            button.Rt().Place(new Vector2(1, 0.5f), new Vector2(-150, 0), new Vector2(230, 96));
            return roller;
        }

        [BookShot("ch05-number", W = 640, H = 340, Panels = 3, Scene = "Ch05_Number")]
        static void Number(Shot s)
        {
            s.Ref(1280, 680);
            var gold = Counter(s.Root, "item_coin", Color.white, "金币", 150, "", UIKit.Hex("#E09A1A"), out var claim, "领取");
            var power = Counter(s.Root, "trophy", UIKit.Hex("#F2B127"), "战力", -110, "", Skin.primary, out var up, "升级");
            var demo = s.Root.gameObject.AddComponent<NumberRollerDemo>();
            SetField(demo, "m_Gold", gold); SetField(demo, "m_Power", power); SetField(demo, "m_Claim", claim); SetField(demo, "m_Upgrade", up);
            float[] t = { 0f, 0.35f, 1f };
            string[] cap = { "t = 0（点击“领取”）", "t = 0.35（滚动中、放大）", "t = 1（停在新值）" };
            int panel = s.Panel;
            T(s.Root, cap[panel], 0, -290, 30, Ink, 1000, true);
            s.BeforeCapture(() =>
            {
                gold.SetInstant(12800); gold.RollTo(17240); gold.Evaluate(t[panel]);
                power.SetInstant(35210); power.RollTo(36980); power.Evaluate(t[panel]);
            });
        }
    }
}
