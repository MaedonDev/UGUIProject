// 第 4 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    static class Shots
    {
        static UISkin Skin => UISkin.Instance;
        static Color Ink => Skin.ink;
        static Color Muted => Skin.muted;

        static TextMeshProUGUI T(Transform p, string text, float x, float y, float size = 30, Color? col = null, float w = 600, bool bold = false)
        {
            var t = UIKit.Text(p, text, size, col ?? Ink, TextAlignmentOptions.Center, bold);
            t.rectTransform.Place(x, y, w, size * 1.6f);
            return t;
        }

        static Image Img(Transform p, Sprite sp, float x, float y, float w, float h, Image.Type type = Image.Type.Simple, string name = "Image")
        {
            var rt = UIKit.Node(p, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sp; img.type = type; img.raycastTarget = false;
            rt.Place(x, y, w, h);
            return img;
        }

        static void SetField(Object o, string field, object value)
        {
            var so = new SerializedObject(o);
            var p = so.FindProperty(field);
            switch (value)
            {
                case Object obj: p.objectReferenceValue = obj; break;
                case float f: p.floatValue = f; break;
                case Vector2 v: p.vector2Value = v; break;
                case bool b: p.boolValue = b; break;
                case Object[] arr:
                    p.arraySize = arr.Length;
                    for (int i = 0; i < arr.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
                    break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------------------------------------------------------- 4.x PPU 与 Set Native Size
        [BookShot("ch04-native", W = 960, H = 400)]
        static void Native(Shot s)
        {
            var tex = UIKit.Sp("avatar_3").texture;
            float[] ppus = { 50, 100, 200 };
            for (int i = 0; i < 3; i++)
            {
                var sp = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), ppus[i]);
                s.Temp.Add(sp);
                float x = -560 + i * 560;
                var img = Img(s.Root, sp, x, 40, 10, 10);
                img.SetNativeSize();
                img.rectTransform.anchoredPosition = new Vector2(x, 60);
                var outline = UIKit.Image(s.Root, "round_outline_8", Skin.primary, "Outline");
                outline.rectTransform.Place(x, 60, img.rectTransform.sizeDelta.x + 12, img.rectTransform.sizeDelta.y + 12);
                var size = img.rectTransform.sizeDelta;
                T(s.Root, $"Pixels Per Unit = {ppus[i]}", x, -170, 34, Ink, 560, true);
                T(s.Root, $"Set Native Size → {size.x:0} × {size.y:0}", x, -230, 30, Muted, 560);
            }
            T(s.Root, "同一张 128×128 的图，Canvas 的 Reference Pixels Per Unit = 100", 0, -330, 28, Muted, 1600);
        }

        // ---------------------------------------------------------------- 四种 Image Type
        [BookShot("ch04-types", W = 960, H = 560)]
        static void Types(Shot s)
        {
            Ch04Textures.Ensure();
            var frame = Ch04Textures.Sprite("dialog_frame");
            var tile = Ch04Textures.Sprite("tile_pattern");
            var cells = new[] { new Vector2(-470, 250), new Vector2(470, 250), new Vector2(-470, -290), new Vector2(470, -290) };
            string[] names = { "Simple：整张图直接拉伸", "Sliced：九宫格，四角不变", "Tiled：按原尺寸重复铺满", "Filled：只画一部分（这里 Radial 360，0.7）" };
            for (int i = 0; i < 4; i++)
            {
                var c = cells[i];
                var img = Img(s.Root, i == 2 ? tile : frame, c.x, c.y - 30, 820, 330,
                    i == 0 ? Image.Type.Simple : i == 1 ? Image.Type.Sliced : i == 2 ? Image.Type.Tiled : Image.Type.Filled, names[i]);
                if (i == 3) { img.fillMethod = Image.FillMethod.Radial360; img.fillAmount = 0.7f; img.fillOrigin = (int)Image.Origin360.Top; }
                T(s.Root, names[i], c.x, c.y + 175, 34, Ink, 900, true);
            }
        }

        // ---------------------------------------------------------------- Sliced 不同尺寸 + 边框线
        [BookShot("ch04-sliced", W = 960, H = 440)]
        static void SlicedSizes(Shot s)
        {
            Ch04Textures.Ensure();
            var frame = Ch04Textures.Sprite("dialog_frame");
            // 左：原图 96×96 放大 3 倍，画出四条切分线
            var src = Img(s.Root, frame, -700, 40, 288, 288);
            for (int i = 0; i < 4; i++)
            {
                bool vertical = i < 2;
                float off = (i % 2 == 0 ? -1 : 1) * (144 - 54 * 288f / 160);
                var line = UIKit.Image(s.Root, null, new Color(0.9f, 0.28f, 0.3f), "SliceLine");
                line.rectTransform.Place(-700 + (vertical ? off : 0), 40 + (vertical ? 0 : off), vertical ? 4 : 330, vertical ? 330 : 4);
            }
            T(s.Root, "原图 160×160，Border 四边各 54", -700, -170, 30, Ink, 520, true);
            T(s.Root, "（放大 1.8 倍显示，红线是切分线）", -700, -220, 26, Muted, 520);

            Img(s.Root, frame, -120, 120, 520, 200, Image.Type.Sliced);
            T(s.Root, "520 × 200", -120, -20, 28, Muted);
            Img(s.Root, frame, 400, 70, 260, 300, Image.Type.Sliced);
            T(s.Root, "260 × 300", 400, -120, 28, Muted);
            var nf = Img(s.Root, frame, 760, 70, 240, 300, Image.Type.Sliced);
            nf.fillCenter = false;
            T(s.Root, "Fill Center 关", 760, -120, 28, Muted);
            var small = Img(s.Root, frame, -120, -210, 80, 80, Image.Type.Sliced);
            T(s.Root, "80 × 80：比两边边距之和（108）还小，四角被等比压缩", 280, -210, 26, Muted, 700);
            small.name = "Small";
        }

        // ---------------------------------------------------------------- Pixels Per Unit Multiplier
        [BookShot("ch04-ppumul", W = 960, H = 300)]
        static void PpuMul(Shot s)
        {
            float[] m = { 0.5f, 1f, 2f, 4f };
            for (int i = 0; i < 4; i++)
            {
                float x = -690 + i * 460;
                var img = Img(s.Root, UIKit.Sp("round_32"), x, 50, 380, 150, Image.Type.Sliced);
                img.color = Skin.primary;
                img.pixelsPerUnitMultiplier = m[i];
                T(img.transform, "按钮", 0, 0, 40, Color.white, 300, true);
                T(s.Root, $"Multiplier = {m[i]}", x, -95, 32, Ink, 440, true);
            }
            T(s.Root, "同一张 round_32（72×72，Border 34），数值越大，圆角越小", 0, -200, 28, Muted, 1600);
        }

        // ---------------------------------------------------------------- Filled 各种 Method × fillAmount
        [BookShot("ch04-filled", W = 960, H = 600)]
        static void Filled(Shot s)
        {
            var sp = UIKit.Sp("avatar_4");
            var methods = new[] { Image.FillMethod.Horizontal, Image.FillMethod.Vertical, Image.FillMethod.Radial90, Image.FillMethod.Radial180, Image.FillMethod.Radial360 };
            float[] amounts = { 0.25f, 0.5f, 0.75f };
            for (int c = 0; c < 5; c++)
            {
                float x = -660 + c * 350;
                T(s.Root, methods[c].ToString(), x, 500, 34, Ink, 340, true);
                for (int r = 0; r < 3; r++)
                {
                    float y = 300 - r * 320;
                    var ghost = Img(s.Root, sp, x, y, 240, 240);
                    ghost.color = new Color(1, 1, 1, 0.18f);
                    var img = Img(s.Root, sp, x, y, 240, 240, Image.Type.Filled, $"{methods[c]}_{amounts[r]}");
                    img.fillMethod = methods[c];
                    img.fillAmount = amounts[r];
                    img.fillClockwise = true;
                    img.fillOrigin = 0;
                }
            }
            for (int r = 0; r < 3; r++)
                T(s.Root, $"{amounts[r]}", -880, 300 - r * 320, 34, Skin.primary, 160, true);
            T(s.Root, "Fill Amount", -880, 500, 26, Muted, 200);
        }

        // ---------------------------------------------------------------- Radial 360 的 Origin × Clockwise
        [BookShot("ch04-origin", W = 960, H = 460)]
        static void Origins(Shot s)
        {
            var origins = new[] { Image.Origin360.Bottom, Image.Origin360.Right, Image.Origin360.Top, Image.Origin360.Left };
            for (int r = 0; r < 2; r++)
            {
                bool cw = r == 0;
                for (int c = 0; c < 4; c++)
                {
                    float x = -540 + c * 400, y = 230 - r * 430;
                    var ghost = Img(s.Root, UIKit.Sp("ring"), x, y, 260, 260);
                    ghost.color = new Color(0, 0, 0, 0.08f);
                    var img = Img(s.Root, UIKit.Sp("ring"), x, y, 260, 260, Image.Type.Filled);
                    img.color = cw ? Skin.primary : Skin.accent;
                    img.fillMethod = Image.FillMethod.Radial360;
                    img.fillOrigin = (int)origins[c];
                    img.fillClockwise = cw;
                    img.fillAmount = 0.3f;
                    T(s.Root, $"Origin = {origins[c]}", x, y - 170, 28, Ink, 380, true);
                }
                T(s.Root, cw ? "Clockwise\n= true" : "Clockwise\n= false", -880, 230 - r * 430, 30, cw ? Skin.primary : Skin.accent, 200, true)
                    .rectTransform.sizeDelta = new Vector2(200, 120);
            }
        }

        // ---------------------------------------------------------------- Preserve Aspect
        [BookShot("ch04-aspect", W = 960, H = 360)]
        static void Aspect(Shot s)
        {
            var sp = UIKit.Sp("bg_landscape");
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -420 : 420;
                var box = UIKit.Image(s.Root, "round_outline_8", Skin.muted, "Box");
                box.rectTransform.Place(x, 40, 470, 470);
                var img = Img(s.Root, sp, x, 40, 450, 450);
                img.preserveAspect = i == 1;
                T(s.Root, i == 0 ? "Preserve Aspect 关：被压扁" : "Preserve Aspect 开：按比例缩进框里", x, -250, 32, Ink, 800, true);
            }
            T(s.Root, "灰框 = RectTransform 的范围（450 × 450），图片原始比例 2:1", 0, -320, 26, Muted, 1600);
        }

        // ---------------------------------------------------------------- RawImage 的 UV Rect
        [BookShot("ch04-uvrect", W = 960, H = 380)]
        static void UvRect(Shot s)
        {
            Ch04Textures.Ensure();
            var tile = Ch04Textures.Texture("tile_pattern");
            var coin = UIKit.Sp("item_coin").texture;
            var cases = new (Texture tex, Rect uv, string label, string sub)[]
            {
                (tile, new Rect(0, 0, 1, 1), "(0, 0, 1, 1)", "默认：整张图"),
                (tile, new Rect(0, 0, 3, 1.5f), "(0, 0, 3, 1.5)", "Repeat：重复 3×1.5 次"),
                (tile, new Rect(0.25f, 0.25f, 0.5f, 0.5f), "(0.25, 0.25, 0.5, 0.5)", "只取中间一块：放大"),
                (coin, new Rect(-1, -1, 3, 3), "(-1, -1, 3, 3)", "Clamp：超出部分取边缘像素"),
            };
            for (int i = 0; i < 4; i++)
            {
                float x = -705 + i * 470;
                var bg = UIKit.Image(s.Root, "checker", Color.white, "Checker");
                bg.type = Image.Type.Tiled; bg.rectTransform.Place(x, 70, 400, 300);
                var raw = UIKit.Node(s.Root, "RawImage").gameObject.AddComponent<RawImage>();
                raw.texture = cases[i].tex; raw.uvRect = cases[i].uv;
                raw.rectTransform.Place(x, 70, 400, 300);
                T(s.Root, cases[i].label, x, -135, 30, Ink, 460, true);
                T(s.Root, cases[i].sub, x, -190, 26, Muted, 460);
            }
            T(s.Root, "UV Rect = (X, Y, W, H)；前三个纹理 Wrap Mode = Repeat，最后一个是 Clamp 的金币图", 0, -300, 26, Muted, 1700);
        }

        // ================================================================ 案例 4.1 九宫格对话框
        [BookShot("ch04-dialog", W = 960, H = 540, Scene = "Ch04_Dialog")]
        static void Dialog(Shot s)
        {
            Ch04Textures.Ensure();
            s.Background(UIKit.Hex("#2E3350"));
            var dim = UIKit.Image(s.Root, null, new Color(0, 0, 0, 0.35f), "Dim");
            dim.rectTransform.Stretch();

            var dlgImg = UIKit.Node(s.Root, "Dialog").gameObject.AddComponent<Image>();
            dlgImg.sprite = Ch04Textures.Sprite("dialog_frame");
            dlgImg.type = Image.Type.Sliced;
            var dlg = dlgImg.rectTransform;
            dlg.anchorMin = dlg.anchorMax = new Vector2(0.5f, 0.5f);
            dlg.pivot = new Vector2(0, 1);                     // 轴心在左上角：拖右下角时左上角不动
            dlg.anchoredPosition = new Vector2(-480, 350);
            dlg.sizeDelta = new Vector2(960, 660);

            // 标题条：顶部拉伸
            var title = UIKit.Text(dlg, "任务完成", 52, UIKit.Hex("#5A3D22"), TextAlignmentOptions.Center, true, "Title");
            title.rectTransform.anchorMin = new Vector2(0, 1); title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0.5f, 1);
            title.rectTransform.offsetMin = new Vector2(40, -130); title.rectTransform.offsetMax = new Vector2(-40, -40);

            var body = UIKit.Text(dlg, "你帮村长找回了走失的小羊。\n这是他的一点心意：", 36, UIKit.Hex("#6B4E31"), TextAlignmentOptions.Top, false, "Body");
            body.rectTransform.Stretch(70, 140, 70, 330);

            // 奖励格子：顶部居中固定
            var row = UIKit.Node(dlg, "Rewards");
            row.anchorMin = row.anchorMax = new Vector2(0.5f, 1); row.pivot = new Vector2(0.5f, 1);
            row.anchoredPosition = new Vector2(0, -290); row.sizeDelta = new Vector2(600, 140);
            row.HLayout(30);
            foreach (var item in new[] { "item_coin", "item_gem", "item_potion_red" })
            {
                var slot = UIKit.Image(row, "slot", Color.white, "Slot_" + item);
                slot.rectTransform.sizeDelta = new Vector2(140, 140);
                var ic = UIKit.Icon(slot.transform, item, 100, Color.white);
                ic.rectTransform.Place(0, 0, 100, 100);
            }

            // 底部按钮：底部居中
            var btns = UIKit.Node(dlg, "Buttons");
            btns.anchorMin = new Vector2(0, 0); btns.anchorMax = new Vector2(1, 0); btns.pivot = new Vector2(0.5f, 0);
            btns.offsetMin = new Vector2(40, 50); btns.offsetMax = new Vector2(-40, 160);
            btns.HLayout(40);
            UIKit.Button(btns, "领取", UIKit.Hex("#E0A94A"), 260, 100);
            UIKit.Button(btns, "关闭", UIKit.Hex("#B9A889"), 260, 100);

            // 右下角把手
            var handle = UIKit.Image(dlg, "play", UIKit.Hex("#5A3D22"), "ResizeHandle");
            handle.rectTransform.Place(new Vector2(1, 0), new Vector2(-46, 46), new Vector2(60, 60));
            handle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            handle.rectTransform.anchoredPosition = new Vector2(-46, 46);
            handle.rectTransform.localEulerAngles = new Vector3(0, 0, -45);   // 三角形尖端指向右下
            var rz = handle.gameObject.AddComponent<DialogResizer>();
            SetField(rz, "m_Target", dlg);

            var sw = UIKit.Button(s.Root, "Image Type：Sliced", Skin.primary, 520, 84, null, 32, "TypeSwitch");
            sw.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(520, 84));
            var its = sw.gameObject.AddComponent<ImageTypeSwitch>();
            SetField(its, "m_Target", dlgImg);
            SetField(its, "m_Label", sw.GetComponentInChildren<TextMeshProUGUI>());

            var hint = T(s.Root, "拖动右下角的把手改变大小；点下方按钮切换 Image Type", 0, 0, 28, new Color(1, 1, 1, 0.7f), 1400);
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1400, 50));
        }

        // ================================================================ 案例 4.2 技能冷却
        static SkillCooldown Skill(Transform parent, string icon, float x, float y, float size = 220)
        {
            var bgImg = UIKit.Image(parent, "circle", UIKit.Hex("#2B3050"), "Skill_" + icon);
            var root = bgImg.rectTransform; root.Place(x, y, size, size);
            var btn = bgImg.gameObject.AddComponent<Button>();
            var ring = UIKit.Image(root, "ring_thin", UIKit.Hex("#8C93D9"), "Ring"); ring.rectTransform.Stretch(); ring.raycastTarget = false;
            var ic = UIKit.Icon(root, icon, size * 0.56f, Color.white, "Icon"); ic.rectTransform.Place(0, 0, size * 0.56f, size * 0.56f);
            var mask = UIKit.Image(root, "circle", new Color(0.05f, 0.06f, 0.12f, 0.72f), "CooldownMask");
            mask.raycastTarget = false;
            mask.type = Image.Type.Filled; mask.fillMethod = Image.FillMethod.Radial360;
            mask.fillOrigin = (int)Image.Origin360.Top; mask.fillClockwise = false;   // 逆时针退去 = 时钟顺时针走
            mask.rectTransform.Stretch();
            var flash = UIKit.Image(root, "circle_soft", Color.white, "ReadyFlash"); flash.rectTransform.Stretch(-30, -30, -30, -30); flash.raycastTarget = false;
            var label = UIKit.Text(root, "", size * 0.32f, Color.white, TextAlignmentOptions.Center, true, "Countdown");
            label.rectTransform.Stretch();
            var cd = bgImg.gameObject.AddComponent<SkillCooldown>();
            SetField(cd, "m_Mask", mask); SetField(cd, "m_Label", label); SetField(cd, "m_ReadyFlash", flash);
            flash.enabled = false;
            return cd;
        }

        [BookShot("ch04-cooldown", W = 960, H = 400, Scene = "Ch04_Cooldown")]
        static void Cooldown(Shot s)
        {
            s.Background(UIKit.Hex("#1B1F33"));
            var a = Skill(s.Root, "item_sword", -480, 60);
            var b = Skill(s.Root, "item_shield", 0, 60);
            var c = Skill(s.Root, "item_potion_red", 480, 60);
            T(s.Root, "就绪", -480, -120, 32, Color.white, 300, true);
            T(s.Root, "冷却中（剩 1.6 秒）", 0, -120, 32, Color.white, 400, true);
            T(s.Root, "刚释放（剩 4.2 秒）", 480, -120, 32, Color.white, 400, true);
            var hint = T(s.Root, "点击技能图标释放，冷却 5 秒", 0, -280, 28, new Color(1, 1, 1, 0.6f), 900);
            s.BeforeCapture(() => { a.SetRemain(0); b.SetRemain(1.6f); c.SetRemain(4.2f); });
        }

        // ================================================================ 案例 4.3 延迟白条血条
        static DelayedHealthBar Bar(Transform parent, float x, float y, float w = 900, float h = 64)
        {
            Ch04Textures.Ensure();
            var bg = UIKit.Image(parent, "round_8", UIKit.Hex("#151827"), "HealthBar");
            bg.rectTransform.Place(x, y, w, h);
            var area = UIKit.Node(bg.transform, "FillArea"); area.Stretch(6, 6, 6, 6);
            var fillSp = Ch04Textures.Sprite("bar_fill");
            var delay = UIKit.Node(area, "Delay").gameObject.AddComponent<Image>();
            delay.sprite = fillSp; delay.type = Image.Type.Filled; delay.fillMethod = Image.FillMethod.Horizontal;
            delay.color = new Color(1, 1, 1, 0.92f); delay.rectTransform.Stretch(); delay.raycastTarget = false;
            var fill = UIKit.Node(area, "Fill").gameObject.AddComponent<Image>();
            fill.sprite = fillSp; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal;
            fill.rectTransform.Stretch(); fill.raycastTarget = false;
            var label = UIKit.Text(bg.transform, "1000 / 1000", h * 0.5f, Color.white, TextAlignmentOptions.Center, true, "Label");
            label.rectTransform.Stretch();
            var bar = bg.gameObject.AddComponent<DelayedHealthBar>();
            SetField(bar, "m_Fill", fill); SetField(bar, "m_Delay", delay); SetField(bar, "m_Label", label);
            return bar;
        }

        [BookShot("ch04-hpbar", W = 960, H = 400)]
        static void HpBarShot(Shot s)
        {
            s.Background(UIKit.Hex("#2E3350"));
            var b0 = Bar(s.Root, 120, 230);
            var b1 = Bar(s.Root, 120, 30);
            var b2 = Bar(s.Root, 120, -170);
            T(s.Root, "① 满血", -650, 230, 34, Color.white, 300, true);
            T(s.Root, "② 受击瞬间", -650, 30, 34, Color.white, 300, true);
            T(s.Root, "③ 白条追赶中", -650, -170, 34, Color.white, 300, true);
            T(s.Root, "血条立刻跳到 620，白条先停 0.45 秒，再以每秒 60% 满血量的速度追上", 0, -320, 28, new Color(1, 1, 1, 0.65f), 1700);
            s.BeforeCapture(() =>
            {
                b0.SetInstant(1000);
                b1.SetInstant(1000); b1.Damage(380);
                b2.SetInstant(1000); b2.Damage(380); b2.Tick(0.5f); b2.Tick(0.25f);
            });
        }

        [BookScene("Ch04_HealthBar")]
        static void HpBarScene(Shot s)
        {
            s.Background(UIKit.Hex("#2E3350"));
            var bar = Bar(s.Root, 0, 120, 1000, 72);
            var av = UIKit.Image(s.Root, "avatar_2", Color.white, "Avatar");
            av.rectTransform.Place(-640, 120, 140, 140);
            T(s.Root, "森林守卫 Lv.12", 0, 220, 36, Color.white, 600, true);
            var row = UIKit.Node(s.Root, "Buttons"); row.Place(0, -120, 1200, 110); row.HLayout(40);
            var hit = UIKit.Button(row, "受伤 -80", Skin.danger, 300, 100);
            var big = UIKit.Button(row, "重击 -320", UIKit.Hex("#A8323A"), 300, 100);
            var heal = UIKit.Button(row, "治疗 +250", Skin.success, 300, 100);
            var demo = s.Root.gameObject.AddComponent<HealthBarDemo>();
            SetField(demo, "m_Bar", bar); SetField(demo, "m_Hit", hit); SetField(demo, "m_BigHit", big); SetField(demo, "m_Heal", heal);
            T(s.Root, "连续点“受伤”，看白条如何攒住整段伤害再一起退去", 0, -300, 28, new Color(1, 1, 1, 0.6f), 1400);
        }

        // ================================================================ 案例 4.4 不规则按钮
        static Image ShapeButton(Transform parent, string tex, float x, float y, float size, bool alphaHit, string name)
        {
            var img = UIKit.Node(parent, name).gameObject.AddComponent<Image>();
            img.sprite = Ch04Textures.Sprite(tex);
            img.rectTransform.Place(x, y, size, size);
            var b = img.gameObject.AddComponent<Button>();
            var cb = b.colors; cb.highlightedColor = new Color(1, 0.95f, 0.85f); cb.pressedColor = new Color(0.8f, 0.8f, 0.8f); b.colors = cb;
            if (alphaHit) img.gameObject.AddComponent<AlphaHitArea>();
            return img;
        }

        [BookShot("ch04-hit", W = 960, H = 460, Scene = "Ch04_AlphaHit")]
        static void AlphaHit(Shot s)
        {
            Ch04Textures.Ensure();
            var bgBtn = UIKit.Image(s.Root, null, UIKit.Hex("#E4E7F0"), "背景");
            bgBtn.rectTransform.Stretch();
            bgBtn.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
            var a = ShapeButton(s.Root, "star_button", -560, 60, 360, false, "星星（未开启）");
            var b = ShapeButton(s.Root, "star_button", 0, 60, 360, true, "星星");
            var c = ShapeButton(s.Root, "ring_button", 560, 60, 360, true, "圆环");
            T(s.Root, "alphaHitTestMinimumThreshold = 0", -560, -170, 28, Ink, 560, true);
            T(s.Root, "= 0.5", 0, -170, 28, Ink, 560, true);
            T(s.Root, "= 0.5（中间的洞点不中）", 560, -170, 28, Ink, 560, true);
            var log = T(s.Root, "点星星、圆环或背景试试", 0, -290, 32, Skin.primary, 1400, true);
            var hl = s.Root.gameObject.AddComponent<HitLog>();
            SetField(hl, "m_Buttons", new Object[] { bgBtn.GetComponent<Button>(), a.GetComponent<Button>(), b.GetComponent<Button>(), c.GetComponent<Button>() });
            SetField(hl, "m_Output", log);
            log.raycastTarget = false;

            // 配图专用：在每个按钮上撒一层采样点，绿 = 能点中，红 = 点不中（直接调用 Image.IsRaycastLocationValid）
            s.BeforeCapture(() =>
            {
                b.GetComponent<AlphaHitArea>().Apply();
                c.GetComponent<AlphaHitArea>().Apply();
                var dots = UIKit.Node(s.Root, "Dots"); dots.Stretch();
                foreach (var img in new[] { a, b, c })
                {
                    var rt = img.rectTransform;
                    var corners = new Vector3[4]; rt.GetWorldCorners(corners);
                    int n = 15;
                    for (int i = 0; i < n; i++)
                        for (int j = 0; j < n; j++)
                        {
                            var world = Vector3.Lerp(Vector3.Lerp(corners[0], corners[3], (i + 0.5f) / n), Vector3.Lerp(corners[1], corners[2], (i + 0.5f) / n), (j + 0.5f) / n);
                            var screen = RectTransformUtility.WorldToScreenPoint(s.Cam, world);
                            bool ok = img.IsRaycastLocationValid(screen, s.Cam);
                            var d = UIKit.Image(dots, "circle_small", ok ? new Color(0.1f, 0.65f, 0.35f, 0.95f) : new Color(0.9f, 0.25f, 0.3f, 0.55f), "Dot");
                            d.raycastTarget = false;
                            d.rectTransform.position = world;
                            d.rectTransform.sizeDelta = new Vector2(ok ? 12 : 8, ok ? 12 : 8);
                        }
                }
            });
        }

        // ================================================================ 案例 4.5 无限滚动背景
        static RawImage Layer(Transform parent, Texture tex, string name, float height, float speed)
        {
            var raw = UIKit.Node(parent, name).gameObject.AddComponent<RawImage>();
            raw.texture = tex; raw.raycastTarget = false;
            var rt = raw.rectTransform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, height); rt.anchoredPosition = Vector2.zero;
            if (speed != 0)
            {
                var sc = raw.gameObject.AddComponent<UVScroller>();
                SetField(sc, "m_Speed", new Vector2(speed, 0));
            }
            return raw;
        }

        [BookShot("ch04-parallax", W = 960, H = 400, Scene = "Ch04_Parallax")]
        static void Parallax(Shot s)
        {
            Ch04Textures.Ensure();
            var sky = UIKit.Node(s.Root, "Sky").gameObject.AddComponent<RawImage>();
            sky.texture = Ch04Textures.Texture("sky"); sky.rectTransform.Stretch(); sky.raycastTarget = false;
            var sun = UIKit.Image(s.Root, "circle_soft", new Color(1, 0.95f, 0.8f, 0.9f), "Sun");
            sun.rectTransform.Place(500, 140, 260, 260);
            var far = Layer(s.Root, Ch04Textures.Texture("hills_far"), "HillsFar", 420, 0.015f);
            var near = Layer(s.Root, Ch04Textures.Texture("hills_near"), "HillsNear", 300, 0.05f);
            var pillBg = UIKit.Image(s.Root, "pill", new Color(0.12f, 0.14f, 0.3f, 0.55f), "TitleBg");
            pillBg.rectTransform.Place(0, 280, 1000, 90);
            T(s.Root, "无限滚动背景：两层 RawImage，只改 UV Rect", 0, 280, 40, Color.white, 1000, true);
            s.BeforeCapture(() =>
            {
                far.GetComponent<UVScroller>().FitAspect(); near.GetComponent<UVScroller>().FitAspect();
                far.GetComponent<UVScroller>().Scroll(20f); near.GetComponent<UVScroller>().Scroll(5f);
            });
        }

        // ================================================================ 案例 4.6 头像框
        static AvatarFrame Avatar(Transform parent, float x, float y, string avatar, int level, int rarity, bool dot, Sprite[] all)
        {
            var root = UIKit.Node(parent, "AvatarFrame_" + avatar); root.Place(x, y, 200, 200);
            var hit = root.gameObject.AddComponent<Image>(); hit.color = new Color(1, 1, 1, 0);   // 透明点击区
            var btn = root.gameObject.AddComponent<Button>(); btn.transition = Selectable.Transition.None;
            var maskImg = UIKit.Image(root, "circle", Color.white, "Mask"); maskImg.rectTransform.Place(0, 0, 164, 164);
            maskImg.gameObject.AddComponent<Mask>().showMaskGraphic = false;   // Mask 原理见第 9、14 章
            maskImg.raycastTarget = false;
            var av = UIKit.Image(maskImg.transform, avatar, Color.white, "Avatar"); av.rectTransform.Stretch(); av.raycastTarget = false;
            var ring = UIKit.Image(root, "ring", AvatarFrame.RarityColors[rarity], "Ring"); ring.rectTransform.Stretch(); ring.raycastTarget = false;
            var badge = UIKit.Image(root, "pill", UIKit.Hex("#1F2436"), "LevelBadge"); badge.raycastTarget = false;
            badge.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, -14), new Vector2(110, 48));
            var lv = UIKit.Text(badge.transform, level.ToString(), 30, Color.white, TextAlignmentOptions.Center, true, "Level"); lv.rectTransform.Stretch();
            var red = UIKit.Image(root, "circle", Skin.danger, "RedDot"); red.raycastTarget = false;
            red.rectTransform.Place(new Vector2(0.85f, 0.85f), Vector2.zero, new Vector2(40, 40));
            var redRing = UIKit.Image(red.transform, "ring_thin", Color.white, "Border"); redRing.rectTransform.Stretch(); redRing.raycastTarget = false;
            red.gameObject.SetActive(dot);
            var f = root.gameObject.AddComponent<AvatarFrame>();
            SetField(f, "m_Avatar", av); SetField(f, "m_Ring", ring); SetField(f, "m_Level", lv); SetField(f, "m_RedDot", red.gameObject);
            SetField(f, "m_Avatars", all);
            return f;
        }

        [BookShot("ch04-avatar", W = 960, H = 340, Scene = "Ch04_Avatar")]
        static void AvatarShot(Shot s)
        {
            var all = new Object[6]; var sps = new Sprite[6];
            for (int i = 0; i < 6; i++) { sps[i] = UIKit.Sp("avatar_" + (i + 1)); all[i] = sps[i]; }
            string[] names = { "普通", "优秀", "稀有", "史诗", "传说" };
            int[] lv = { 3, 18, 27, 45, 60 };
            for (int i = 0; i < 5; i++)
            {
                float x = -720 + i * 360;
                Avatar(s.Root, x, 60, "avatar_" + (i + 1), lv[i], i, i % 2 == 1, sps);
                T(s.Root, names[i], x, -130, 32, AvatarFrame.RarityColors[i], 300, true);
            }
            T(s.Root, "点击头像框可以轮换头像（场景 Ch04_Avatar）", 0, -240, 26, Muted, 1200);
        }
    }
}
