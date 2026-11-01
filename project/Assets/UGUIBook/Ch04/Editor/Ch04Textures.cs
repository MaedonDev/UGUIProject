// 第 4 章用到的几张图片：全部由代码画出来、存成 PNG，再用代码设置导入参数。
// 菜单 UGUIBook › Ch04 › 重新生成本章纹理 可以随时重建。
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UGUIBook.Ch04
{
    public static class Ch04Textures
    {
        public const string Dir = "Assets/UGUIBook/Ch04/Textures";

        [MenuItem("UGUIBook/Ch04/重新生成本章纹理")]
        public static void Regenerate() => Ensure(true);

        /// 缺哪张生成哪张（force = 全部重画）
        public static void Ensure(bool force = false)
        {
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch04", "Textures");
            Make("dialog_frame", 160, 160, DrawDialogFrame, force, new Vector4(54, 54, 54, 54));
            Make("tile_pattern", 64, 64, DrawTile, force, Vector4.zero, wrap: TextureWrapMode.Repeat);
            Make("bar_fill", 32, 32, DrawBarFill, force, Vector4.zero);
            Make("star_button", 256, 256, DrawStar, force, Vector4.zero, readable: true);
            Make("ring_button", 256, 256, DrawRing, force, Vector4.zero, readable: true);
            Make("hills_far", 512, 160, (x, y) => Hills(x, y, 512, 92, new[] { 2f, 5f, 9f }, new[] { 22f, 10f, 4f }, Hex("#8E9AD6")), force, Vector4.zero, wrap: TextureWrapMode.Repeat, sprite: false);
            Make("hills_near", 512, 160, (x, y) => Hills(x, y, 512, 58, new[] { 3f, 7f, 13f }, new[] { 20f, 9f, 3f }, Hex("#4C5AA6")), force, Vector4.zero, wrap: TextureWrapMode.Repeat, sprite: false);
            Make("sky", 8, 256, (x, y) => Color.Lerp(Hex("#FFD6A5"), Hex("#6C7BD9"), y / 255f), force, Vector4.zero, sprite: false);
        }

        public static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{Dir}/{name}.png");
        public static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Dir}/{name}.png");

        static void Make(string name, int w, int h, Func<int, int, Color> draw, bool force, Vector4 border,
            bool readable = false, TextureWrapMode wrap = TextureWrapMode.Clamp, bool sprite = true)
        {
            string path = $"{Dir}/{name}.png";
            if (!force && File.Exists(path)) return;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, draw(x, y));
            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            if (sprite) ApplySpriteImport(path, border, 100, readable, wrap);
            else ApplyTextureImport(path, wrap);
        }

        //#region import
        /// 用代码设置纹理导入参数，等价于在 Inspector 里把 Texture Type 改成 Sprite (2D and UI)、
        /// 填 Pixels Per Unit、Wrap Mode、Read/Write，以及在 Sprite Editor 里拖九宫格边框
        public static void ApplySpriteImport(string path, Vector4 border, float ppu = 100, bool readable = false,
            TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;            // Sprite (2D and UI)
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.mipmapEnabled = false;                               // UI 基本 1:1 显示，不需要 Mipmap
            ti.alphaIsTransparency = true;
            ti.wrapMode = wrap;
            ti.isReadable = readable;                               // 只有做像素级点击检测时才需要
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spriteBorder = border;                               // 九宫格边距（像素）：x=左 y=下 z=右 w=上
            var st = new TextureImporterSettings();
            ti.ReadTextureSettings(st);
            st.spriteMeshType = SpriteMeshType.FullRect;            // Tiled 和自定义 Shader 用 FullRect 更省心
            ti.SetTextureSettings(st);
            ti.SaveAndReimport();
        }
        //#endregion

        static void ApplyTextureImport(string path, TextureWrapMode wrap)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Default;           // RawImage 直接用 Texture，不必是 Sprite
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.wrapModeU = wrap;                                    // 横向滚动只需要 U 方向 Repeat
            ti.wrapModeV = TextureWrapMode.Clamp;                   // V 方向若也 Repeat，双线性过滤会把底部像素“渗”到顶边，出现一条细线
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        // ---------------- 画图小工具（有符号距离 + 1 像素抗锯齿） ----------------
        static Color Hex(string s) { ColorUtility.TryParseHtmlString(s, out var c); return c; }
        static float Cover(float d) => Mathf.Clamp01(0.5f - d);   // d<0 在形状内
        static Color Over(Color dst, Color src, float a)
        {
            a *= src.a;
            float outA = a + dst.a * (1 - a);
            if (outA <= 0) return new Color(0, 0, 0, 0);
            var rgb = ((Vector4)src * a + (Vector4)dst * dst.a * (1 - a)) / outA;
            return new Color(rgb.x, rgb.y, rgb.z, outA);
        }
        static float RoundRect(float x, float y, float x0, float y0, float x1, float y1, float r)
        {
            float cx = Mathf.Clamp(x, x0 + r, x1 - r), cy = Mathf.Clamp(y, y0 + r, y1 - r);
            float dx = x - cx, dy = y - cy;
            bool inside = x > x0 && x < x1 && y > y0 && y < y1;
            float dist = Mathf.Sqrt(dx * dx + dy * dy) - r;
            if (inside && dx == 0 && dy == 0) dist = -Mathf.Min(Mathf.Min(x - x0, x1 - x), Mathf.Min(y - y0, y1 - y));
            return dist;
        }

        // 对话框边框：深棕外框 + 金边 + 米色底 + 四角菱形花饰（花饰全在 54px 的边距内，拉伸时不会变形）
        static Color DrawDialogFrame(int ix, int iy)
        {
            const float k = 96f / 160f;                       // 按 96×96 设计，输出 160×160
            float x = (ix + 0.5f) * k, y = (iy + 0.5f) * k;
            float Cover(float d) => Mathf.Clamp01(0.5f - d / k);
            var c = new Color(0, 0, 0, 0);
            c = Over(c, Hex("#5A3D22"), Cover(RoundRect(x, y, 0, 0, 96, 96, 16)));
            c = Over(c, Hex("#E0A94A"), Cover(RoundRect(x, y, 4, 4, 92, 92, 12)));
            c = Over(c, Hex("#FBF3DD"), Cover(RoundRect(x, y, 8, 8, 88, 88, 9)));
            // 内侧细线：沿边一圈，拉伸后依然是细线
            float line = Mathf.Abs(RoundRect(x, y, 13, 13, 83, 83, 5)) - 0.8f;
            c = Over(c, new Color(0.35f, 0.24f, 0.13f, 0.45f), Cover(line));
            foreach (var p in new[] { new Vector2(16, 16), new Vector2(80, 16), new Vector2(16, 80), new Vector2(80, 80) })
            {
                float d = Mathf.Abs(x - p.x) + Mathf.Abs(y - p.y) - 7f;
                c = Over(c, Hex("#C0392B"), Cover(d));
                c = Over(c, Hex("#F6D27A"), Cover(Mathf.Abs(x - p.x) + Mathf.Abs(y - p.y) - 3f));
            }
            return c;
        }

        // 无缝平铺花纹：64×64，上下左右对称，所以首尾能接上
        static Color DrawTile(int ix, int iy)
        {
            float x = ix + 0.5f, y = iy + 0.5f;
            var c = Hex("#3A3F6B");
            float d = Mathf.Abs(x - 32) + Mathf.Abs(y - 32) - 22f;
            c = Over(c, Hex("#4E558F"), Cover(d));
            c = Over(c, Hex("#FFB547"), Cover(Vector2.Distance(new Vector2(x, y), new Vector2(32, 32)) - 5f));
            foreach (var p in new[] { new Vector2(0, 0), new Vector2(64, 0), new Vector2(0, 64), new Vector2(64, 64) })
                c = Over(c, Hex("#8C93D9"), Cover(Vector2.Distance(new Vector2(x, y), p) - 4f));
            return c;
        }

        // 血条填充：白色、带一点竖直方向的高光（染色后还能看出体积）
        static Color DrawBarFill(int ix, int iy)
        {
            float t = iy / 31f;
            float v = Mathf.Lerp(0.78f, 1f, Mathf.SmoothStep(0, 1, t));
            if (iy > 20 && iy < 26) v = 1f;
            return new Color(v, v, v, 1);
        }

        static float StarDist(float x, float y, float cx, float cy, float rOut, float rIn)
        {
            // 五角星多边形的有符号距离（逐边求点到线段距离 + 奇偶规则判断内外）
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = Mathf.PI / 2 + i * Mathf.PI / 5;
                float r = i % 2 == 0 ? rOut : rIn;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            var p = new Vector2(x, y);
            float best = float.MaxValue; bool inside = false;
            for (int i = 0, j = 9; i < 10; j = i++)
            {
                Vector2 a = pts[j], b = pts[i], ab = b - a;
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
                best = Mathf.Min(best, Vector2.Distance(p, a + ab * t));
                if ((a.y > y) != (b.y > y) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y) + a.x) inside = !inside;
            }
            return inside ? -best : best;
        }

        static Color DrawStar(int ix, int iy)
        {
            float x = ix + 0.5f, y = iy + 0.5f;
            var c = new Color(0, 0, 0, 0);
            float d = StarDist(x, y, 128, 120, 124, 52);
            c = Over(c, Hex("#C46A00"), Cover(d - 4));
            c = Over(c, Hex("#FFB02E"), Cover(d + 4));
            c = Over(c, Hex("#FFD36B"), Cover(StarDist(x, y, 128, 128, 70, 30)));
            return c;
        }

        static Color DrawRing(int ix, int iy)
        {
            float x = ix + 0.5f, y = iy + 0.5f;
            float r = Vector2.Distance(new Vector2(x, y), new Vector2(128, 128));
            var c = new Color(0, 0, 0, 0);
            float ring = Mathf.Abs(r - 92) - 34;              // 半径 58~126 是圆环
            c = Over(c, Hex("#1E8C6B"), Cover(ring));
            c = Over(c, Hex("#35C49A"), Cover(Mathf.Abs(r - 92) - 28));
            return c;
        }

        // 可横向无缝平铺的山丘剪影：正弦波频率取整数，保证 x=0 和 x=W 处高度一致
        static Color Hills(int ix, int iy, int w, float baseH, float[] freq, float[] amp, Color col)
        {
            float x = ix + 0.5f, y = iy + 0.5f, h = baseH;
            for (int i = 0; i < freq.Length; i++) h += amp[i] * Mathf.Sin(2 * Mathf.PI * freq[i] * x / w + i * 1.7f);
            float a = Mathf.Clamp01(h - y + 0.5f);
            return new Color(col.r, col.g, col.b, a);
        }
    }
}
