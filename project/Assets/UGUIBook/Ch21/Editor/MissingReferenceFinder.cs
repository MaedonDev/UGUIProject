// 检查丢失引用：
//   · Missing Script（脚本被删/改名后组件变成 “Missing (Mono Script)”）
//   · 对象引用丢失（Inspector 里显示 “Missing (Sprite)” 之类：曾经指向某个资源/物体，现在找不到了）
//   · UnityEvent 回调丢失（OnClick 里绑的物体被删了，或者方法被改名了）
// 菜单：Tools › UGUIBook › 检查丢失引用（当前场景） / Assets › UGUIBook › 检查丢失引用（选中的预制体/文件夹）
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace UGUIBook.Ch21
{
    public static class MissingReferenceFinder
    {
        //#region check
        public static void Check(GameObject root, string assetPath, List<UIIssue> issues)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var go = t.gameObject;
                string path = UIToolsCommon.PathOf(t, root.transform);
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missing > 0)
                    issues.Add(new UIIssue { assetPath = assetPath, objectPath = path, rule = "MissingScript", message = $"{missing} 个脚本丢失", context = assetPath == null ? go : null });

                foreach (var comp in go.GetComponents<Component>())
                {
                    if (comp == null) continue;               // 就是上面统计过的 Missing Script
                    var so = new SerializedObject(comp);
                    var sp = so.GetIterator();
                    while (sp.NextVisible(true))
                    {
                        if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;
                        // 值是 null 但实例 ID 不为 0：曾经引用过某个对象，现在丢了（真正的 “None” 实例 ID 是 0）
                        if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
                            issues.Add(new UIIssue { assetPath = assetPath, objectPath = path, rule = "MissingRef", message = $"{comp.GetType().Name}.{sp.propertyPath} 引用丢失", context = assetPath == null ? comp : null });
                    }
                    CheckEvents(comp, path, assetPath, issues);
                }
            }
        }

        // 遍历组件上所有 UnityEvent 字段（Button.onClick、Toggle.onValueChanged……），检查每个持久化回调
        static void CheckEvents(Component comp, string path, string assetPath, List<UIIssue> issues)
        {
            var fields = comp.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                if (!typeof(UnityEventBase).IsAssignableFrom(f.FieldType)) continue;
                if (!(f.GetValue(comp) is UnityEventBase ev)) continue;
                for (int i = 0; i < ev.GetPersistentEventCount(); i++)
                {
                    var target = ev.GetPersistentTarget(i);
                    var method = ev.GetPersistentMethodName(i);
                    string where = $"{comp.GetType().Name}.{f.Name.TrimStart('m', '_')}[{i}]";
                    if (target == null)
                        issues.Add(new UIIssue { assetPath = assetPath, objectPath = path, rule = "MissingEventTarget", message = where + " 的目标物体丢失", context = assetPath == null ? comp : null });
                    else if (string.IsNullOrEmpty(method) || target.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static) == null
                             && target.GetType().GetProperty(method.StartsWith("set_") ? method.Substring(4) : method) == null)
                        issues.Add(new UIIssue { assetPath = assetPath, objectPath = path, rule = "MissingEventMethod", message = $"{where} 调用的方法 {target.GetType().Name}.{method} 不存在", context = assetPath == null ? comp : null });
                }
            }
        }
        //#endregion

        [MenuItem("Tools/UGUIBook/检查丢失引用（当前场景）")]
        static void CheckScene()
        {
            var issues = new List<UIIssue>();
            foreach (var root in UIToolsCommon.SceneRoots()) Check(root, null, issues);
            UIIssueWindow.Show("丢失引用", issues, $"当前场景：发现 {issues.Count} 处问题");
        }

        [MenuItem("Assets/UGUIBook/检查丢失引用")]
        static void CheckPrefabs()
        {
            var issues = new List<UIIssue>();
            var paths = UIToolsCommon.SelectedPrefabPaths();
            UIToolsCommon.ForEachPrefab(paths, (root, path) => { Check(root, path, issues); return false; }, "检查丢失引用");
            UIIssueWindow.Show("丢失引用", issues, $"{paths.Count} 个预制体：发现 {issues.Count} 处问题");
        }
    }
}
