// 作者用来批量实测的入口（命令行，不进书）：
//   Unity -batchmode -projectPath <工程> -executeMethod UGUIBook.Ch12.Ch12LabRunner.Run -ugbLab rebuild|batch
// 打开对应实验场景 → 进入 Play 模式 → 跑基准 → 把结果写到 Assets/UGUIBook/Ch12/Data/ → 退出。
// 读者直接打开场景点“跑一遍基准 / 测量全部”按钮，得到的是同一套代码的结果。
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UGUIBook.Ch12
{
    [InitializeOnLoad]
    static class Ch12LabRunner
    {
        static string Arg(string name)
        {
            var a = Environment.GetCommandLineArgs();
            for (int i = 0; i < a.Length - 1; i++) if (a[i] == name) return a[i + 1];
            return null;
        }

        static readonly string Which = Arg("-ugbLab");
        static int s_Frames;
        static bool s_Started;

        static Ch12LabRunner()
        {
            if (Which != null) EditorApplication.update += Tick;
        }

        public static void Run()
        {
            string scene = Which == "batch" ? "Ch12_BatchLab" : "Ch12_RebuildLab";
            EditorSceneManager.OpenScene($"Assets/UGUIBook/Ch12/Scenes/{scene}.unity");
            EditorApplication.EnterPlaymode();
        }

        static void Tick()
        {
            if (!EditorApplication.isPlaying) return;
            s_Frames++;
            if (s_Frames > 30000) { Debug.LogError("[Ch12Lab] timeout"); EditorApplication.Exit(1); return; }
            if (!s_Started && s_Frames > 10)
            {
                s_Started = true;
                // batchmode 没有 Game 视图，让主相机画到 RenderTexture 上，保证真的发生渲染与合批
                var cam = Camera.main;
                cam.targetTexture = new RenderTexture(1920, 1080, 24);
                if (Which == "batch") { var lab = UnityEngine.Object.FindObjectOfType<BatchLab>(); lab.StartCoroutine(lab.MeasureAll()); }
                else { var lab = UnityEngine.Object.FindObjectOfType<RebuildLab>(); lab.StartCoroutine(lab.RunBenchmark()); }
                return;
            }
            if (!s_Started) return;
            string report = null;
            if (Which == "batch") { var lab = UnityEngine.Object.FindObjectOfType<BatchLab>(); if (lab.done) report = lab.lastReport; }
            else { var lab = UnityEngine.Object.FindObjectOfType<RebuildLab>(); if (lab.benchmarkDone) report = lab.lastReport; }
            if (report == null) return;

            string env = $"# 环境：Unity {Application.unityVersion} / {SystemInfo.operatingSystem} / {SystemInfo.processorType} / {SystemInfo.graphicsDeviceName} / 编辑器 Play 模式（-batchmode），{DateTime.Now:yyyy-MM-dd}\n";
            Directory.CreateDirectory("Assets/UGUIBook/Ch12/Data");
            string path = $"Assets/UGUIBook/Ch12/Data/{(Which == "batch" ? "batch" : "rebuild")}_results.txt";
            File.WriteAllText(path, env + report);
            Debug.Log("[Ch12Lab] wrote " + path + "\n" + env + report);
            EditorApplication.Exit(0);
        }
    }
}
