// 本地化文本组件：挂在 TMP 文字上，填 key，切换语言时自动刷新文字、字体和字号。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch19
{
    //#region comp
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] string m_Key;
        [Tooltip("勾选后按语言切换字体与字号（LocFontSet）")]
        [SerializeField] bool m_ApplyFont = true;

        TMP_Text m_Text;
        float m_BaseSize = -1;          // 设计时的字号（以中文为准），各语言在它的基础上乘倍率
        object[] m_Args;

        public string Key { get => m_Key; set { m_Key = value; Refresh(); } }

        /// 带参数的文本，比如 “你有 {0} 个金币”
        public void SetArgs(params object[] args) { m_Args = args; Refresh(); }

        void Awake()
        {
            m_Text = GetComponent<TMP_Text>();
            m_BaseSize = m_Text.fontSize;
        }

        void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();                  // 界面隐藏期间切过语言，重新显示时要补一次
        }

        void OnDisable() => Loc.Changed -= Refresh;

        public void Refresh()
        {
            if (m_Text == null) Awake();
            if (string.IsNullOrEmpty(m_Key)) return;
            var s = m_Args == null ? Loc.Get(m_Key) : string.Format(Loc.Get(m_Key), m_Args);
            if (m_ApplyFont && LocFontSet.Instance != null)
            {
                var e = LocFontSet.Instance.Find(Loc.Language);
                if (e != null)
                {
                    if (e.font != null && m_Text.font != e.font) m_Text.font = e.font;
                    m_Text.fontSize = m_BaseSize * e.sizeScale;
                    if (m_Text.enableAutoSizing) m_Text.fontSizeMax = m_BaseSize * e.sizeScale;
                    m_Text.characterSpacing = e.characterSpacing;
                }
            }
            m_Text.text = s;
        }
    }
    //#endregion
}
