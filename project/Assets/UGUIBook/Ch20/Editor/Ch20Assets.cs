// 第 20 章实验用到的资源：一份 SpriteAtlas（把几张图标/底板打进同一张图集），一个“看起来不动”的 Animator。
// 菜单 UGUIBook › Ch20 › Create Lab Assets 也能手动执行。
using System.IO;
using System.Linq;
using UGUIBook.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    public static class Ch20Assets
    {
        public const string Dir = "Assets/UGUIBook/Ch20";
        public const string AtlasDir = Dir + "/Sprites/Atlas";
        public const string AtlasPath = Dir + "/Sprites/Ch20Icons.spriteatlas";
        static readonly string[] AtlasSprites =
            { "star", "heart", "round_16", "item_sword", "item_shield", "item_potion_red", "item_gem", "item_coin", "item_apple" };

        [MenuItem("UGUIBook/Ch20/Create Lab Assets")]
        public static void EnsureAll()
        {
            EnsureAtlas();
            EnsureIdleController();
            EnsurePulseController();
        }

        /// 把 _Core 里的几张 PNG 复制一份到 Ch20/Sprites/Atlas，导入为精灵（不动 _Core 的原图）
        static void CopySprites()
        {
            Directory.CreateDirectory(AtlasDir);
            foreach (var n in AtlasSprites)
            {
                string dst = $"{AtlasDir}/{n}.png";
                if (!File.Exists(dst)) File.Copy($"Assets/UGUIBook/_Core/Sprites/{n}.png", dst);
            }
            AssetDatabase.Refresh();
            foreach (var n in AtlasSprites)
            {
                var src = (TextureImporter)AssetImporter.GetAtPath($"Assets/UGUIBook/_Core/Sprites/{n}.png");
                var ti = (TextureImporter)AssetImporter.GetAtPath($"{AtlasDir}/{n}.png");
                if (ti.textureType == TextureImporterType.Sprite && ti.spriteBorder == src.spriteBorder) continue;
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.mipmapEnabled = false;
                ti.alphaIsTransparency = true;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.spriteBorder = src.spriteBorder;          // 九宫格边距照抄原图
                ti.SaveAndReimport();
            }
        }

        //#region atlas
        /// 用代码创建 SpriteAtlas（V1 格式）。等价于 Project 窗口右键 Create › 2D › Sprite Atlas，再把精灵拖进 Objects for Packing
        public static SpriteAtlas EnsureAtlas()
        {
            CopySprites();
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(AtlasPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, AtlasPath);
                atlas.SetPackingSettings(new SpriteAtlasPackingSettings
                {
                    blockOffset = 1, padding = 4,
                    enableRotation = false,      // UI 精灵不要旋转打包，否则九宫格/Filled 会出问题
                    enableTightPacking = false,  // UI 用矩形网格（FullRect），紧密打包没有意义
                });
                atlas.SetTextureSettings(new SpriteAtlasTextureSettings
                {
                    generateMipMaps = false, sRGB = true, filterMode = FilterMode.Bilinear,
                });
                // 直接添加文件夹：以后往文件夹里放新图，会自动进图集
                atlas.Add(new Object[] { AssetDatabase.LoadAssetAtPath<DefaultAsset>(AtlasDir) });
                AssetDatabase.SaveAssets();
            }
            return atlas;
        }

        /// 打包图集（编辑器在进入 Play 模式时也会自动打包，前提是 Sprite Packer Mode 已启用）
        public static void PackAtlas()
        {
            var atlas = EnsureAtlas();
            SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        }
        //#endregion

        public static Sprite[] LoadAtlasSprites() =>
            AtlasSprites.Select(n => AssetDatabase.LoadAssetAtPath<Sprite>($"{AtlasDir}/{n}.png")).ToArray();

        //#region idle
        /// 一个“看起来不动”的动画：Image 颜色的 alpha 永远是 1。画面毫无变化，但 Animator 每帧都在写属性
        public static RuntimeAnimatorController EnsureIdleController()
        {
            string dir = Dir + "/Anim", clipPath = dir + "/Idle.anim", ctrlPath = dir + "/Idle.controller";
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (ctrl != null) return ctrl;
            Directory.CreateDirectory(dir);
            var clip = new AnimationClip { name = "Idle" };
            clip.SetCurve("", typeof(Image), "m_Color.a", AnimationCurve.Constant(0, 1, 1));
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, clipPath);
            ctrl = AnimatorController.CreateAnimatorControllerAtPathWithClip(ctrlPath, clip);
            AssetDatabase.SaveAssets();
            return ctrl;
        }
        //#endregion

        /// 对照组：透明度在 1 和 0.9 之间来回变化的动画
        public static RuntimeAnimatorController EnsurePulseController()
        {
            string dir = Dir + "/Anim", clipPath = dir + "/Pulse.anim", ctrlPath = dir + "/Pulse.controller";
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ctrlPath);
            if (ctrl != null) return ctrl;
            Directory.CreateDirectory(dir);
            var clip = new AnimationClip { name = "Pulse" };
            clip.SetCurve("", typeof(Image), "m_Color.a", new AnimationCurve(new Keyframe(0, 1), new Keyframe(0.5f, 0.9f), new Keyframe(1, 1)));
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, clipPath);
            ctrl = AnimatorController.CreateAnimatorControllerAtPathWithClip(ctrlPath, clip);
            AssetDatabase.SaveAssets();
            return ctrl;
        }
    }
}
