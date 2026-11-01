// 第 21 章编辑器工具的公共部分：遍历“选中的预制体/文件夹”或“当前打开的场景”，对每个根物体执行操作，改了就保存。
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch21
{
    public static class UIToolsCommon
    {
        /// 当前选中的预制体路径（选中文件夹时递归包含里面所有预制体）
        public static List<string> SelectedPrefabPaths()
        {
            var result = new List<string>();
            foreach (var o in Selection.objects)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path)) continue;
                if (AssetDatabase.IsValidFolder(path))
                    result.AddRange(AssetDatabase.FindAssets("t:Prefab", new[] { path }).Select(AssetDatabase.GUIDToAssetPath));
                else if (path.EndsWith(".prefab")) result.Add(path);
            }
            return result.Distinct().ToList();
        }

        //#region foreach-prefab
        /// 逐个打开预制体内容（不进 Prefab Mode，不影响场景），执行 action；action 返回 true 表示改了，就保存回去
        public static int ForEachPrefab(IEnumerable<string> paths, Func<GameObject, string, bool> action, string title = "处理预制体")
        {
            int changed = 0;
            var list = paths.ToList();
            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (EditorUtility.DisplayCancelableProgressBar(title, list[i], (float)i / list.Count)) break;
                    var root = PrefabUtility.LoadPrefabContents(list[i]);
                    try
                    {
                        if (action(root, list[i]))
                        {
                            PrefabUtility.SaveAsPrefabAsset(root, list[i]);
                            changed++;
                        }
                    }
                    finally { PrefabUtility.UnloadPrefabContents(root); }
                }
            }
            finally { EditorUtility.ClearProgressBar(); }
            return changed;
        }
        //#endregion

        /// 当前打开的所有场景里的根物体
        public static IEnumerable<GameObject> SceneRoots()
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var sc = EditorSceneManager.GetSceneAt(i);
                if (!sc.isLoaded) continue;
                foreach (var go in sc.GetRootGameObjects()) yield return go;
            }
        }

        public static string PathOf(Transform t, Transform root = null)
        {
            var parts = new List<string>();
            for (var x = t; x != null && x != root?.parent; x = x.parent) parts.Insert(0, x.name);
            return string.Join("/", parts);
        }

        public static void MarkSceneDirty(GameObject go)
        {
            if (!EditorUtility.IsPersistent(go) && go.scene.IsValid()) EditorSceneManager.MarkSceneDirty(go.scene);
        }
    }

    /// 一条检查结果：在窗口里点一下能定位到对应物体
    [Serializable]
    public class UIIssue
    {
        public string assetPath;     // 预制体路径；场景物体为空
        public string objectPath;    // 物体在层级里的路径
        public string rule;
        public string message;
        public Object context;       // 场景物体可以直接引用

        public override string ToString() => $"[{rule}] {(string.IsNullOrEmpty(assetPath) ? "" : Path.GetFileName(assetPath) + " : ")}{objectPath} — {message}";

        public void Ping()
        {
            if (context != null) { Selection.activeObject = context; EditorGUIUtility.PingObject(context); return; }
            if (!string.IsNullOrEmpty(assetPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                EditorGUIUtility.PingObject(asset);
                Selection.activeObject = asset;
            }
        }
    }

    /// 结果列表窗口（几个检查工具共用）
    public class UIIssueWindow : EditorWindow
    {
        List<UIIssue> m_Issues = new List<UIIssue>();
        Vector2 m_Scroll;
        string m_Summary;

        public static void Show(string title, List<UIIssue> issues, string summary)
        {
            var w = GetWindow<UIIssueWindow>(title);
            w.titleContent = new GUIContent(title);
            w.m_Issues = issues; w.m_Summary = summary;
            w.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(m_Summary ?? "", m_Issues.Count == 0 ? MessageType.Info : MessageType.Warning);
            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
            foreach (var issue in m_Issues)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("定位", GUILayout.Width(44))) issue.Ping();
                    EditorGUILayout.LabelField(issue.ToString(), EditorStyles.wordWrappedLabel);
                }
            }
            EditorGUILayout.EndScrollView();
            if (m_Issues.Count > 0 && GUILayout.Button("复制全部到剪贴板"))
                EditorGUIUtility.systemCopyBuffer = string.Join("\n", m_Issues.Select(i => i.ToString()));
        }
    }
}
