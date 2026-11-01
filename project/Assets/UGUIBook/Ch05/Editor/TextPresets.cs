// 生成 TMP 材质预设（Material Preset）：同一个字体图集，换不同的描边/投影/发光参数。
// 命名规则“字体名 - 样式名”，并且 _MainTex 指向同一张图集，这样 TMP Inspector 的 Material Preset 下拉框里能直接选到。
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;

namespace UGUIBook.Ch05
{
    public static class TextPresets
    {
        //#region preset
        static readonly HashSet<string> s_Rebuilt = new HashSet<string>();

        public static Material Create(Shot s, TMP_FontAsset font, string style, Action<Material> setup)
        {
            string path = $"Assets/UGUIBook/Ch05/Materials/{font.name} - {style}.mat";
            if (s_Rebuilt.Add(path)) AssetDatabase.DeleteAsset(path);   // 每次批处理先删掉重建一次，保证参数是干净的默认值
            // 用完整版 Distance Field Shader（字体默认材质是 Mobile 版，不支持 Glow、Bevel 等）
            var m = s.MatAsset("Ch05", $"{font.name} - {style}", "TextMeshPro/Distance Field");
            // 只拷贝“和这个字体图集绑定”的几项：图集纹理、图集尺寸、Gradient Scale（= Padding + 1）、粗体权重
            var src = font.material;
            m.SetTexture(ShaderUtilities.ID_MainTex, src.GetTexture(ShaderUtilities.ID_MainTex));
            int[] ids =
            {
                ShaderUtilities.ID_TextureWidth, ShaderUtilities.ID_TextureHeight, ShaderUtilities.ID_GradientScale,
                ShaderUtilities.ID_WeightNormal, ShaderUtilities.ID_WeightBold,
            };
            foreach (int id in ids)
                m.SetFloat(id, src.GetFloat(id));
            setup(m);
            ShaderUtilities.UpdateShaderRatios(m);             // 按新参数重算 ScaleRatio（编辑器里改材质时 TMP 也会做）
            EditorUtility.SetDirty(m);
            AssetDatabase.SaveAssetIfDirty(m);
            return m;
        }

        public static void Outline(Material m, Color color, float width)
        {
            m.SetColor(ShaderUtilities.ID_OutlineColor, color);
            m.SetFloat(ShaderUtilities.ID_OutlineWidth, width);   // 0~1，相对于 SDF 的可用范围
        }

        public static void Underlay(Material m, Color color, float x, float y, float dilate, float softness)
        {
            m.EnableKeyword(ShaderUtilities.Keyword_Underlay);    // UNDERLAY_ON
            m.SetColor(ShaderUtilities.ID_UnderlayColor, color);
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, x);
            m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, y);
            m.SetFloat(ShaderUtilities.ID_UnderlayDilate, dilate);
            m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, softness);
        }

        public static void Glow(Material m, Color color, float outer, float power)
        {
            m.EnableKeyword(ShaderUtilities.Keyword_Glow);        // GLOW_ON
            m.SetColor(ShaderUtilities.ID_GlowColor, color);
            m.SetFloat(ShaderUtilities.ID_GlowOuter, outer);
            m.SetFloat(ShaderUtilities.ID_GlowPower, power);
        }
        //#endregion
    }
}
