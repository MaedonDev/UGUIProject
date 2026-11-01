// 配套精灵的导入设置：_Core/Sprites 下的 PNG 一律导入为 Sprite，九宫格边距读 sprites.json（由 tools/gen_sprites.py 生成）
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UGUIBook.Core
{
    public class BookSpriteImporter : AssetPostprocessor
    {
        const string Dir = "Assets/UGUIBook/_Core/Sprites/";
        static Dictionary<string, Vector4> s_Borders;

        [System.Serializable] class Entry { public string name; public int[] border; }

        static Dictionary<string, Vector4> Borders()
        {
            if (s_Borders != null) return s_Borders;
            s_Borders = new Dictionary<string, Vector4>();
            var path = Dir + "sprites.json";
            if (!File.Exists(path)) return s_Borders;
            // sprites.json 形如 {"round_8": {"border": [10,10,10,10]}, ...}；JsonUtility 不支持字典，这里手动解析
            var text = File.ReadAllText(path);
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(text,
                         "\"(\\w+)\":\\s*\\{\\s*\"border\":\\s*\\[\\s*(\\d+),\\s*(\\d+),\\s*(\\d+),\\s*(\\d+)\\s*\\]"))
            {
                s_Borders[m.Groups[1].Value] = new Vector4(int.Parse(m.Groups[2].Value), int.Parse(m.Groups[3].Value),
                    int.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value));
            }
            return s_Borders;
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Dir) || !assetPath.EndsWith(".png")) return;
            var ti = (TextureImporter)assetImporter;
            var name = Path.GetFileNameWithoutExtension(assetPath);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.sRGBTexture = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.filterMode = FilterMode.Bilinear;
            bool tiles = name == "checker" || name == "stripes" || name == "noise";
            ti.wrapMode = tiles ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            ti.isReadable = name == "noise";
            // 精灵网格用 FullRect：Tiled 平铺、自定义 Shader 读 UV 都更省心
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            ti.SetTextureSettings(settings);
            if (Borders().TryGetValue(name, out var b)) ti.spriteBorder = b;
        }
    }
}
