// 极简本地化：一张 CSV 表（第一列 key，后面每列一种语言），运行时按 key 取当前语言的字符串。
// 表放在 Resources/Ch19/strings.csv，用 Excel / WPS / 在线表格编辑后另存为 UTF-8 CSV 即可。
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UGUIBook.Ch19
{
    public static class Loc
    {
        public const string Pseudo = "pseudo";   // 伪本地化：自动把英文“拉长”，专门用来测排版
        const string PrefKey = "ugui_book_lang";

        static Dictionary<string, string[]> s_Table;
        static List<string> s_Languages;
        static int s_Column;

        public static string Language { get; private set; } = "zh";
        public static event Action Changed;

        public static IReadOnlyList<string> Languages { get { EnsureLoaded(); return s_Languages; } }

        //#region load
        public static void EnsureLoaded()
        {
            if (s_Table != null) return;
            var csv = Resources.Load<TextAsset>("Ch19/strings");
            Load(csv != null ? csv.text : "key,zh\n");
            // 上次选过的语言 > 系统语言
            var saved = PlayerPrefs.GetString(PrefKey, "");
            Select(saved != "" ? saved : FromSystemLanguage(Application.systemLanguage));
        }

        public static void Load(string csvText)
        {
            var rows = ParseCsv(csvText);
            s_Table = new Dictionary<string, string[]>();
            s_Languages = new List<string>();
            if (rows.Count == 0) return;
            // 表头：key, zh, en, de, note ……（note 等非语言列以 note 开头，忽略）
            for (int c = 1; c < rows[0].Count && !rows[0][c].StartsWith("note"); c++) s_Languages.Add(rows[0][c].Trim());
            for (int r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                if (row.Count == 0 || string.IsNullOrWhiteSpace(row[0]) || row[0].StartsWith("#")) continue;
                var values = new string[s_Languages.Count];
                for (int c = 0; c < values.Length; c++) values[c] = c + 1 < row.Count ? row[c + 1] : "";
                s_Table[row[0].Trim()] = values;
            }
        }
        //#endregion

        //#region get
        public static void SetLanguage(string code)
        {
            EnsureLoaded();
            Select(code);
            PlayerPrefs.SetString(PrefKey, Language);
            Changed?.Invoke();          // 所有 LocalizedText 收到通知后各自刷新
        }

        static void Select(string code)
        {
            if (code != Pseudo && !s_Languages.Contains(code)) code = s_Languages.Count > 0 ? s_Languages[0] : "zh";
            Language = code;
            s_Column = code == Pseudo ? s_Languages.IndexOf("en") : s_Languages.IndexOf(code);
        }

        public static string Get(string key)
        {
            EnsureLoaded();
            if (string.IsNullOrEmpty(key)) return "";
            if (!s_Table.TryGetValue(key, out var values)) return "#" + key;          // 缺 key：显眼地露出来，方便测试发现
            string s = s_Column >= 0 && s_Column < values.Length ? values[s_Column] : "";
            if (string.IsNullOrEmpty(s)) s = values[0];                                // 缺翻译：回退到第一种语言
            return Language == Pseudo ? MakePseudo(s) : s;
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        public static bool Has(string key) { EnsureLoaded(); return s_Table.ContainsKey(key); }
        //#endregion

        public static string FromSystemLanguage(SystemLanguage l) => l switch
        {
            SystemLanguage.Chinese or SystemLanguage.ChineseSimplified or SystemLanguage.ChineseTraditional => "zh",
            SystemLanguage.German => "de",
            _ => "en",
        };

        //#region pseudo
        /// 伪本地化：元音重复一遍（长度约 +35%）并加上括号，能一眼看出哪些字没走本地化、哪里会被截断。
        /// {0} 这样的占位符和 <b> 这样的富文本标签原样保留
        public static string MakePseudo(string s)
        {
            var sb = new StringBuilder("[");
            int depth = 0;
            foreach (char ch in s)
            {
                if (ch == '{' || ch == '<') depth++;
                if (ch == '}' || ch == '>') depth--;
                sb.Append(ch);
                if (depth == 0 && "aeiouAEIOU".IndexOf(ch) >= 0) sb.Append(ch);
            }
            return sb.Append(']').ToString();
        }
        //#endregion

        //#region csv
        /// RFC 4180 风格的 CSV 解析：支持双引号包裹、引号内的逗号和换行、"" 表示一个引号
        public static List<List<string>> ParseCsv(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var cell = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (quoted)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { cell.Append('"'); i++; }
                    else if (c == '"') quoted = false;
                    else cell.Append(c);
                }
                else if (c == '"') quoted = true;
                else if (c == ',') { row.Add(cell.ToString()); cell.Clear(); }
                else if (c == '\n' || c == '\r')
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    row.Add(cell.ToString()); cell.Clear();
                    rows.Add(row); row = new List<string>();
                }
                else if (c != '﻿') cell.Append(c);   // 跳过 Excel 存出来的 BOM
            }
            if (cell.Length > 0 || row.Count > 0) { row.Add(cell.ToString()); rows.Add(row); }
            return rows;
        }
        //#endregion
    }
}
