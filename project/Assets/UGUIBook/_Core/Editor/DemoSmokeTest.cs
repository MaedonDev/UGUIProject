// 作者用的冒烟测试（命令行，不进书）：
//   Unity -batchmode -projectPath <工程> -executeMethod UGUIBook.Core.DemoSmokeTest.Run -ugbSmoke 1 [-ugbFrom Ch05]
// 打开 Demos.unity → 进入 Play 模式 → 依次点目录里的每个按钮打开场景、跑若干帧、点“返回目录”，
// 记录所有运行时报错（截图流程不进 Play 模式，Awake/Start/Update 里的问题只有这样才能发现）。
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGUIBook.Core
{
    [InitializeOnLoad]
    static class DemoSmokeTest
    {
        static string Arg(string name)
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
            return null;
        }

        static readonly bool Active = Arg("-ugbSmoke") != null;
        static readonly string From = Arg("-ugbFrom");
        static readonly string ShotPath = Arg("-ugbShot");     // 可选：把目录界面截一张图（检查运行时生成的按钮）
        const int FramesPerScene = 90;

        static int s_Frame, s_Index = -1, s_Wait;
        static List<Button> s_Buttons;
        static List<string> s_Names;
        static string s_Current = "Demos";
        static readonly List<string> s_Errors = new List<string>();
        static readonly HashSet<string> s_Seen = new HashSet<string>();
        static RenderTexture s_Rt;

        static DemoSmokeTest()
        {
            if (!Active) return;
            EditorApplication.update += Tick;
            Application.logMessageReceived += OnLog;
        }

        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/UGUIBook/Demos.unity");
            EditorApplication.EnterPlaymode();
        }

        static void OnLog(string msg, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (msg.StartsWith("[Smoke]")) return;
            var key = s_Current + "|" + msg;
            if (!s_Seen.Add(key)) return;
            var first = stack?.Split('\n').FirstOrDefault(l => l.Contains("UGUIBook")) ?? "";
            s_Errors.Add($"{s_Current}: {type}: {msg.Split('\n')[0]}  @ {first.Trim()}");
        }

        // batchmode 没有 Game 视图：让当前场景的相机都画到 RT 上，保证渲染路径真的走一遍
        static void RouteCameras()
        {
            if (s_Rt == null) s_Rt = new RenderTexture(1920, 1080, 24);
            foreach (var c in Camera.allCameras) if (c.targetTexture == null) c.targetTexture = s_Rt;
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            s_Frame++;
            if (s_Frame > 200000) { Finish("timeout"); return; }
            RouteCameras();
            if (s_Wait-- > 0) return;

            if (s_Buttons == null)
            {
                var launcher = UnityEngine.Object.FindObjectOfType<DemoLauncher>();
                if (launcher == null) { Finish("no DemoLauncher"); return; }
                s_Buttons = launcher.Content.GetComponentsInChildren<Button>().ToList();
                s_Names = s_Buttons.Select(b => b.name).ToList();
                Debug.Log($"[Smoke] launcher shows {s_Buttons.Count} buttons, entries {launcher.Entries.Count}");
                if (s_Buttons.Count != launcher.Entries.Count) s_Errors.Add($"Demos: button count {s_Buttons.Count} != entries {launcher.Entries.Count}");
                if (ShotPath != null) { Capture(ShotPath); Finish(null); return; }
                if (From != null) s_Index = s_Names.FindIndex(n => n.StartsWith(From)) - 1;
                s_Wait = 5;
                return;
            }

            if (SceneManager.GetActiveScene().name != "Demos")
            {
                // 在 demo 场景里跑完了：检查返回按钮，再点它回目录
                var back = GameObject.Find("DemoBackCanvas");
                if (back == null || !back.activeSelf) { s_Errors.Add($"{s_Current}: back button missing"); SceneManager.LoadScene("Demos"); }
                else back.GetComponentInChildren<Button>().onClick.Invoke();
                s_Current = "Demos"; s_Buttons = null; s_Wait = 5;
                return;
            }

            s_Index++;
            if (s_Index >= s_Names.Count) { Finish(null); return; }
            s_Current = s_Names[s_Index];
            var btn = s_Buttons.FirstOrDefault(b => b != null && b.name == s_Current);
            if (btn == null) { s_Errors.Add($"{s_Current}: launcher button missing"); return; }
            btn.onClick.Invoke();
            s_Wait = FramesPerScene;
        }

        static void Capture(string path)
        {
            var cam = Camera.main;
            foreach (var c in UnityEngine.Object.FindObjectsOfType<Canvas>())
                if (c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 10; }
            var rt = new RenderTexture(1920, 1080, 24);
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            System.IO.File.WriteAllBytes(path, tex.EncodeToJPG(90));
            RenderTexture.active = null;
            Debug.Log("[Smoke] captured " + path);
        }

        static void Finish(string fatal)
        {
            if (fatal != null) s_Errors.Add("FATAL: " + fatal);
            Debug.Log($"[Smoke] visited {Math.Max(0, s_Index)} scenes");
            foreach (var e in s_Errors) Debug.Log("[Smoke] ERR " + e);
            Debug.Log($"[Smoke] SUMMARY errors={s_Errors.Count}");
            EditorApplication.Exit(s_Errors.Count == 0 ? 0 : 1);
        }
    }
}
