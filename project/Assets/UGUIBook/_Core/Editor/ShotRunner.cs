// 批处理入口：检查本章 Shader，生成本章所有 [BookScene]/[BookShot] 场景，渲染所有配图。
// Unity -batchmode -projectPath <工程> -executeMethod UGUIBook.Core.ShotRunner.Run -chapters ch05,ch06 [-shot id] [-noshots] [-noscenes] -outdir <目录>
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Core
{
    public static class ShotRunner
    {
        static readonly List<string> Errors = new List<string>();

        static string Arg(string name, string def = null)
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
            return def;
        }
        static bool Flag(string name) => Environment.GetCommandLineArgs().Contains(name);

        public static void Run()
        {
            int code = 0;
            try { code = RunInner(); }
            catch (Exception e) { Debug.LogError("[UGUIBook] Runner crashed: " + e); code = 2; }
            EditorApplication.Exit(code);
        }

        static IEnumerable<(MethodInfo m, T attr)> Find<T>() where T : Attribute =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return new Type[0]; } })
                .SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Select(m => (m, attr: m.GetCustomAttribute<T>()))
                .Where(x => x.attr != null);

        static int RunInner()
        {
            var chapters = (Arg("-chapters", "") ?? "").ToLowerInvariant().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string onlyShot = Arg("-shot");
            string outDir = Arg("-outdir", Path.GetFullPath(Path.Combine(Application.dataPath, "../../src/img")));
            Directory.CreateDirectory(outDir);
            ShaderUtil.allowAsyncCompilation = false;
            Application.logMessageReceived += OnLog;
            BookSetup.EnsureSetup();

            bool InChapters(string s) => chapters.Length == 0 || chapters.Any(c => s.ToLowerInvariant().Contains("/" + c) || s.ToLowerInvariant().StartsWith(c));

            // 1) Shader 编译检查
            var shaderPaths = AssetDatabase.FindAssets("t:Shader", new[] { "Assets/UGUIBook" })
                .Select(AssetDatabase.GUIDToAssetPath).Where(p => InChapters(p)).ToList();
            foreach (var p in shaderPaths) CheckShader(p);
            Debug.Log($"[UGUIBook] Checked {shaderPaths.Count} shaders");

            // 2) 只生成场景的方法
            if (!Flag("-noscenes") && onlyShot == null)
            {
                foreach (var (m, attr) in Find<BookSceneAttribute>().Where(x => InChapters(x.attr.Name)).OrderBy(x => x.attr.Name))
                {
                    try
                    {
                        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                        var s = NewShot(0, 960, 540);
                        m.Invoke(null, new object[] { s });
                        SaveScene(attr.Name);
                    }
                    catch (Exception e) { Errors.Add($"SCENE {attr.Name}: {(e.InnerException ?? e).Message}\n{(e.InnerException ?? e).StackTrace}"); }
                }
            }

            // 3) 配图（+ 场景）
            if (!Flag("-noshots"))
            {
                var shots = Find<BookShotAttribute>()
                    .Where(x => InChapters(x.attr.Id) && (onlyShot == null || x.attr.Id == onlyShot))
                    .OrderBy(x => x.attr.Id).ToList();
                var ids = new HashSet<string>();
                foreach (var (m, attr) in shots)
                {
                    if (!ids.Add(attr.Id)) { Errors.Add("SHOT duplicate id " + attr.Id); continue; }
                    try { RenderShot(m, attr, outDir, !Flag("-noscenes")); }
                    catch (Exception e) { Errors.Add($"SHOT {attr.Id}: {(e.InnerException ?? e).Message}\n{(e.InnerException ?? e).StackTrace}"); }
                }
                Debug.Log($"[UGUIBook] Rendered {shots.Count} shots");
            }

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AssetDatabase.SaveAssets();
            Application.logMessageReceived -= OnLog;
            var sb = new StringBuilder();
            foreach (var e in Errors.Distinct()) sb.AppendLine("UGUIBOOK-ERROR " + e);
            sb.AppendLine($"UGUIBOOK-SUMMARY errors={Errors.Distinct().Count()}");
            Debug.Log(sb.ToString());
            File.WriteAllText(Path.Combine(outDir, "_last_report.txt"), sb.ToString());
            return Errors.Count > 0 ? 1 : 0;
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                if (msg.StartsWith("[Licensing") || msg.Contains("Licensing::")) return;
                Errors.Add("LOG " + msg + (type == LogType.Exception ? "\n" + stack : ""));
            }
        }

        static void CheckShader(string path)
        {
            var sh = AssetDatabase.LoadAssetAtPath<Shader>(path);
            if (sh == null) { Errors.Add("SHADER load failed " + path); return; }
            var mat = new Material(sh);
            for (int i = 0; i < mat.passCount; i++) ShaderUtil.CompilePass(mat, i, true);
            foreach (var m in ShaderUtil.GetShaderMessages(sh))
                if (m.severity == UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error)
                    Errors.Add($"SHADER {path}:{m.line} {m.message} [{m.platform}] {m.messageDetails}");
            Object.DestroyImmediate(mat);
        }

        // ---------------- 搭景 ----------------
        static Shot NewShot(int panel, int w, int h)
        {
            var s = new Shot { Panel = panel, W = w, H = h };
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            s.Cam = camGo.AddComponent<Camera>();
            s.Cam.clearFlags = CameraClearFlags.SolidColor;
            s.Cam.backgroundColor = UISkin.Instance.page;
            s.Cam.orthographic = true;
            s.Cam.transform.position = new Vector3(0, 0, -10);
            s.Cam.nearClipPlane = 0.3f; s.Cam.farClipPlane = 1000f;
            camGo.AddComponent<AudioListener>();
            s.Canvas = UIKit.Canvas("Canvas", RenderMode.ScreenSpaceOverlay, w * 2, h * 2);
            s.Scaler = s.Canvas.GetComponent<CanvasScaler>();
            s.Root = (RectTransform)s.Canvas.transform;
            UIKit.EnsureEventSystem();
            return s;
        }

        static string ChapterDir(string id)
        {
            // "ch06-xxx" / "Ch06_Xxx" -> "Ch06"
            var digits = new string(id.Skip(2).TakeWhile(char.IsDigit).ToArray());
            return "Ch" + digits;
        }

        static void SaveScene(string name)
        {
            string dir = $"Assets/UGUIBook/{ChapterDir(name)}/Scenes";
            Directory.CreateDirectory(dir);
            var scene = EditorSceneManager.GetActiveScene();
            if (!EditorSceneManager.SaveScene(scene, $"{dir}/{name}.unity"))
                Errors.Add("SCENE save failed " + name);
            else Debug.Log("[UGUIBook] scene " + name);
        }

        static void RenderShot(MethodInfo method, BookShotAttribute attr, string outDir, bool saveScenes)
        {
            var panels = new List<Texture2D>();
            int pw = Mathf.RoundToInt(attr.W * attr.Scale), ph = Mathf.RoundToInt(attr.H * attr.Scale);
            for (int p = 0; p < attr.Panels; p++)
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var s = NewShot(p, attr.W, attr.H);
                method.Invoke(null, new object[] { s });
                if (p == 0 && saveScenes && !string.IsNullOrEmpty(attr.Scene)) SaveScene(attr.Scene);
                if (p == 0) DumpTree(s, Path.Combine(outDir, "..", "tree", attr.Id + ".txt"));
                panels.Add(Capture(s, pw, ph));
                foreach (var o in s.Temp) if (o != null) Object.DestroyImmediate(o);
            }
            int gap = Mathf.RoundToInt(12 * attr.Scale);
            int w = pw * panels.Count + gap * (panels.Count - 1);
            var outTex = new Texture2D(w, ph, TextureFormat.RGB24, false);
            outTex.SetPixels32(Enumerable.Repeat(new Color32(255, 255, 255, 255), w * ph).ToArray());
            for (int i = 0; i < panels.Count; i++)
            {
                outTex.SetPixels(i * (pw + gap), 0, pw, ph, panels[i].GetPixels());
                Object.DestroyImmediate(panels[i]);
            }
            outTex.Apply();
            if (attr.Png) File.WriteAllBytes(Path.Combine(outDir, attr.Id + ".png"), outTex.EncodeToPNG());
            else File.WriteAllBytes(Path.Combine(outDir, attr.Id + ".jpg"), outTex.EncodeToJPG(90));
            Object.DestroyImmediate(outTex);
            Debug.Log("[UGUIBook] shot " + attr.Id);
        }

        /// 导出层级树（正文可以用 <div class="tree" data-tree="id"> 显示成 Hierarchy 面板的样子）
        static void DumpTree(Shot s, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var sb = new StringBuilder();
            void Walk(Transform t, int depth)
            {
                var comps = t.GetComponents<Component>().Where(c => c != null && !(c is Transform) && !(c is CanvasRenderer))
                    .Select(c => c.GetType().Name);
                sb.Append(depth).Append('|').Append(t.gameObject.activeSelf ? "" : "!").Append(t.name).Append('|')
                  .AppendLine(string.Join(",", comps));
                foreach (Transform c in t) Walk(c, depth + 1);
            }
            foreach (var go in EditorSceneManager.GetActiveScene().GetRootGameObjects()) Walk(go.transform, 0);
            File.WriteAllText(path, sb.ToString());
        }

        static void InvokeHandle(CanvasScaler sc) =>
            typeof(CanvasScaler).GetMethod("Handle", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(sc, null);

        static void Refresh()
        {
            foreach (var sc in Object.FindObjectsOfType<CanvasScaler>()) InvokeHandle(sc);
            Canvas.ForceUpdateCanvases();
            foreach (var t in Object.FindObjectsOfType<TMP_Text>()) t.ForceMeshUpdate();
            Canvas.ForceUpdateCanvases();
        }

        static Texture2D Capture(Shot s, int w, int h)
        {
            var desc = new RenderTextureDescriptor(w, h, RenderTextureFormat.ARGB32, 24) { sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear, msaaSamples = 4 };
            var rt = new RenderTexture(desc); rt.Create();
            s.Cam.targetTexture = rt;
            s.Cam.aspect = (float)w / h;
            // Overlay 画布不经过相机，截不到：临时改成 Screen Space - Camera，挂到截图相机上（排序靠 sortingOrder，距离统一）
            foreach (var c in Object.FindObjectsOfType<Canvas>())
            {
                if (!c.isRootCanvas || c.renderMode != RenderMode.ScreenSpaceOverlay) continue;
                c.renderMode = RenderMode.ScreenSpaceCamera;
                c.worldCamera = s.Cam;
                c.planeDistance = 100f;
            }
            foreach (var c in Object.FindObjectsOfType<Canvas>())
                if (c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceCamera && c.worldCamera == null) c.worldCamera = s.Cam;

            Refresh();
            foreach (var a in s.Before) a();
            Refresh();
            if (s.Annot.Count > 0)
            {
                var ac = UIKit.Canvas("Annotations", RenderMode.ScreenSpaceCamera, s.Scaler.referenceResolution.x, s.Scaler.referenceResolution.y, s.Scaler.matchWidthOrHeight);
                // 标注贴着近裁剪面放，透视相机下也不会被近处的 3D 物体挡住
                ac.worldCamera = s.Cam; ac.planeDistance = s.Cam.nearClipPlane + 0.05f; ac.sortingOrder = 30000;
                s.AnnotRoot = (RectTransform)ac.transform;
                Refresh();
                foreach (var a in s.Annot) a();
                Refresh();
            }
            s.Cam.Render();
            var prev = RenderTexture.active;
            var resolved = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, desc.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
            Graphics.Blit(rt, resolved);
            RenderTexture.active = resolved;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontSave };
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(resolved);
            s.Cam.targetTexture = null;
            rt.Release(); Object.DestroyImmediate(rt);
            return tex;
        }
    }
}
