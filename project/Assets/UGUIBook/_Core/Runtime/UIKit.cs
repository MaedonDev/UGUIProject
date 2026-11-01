// 用代码搭 UI 的小工具集。内部仍然调用 Unity 自己的 DefaultControls / TMP_DefaultControls
// （也就是编辑器菜单 GameObject › UI 用的同一套代码），只是换成本书的精灵、字体和更适合 1920×1080 的尺寸。
// 配套场景和书中配图都是用它搭出来的；你在自己的项目里也可以照抄。
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Core
{
    public static class UIKit
    {
        public static UISkin Skin => UISkin.Instance;
        public static Sprite Sp(string name) => UISkin.Sprite(name);

        public static DefaultControls.Resources LegacyRes => new DefaultControls.Resources
        {
            standard = Sp("round_16"), background = Sp("round_16"), inputField = Sp("round_outline_16"),
            knob = Sp("circle"), checkmark = Sp("check"), dropdown = Sp("arrow_down"), mask = Sp("round_16"),
        };

        public static TMP_DefaultControls.Resources TmpRes => new TMP_DefaultControls.Resources
        {
            standard = Sp("round_16"), background = Sp("round_16"), inputField = Sp("round_outline_16"),
            knob = Sp("circle"), checkmark = Sp("check"), dropdown = Sp("arrow_down"), mask = Sp("round_16"),
        };

        // ---------------- 画布 ----------------

        /// 创建 Canvas（带 CanvasScaler + GraphicRaycaster），默认 Screen Space - Overlay、参考分辨率 1920×1080
        public static Canvas Canvas(string name = "Canvas", RenderMode mode = RenderMode.ScreenSpaceOverlay,
            float refW = 1920, float refH = 1080, float match = 0.5f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var c = go.AddComponent<Canvas>();
            c.renderMode = mode;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(refW, refH);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = match;
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        /// 场景里没有 EventSystem 就建一个（没有它，UI 收不到任何点击）
        public static EventSystem EnsureEventSystem()
        {
            var es = Object.FindObjectOfType<EventSystem>();
            if (es != null) return es;
            var go = new GameObject("EventSystem");
            es = go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            return es;
        }

        // ---------------- RectTransform ----------------

        public static RectTransform Node(Transform parent, string name = "Node")
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// 锚点和轴心都放在 anchor（0~1），再设位置和尺寸。例：Place(rt, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(600, 80)) = 顶部居中
        public static RectTransform Place(this RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Place(this RectTransform rt, float x, float y, float w, float h) =>
            rt.Place(new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(w, h));

        /// 撑满父节点，四边留白（左、上、右、下）
        public static RectTransform Stretch(this RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static RectTransform Size(this RectTransform rt, float w, float h) { rt.sizeDelta = new Vector2(w, h); return rt; }

        public static RectTransform Rt(this Component c) => (RectTransform)c.transform;

        // ---------------- 图形 ----------------

        /// 图片。精灵有九宫格边距时自动用 Sliced
        public static Image Image(Transform parent, string sprite, Color color, string name = "Image")
        {
            var rt = Node(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            if (!string.IsNullOrEmpty(sprite))
            {
                img.sprite = Sp(sprite);
                if (img.sprite != null && img.sprite.border != Vector4.zero) { img.type = UnityEngine.UI.Image.Type.Sliced; img.pixelsPerUnitMultiplier = 1; }
            }
            img.color = color;
            return img;
        }

        public static Image Panel(Transform parent, Color color, int radius = 16, string name = "Panel") =>
            Image(parent, radius > 0 ? "round_" + radius : null, color, name);

        /// 带软阴影的白色卡片。UGUI 按层级顺序绘制、父节点总是先于子节点，
        /// 所以阴影不能挂在卡片 Image 下面，结构是：Card(空节点) → Shadow(向外撑出一圈) → Bg(白底)，内容继续往 Card 下加
        public static RectTransform Card(Transform parent, string name = "Card", int radius = 16, bool shadow = true, Color? color = null)
        {
            var card = Node(parent, name);
            if (shadow)
            {
                var sh = Image(card, "shadow_16", new Color(0.1f, 0.12f, 0.25f, 0.22f), "Shadow");
                sh.raycastTarget = false;
                sh.rectTransform.Stretch(-20, -14, -20, -28);
            }
            var bg = Panel(card, color ?? Skin.surface, radius, "Bg");
            bg.rectTransform.Stretch();
            return card;
        }

        public static Image Icon(Transform parent, string sprite, float size, Color color, string name = null)
        {
            var img = Image(parent, sprite, color, name ?? "Icon_" + sprite);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            img.preserveAspect = true;
            img.raycastTarget = false;
            return img;
        }

        /// TextMeshPro 文字（中文字体已设为默认）
        public static TextMeshProUGUI Text(Transform parent, string text, float size = 36, Color? color = null,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false, string name = "Text")
        {
            var rt = Node(parent, name);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = bold && Skin.fontBold != null ? Skin.fontBold : Skin.font;
            t.text = text;
            t.fontSize = size;
            t.color = color ?? Skin.ink;
            t.alignment = align;
            t.raycastTarget = false;
            rt.sizeDelta = new Vector2(400, size * 1.6f);
            return t;
        }

        /// 旧版 Text（UnityEngine.UI.Text），用于对比讲解
        public static Text LegacyText(Transform parent, string text, int size = 36, Color? color = null,
            TextAnchor align = TextAnchor.MiddleCenter, string name = "Text (Legacy)")
        {
            var rt = Node(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Skin.legacyFont;
            t.text = text;
            t.fontSize = size;
            t.color = color ?? Skin.ink;
            t.alignment = align;
            t.raycastTarget = false;
            rt.sizeDelta = new Vector2(400, size * 1.6f);
            return t;
        }

        // ---------------- 控件 ----------------

        /// 按钮：圆角底 + TMP 文字（可选左侧图标）
        public static Button Button(Transform parent, string label, Color? bg = null, float w = 360, float h = 104,
            string icon = null, float fontSize = 38, string name = null)
        {
            var go = TMP_DefaultControls.CreateButton(TmpRes);
            go.name = name ?? "Button_" + label;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.GetComponent<Image>();
            img.sprite = Sp(h >= 90 ? "round_24" : "round_16");
            img.color = bg ?? Skin.primary;
            var t = go.GetComponentInChildren<TextMeshProUGUI>();
            t.font = Skin.fontBold != null ? Skin.fontBold : Skin.font;
            t.text = label;
            t.fontSize = fontSize;
            t.color = IsLight(img.color) ? Skin.ink : Color.white;
            if (icon != null)
            {
                var ic = Icon(rt, icon, fontSize * 1.1f, t.color);
                ic.rectTransform.anchorMin = ic.rectTransform.anchorMax = new Vector2(0, 0.5f);
                ic.rectTransform.anchoredPosition = new Vector2(h * 0.62f, 0);
                t.rectTransform.offsetMin = new Vector2(h * 0.62f + fontSize * 0.4f, 0);
            }
            return go.GetComponent<Button>();
        }

        /// 只有图标的圆形按钮
        public static Button IconButton(Transform parent, string icon, Color? bg = null, float size = 96, Color? iconColor = null)
        {
            var img = Image(parent, "circle", bg ?? Skin.primary, "IconButton_" + icon);
            img.rectTransform.sizeDelta = new Vector2(size, size);
            var b = img.gameObject.AddComponent<Button>();
            Icon(img.transform, icon, size * 0.5f, iconColor ?? (IsLight(img.color) ? Skin.ink : Color.white));
            return b;
        }

        public static Toggle Toggle(Transform parent, string label, bool on = false, float w = 360, float h = 64)
        {
            var go = DefaultControls.CreateToggle(LegacyRes);
            go.name = "Toggle_" + label;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var tg = go.GetComponent<Toggle>();
            var bg = (RectTransform)go.transform.Find("Background");
            bg.anchorMin = bg.anchorMax = new Vector2(0, 0.5f); bg.pivot = new Vector2(0, 0.5f);
            bg.anchoredPosition = Vector2.zero; bg.sizeDelta = new Vector2(h * 0.8f, h * 0.8f);
            var bgImg = bg.GetComponent<Image>(); bgImg.sprite = Sp("round_8"); bgImg.color = new Color(0.88f, 0.9f, 0.95f);
            var ck = (RectTransform)bg.Find("Checkmark");
            ck.anchorMin = Vector2.zero; ck.anchorMax = Vector2.one; ck.offsetMin = new Vector2(6, 6); ck.offsetMax = new Vector2(-6, -6);
            ck.GetComponent<Image>().color = Skin.primary;
            // 把默认的旧版 Text 换成 TMP
            var old = go.transform.Find("Label");
            Object.DestroyImmediate(old.gameObject);
            var t = Text(rt, label, h * 0.5f, Skin.ink, TextAlignmentOptions.MidlineLeft, name: "Label");
            t.rectTransform.Stretch(h * 0.8f + 18, 0, 0, 0);
            tg.isOn = on;
            return tg;
        }

        public static Slider Slider(Transform parent, float value = 0.5f, float w = 520, float h = 40, Color? fill = null)
        {
            var go = DefaultControls.CreateSlider(LegacyRes);
            go.name = "Slider";
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var s = go.GetComponent<Slider>();
            var bg = go.transform.Find("Background").GetComponent<Image>();
            bg.sprite = Sp("pill"); bg.color = new Color(0.86f, 0.88f, 0.93f);
            ((RectTransform)bg.transform).anchorMin = new Vector2(0, 0.3f); ((RectTransform)bg.transform).anchorMax = new Vector2(1, 0.7f);
            var fa = (RectTransform)go.transform.Find("Fill Area");
            fa.anchorMin = new Vector2(0, 0.3f); fa.anchorMax = new Vector2(1, 0.7f); fa.offsetMin = new Vector2(h * 0.25f, 0); fa.offsetMax = new Vector2(-h * 0.25f, 0);
            var f = fa.Find("Fill").GetComponent<Image>();
            f.sprite = Sp("pill"); f.color = fill ?? Skin.primary;
            var fr = (RectTransform)f.transform; fr.offsetMin = new Vector2(-h * 0.25f, 0); fr.offsetMax = new Vector2(h * 0.25f, 0);
            var ha = (RectTransform)go.transform.Find("Handle Slide Area");
            ha.offsetMin = new Vector2(h * 0.5f, 0); ha.offsetMax = new Vector2(-h * 0.5f, 0);
            var hd = ha.Find("Handle").GetComponent<Image>();
            ((RectTransform)hd.transform).sizeDelta = new Vector2(h, 0);
            hd.color = Color.white;
            var ring = Image(hd.transform, "ring_thin", fill ?? Skin.primary, "Ring"); ring.rectTransform.Stretch(); ring.raycastTarget = false;
            s.value = value;
            return s;
        }

        public static Scrollbar Scrollbar(Transform parent, Scrollbar.Direction dir = UnityEngine.UI.Scrollbar.Direction.BottomToTop, float thickness = 16)
        {
            var go = DefaultControls.CreateScrollbar(LegacyRes);
            go.name = "Scrollbar";
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            var sb = go.GetComponent<Scrollbar>();
            sb.SetDirection(dir, true);
            go.GetComponent<Image>().sprite = Sp("pill");
            go.GetComponent<Image>().color = new Color(0, 0, 0, 0.08f);
            var hd = go.transform.Find("Sliding Area/Handle").GetComponent<Image>();
            hd.sprite = Sp("pill"); hd.color = new Color(0, 0, 0, 0.3f);
            bool vertical = dir == UnityEngine.UI.Scrollbar.Direction.BottomToTop || dir == UnityEngine.UI.Scrollbar.Direction.TopToBottom;
            rt.sizeDelta = vertical ? new Vector2(thickness, 400) : new Vector2(400, thickness);
            return sb;
        }

        public static TMP_InputField Input(Transform parent, string placeholder, float w = 560, float h = 96, float fontSize = 34)
        {
            var go = TMP_DefaultControls.CreateInputField(TmpRes);
            go.name = "InputField";
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var img = go.GetComponent<Image>(); img.sprite = Sp("round_16"); img.color = Color.white;
            var outline = Image(rt, "round_outline_16", new Color(0.78f, 0.8f, 0.88f), "Outline");
            outline.rectTransform.Stretch(); outline.raycastTarget = false; outline.transform.SetAsFirstSibling();
            var f = go.GetComponent<TMP_InputField>();
            f.textViewport.offsetMin = new Vector2(28, 8); f.textViewport.offsetMax = new Vector2(-28, -8);
            foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                t.font = Skin.font; t.fontSize = fontSize; t.alignment = TextAlignmentOptions.MidlineLeft;
            }
            var ph = (TextMeshProUGUI)f.placeholder; ph.text = placeholder; ph.color = Skin.muted; ph.fontStyle = FontStyles.Normal;
            f.textComponent.color = Skin.ink;
            f.pointSize = fontSize;
            return f;
        }

        public static TMP_Dropdown Dropdown(Transform parent, IList<string> options, float w = 420, float h = 96, float fontSize = 34)
        {
            var go = TMP_DefaultControls.CreateDropdown(TmpRes);
            go.name = "Dropdown";
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var dd = go.GetComponent<TMP_Dropdown>();
            go.GetComponent<Image>().color = Color.white;
            var outline = Image(rt, "round_outline_16", new Color(0.78f, 0.8f, 0.88f), "Outline");
            outline.rectTransform.Stretch(); outline.raycastTarget = false; outline.transform.SetAsFirstSibling();
            foreach (var t in go.GetComponentsInChildren<TextMeshProUGUI>(true)) { t.font = Skin.font; t.fontSize = fontSize; t.color = Skin.ink; }
            ((RectTransform)dd.captionText.transform).offsetMin = new Vector2(28, 6);
            var arrow = (RectTransform)go.transform.Find("Arrow");
            arrow.sizeDelta = new Vector2(h * 0.4f, h * 0.4f); arrow.anchoredPosition = new Vector2(-h * 0.4f, 0);
            arrow.GetComponent<Image>().color = Skin.muted;
            var item = (RectTransform)dd.template.Find("Viewport/Content/Item");
            item.sizeDelta = new Vector2(0, h * 0.8f);
            ((RectTransform)item.parent).sizeDelta = new Vector2(0, h * 0.8f + 8);
            dd.template.sizeDelta = new Vector2(0, h * 0.8f * Mathf.Min(options.Count, 5) + 16);
            dd.ClearOptions();
            dd.AddOptions(new List<string>(options));
            return dd;
        }

        /// 滚动视图：返回 ScrollRect，内容挂在 sr.content 下
        public static ScrollRect ScrollView(Transform parent, bool vertical = true, bool horizontal = false, float w = 600, float h = 800)
        {
            var go = DefaultControls.CreateScrollView(LegacyRes);
            go.name = "Scroll View";
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(w, h);
            var sr = go.GetComponent<ScrollRect>();
            sr.vertical = vertical; sr.horizontal = horizontal;
            go.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            if (!horizontal && sr.horizontalScrollbar != null) { Object.DestroyImmediate(sr.horizontalScrollbar.gameObject); sr.horizontalScrollbar = null; }
            if (!vertical && sr.verticalScrollbar != null) { Object.DestroyImmediate(sr.verticalScrollbar.gameObject); sr.verticalScrollbar = null; }
            foreach (var sb in go.GetComponentsInChildren<Scrollbar>(true))
            {
                sb.GetComponent<Image>().color = new Color(0, 0, 0, 0.06f); sb.GetComponent<Image>().sprite = Sp("pill");
                sb.handleRect.GetComponent<Image>().color = new Color(0, 0, 0, 0.28f); sb.handleRect.GetComponent<Image>().sprite = Sp("pill");
            }
            // Viewport 默认用 Mask + Image；换成 RectMask2D 更省（第 9、20 章会解释原因）
            var vp = sr.viewport;
            Object.DestroyImmediate(vp.GetComponent<Mask>());
            Object.DestroyImmediate(vp.GetComponent<Image>());
            vp.gameObject.AddComponent<RectMask2D>();
            return sr;
        }

        // ---------------- 布局 ----------------

        public static VerticalLayoutGroup VLayout(this RectTransform rt, float spacing = 16, int pad = 0,
            TextAnchor align = TextAnchor.UpperCenter, bool expandWidth = true)
        {
            var g = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            g.spacing = spacing; g.padding = new RectOffset(pad, pad, pad, pad); g.childAlignment = align;
            g.childControlWidth = expandWidth; g.childForceExpandWidth = expandWidth;
            g.childControlHeight = false; g.childForceExpandHeight = false;
            return g;
        }

        public static HorizontalLayoutGroup HLayout(this RectTransform rt, float spacing = 16, int pad = 0,
            TextAnchor align = TextAnchor.MiddleCenter)
        {
            var g = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            g.spacing = spacing; g.padding = new RectOffset(pad, pad, pad, pad); g.childAlignment = align;
            g.childControlWidth = false; g.childForceExpandWidth = false;
            g.childControlHeight = false; g.childForceExpandHeight = false;
            return g;
        }

        public static GridLayoutGroup Grid(this RectTransform rt, Vector2 cell, Vector2 spacing, int pad = 0)
        {
            var g = rt.gameObject.AddComponent<GridLayoutGroup>();
            g.cellSize = cell; g.spacing = spacing; g.padding = new RectOffset(pad, pad, pad, pad);
            return g;
        }

        // ---------------- 杂项 ----------------

        public static bool IsLight(Color c) => c.a < 0.4f || (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) > 0.68f;

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex.StartsWith("#") ? hex : "#" + hex, out var c);
            return c;
        }
    }
}
