// 用代码生成 TMP 字体资源：和 Window › TextMeshPro › Font Asset Creator 做的是同一件事，
// 但参数写在代码里，可以重复执行、可以进 CI，换字库时不会漏设某个选项。
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace UGUIBook.Ch05
{
    public static class FontAssetTool
    {
        //#region create
        /// 生成“静态”字体资源：只包含 characters 里的字，构建后不再需要 TTF。
        /// samplingSize = Sampling Point Size，padding = Padding，atlasSize = Atlas Resolution
        public static TMP_FontAsset CreateStatic(Font source, string characters, string path,
            int samplingSize = 64, int padding = 6, int atlasSize = 1024,
            GlyphRenderMode renderMode = GlyphRenderMode.SDFAA)
        {
            AssetDatabase.DeleteAsset(path);   // 重新生成：先删旧的
            // 先按“动态”创建（只有动态模式能往里加字），加完字再切成静态
            var fa = TMP_FontAsset.CreateFontAsset(source, samplingSize, padding, renderMode,
                atlasSize, atlasSize, AtlasPopulationMode.Dynamic, true);
            fa.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(fa, path);
            // 图集纹理和默认材质是字体资源的“子资源”，要手动加进同一个 .asset
            fa.atlasTextures[0].name = fa.name + " Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = fa.name + " Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);

            // TMP 3.0.7 的小坑：m_AtlasTexture 是在第一次读 atlasTexture 属性时才赋值的，
            // 不先读一下，加字时一旦需要开第二张图集就会空引用
            _ = fa.atlasTexture;
            if (!fa.TryAddCharacters(characters, out string missing))
                Debug.LogWarning($"[{fa.name}] 源字体里没有这些字：{missing}");

            // 一张图集放不下时（Multi Atlas Textures 打开）会多出几页，也要存进资源
            for (int i = 1; i < fa.atlasTextures.Length; i++)
            {
                var t = fa.atlasTextures[i];
                if (t == null || AssetDatabase.Contains(t)) continue;
                t.name = $"{fa.name} Atlas {i}";
                AssetDatabase.AddObjectToAsset(t, fa);
            }

            fa.atlasPopulationMode = AtlasPopulationMode.Static;   // 编辑器下会同时清掉对 TTF 的引用
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            return fa;
        }
        //#endregion

        //#region menu
        /// 选中一个 TTF/OTF，再执行菜单：读取同目录下的 charset.txt（常用字表），生成同名静态字体
        [MenuItem("Assets/UGUIBook/用 charset.txt 生成静态字体")]
        static void CreateFromSelection()
        {
            var font = Selection.activeObject as Font;
            var fontPath = AssetDatabase.GetAssetPath(font);
            var charsetPath = Path.Combine(Path.GetDirectoryName(fontPath), "charset.txt");
            if (!File.Exists(charsetPath)) { Debug.LogError("找不到 " + charsetPath); return; }
            string chars = File.ReadAllText(charsetPath);
            string outPath = Path.ChangeExtension(fontPath, null) + " Static SDF.asset";
            var fa = CreateStatic(font, chars, outPath, 48, 5, 2048);
            Debug.Log($"生成 {outPath}：{fa.characterTable.Count} 个字，{fa.atlasTextures.Length} 张图集");
        }

        [MenuItem("Assets/UGUIBook/用 charset.txt 生成静态字体", true)]
        static bool Validate() => Selection.activeObject is Font;
        //#endregion
    }
}
