// 第 6 章 Sprite Swap 用的五张按钮图（Normal / Highlighted / Pressed / Selected / Disabled），代码生成
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UGUIBook.Ch06
{
    public static class Ch06Textures
    {
        const string Dir = "Assets/UGUIBook/Ch06/Textures";
        public static readonly string[] Names = { "btn_normal", "btn_highlighted", "btn_pressed", "btn_selected", "btn_disabled" };

        [MenuItem("UGUIBook/Ch06/重新生成按钮图")]
        static void Menu() => Ensure(true);

        public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{name}.png");

        public static void Ensure(bool force = false)
        {
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch06", "Textures");
            Make("btn_normal", force, (x, y) => Btn(x, y, "#7B6CF0", "#5241CF", "#3A2BA3", 0, null));
            Make("btn_highlighted", force, (x, y) => Btn(x, y, "#9A8DFF", "#6B5CEB", "#4A3BB8", 0, null));
            Make("btn_pressed", force, (x, y) => Btn(x, y, "#4A3AB8", "#5E4ED6", "#3A2BA3", 6, null));
            Make("btn_selected", force, (x, y) => Btn(x, y, "#7B6CF0", "#5241CF", "#3A2BA3", 0, "#FFC93C"));
            Make("btn_disabled", force, (x, y) => Btn(x, y, "#C9CCD8", "#AEB2C2", "#8F93A5", 0, null));
        }

        static void Make(string name, bool force, Func<int, int, Color> draw)
        {
            string path = $"{Dir}/{name}.png";
            if (!force && File.Exists(path)) return;
            var tex = new Texture2D(96, 96, TextureFormat.RGBA32, false);
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++) tex.SetPixel(x, y, draw(x, y));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.mipmapEnabled = false; ti.alphaIsTransparency = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spriteBorder = new Vector4(32, 32, 32, 32);
            ti.SaveAndReimport();
        }

        static Color Hex(string s) { ColorUtility.TryParseHtmlString(s, out var c); return c; }
        static float Cover(float d) => Mathf.Clamp01(0.5f - d);
        static float RR(float x, float y, float x0, float y0, float x1, float y1, float r)
        {
            float cx = Mathf.Clamp(x, x0 + r, x1 - r), cy = Mathf.Clamp(y, y0 + r, y1 - r);
            float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;
            if (x > x0 + r && x < x1 - r && y > y0 + r && y < y1 - r) d = -Mathf.Min(Mathf.Min(x - x0, x1 - x), Mathf.Min(y - y0, y1 - y));
            return d;
        }
        static Color Over(Color dst, Color src, float a)
        {
            a *= src.a; float o = a + dst.a * (1 - a);
            if (o <= 0) return Color.clear;
            var rgb = ((Vector4)src * a + (Vector4)dst * dst.a * (1 - a)) / o;
            return new Color(rgb.x, rgb.y, rgb.z, o);
        }

        // 立体按钮：底部一条深色“厚度”，按下态把面板下移、厚度消失
        static Color Btn(int ix, int iy, string top, string bottom, string lip, float press, string ring)
        {
            float x = ix + 0.5f, y = iy + 0.5f;
            var c = Color.clear;
            if (ring != null) c = Over(c, Hex(ring), Cover(RR(x, y, 0, 0, 96, 96, 24)));
            float inset = ring != null ? 5 : 0;
            c = Over(c, Hex(lip), Cover(RR(x, y, inset, inset, 96 - inset, 96 - inset - 6, 20)));
            float faceBottom = inset + 8 - press;
            var face = Color.Lerp(Hex(bottom), Hex(top), Mathf.InverseLerp(faceBottom, 96 - inset, y));
            c = Over(c, face, Cover(RR(x, y, inset, faceBottom, 96 - inset, 96 - inset - press, 20)));
            // 顶部高光
            c = Over(c, new Color(1, 1, 1, 0.18f), Cover(RR(x, y, inset + 8, 96 - inset - press - 22, 96 - inset - 8, 96 - inset - press - 6, 8)));
            return c;
        }
    }
}
