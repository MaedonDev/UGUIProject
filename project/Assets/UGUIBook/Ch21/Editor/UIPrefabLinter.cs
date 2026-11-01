// UI 预制体规范检查：把团队约定写成规则，一键扫一遍。规则都很朴素，按项目需要增删即可。
// 菜单：Assets › UGUIBook › UI 规范检查（选中的预制体/文件夹）、Tools › UGUIBook › UI 规范检查（当前场景）
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    public static class UIPrefabLinter
    {
        //#region rules
        public class Rule
        {
            public string id, desc;
            public Func<GameObject, string> check;   // 返回 null 表示通过，否则返回问题描述
        }

        // 编辑器自动起的名字：GameObject、Image (1)、Text (TMP) ……
        static readonly Regex DefaultName = new Regex(@"^(GameObject|Image|RawImage|Text|Text \(TMP\)|Button|Panel)( \(\d+\))?$");

        public static readonly List<Rule> Rules = new List<Rule>
        {
            new Rule { id = "Naming", desc = "不要用编辑器默认名字", check = go =>
                DefaultName.IsMatch(go.name) ? $"默认名字“{go.name}”，代码查找和排查问题都很痛苦" : null },
            new Rule { id = "TextRaycast", desc = "文字不需要 Raycast Target", check = go =>
            {
                var g = go.GetComponent<Graphic>();
                bool isText = g is TMP_Text || g is Text;
                return isText && g.raycastTarget && go.GetComponent<IEventSystemHandler>() == null
                       && !(go.GetComponentInParent<TMP_InputField>(true) != null) ? "文字开着 Raycast Target" : null;
            }},
            new Rule { id = "LegacyText", desc = "统一用 TextMeshPro", check = go =>
                go.GetComponent<Text>() != null ? "使用了旧版 Text 组件" : null },
            new Rule { id = "AutoSize", desc = "慎用 Auto Size", check = go =>
            {
                var t = go.GetComponent<TMP_Text>();
                return t != null && t.enableAutoSizing ? "开启了 Auto Size（每次改字都要多次排版，见第 20 章实测）" : null;
            }},
            new Rule { id = "EmptyImage", desc = "Image 没有精灵又不透明", check = go =>
            {
                var img = go.GetComponent<Image>();
                return img != null && img.sprite == null && img.color.a > 0 && go.GetComponent<Mask>() == null
                       ? "Image 没设精灵（会画一个纯色方块），确认是不是忘了" : null;
            }},
            new Rule { id = "InvisibleBlocker", desc = "透明图片挡点击", check = go =>
            {
                var g = go.GetComponent<Graphic>();
                return g != null && g.raycastTarget && g.color.a == 0 && !(g is TMP_Text) && go.GetComponent<Selectable>() == null
                       ? "完全透明却接收射线：会挡住下面的按钮；只做点击区请用 EmptyRaycast（第 20 章）" : null;
            }},
            new Rule { id = "Scale", desc = "缩放应为 1", check = go =>
            {
                var s = go.transform.localScale;
                return go.transform is RectTransform && go.transform.parent != null && s != Vector3.one && s != Vector3.zero
                       ? $"localScale = {s}，尺寸请用 sizeDelta 调，缩放留给动画" : null;
            }},
            new Rule { id = "Mask", desc = "矩形裁剪用 RectMask2D", check = go =>
            {
                var m = go.GetComponent<Mask>(); var img = go.GetComponent<Image>();
                bool rectShape = img != null && (img.sprite == null || img.sprite.name.StartsWith("round_") == false && img.type != Image.Type.Sliced);
                return m != null && rectShape ? "矩形区域用了 Mask（模板缓冲，额外 Draw Call），可换 RectMask2D" : null;
            }},
            new Rule { id = "NestedRaycaster", desc = "嵌套 Canvas 有按钮却没有 GraphicRaycaster", check = go =>
            {
                var c = go.GetComponent<Canvas>();
                // 不在层级根上的 Canvas 就是嵌套 Canvas（预制体单独打开时它会被当成根画布，所以不能用 isRootCanvas 判断）
                return c != null && go.transform.parent != null && go.GetComponent<GraphicRaycaster>() == null
                       && go.GetComponentsInChildren<Selectable>(true).Length > 0 ? "嵌套 Canvas 下有可交互控件，但没挂 GraphicRaycaster，点不到" : null;
            }},
            new Rule { id = "LayoutDepth", desc = "布局组嵌套不超过 3 层", check = go =>
            {
                if (go.GetComponent<LayoutGroup>() == null) return null;
                int depth = 0;
                for (var t = go.transform; t != null; t = t.parent) if (t.GetComponent<LayoutGroup>() != null) depth++;
                return depth > 3 ? $"布局组嵌套了 {depth} 层，改一个字会让整棵树重新布局（第 20 章实测）" : null;
            }},
            new Rule { id = "Animator", desc = "UI 上的 Animator", check = go =>
                go.GetComponent<Animator>() != null && go.transform is RectTransform ? "UI 上挂了 Animator：动画播放期间每帧整树标脏，播完记得禁用" : null },
        };
        //#endregion

        //#region run
        public static List<UIIssue> Lint(GameObject root, string assetPath)
        {
            var issues = new List<UIIssue>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                foreach (var r in Rules)
                {
                    string msg;
                    try { msg = r.check(t.gameObject); }
                    catch (Exception e) { msg = "规则出错：" + e.Message; }
                    if (msg != null)
                        issues.Add(new UIIssue { assetPath = assetPath, objectPath = UIToolsCommon.PathOf(t, root.transform), rule = r.id, message = msg, context = assetPath == null ? t.gameObject : null });
                }
            return issues;
        }
        //#endregion

        [MenuItem("Assets/UGUIBook/UI 规范检查")]
        static void LintPrefabs()
        {
            var issues = new List<UIIssue>();
            var paths = UIToolsCommon.SelectedPrefabPaths();
            UIToolsCommon.ForEachPrefab(paths, (root, p) => { issues.AddRange(Lint(root, p)); return false; }, "UI 规范检查");
            UIIssueWindow.Show("UI 规范检查", issues, $"{paths.Count} 个预制体，{issues.Count} 条问题；规则 {Rules.Count} 条");
        }

        [MenuItem("Tools/UGUIBook/UI 规范检查（当前场景）")]
        static void LintScene()
        {
            var issues = UIToolsCommon.SceneRoots().Where(r => r.GetComponentInChildren<Canvas>(true) != null).SelectMany(r => Lint(r, null)).ToList();
            UIIssueWindow.Show("UI 规范检查", issues, $"当前场景 {issues.Count} 条问题；规则 {Rules.Count} 条");
        }
    }
}
