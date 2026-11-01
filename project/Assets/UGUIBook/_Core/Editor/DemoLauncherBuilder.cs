// 生成 Assets/UGUIBook/Demos.unity：扫描所有 ChNN/Scenes 下的配套场景，写入 Build Settings，并搭出目录界面。
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Core
{
    public static class DemoLauncherBuilder
    {
        const string LauncherPath = "Assets/UGUIBook/Demos.unity";

        public static void Run() { Build(); EditorApplication.Exit(0); }

        [MenuItem("UGUIBook/Build Demo Launcher")]
        public static void Build()
        {
            var titles = ChapterTitles();
            var scenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/UGUIBook" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.Contains("/Scenes/") && Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(p))).StartsWith("Ch"))
                .OrderBy(p => p).ToList();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = UISkin.Instance.page; cam.orthographic = true;
            var canvas = UIKit.Canvas();
            UIKit.EnsureEventSystem();
            var root = (RectTransform)canvas.transform;
            var head = UIKit.Panel(root, UISkin.Instance.dark, 0, "Header");
            head.rectTransform.Place(new Vector2(0.5f, 1), Vector2.zero, new Vector2(0, 200));
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = new Vector2(1, 1);
            var t = UIKit.Text(head.transform, "UGUI 从入门到精通 · 配套场景", 56, Color.white, TextAlignmentOptions.MidlineLeft, true, "Title");
            t.rectTransform.Stretch(80, 0, 80, 40);
            var sub = UIKit.Text(head.transform, $"共 {scenes.Count} 个场景，点击打开；左下角“返回目录”回到这里", 28, new Color(0.8f, 0.78f, 0.95f), TextAlignmentOptions.BottomLeft, false, "Sub");
            sub.rectTransform.Stretch(82, 120, 80, 24);
            var sr = UIKit.ScrollView(root, true, false, 1400, 800);
            sr.Rt().anchorMin = new Vector2(0.5f, 0); sr.Rt().anchorMax = new Vector2(0.5f, 1);
            sr.Rt().offsetMin = new Vector2(-700, 30); sr.Rt().offsetMax = new Vector2(700, -230);
            var content = sr.content;
            var vl = content.VLayout(12, 0, TextAnchor.UpperLeft, true);
            vl.padding = new RectOffset(8, 40, 8, 40);
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var launcher = canvas.gameObject.AddComponent<DemoLauncher>();
            launcher.Content = content;
            foreach (var p in scenes)
            {
                var ch = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(p)));   // Ch06
                titles.TryGetValue(ch, out var title);
                launcher.Entries.Add(new DemoLauncher.Entry
                {
                    chapter = $"第 {int.Parse(ch.Substring(2))} 章  {title}",
                    scene = Path.GetFileNameWithoutExtension(p),
                });
            }
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), LauncherPath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LauncherPath, true) }
                .Concat(scenes.Select(p => new EditorBuildSettingsScene(p, true))).ToArray();
            Debug.Log($"[UGUIBook] launcher: {scenes.Count} scenes");
        }

        /// 从 books/ugui/src/cNN.html 读章节标题（工程单独分发时没有 src，就只显示章号）
        static System.Collections.Generic.Dictionary<string, string> ChapterTitles()
        {
            var d = new System.Collections.Generic.Dictionary<string, string>();
            var src = Path.GetFullPath(Path.Combine(Application.dataPath, "../../src"));
            var json = Path.Combine(Application.dataPath, "UGUIBook/_Core/Editor/chapters.txt");
            if (Directory.Exists(src))
            {
                var lines = Directory.GetFiles(src, "c??.html").OrderBy(f => f).Select(f =>
                {
                    var m = System.Text.RegularExpressions.Regex.Match(File.ReadAllText(f), "id=\"c(\\d\\d)\" data-title=\"([^\"]*)\"");
                    return m.Success ? $"Ch{m.Groups[1].Value}|{m.Groups[2].Value}" : null;
                }).Where(x => x != null).ToArray();
                File.WriteAllLines(json, lines);
            }
            if (File.Exists(json))
                foreach (var l in File.ReadAllLines(json)) { var kv = l.Split('|'); if (kv.Length == 2) d[kv[0]] = kv[1]; }
            return d;
        }
    }
}
