// 批量替换字体 / 精灵：把选中的预制体（或文件夹）或当前场景里，所有用到 A 的地方换成 B。
// 换字体常见于“美术换了一套字体”“TMP 字体资源重新生成了”；换精灵常见于“图标改版”“两张重复的图合并成一张”。
// 菜单：Tools › UGUIBook › 批量替换字体与精灵
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    public class UIBatchReplace : EditorWindow
    {
        enum Kind { TMPFont, LegacyFont, Sprite }
        enum Scope { SelectedPrefabs, OpenScenes }

        Kind m_Kind;
        Scope m_Scope;
        Object m_From, m_To;
        string m_Log = "";

        [MenuItem("Tools/UGUIBook/批量替换字体与精灵")]
        static void Open() => GetWindow<UIBatchReplace>("批量替换");

        void OnGUI()
        {
            m_Kind = (Kind)EditorGUILayout.EnumPopup("替换什么", m_Kind);
            m_Scope = (Scope)EditorGUILayout.EnumPopup("范围", m_Scope);
            var type = m_Kind == Kind.TMPFont ? typeof(TMP_FontAsset) : m_Kind == Kind.LegacyFont ? typeof(Font) : typeof(Sprite);
            m_From = EditorGUILayout.ObjectField("把这个（留空=全部）", m_From, type, false);
            m_To = EditorGUILayout.ObjectField("换成", m_To, type, false);
            EditorGUILayout.HelpBox(m_Scope == Scope.SelectedPrefabs ? "在 Project 窗口选中预制体或文件夹" : "作用于当前打开的所有场景（可撤销）", MessageType.None);
            using (new EditorGUI.DisabledScope(m_To == null))
            {
                if (GUILayout.Button("预览（只统计不修改）")) Run(false);
                if (GUILayout.Button("执行替换")) Run(true);
            }
            EditorGUILayout.LabelField(m_Log, EditorStyles.wordWrappedLabel);
        }

        void Run(bool apply)
        {
            int hits = 0;
            if (m_Scope == Scope.OpenScenes)
            {
                foreach (var root in UIToolsCommon.SceneRoots()) hits += Replace(root, m_Kind, m_From, m_To, apply, true);
                m_Log = $"当前场景：{(apply ? "替换了" : "将替换")} {hits} 处";
            }
            else
            {
                var paths = UIToolsCommon.SelectedPrefabPaths();
                int files = UIToolsCommon.ForEachPrefab(paths, (root, p) =>
                {
                    int n = Replace(root, m_Kind, m_From, m_To, apply, false);
                    hits += n;
                    return apply && n > 0;
                }, "批量替换");
                m_Log = $"{paths.Count} 个预制体：{(apply ? $"修改了 {files} 个，" : "")}{(apply ? "替换了" : "将替换")} {hits} 处";
            }
            Debug.Log("[UIBatchReplace] " + m_Log);
        }

        //#region replace
        /// 返回匹配到的数量；apply=false 时只统计
        public static int Replace(GameObject root, int kind, Object from, Object to, bool apply, bool undo) =>
            Replace(root, (Kind)kind, from, to, apply, undo);

        static int Replace(GameObject root, Kind kind, Object from, Object to, bool apply, bool undo)
        {
            int n = 0;
            void Set<T>(T comp, System.Action act) where T : Object
            {
                n++;
                if (!apply) return;
                if (undo) Undo.RecordObject(comp, "Batch Replace");
                act();
                if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(comp);
                EditorUtility.SetDirty(comp);
            }
            switch (kind)
            {
                case Kind.TMPFont:
                    foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
                        if (t.font != to && (from == null || t.font == from))
                            // 换字体时材质也要跟着换成新字体的默认材质，否则还在用旧字体图集的材质预设
                            Set(t, () => { t.font = (TMP_FontAsset)to; t.fontSharedMaterial = ((TMP_FontAsset)to).material; });
                    break;
                case Kind.LegacyFont:
                    foreach (var t in root.GetComponentsInChildren<Text>(true))
                        if (t.font != to && (from == null || t.font == from)) Set(t, () => t.font = (Font)to);
                    break;
                case Kind.Sprite:
                    if (from == null) break;   // 精灵不支持“全部替换”，太危险
                    foreach (var img in root.GetComponentsInChildren<Image>(true))
                        if (img.sprite == from) Set(img, () => img.sprite = (Sprite)to);
                    // Button 的 Sprite Swap 状态图也要换
                    foreach (var sel in root.GetComponentsInChildren<Selectable>(true))
                    {
                        var st = sel.spriteState;
                        bool hit = st.highlightedSprite == from || st.pressedSprite == from || st.selectedSprite == from || st.disabledSprite == from;
                        if (!hit) continue;
                        Set(sel, () =>
                        {
                            if (st.highlightedSprite == from) st.highlightedSprite = (Sprite)to;
                            if (st.pressedSprite == from) st.pressedSprite = (Sprite)to;
                            if (st.selectedSprite == from) st.selectedSprite = (Sprite)to;
                            if (st.disabledSprite == from) st.disabledSprite = (Sprite)to;
                            sel.spriteState = st;
                        });
                    }
                    break;
            }
            if (apply && undo && n > 0) UIToolsCommon.MarkSceneDirty(root);
            return n;
        }
        //#endregion
    }
}
