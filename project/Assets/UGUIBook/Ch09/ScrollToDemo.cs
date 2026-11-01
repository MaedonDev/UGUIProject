// 配套场景 Ch09_ScrollTo：生成 100 行，输入序号跳转，可选对齐方式，目标行闪一下高亮。
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    public class ScrollToDemo : MonoBehaviour
    {
        [SerializeField] ScrollRect m_Scroll;
        [SerializeField] RectTransform m_RowTemplate;
        [SerializeField] TMP_InputField m_Index;
        [SerializeField] TMP_Dropdown m_Align;       // 顶部 / 居中 / 底部
        [SerializeField] Button m_Go, m_Top, m_Bottom;
        [SerializeField] int m_Count = 100;
        [SerializeField] Color m_Highlight = new Color32(0xFF, 0xE3, 0xC9, 0xFF);

        readonly List<Image> m_Rows = new List<Image>();
        Color m_Normal;

        void Awake()
        {
            Build();
            m_Go.onClick.AddListener(Go);
            m_Index.onSubmit.AddListener(_ => Go());
            m_Top.onClick.AddListener(() => m_Scroll.GetComponent<SmoothScroller>().ScrollTo(m_Rows[0].rectTransform, 0));
            m_Bottom.onClick.AddListener(() => m_Scroll.GetComponent<SmoothScroller>().ScrollTo(m_Rows[m_Count - 1].rectTransform, 1));
        }

        public void Build()
        {
            if (m_Rows.Count > 0) return;
            for (int i = 0; i < m_Count; i++)
            {
                var row = Instantiate(m_RowTemplate, m_Scroll.content);
                row.gameObject.SetActive(true);
                row.name = "Row_" + (i + 1);
                row.GetComponentInChildren<TMP_Text>().text = $"第 {i + 1} 项";
                m_Rows.Add(row.GetComponent<Image>());
            }
            m_Normal = m_Rows[0].color;
        }

        void Go()
        {
            if (!int.TryParse(m_Index.text, out int n)) return;
            Jump(Mathf.Clamp(n, 1, m_Count), false);
        }

        public void Jump(int n, bool instant)
        {
            float align = m_Align.value * 0.5f;          // 0 / 0.5 / 1
            var row = m_Rows[n - 1];
            m_Scroll.GetComponent<SmoothScroller>().ScrollTo(row.rectTransform, align, instant);
            foreach (var r in m_Rows) r.color = m_Normal;
            row.color = m_Highlight;
            if (Application.isPlaying) StartCoroutine(FadeBack(row));
        }

        IEnumerator FadeBack(Image row)
        {
            yield return new WaitForSecondsRealtime(1.2f);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.4f) { row.color = Color.Lerp(m_Highlight, m_Normal, t); yield return null; }
            row.color = m_Normal;
        }

        public void Setup(ScrollRect sr, RectTransform tpl, TMP_InputField index, TMP_Dropdown align, Button go, Button top, Button bottom)
        {
            m_Scroll = sr; m_RowTemplate = tpl; m_Index = index; m_Align = align; m_Go = go; m_Top = top; m_Bottom = bottom;
        }
    }
}
