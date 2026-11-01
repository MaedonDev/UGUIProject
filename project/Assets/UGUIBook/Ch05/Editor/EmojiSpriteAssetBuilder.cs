// 生成本章的表情图集 + TMP Sprite Asset（图文混排 <sprite name="smile"> 用）。
// 表情脸是代码画的，物品图标取自 _Core/Sprites 的 PNG。菜单 UGUIBook › Ch05 › 重新生成表情 Sprite Asset。
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

namespace UGUIBook.Ch05
{
    public static class EmojiSpriteAssetBuilder
    {
        public const string Dir = "Assets/UGUIBook/Ch05/Sprites";
        public const string TexPath = Dir + "/emoji.png";
        public const string AssetPath = Dir + "/Emoji.asset";
        const int Cell = 128, Cols = 4;

        static readonly string[] Faces = { "smile", "laugh", "cry", "angry", "cool", "love", "wow", "sleepy" };
        static readonly (string name, string file, Color tint)[] Icons =
        {
            ("coin", "item_coin", Color.white), ("gem", "item_gem", Color.white),
            ("heart", "heart", new Color32(0xE5, 0x48, 0x4D, 255)), ("star", "star", new Color32(0xFF, 0xB0, 0x2E, 255)),
            ("potion", "item_potion_red", Color.white), ("sword", "item_sword", Color.white),
            ("trophy", "trophy", new Color32(0xF2, 0xB1, 0x27, 255)), ("gift", "gift", new Color32(0xEC, 0x4F, 0x9B, 255)),
        };

        [MenuItem("UGUIBook/Ch05/重新生成表情 Sprite Asset")]
        static void Menu() => Ensure(true);

        public static TMP_SpriteAsset Ensure(bool force = false)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(AssetPath);
            if (existing != null && !force) return existing;
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch05", "Sprites");

            // 1) 拼图集：4 列 × 4 行，每格 128
            var names = new List<string>();
            var atlas = new Texture2D(Cell * Cols, Cell * Cols, TextureFormat.RGBA32, false);
            atlas.SetPixels32(new Color32[atlas.width * atlas.height]);
            int index = 0;
            foreach (var f in Faces) { Blit(atlas, index++, (x, y) => DrawFace(f, x, y)); names.Add(f); }
            foreach (var ic in Icons)
            {
                var src = new Texture2D(2, 2);
                src.LoadImage(File.ReadAllBytes($"Assets/UGUIBook/_Core/Sprites/{ic.file}.png"));
                int sw = src.width, sh = src.height;
                Blit(atlas, index++, (x, y) =>
                {
                    // 图标缩到 88%，四周留点空，排在文字里不显得拥挤
                    float u = (x + 0.5f) / Cell, v = (y + 0.5f) / Cell;
                    u = (u - 0.5f) / 0.88f + 0.5f; v = (v - 0.5f) / 0.88f + 0.5f;
                    if (u < 0 || u > 1 || v < 0 || v > 1) return Color.clear;
                    return src.GetPixelBilinear(u, v) * ic.tint;
                });
                UnityEngine.Object.DestroyImmediate(src);
                names.Add(ic.name);
            }
            atlas.Apply();
            File.WriteAllBytes(TexPath, atlas.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(atlas);
            AssetDatabase.ImportAsset(TexPath, ImportAssetOptions.ForceSynchronousImport);
            var ti = (TextureImporter)AssetImporter.GetAtPath(TexPath);
            ti.textureType = TextureImporterType.Default;    // TMP 只要纹理和每个格子的矩形，不需要 Unity Sprite
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexPath);

            // 2) 建 Sprite Asset
            AssetDatabase.DeleteAsset(AssetPath);
            var sa = BuildSpriteAsset(tex, names);
            AssetDatabase.SaveAssets();
            return sa;
        }

        //#region build
        static TMP_SpriteAsset BuildSpriteAsset(Texture2D tex, List<string> names)
        {
            var sa = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            AssetDatabase.CreateAsset(sa, AssetPath);
            // 版本号字段的 setter 是 internal，用 SerializedObject 写；不写的话 TMP 会把它当旧格式“升级”
            var so = new SerializedObject(sa);
            so.FindProperty("m_Version").stringValue = "1.1.0";
            so.ApplyModifiedPropertiesWithoutUndo();
            sa.hashCode = TMP_TextUtilities.GetSimpleHashCode(sa.name);
            sa.spriteSheet = tex;

            for (int i = 0; i < names.Count; i++)
            {
                int col = i % Cols, row = i / Cols;
                // 纹理坐标原点在左下角：第 0 行放在最上面
                var rect = new GlyphRect(col * Cell, tex.height - (row + 1) * Cell, Cell, Cell);
                // 度量：宽高 128，水平 Bearing 0，BearingY = 基线以上 84%，前进宽度 128
                var glyph = new TMP_SpriteGlyph { index = (uint)i, glyphRect = rect, scale = 1f,
                    metrics = new GlyphMetrics(Cell, Cell, 0, Cell * 0.84f, Cell) };
                sa.spriteGlyphTable.Add(glyph);
                var ch = new TMP_SpriteCharacter(0xFFFE, glyph) { name = names[i], scale = 1.2f };   // 比字略大一点更好看
                sa.spriteCharacterTable.Add(ch);
            }

            // 材质：TextMeshPro/Sprite Shader，作为子资源存进同一个 .asset
            var mat = new Material(Shader.Find("TextMeshPro/Sprite")) { name = "Emoji Material", hideFlags = HideFlags.HideInHierarchy };
            mat.SetTexture(ShaderUtilities.ID_MainTex, tex);
            AssetDatabase.AddObjectToAsset(mat, sa);
            sa.material = mat;
            sa.UpdateLookupTables();
            EditorUtility.SetDirty(sa);
            return sa;
        }
        //#endregion

        static void Blit(Texture2D atlas, int index, Func<int, int, Color> draw)
        {
            int ox = (index % Cols) * Cell, oy = atlas.height - (index / Cols + 1) * Cell;
            for (int y = 0; y < Cell; y++)
                for (int x = 0; x < Cell; x++)
                    atlas.SetPixel(ox + x, oy + y, draw(x, y));
        }

        // ---------------- 画表情（有符号距离 + 抗锯齿） ----------------
        static float Cov(float d) => Mathf.Clamp01(0.5f - d);
        static Color Over(Color dst, Color src, float a)
        {
            a *= src.a;
            float outA = a + dst.a * (1 - a);
            if (outA <= 0) return Color.clear;
            var rgb = ((Vector4)src * a + (Vector4)dst * dst.a * (1 - a)) / outA;
            return new Color(rgb.x, rgb.y, rgb.z, outA);
        }
        static float Circle(Vector2 p, Vector2 c, float r) => Vector2.Distance(p, c) - r;
        static float Seg(Vector2 p, Vector2 a, Vector2 b, float w)
        {
            var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
            return Vector2.Distance(p, a + ab * t) - w;
        }
        // 圆弧：圆心 c、半径 r、线宽 w，只保留 y 方向 dir 一侧（dir=-1 下半圆，+1 上半圆）
        static float Arc(Vector2 p, Vector2 c, float r, float w, int dir)
        {
            var d = p - c;
            if (Mathf.Sign(d.y) != dir) { var e1 = c + new Vector2(-r, 0); var e2 = c + new Vector2(r, 0); return Mathf.Min(Vector2.Distance(p, e1), Vector2.Distance(p, e2)) - w; }
            return Mathf.Abs(d.magnitude - r) - w;
        }
        static float Heart(Vector2 p, Vector2 c, float s)
        {
            // 两个圆 + 一个倒三角拼成的心形
            var q = (p - c) / s;
            float d1 = Circle(q, new Vector2(-0.5f, 0.3f), 0.55f), d2 = Circle(q, new Vector2(0.5f, 0.3f), 0.55f);
            var tip = new Vector2(0, -1f);
            var nl = new Vector2(-1.12f, -1.02f).normalized; var nr = new Vector2(1.12f, -1.02f).normalized;
            float tri = Mathf.Max(Mathf.Max(Vector2.Dot(nl, q - tip), Vector2.Dot(nr, q - tip)), q.y - 0.3f);
            return Mathf.Min(Mathf.Min(d1, d2), tri) * s;
        }

        static Color DrawFace(string kind, int ix, int iy)
        {
            var p = new Vector2(ix + 0.5f, iy + 0.5f);
            var c = new Vector2(64, 64);
            var col = Color.clear;
            var ink = new Color32(0x5A, 0x34, 0x12, 255);
            Color skin = kind == "angry" ? new Color32(0xFF, 0x7A, 0x59, 255) : new Color32(0xFF, 0xC9, 0x3C, 255);
            Color rim = kind == "angry" ? new Color32(0xD9, 0x48, 0x2E, 255) : new Color32(0xE0, 0x9A, 0x1A, 255);
            col = Over(col, rim, Cov(Circle(p, c, 58)));
            col = Over(col, skin, Cov(Circle(p, c, 53)));
            Vector2 le = new Vector2(44, 74), re = new Vector2(84, 74);
            switch (kind)
            {
                case "smile":
                    col = Over(col, ink, Cov(Circle(p, le, 7))); col = Over(col, ink, Cov(Circle(p, re, 7)));
                    col = Over(col, ink, Cov(Arc(p, new Vector2(64, 56), 24, 3.5f, -1)));
                    break;
                case "laugh":
                    col = Over(col, ink, Cov(Arc(p, le - new Vector2(0, 4), 9, 3.2f, 1))); col = Over(col, ink, Cov(Arc(p, re - new Vector2(0, 4), 9, 3.2f, 1)));
                    {
                        float mouth = Mathf.Max(Circle(p, new Vector2(64, 50), 26), p.y - 50);
                        col = Over(col, new Color32(0x8A, 0x2B, 0x1E, 255), Cov(mouth));
                        col = Over(col, new Color32(0xFF, 0x6B, 0x6B, 255), Cov(Mathf.Max(Circle(p, new Vector2(64, 30), 13), mouth)));
                    }
                    break;
                case "cry":
                    col = Over(col, ink, Cov(Circle(p, le, 7))); col = Over(col, ink, Cov(Circle(p, re, 7)));
                    col = Over(col, ink, Cov(Arc(p, new Vector2(64, 30), 20, 3.5f, 1)));
                    col = Over(col, new Color32(0x4F, 0xB3, 0xF6, 255), Cov(Circle(p, new Vector2(38, 54), 9)));
                    col = Over(col, new Color32(0x4F, 0xB3, 0xF6, 255), Cov(Seg(p, new Vector2(38, 54), new Vector2(38, 68), 4)));
                    break;
                case "angry":
                    col = Over(col, ink, Cov(Circle(p, le - new Vector2(0, 4), 6))); col = Over(col, ink, Cov(Circle(p, re - new Vector2(0, 4), 6)));
                    col = Over(col, ink, Cov(Seg(p, new Vector2(30, 92), new Vector2(54, 82), 4)));
                    col = Over(col, ink, Cov(Seg(p, new Vector2(98, 92), new Vector2(74, 82), 4)));
                    col = Over(col, ink, Cov(Arc(p, new Vector2(64, 30), 18, 3.5f, 1)));
                    break;
                case "cool":
                    {
                        var glass = new Color32(0x1F, 0x24, 0x36, 255);
                        col = Over(col, glass, Cov(Mathf.Max(Circle(p, new Vector2(44, 74), 15), p.y - 80)));
                        col = Over(col, glass, Cov(Mathf.Max(Circle(p, new Vector2(84, 74), 15), p.y - 80)));
                        col = Over(col, glass, Cov(Seg(p, new Vector2(20, 80), new Vector2(108, 80), 3)));
                        col = Over(col, ink, Cov(Arc(p, new Vector2(64, 52), 18, 3.5f, -1)));
                    }
                    break;
                case "love":
                    col = Over(col, new Color32(0xE5, 0x48, 0x4D, 255), Cov(Heart(p, le, 11)));
                    col = Over(col, new Color32(0xE5, 0x48, 0x4D, 255), Cov(Heart(p, re, 11)));
                    col = Over(col, ink, Cov(Arc(p, new Vector2(64, 52), 20, 3.5f, -1)));
                    break;
                case "wow":
                    col = Over(col, ink, Cov(Circle(p, le, 8))); col = Over(col, ink, Cov(Circle(p, re, 8)));
                    col = Over(col, new Color32(0x8A, 0x2B, 0x1E, 255), Cov(Circle(p, new Vector2(64, 38), 12)));
                    break;
                case "sleepy":
                    col = Over(col, ink, Cov(Seg(p, le - new Vector2(10, 0), le + new Vector2(10, 0), 3)));
                    col = Over(col, ink, Cov(Seg(p, re - new Vector2(10, 0), re + new Vector2(10, 0), 3)));
                    col = Over(col, ink, Cov(Circle(p, new Vector2(64, 40), 6)));
                    col = Over(col, new Color(0.55f, 0.8f, 1f, 0.9f), Cov(Circle(p, new Vector2(92, 46), 10)));
                    break;
            }
            return col;
        }
    }
}
