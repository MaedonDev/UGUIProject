// 每种语言用什么字体、字号缩放多少。放在 Resources/Ch19/LocFontSet.asset。
// 真实项目里通常是：中文用思源黑体，英文/德文换一款拉丁字体（字形更好看、字号可略大），日韩各自一套。
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch19
{
    [CreateAssetMenu(menuName = "UGUIBook/Ch19/Loc Font Set")]
    public class LocFontSet : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public string language = "en";
            public TMP_FontAsset font;
            [Tooltip("字号倍率：拉丁字母视觉上比汉字小，常设 1.05~1.1；德语偏长时也可以略小")]
            public float sizeScale = 1f;
            [Tooltip("额外字间距（TMP 的 characterSpacing，单位 em/100）")]
            public float characterSpacing;
        }

        public List<Entry> entries = new List<Entry>();

        static LocFontSet s_Instance;
        public static LocFontSet Instance => s_Instance != null ? s_Instance : (s_Instance = Resources.Load<LocFontSet>("Ch19/LocFontSet"));

        public Entry Find(string language)
        {
            if (language == Loc.Pseudo) language = "en";
            foreach (var e in entries) if (e.language == language) return e;
            return null;
        }
    }
}
