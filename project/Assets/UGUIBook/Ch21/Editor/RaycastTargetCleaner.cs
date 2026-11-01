// 一键关闭“不需要接收点击”的 Graphic 的 Raycast Target。
// 判定规则（保守：拿不准的一律保留）：
//   保留 —— ① 自己身上有任何事件接口的组件（Button、ScrollRect、EventTrigger、你写的 IPointerClickHandler……）
//          ② 是某个 Selectable 的 targetGraphic（按钮的底图）
//          ③ 是 InputField 的文字区域、Scrollbar/Slider 的把手区域这类“由控件自己处理”的部分（它们的父节点上有 Selectable）
//          ④ 名字里带 "Raycast"/"Block"/"Click" 的，视为作者故意留的点击遮挡层
//          ⑤ 窗口底板：铺满父节点、且是同级里第一张开着射线的图——它负责挡住下层界面的点击
//   关闭 —— 其余所有 Graphic（纯装饰的图片、文字、图标）
// 菜单：GameObject › UGUIBook › 关闭非交互元素的 Raycast Target（作用于选中物体及子物体，支持撤销）
//       Assets › UGUIBook › 关闭非交互元素的 Raycast Target（作用于选中的预制体/文件夹）
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    public static class RaycastTargetCleaner
    {
        static readonly string[] KeepNames = { "raycast", "block", "click" };

        //#region rules
        /// 找出 root 下所有“可以关掉 Raycast Target”的 Graphic
        public static List<Graphic> FindUnneeded(GameObject root)
        {
            var all = root.GetComponentsInChildren<Graphic>(true);
            // ② 所有 Selectable 的 targetGraphic
            var targets = new HashSet<Graphic>(root.GetComponentsInChildren<Selectable>(true)
                .Select(s => s.targetGraphic).Where(g => g != null));
            var result = new List<Graphic>();
            foreach (var g in all)
            {
                if (!g.raycastTarget) continue;
                if (g is TMPro.TMP_SubMeshUI) { result.Add(g); continue; }               // TMP 的子网格（回退字体/多图集）属于文字
                if (targets.Contains(g)) continue;                                         // ②
                if (g.GetComponents<MonoBehaviour>().Any(m => m is IEventSystemHandler)) continue; // ①
                if (IsControlPart(g)) continue;                                            // ③
                if (IsBlockerBackground(g)) continue;                                      // ⑤
                var lower = g.name.ToLowerInvariant();
                if (KeepNames.Any(lower.Contains)) continue;                               // ④
                result.Add(g);
            }
            return result;
        }

        // ③ 控件自己身上没有可点的 Graphic（Slider、Toggle 的根节点默认就没有 Image），
        // 点击全靠子物体的图片“接住”再冒泡上去 —— 这些子图片必须保留（文字除外）
        static bool IsControlPart(Graphic g)
        {
            if (g is TMPro.TMP_Text || g is Text) return false;
            var owner = g.transform.parent != null ? g.transform.parent.GetComponentInParent<IEventSystemHandler>(true) as Component : null;
            if (owner == null) return false;
            var own = owner.GetComponent<Graphic>();
            return own == null || !own.raycastTarget;
        }
        static bool IsBlockerBackground(Graphic g)
        {
            if (g is TMPro.TMP_Text || g is Text) return false;
            var rt = g.rectTransform;
            if (rt.anchorMin != Vector2.zero || rt.anchorMax != Vector2.one || rt.parent == null) return false;
            for (int i = 0; i < rt.GetSiblingIndex(); i++)
            {
                var other = rt.parent.GetChild(i).GetComponent<Graphic>();
                var ort = other != null ? other.rectTransform : null;
                if (other != null && other.raycastTarget && ort.anchorMin == Vector2.zero && ort.anchorMax == Vector2.one) return false;
            }
            return true;
        }
        //#endregion

        //#region apply
        public static int Apply(GameObject root, bool undo)
        {
            var list = FindUnneeded(root);
            foreach (var g in list)
            {
                if (undo) Undo.RecordObject(g, "Disable Raycast Target");
                g.raycastTarget = false;
                if (undo) PrefabUtility.RecordPrefabInstancePropertyModifications(g);
            }
            if (undo && list.Count > 0) UIToolsCommon.MarkSceneDirty(root);
            return list.Count;
        }
        //#endregion

        [MenuItem("GameObject/UGUIBook/关闭非交互元素的 Raycast Target", false, 30)]
        static void ForSelection()
        {
            int n = 0;
            Undo.IncrementCurrentGroup();
            foreach (var go in Selection.gameObjects) n += Apply(go, true);
            Debug.Log($"[RaycastTargetCleaner] 关闭了 {n} 个 Raycast Target（Ctrl/Cmd+Z 可撤销）");
        }

        [MenuItem("GameObject/UGUIBook/关闭非交互元素的 Raycast Target", true)]
        static bool ForSelectionValidate() => Selection.gameObjects.Length > 0;

        [MenuItem("Assets/UGUIBook/关闭非交互元素的 Raycast Target")]
        static void ForPrefabs()
        {
            int total = 0;
            int files = UIToolsCommon.ForEachPrefab(UIToolsCommon.SelectedPrefabPaths(), (root, path) =>
            {
                int n = Apply(root, false);
                total += n;
                return n > 0;
            }, "关闭 Raycast Target");
            Debug.Log($"[RaycastTargetCleaner] 修改了 {files} 个预制体，共关闭 {total} 个 Raycast Target");
        }
    }
}
