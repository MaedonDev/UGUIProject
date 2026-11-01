// 搜索框实时过滤：输入时按关键字隐藏不匹配的列表项，并用 TMP 富文本 <mark> 高亮命中的文字。
using System;
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class SearchFilter : MonoBehaviour
    {
        [Serializable] public struct Entry { public string name; public string sprite; }

        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Button m_Clear;              // 输入框右侧的“×”
        [SerializeField] RectTransform m_List;        // 列表项的父节点（带 GridLayoutGroup）
        [SerializeField] RectTransform m_Template;    // 列表项模板（未激活）
        [SerializeField] TMP_Text m_Count;            // “找到 3 个结果”
        [SerializeField] Entry[] m_Entries;
        [SerializeField] string m_MarkColor = "#FF8A3D55";

        readonly List<(Entry data, GameObject go, TMP_Text label)> m_Items = new List<(Entry, GameObject, TMP_Text)>();

        //#region bind
        void Awake()
        {
            Build();
            // onValueChanged：每输入/删除一个字符都会触发，适合“边打边筛”
            m_Input.onValueChanged.AddListener(Apply);
            m_Clear.onClick.AddListener(() => { m_Input.text = ""; m_Input.ActivateInputField(); });
        }

        void Start() => Apply(m_Input.text);
        //#endregion

        public void Build()
        {
            if (m_Items.Count > 0) return;
            foreach (var e in m_Entries)
            {
                var go = Instantiate(m_Template, m_List).gameObject;
                go.SetActive(true);
                go.name = "Item_" + e.name;
                go.transform.Find("Icon").GetComponent<Image>().sprite = UISkin.Sprite(e.sprite);
                var label = go.GetComponentInChildren<TMP_Text>();
                label.text = e.name;
                m_Items.Add((e, go, label));
            }
        }

        //#region filter
        public void Apply(string query)
        {
            query = query.Trim();
            m_Clear.gameObject.SetActive(query.Length > 0);
            int shown = 0;
            foreach (var (data, go, label) in m_Items)
            {
                int at = query.Length == 0 ? -1 : data.name.IndexOf(query, StringComparison.OrdinalIgnoreCase);
                bool match = query.Length == 0 || at >= 0;
                go.SetActive(match);        // 隐藏的项不参与 LayoutGroup 排版，剩下的自动往前补位
                if (!match) continue;
                shown++;
                // 用户输入可能含 < >，拼进富文本前先转义成 TMP 能显示的形式，避免被当成标签
                label.text = at < 0 ? Escape(data.name)
                    : Escape(data.name.Substring(0, at)) + $"<mark={m_MarkColor}>" + Escape(data.name.Substring(at, query.Length)) + "</mark>"
                      + Escape(data.name.Substring(at + query.Length));
            }
            m_Count.text = query.Length == 0 ? $"共 {m_Items.Count} 件物品" : shown > 0 ? $"找到 {shown} 个结果" : "没有找到相关物品";
        }

        static string Escape(string s) => s.Replace("<", "<noparse><</noparse>");
        //#endregion

        public void Setup(TMP_InputField input, Button clear, RectTransform list, RectTransform template, TMP_Text count, Entry[] entries)
        {
            m_Input = input; m_Clear = clear; m_List = list; m_Template = template; m_Count = count; m_Entries = entries;
        }
    }
}
