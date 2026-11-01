// 书中配图的“摄影棚”。每个 [BookShot] 方法在一个全新的空场景里搭 UI，ShotRunner 把它渲染成图片；
// 设置了 Scene 的还会把搭好的场景存成 .unity，读者打开就能运行、能点。
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class BookShotAttribute : Attribute
    {
        public readonly string Id;
        /// 输出图片的逻辑尺寸；实际像素 = W×Scale、H×Scale。Canvas 参考分辨率默认是 W×2 × H×2（960×540 → 1920×1080）
        public int W = 960, H = 540, Panels = 1;
        public float Scale = 1.5f;
        /// 同时把搭好的 UI 存成场景：Assets/UGUIBook/ChNN/Scenes/{Scene}.unity（多联图只存第 0 格）
        public string Scene;
        /// 存 PNG（纯色块多、需要透明背景时）；默认 JPG
        public bool Png;
        public BookShotAttribute(string id) { Id = id; }
    }

    /// 只生成场景、不出图
    [AttributeUsage(AttributeTargets.Method)]
    public class BookSceneAttribute : Attribute
    {
        public readonly string Name;
        public BookSceneAttribute(string name) { Name = name; }
    }

    public class Shot
    {
        public int Panel;                 // 多联图时当前是第几格（0 开始）
        public int W, H;                  // 逻辑尺寸（同 BookShot）
        public Camera Cam;                // Main Camera（截图时 Overlay 画布会临时改成 Screen Space - Camera 挂到它上面）
        public Canvas Canvas;             // 默认画布：Overlay + CanvasScaler(ScaleWithScreenSize)
        public CanvasScaler Scaler;
        public RectTransform Root;        // = Canvas 的 RectTransform，往这里加 UI
        public UISkin Skin => UISkin.Instance;
        internal readonly List<Action> Before = new List<Action>();   // 截图前执行（布局已完成）
        internal readonly List<Action> Annot = new List<Action>();    // 截图前在标注层画框、编号、箭头
        internal readonly List<Object> Temp = new List<Object>();
        internal RectTransform AnnotRoot;

        /// 背景：纯色（默认浅灰蓝 #EEF0F6）
        public void Background(Color c) { Cam.clearFlags = CameraClearFlags.SolidColor; Cam.backgroundColor = c; }

        /// 画布参考分辨率（默认 W×2 × H×2）
        public void Ref(float w, float h, float match = 0.5f) { Scaler.referenceResolution = new Vector2(w, h); Scaler.matchWidthOrHeight = match; }

        /// 截图前执行（此时 Canvas 已经按截图尺寸完成布局，可以读 rect、模拟状态）
        public void BeforeCapture(Action a) => Before.Add(a);

        /// 让控件显示成某种交互状态：Normal / Highlighted / Pressed / Selected / Disabled
        public void State(Selectable sel, string state) => Before.Add(() =>
        {
            var enumType = typeof(Selectable).GetNestedType("SelectionState", BindingFlags.NonPublic);
            var v = Enum.Parse(enumType, state);
            typeof(Selectable).GetMethod("DoStateTransition", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(sel, new[] { v, (object)true });
        });

        // ---------------- 资源 ----------------
        public static Sprite Sp(string name) => UISkin.Sprite(name);

        public T Load<T>(string assetPath) where T : Object
        {
            var o = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (o == null) throw new Exception("Asset not found: " + assetPath);
            return o;
        }

        /// 按 Shader 名找到（或创建）一个材质资源：Assets/UGUIBook/ChNN/Materials/{name}.mat。
        /// 存进场景的材质必须是资源文件，new Material() 出来的临时材质存不进场景
        public Material MatAsset(string chapterDir, string name, string shaderName)
        {
            string dir = $"Assets/UGUIBook/{chapterDir}/Materials";
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder($"Assets/UGUIBook/{chapterDir}", "Materials");
            string path = $"{dir}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            var sh = Shader.Find(shaderName);
            if (sh == null) throw new Exception("Shader not found: " + shaderName);
            if (m == null) { m = new Material(sh) { name = name }; AssetDatabase.CreateAsset(m, path); }
            else if (m.shader != sh) m.shader = sh;
            return m;
        }

        // ---------------- 标注层（只出现在配图里，不存进场景） ----------------

        /// 把 target 矩形上的归一化位置（0~1）换算成标注层坐标
        public Vector2 At(RectTransform target, Vector2 normalized)
        {
            var c = new Vector3[4];
            target.GetWorldCorners(c);   // 左下、左上、右上、右下
            var world = Vector3.Lerp(Vector3.Lerp(c[0], c[3], normalized.x), Vector3.Lerp(c[1], c[2], normalized.x), normalized.y);
            return WorldToAnnot(world);
        }

        internal Vector2 WorldToAnnot(Vector3 world)
        {
            var screen = RectTransformUtility.WorldToScreenPoint(Cam, world);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(AnnotRoot, screen, Cam, out var local);
            return local;
        }

        /// 给某个元素套一个彩色描边框，可带标签
        public void Box(RectTransform target, Color? color = null, string label = null, float pad = 6, bool labelBelow = false) => Annot.Add(() =>
        {
            var col = color ?? new Color32(0xE5, 0x48, 0x4D, 0xFF);
            var a = At(target, Vector2.zero); var b = At(target, Vector2.one);
            var img = UIKit.Image(AnnotRoot, "round_outline_8", col, "Box");
            img.raycastTarget = false;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0, 0);
            rt.anchoredPosition = Vector2.Min(a, b) - Vector2.one * pad;
            rt.sizeDelta = new Vector2(Mathf.Abs(b.x - a.x), Mathf.Abs(b.y - a.y)) + Vector2.one * pad * 2;
            if (label != null)
            {
                var pos = labelBelow ? new Vector2(Mathf.Min(a.x, b.x) - pad, Mathf.Min(a.y, b.y) - pad - 6)
                                     : new Vector2(Mathf.Min(a.x, b.x) - pad, Mathf.Max(a.y, b.y) + pad + 6);
                MakeTag(label, pos, col, labelBelow ? new Vector2(0, 1) : new Vector2(0, 0));
            }
        });

        /// 在元素某个位置放一个圆形编号（①②③ 对应正文）
        public void Badge(RectTransform target, string text, Vector2? where = null, Color? color = null) => Annot.Add(() =>
        {
            var p = At(target, where ?? new Vector2(0, 1));
            var c = UIKit.Image(AnnotRoot, "circle", color ?? new Color32(0xE5, 0x48, 0x4D, 0xFF), "Badge");
            c.raycastTarget = false;
            c.rectTransform.Place(new Vector2(0.5f, 0.5f), p, new Vector2(56, 56));
            var t = UIKit.Text(c.transform, text, 32, Color.white, TextAlignmentOptions.Center, true);
            t.rectTransform.Stretch();
        });

        /// 自由位置的文字标签（标注层坐标，原点在画面中心）
        public void Label(Vector2 pos, string text, Color? color = null, float size = 30, Vector2? pivot = null) =>
            Annot.Add(() => MakeTag(text, pos, color ?? new Color32(0x1F, 0x24, 0x36, 0xFF), pivot ?? new Vector2(0.5f, 0.5f), size));

        /// 贴着某个元素放标签：where 是元素上的归一化位置，offset 是额外偏移
        public void LabelAt(RectTransform target, Vector2 where, Vector2 offset, string text, Color? color = null, float size = 30) =>
            Annot.Add(() => MakeTag(text, At(target, where) + offset, color ?? new Color32(0x1F, 0x24, 0x36, 0xFF), new Vector2(0.5f, 0.5f), size));

        /// 箭头：从 from 指向 to（都是标注层坐标；配合 At() 使用）
        public void Arrow(Func<Vector2> from, Func<Vector2> to, Color? color = null, float width = 6) => Annot.Add(() =>
        {
            var col = color ?? new Color32(0xE5, 0x48, 0x4D, 0xFF);
            Vector2 a = from(), b = to(), d = b - a;
            float len = d.magnitude, ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            var line = UIKit.Image(AnnotRoot, null, col, "ArrowLine");
            line.raycastTarget = false;
            var rt = line.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = new Vector2(0, 0.5f);
            rt.anchoredPosition = a; rt.sizeDelta = new Vector2(Mathf.Max(0, len - width * 3), width);
            rt.localEulerAngles = new Vector3(0, 0, ang);
            var head = UIKit.Image(AnnotRoot, "play", col, "ArrowHead");
            head.raycastTarget = false;
            head.rectTransform.Place(new Vector2(0.5f, 0.5f), b - d.normalized * width * 2.2f, Vector2.one * width * 5);
            head.rectTransform.localEulerAngles = new Vector3(0, 0, ang);
        });

        /// 画出 RectTransform 的锚点（四个小三角）和轴心（小圆圈），讲锚点时用
        public void Anchors(RectTransform target, Color? color = null, bool pivot = true) => Annot.Add(() =>
        {
            var col = color ?? new Color32(0x1E, 0x88, 0xE5, 0xFF);
            var parent = (RectTransform)target.parent;
            var mins = target.anchorMin; var maxs = target.anchorMax;
            var pts = new[] { new Vector2(mins.x, mins.y), new Vector2(mins.x, maxs.y), new Vector2(maxs.x, maxs.y), new Vector2(maxs.x, mins.y) };
            float[] rot = { 45, 135, 225, 315 };   // 三角尖端朝向矩形内侧
            for (int i = 0; i < 4; i++)
            {
                var p = At(parent, pts[i]);
                var tri = UIKit.Image(AnnotRoot, "play", col, "Anchor");
                tri.raycastTarget = false;
                tri.rectTransform.Place(new Vector2(0.5f, 0.5f), p, new Vector2(30, 30));
                tri.rectTransform.pivot = new Vector2(0.15f, 0.5f);
                tri.rectTransform.anchoredPosition = p;
                tri.rectTransform.localEulerAngles = new Vector3(0, 0, rot[i] + 180);
            }
            if (pivot)
            {
                var pp = At(target, target.pivot);
                var ring = UIKit.Image(AnnotRoot, "ring", col, "Pivot");
                ring.raycastTarget = false;
                ring.rectTransform.Place(new Vector2(0.5f, 0.5f), pp, new Vector2(30, 30));
            }
        });

        internal void MakeTag(string text, Vector2 pos, Color col, Vector2 pivot, float size = 28)
        {
            var bg = UIKit.Image(AnnotRoot, "round_8", col, "Tag");
            bg.raycastTarget = false;
            var rt = bg.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f); rt.pivot = pivot; rt.anchoredPosition = pos;
            var t = UIKit.Text(rt, text, size, Color.white, TextAlignmentOptions.Center, true);
            t.enableWordWrapping = false;
            t.ForceMeshUpdate();
            var sz = t.GetPreferredValues(text);
            rt.sizeDelta = new Vector2(sz.x + size * 0.9f, size * 1.55f);
            t.rectTransform.Stretch();
        }
    }
}
