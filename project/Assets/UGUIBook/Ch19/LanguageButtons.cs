// 演示用：一排按钮，点哪个切到哪种语言；当前语言高亮
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch19
{
    public class LanguageButtons : MonoBehaviour
    {
        [System.Serializable]
        public class Item { public string language; public Button button; public Image background; }

        public Item[] items;
        public Color normal = new Color(0.88f, 0.9f, 0.95f), selected = new Color32(0x5B, 0x4B, 0xD6, 0xFF);

        void Awake()
        {
            foreach (var it in items)
            {
                var lang = it.language;      // 循环变量先拷一份再进闭包（第 6 章讲过的坑）
                it.button.onClick.AddListener(() => Loc.SetLanguage(lang));
            }
        }

        void OnEnable() { Loc.Changed += Highlight; Highlight(); }
        void OnDisable() => Loc.Changed -= Highlight;

        void Highlight()
        {
            foreach (var it in items) if (it.background != null) it.background.color = it.language == Loc.Language ? selected : normal;
        }
    }
}
