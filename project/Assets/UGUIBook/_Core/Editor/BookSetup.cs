// 配套工程一键设置：导入 TMP 基础资源、生成中文 SDF 字体、设为 TMP 默认字体、生成 UISkin。
// 菜单 UGUIBook › Setup Project；批处理：-executeMethod UGUIBook.Core.BookSetup.Run
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace UGUIBook.Core
{
    public static class BookSetup
    {
        const string Core = "Assets/UGUIBook/_Core";
        const string TmpEssentials = "Packages/com.unity.textmeshpro/Package Resources/TMP Essential Resources.unitypackage";

        public static void Run()
        {
            EnsureSetup();
            EditorApplication.Exit(0);
        }

        [MenuItem("UGUIBook/Setup Project")]
        public static void EnsureSetup()
        {
            if (!AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            {
                AssetDatabase.ImportPackage(TmpEssentials, false);
                AssetDatabase.Refresh();
            }

            var regular = EnsureFont("NotoSansSC-Regular");
            var bold = EnsureFont("NotoSansSC-Bold");

            var settings = AssetDatabase.LoadAssetAtPath<TMP_Settings>("Assets/TextMesh Pro/Resources/TMP Settings.asset");
            if (settings != null && regular != null)
            {
                var so = new SerializedObject(settings);
                so.FindProperty("m_defaultFontAsset").objectReferenceValue = regular;
                so.FindProperty("m_defaultFontAssetPath").stringValue = "";
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            if (!AssetDatabase.IsValidFolder(Core + "/Resources")) AssetDatabase.CreateFolder(Core, "Resources");
            var skinPath = Core + "/Resources/UISkin.asset";
            var skin = AssetDatabase.LoadAssetAtPath<UISkin>(skinPath);
            if (skin == null) { skin = ScriptableObject.CreateInstance<UISkin>(); AssetDatabase.CreateAsset(skin, skinPath); }
            skin.font = regular;
            skin.fontBold = bold;
            skin.legacyFont = AssetDatabase.LoadAssetAtPath<Font>(Core + "/Fonts/NotoSansSC-Regular.ttf");
            skin.sprites = AssetDatabase.FindAssets("t:Sprite", new[] { Core + "/Sprites" })
                .Select(g => AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(s => s != null).OrderBy(s => s.name).ToArray();
            EditorUtility.SetDirty(skin);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UGUIBook] setup done: {skin.sprites.Length} sprites, font={regular}");
        }

        /// 用 TTF 生成动态 SDF 字体资源（第 5 章讲了每个参数的含义）
        static TMP_FontAsset EnsureFont(string name)
        {
            string path = $"{Core}/Fonts/{name} SDF.asset";
            var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (fa != null) return fa;
            var font = AssetDatabase.LoadAssetAtPath<Font>($"{Core}/Fonts/{name}.ttf");
            if (font == null) { Debug.LogError("[UGUIBook] missing font " + name); return null; }
            // 采样 64、边距 6（约 1/10）、SDFAA、1024 图集，动态填充 + 多图集
            fa = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
            fa.name = name + " SDF";
            AssetDatabase.CreateAsset(fa, path);
            fa.atlasTextures[0].name = name + " SDF Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = name + " SDF Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
            AssetDatabase.SaveAssets();
            return fa;
        }

        /// 清空动态字体里拍图时累积的字形（打包配套工程前执行，减小体积）
        [MenuItem("UGUIBook/Clear Dynamic Font Data")]
        public static void ClearFonts()
        {
            foreach (var g in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { Core + "/Fonts" }))
            {
                var fa = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(g));
                fa.ClearFontAssetData(true);
                EditorUtility.SetDirty(fa);
            }
            AssetDatabase.SaveAssets();
        }

        public static void ClearFontsAndExit() { ClearFonts(); EditorApplication.Exit(0); }
    }
}
