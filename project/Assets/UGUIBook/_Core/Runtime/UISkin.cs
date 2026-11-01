// 全书共用的“皮肤”：精灵、字体和配色。放在 Resources/UISkin.asset，运行时和编辑器里都能取到。
// 菜单 UGUIBook › Setup Project 会自动创建并填好它。
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Core
{
    [CreateAssetMenu(menuName = "UGUIBook/UI Skin")]
    public class UISkin : ScriptableObject
    {
        public TMP_FontAsset font;        // 正文：思源黑体（Noto Sans SC）子集，动态 SDF
        public TMP_FontAsset fontBold;
        public Font legacyFont;           // 旧版 Text 组件用的同一款字体
        public Sprite[] sprites;

        [Header("配色")]
        public Color primary = new Color32(0x5B, 0x4B, 0xD6, 0xFF);   // 主色：靛紫
        public Color accent = new Color32(0xFF, 0x8A, 0x3D, 0xFF);    // 强调：橙
        public Color ink = new Color32(0x1F, 0x24, 0x36, 0xFF);       // 深色文字
        public Color muted = new Color32(0x7A, 0x82, 0x99, 0xFF);     // 次要文字
        public Color surface = Color.white;                           // 卡片底色
        public Color page = new Color32(0xEE, 0xF0, 0xF6, 0xFF);      // 页面底色
        public Color dark = new Color32(0x24, 0x28, 0x3B, 0xFF);      // 深色面板
        public Color success = new Color32(0x2F, 0xB3, 0x6A, 0xFF);
        public Color danger = new Color32(0xE5, 0x48, 0x4D, 0xFF);

        static UISkin s_Instance;
        Dictionary<string, Sprite> m_Map;

        public static UISkin Instance
        {
            get
            {
                if (s_Instance == null) s_Instance = Resources.Load<UISkin>("UISkin");
                return s_Instance;
            }
        }

        /// 按文件名取精灵，例如 UISkin.Sprite("round_16")
        public static Sprite Sprite(string name)
        {
            var skin = Instance;
            if (skin.m_Map == null || skin.m_Map.Count != skin.sprites.Length)
            {
                skin.m_Map = new Dictionary<string, Sprite>();
                foreach (var s in skin.sprites) if (s != null) skin.m_Map[s.name] = s;
            }
            if (skin.m_Map.TryGetValue(name, out var sp)) return sp;
            Debug.LogError("[UISkin] 找不到精灵 " + name);
            return null;
        }
    }
}
