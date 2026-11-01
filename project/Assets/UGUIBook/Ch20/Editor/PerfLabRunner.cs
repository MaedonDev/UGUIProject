// 搭建 / 运行“性能实验室”场景。
// 菜单 UGUIBook › Ch20 › Run Perf Lab：生成场景 → 临时启用 Sprite Packer → 进入 Play 模式跑全部实验。
// 批处理：Unity -batchmode -projectPath <工程> -executeMethod UGUIBook.Ch20.PerfLabRunner.RunBatch [-labGroup tmp]
using System;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    public static class PerfLabRunner
    {
        public const string ScenePath = "Assets/UGUIBook/Ch20/Scenes/Ch20_PerfLab.unity";
        const string PrefKey = "UGUIBook.Ch20.PrevPackerMode";

        /// 在当前（空）场景里搭实验室：显示用相机 + 两台手动渲染的实验相机 + 结果面板
        public static PerfLab Build()
        {
            Ch20Assets.EnsureAll();
            var display = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            display.clearFlags = CameraClearFlags.SolidColor;
            display.backgroundColor = UIKit.Hex("#1F2436");
            display.cullingMask = 0;                      // 只负责清屏，界面全在 Overlay 画布上

            var uiCam = new GameObject("LabUICamera").AddComponent<Camera>();
            uiCam.clearFlags = CameraClearFlags.SolidColor;
            uiCam.backgroundColor = UISkin.Instance.page;
            uiCam.cullingMask = 1 << LayerMask.NameToLayer("UI");
            uiCam.transform.position = new Vector3(0, 0, -10);

            var worldCam = new GameObject("LabWorldCamera").AddComponent<Camera>();
            worldCam.cullingMask = 1 << 0;               // Default 层：实验里的 3D 方块
            worldCam.clearFlags = CameraClearFlags.SolidColor;
            worldCam.backgroundColor = new Color(0.2f, 0.25f, 0.35f);
            worldCam.transform.position = new Vector3(0, 0, -20);
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 结果面板（Overlay）：左边预览实验画面，右边滚动显示结果
            var canvas = UIKit.Canvas("LabDisplay");
            canvas.sortingOrder = 100;
            var root = (RectTransform)canvas.transform;
            UIKit.Panel(root, UIKit.Hex("#1F2436"), 0, "Bg").rectTransform.Stretch();
            var title = UIKit.Text(root, "第 20 章 性能实验室：点 Play 后自动逐项运行，约 1 分钟", 34, Color.white, TextAlignmentOptions.MidlineLeft, true);
            title.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -30), new Vector2(1800, 60));
            var preview = new GameObject("Preview", typeof(RectTransform)).AddComponent<RawImage>();
            preview.rectTransform.SetParent(root, false);
            preview.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -110), new Vector2(880, 495));
            preview.raycastTarget = false;
            var report = UIKit.Text(root, "等待开始……", 22, UIKit.Hex("#E0DEEF"), TextAlignmentOptions.TopLeft);
            report.rectTransform.Place(new Vector2(0, 1), new Vector2(960, -110), new Vector2(920, 940));
            report.enableWordWrapping = true;
            var hudText = UIKit.Text(root, "", 24, UIKit.Hex("#FF8A3D"), TextAlignmentOptions.TopLeft);
            hudText.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -640), new Vector2(880, 200));
            var hud = canvas.gameObject.AddComponent<RenderStatsHUD>();
            var hso = new SerializedObject(hud);
            hso.FindProperty("m_Label").objectReferenceValue = hudText;
            hso.ApplyModifiedPropertiesWithoutUndo();
            UIKit.EnsureEventSystem();

            var lab = new GameObject("PerfLab").AddComponent<PerfLab>();
            var so = new SerializedObject(lab);
            so.FindProperty("m_UICamera").objectReferenceValue = uiCam;
            so.FindProperty("m_WorldCamera").objectReferenceValue = worldCam;
            so.FindProperty("m_Preview").objectReferenceValue = preview;
            so.FindProperty("m_Report").objectReferenceValue = report;
            so.ApplyModifiedPropertiesWithoutUndo();
            lab.atlasSprites = Ch20Assets.LoadAtlasSprites();
            lab.idleController = Ch20Assets.EnsureIdleController();
            lab.pulseController = Ch20Assets.EnsurePulseController();
            return lab;
        }

        [MenuItem("UGUIBook/Ch20/Run Perf Lab")]
        public static void RunMenu() => Run(false, "");

        public static void RunBatch()
        {
            var args = Environment.GetCommandLineArgs();
            int i = Array.IndexOf(args, "-labGroup");
            Run(true, i >= 0 && i + 1 < args.Length ? args[i + 1] : "");
        }

        static void Run(bool batch, string group)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var lab = Build();
            lab.exitWhenDone = batch;
            lab.onlyGroup = group;
            System.IO.Directory.CreateDirectory("Assets/UGUIBook/Ch20/Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            // SpriteAtlas 只有在 Sprite Packer Mode 启用时才会在编辑器里生效；临时打开，退出 Play 模式时恢复
            if (EditorSettings.spritePackerMode == SpritePackerMode.Disabled)
            {
                EditorPrefs.SetInt(PrefKey, (int)EditorSettings.spritePackerMode);
                EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            }
            Ch20Assets.PackAtlas();
            EditorApplication.isPlaying = true;
        }

        [InitializeOnLoadMethod]
        static void RestorePackerMode()
        {
            void Restore()
            {
                if (!EditorPrefs.HasKey(PrefKey)) return;
                EditorSettings.spritePackerMode = (SpritePackerMode)EditorPrefs.GetInt(PrefKey);
                EditorPrefs.DeleteKey(PrefKey);
            }
            EditorApplication.playModeStateChanged += s => { if (s == PlayModeStateChange.EnteredEditMode) Restore(); };
            if (!EditorApplication.isPlayingOrWillChangePlaymode) Restore();
        }
    }
}
