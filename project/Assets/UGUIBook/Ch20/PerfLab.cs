// 第 20 章“性能实验室”：在 Play 模式下逐个运行 PerfLabCases 里的实验，每个实验预热若干帧后连续测 N 帧取平均。
// 每帧测的东西：
//   · 本帧排队的布局/图形重建元素数（RebuildCounter），以及各 Graphic 被标脏的次数（Graphic 的 Dirty 回调）
//   · Canvas.ForceUpdateCanvases() 的耗时 = UGUI 的 C# 侧重建（布局 + 网格生成）
//   · Unity 自带的计数器与标记（ProfilerRecorder）：Batches、SetPass、三角形数、Canvas.BuildBatch、托管内存分配
// 运行方式：打开 Ch20/Scenes/Ch20_PerfLab.unity 点 Play；或菜单 UGUIBook › Ch20 › Run Perf Lab。
// 结果显示在屏幕上，同时写入 Ch20/Results/perflab_results.txt（编辑器）或 persistentDataPath（真机）。
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace UGUIBook.Ch20
{
    /// 一个实验能用到的东西
    public class LabContext
    {
        public PerfLab Lab;
        public Canvas Canvas;            // 本实验专用的根画布（Screen Space - Camera）
        public RectTransform Root;
        public Camera UICam, WorldCam;
        public bool RenderWorld;         // 本实验是否同时渲染 3D 相机
        public string Tag;               // 本帧的分组标签（如 show / hide），带标签的指标分开统计
        public readonly List<GameObject> Extra = new List<GameObject>();   // 画布以外创建的物体，实验结束时一起销毁

        internal readonly Dictionary<(string, string), (double sum, int n)> Sums = new Dictionary<(string, string), (double, int)>();
        internal readonly Dictionary<string, double> Once = new Dictionary<string, double>();
        internal bool Measuring;

        /// 累计一个逐帧指标（只在测量阶段记录，最后取平均）
        public void Add(string metric, double v)
        {
            if (!Measuring) return;
            var key = (metric, Tag);
            Sums.TryGetValue(key, out var a);
            Sums[key] = (a.sum + v, a.n + 1);
        }

        /// 记录一次性指标（比如“首次排版耗时”）
        public void Set(string metric, double v) => Once[metric] = v;
    }

    public abstract class LabCase
    {
        public string Group, Id, Title;
        public int Frames = 60;
        public abstract void Setup(LabContext c);
        public virtual void Tick(LabContext c, int frame) { }
    }

    public class PerfLab : MonoBehaviour
    {
        [SerializeField] Camera m_UICamera;
        [SerializeField] Camera m_WorldCamera;
        [SerializeField] RawImage m_Preview;          // 屏幕上预览当前实验画面
        [SerializeField] TMP_Text m_Report;           // 屏幕上的结果表
        [Tooltip("打进 SpriteAtlas 的精灵（与 UISkin 里的同名精灵内容一样，只是来自图集）")]
        public Sprite[] atlasSprites;
        public RuntimeAnimatorController idleController;   // 曲线为常量的动画
        public RuntimeAnimatorController pulseController;  // 透明度真的在变的动画
        [Tooltip("只跑某一组实验（留空 = 全部）：rebuild layout hide batch raycast tmp pool anim world")]
        public string onlyGroup = "";
        [Tooltip("批处理模式下跑完自动退出编辑器")]
        public bool exitWhenDone;

        const int WarmFrames = 10;
        public const int RTWidth = 1920, RTHeight = 1080;

        List<LabCase> m_Cases;
        int m_Index = -1, m_Frame;
        bool m_Warm;
        LabCase m_Case;
        LabContext m_Ctx;
        string m_PrevTag;
        RenderTexture m_RT;
        readonly Stopwatch m_Sw = new Stopwatch();
        readonly List<string> m_Lines = new List<string>();
        readonly StringBuilder m_Screen = new StringBuilder();

        //#region recorders
        ProfilerRecorder m_Batches, m_SetPass, m_Tris, m_BuildBatch, m_UpdateBatches, m_Gc;
        // Canvas 合批的大部分工作在工作线程的 Job 里做，主线程的 Canvas.BuildBatch 只是调度
        static readonly string[] s_JobMarkers = { "Canvas.GeometryJob", "Canvas.PrepareBatchesJob", "Canvas.SortJob",
            "Canvas.CombineInstructionsJob", "Canvas.SplitInstructionsJob" };
        ProfilerRecorder[] m_Jobs;
        int m_DirtyLayout, m_DirtyVerts, m_DirtyMat;
        UnityEngine.Events.UnityAction m_OnLayout, m_OnVerts, m_OnMat;

        void OnEnable()
        {
            m_Batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            m_SetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            m_Tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            m_BuildBatch = ProfilerRecorder.StartNew(ProfilerCategory.Gui, "Canvas.BuildBatch");
            m_UpdateBatches = ProfilerRecorder.StartNew(ProfilerCategory.Gui, "UGUI.Rendering.UpdateBatches");
            m_Gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            m_Jobs = s_JobMarkers.Select(n => ProfilerRecorder.StartNew(ProfilerCategory.Render, n)).ToArray();
            // 委托只建一次，避免测量过程本身产生 GC
            m_OnLayout = () => m_DirtyLayout++;
            m_OnVerts = () => m_DirtyVerts++;
            m_OnMat = () => m_DirtyMat++;
        }

        void OnDisable()
        {
            m_Batches.Dispose(); m_SetPass.Dispose(); m_Tris.Dispose();
            m_BuildBatch.Dispose(); m_UpdateBatches.Dispose(); m_Gc.Dispose();
            foreach (var r in m_Jobs) r.Dispose();
            if (m_RT != null) m_RT.Release();
        }
        //#endregion

        void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            m_RT = new RenderTexture(RTWidth, RTHeight, 24, RenderTextureFormat.ARGB32) { name = "PerfLabRT" };
            m_UICamera.targetTexture = m_RT;
            m_UICamera.enabled = false;      // 不让相机自动渲染，每帧由实验室手动 Render，渲染次数可控
            if (m_WorldCamera != null) { m_WorldCamera.targetTexture = m_RT; m_WorldCamera.enabled = false; }
            if (m_Preview != null) m_Preview.texture = m_RT;
            m_Cases = PerfLabCases.All(this).Where(c => string.IsNullOrEmpty(onlyGroup) || c.Group == onlyGroup).ToList();
            m_Lines.Add("# env " + Env());
            Debug.Log("LAB env " + Env());
        }

        public static string Env() =>
            $"{SystemInfo.processorType} ({SystemInfo.processorCount} cores) / {SystemInfo.operatingSystem} / " +
            $"Unity {Application.unityVersion} / {SystemInfo.graphicsDeviceType} / " +
            (Application.isEditor ? (Application.isBatchMode ? "Editor Play Mode (batchmode)" : "Editor Play Mode") : "Player");

        void Update()
        {
            if (m_Case == null && !NextCase()) return;
            m_Ctx.Tag = null;
            m_Ctx.Measuring = !m_Warm;
            m_Case.Tick(m_Ctx, m_Frame);
        }

        //#region measure
        void LateUpdate()
        {
            if (m_Case == null) return;
            var c = m_Ctx;
            // ① 本帧攒下的重建队列（此时还没执行 willRenderCanvases）
            c.Add("layoutQ", RebuildCounter.LayoutCount);
            c.Add("graphicQ", RebuildCounter.GraphicCount);
            // ② C# 侧重建：布局 + 网格生成，全在 CanvasUpdateRegistry.PerformUpdate 里
            m_Sw.Restart();
            Canvas.ForceUpdateCanvases();
            c.Add("rebuild_ms", m_Sw.Elapsed.TotalMilliseconds);
            // ③ 渲染（UI 画布在这里被原生层合批、提交）
            if (c.RenderWorld)
            {
                m_Sw.Restart(); c.WorldCam.Render();
                c.Add("world_ms", m_Sw.Elapsed.TotalMilliseconds);
            }
            m_UICamera.clearFlags = c.RenderWorld ? CameraClearFlags.Depth : CameraClearFlags.SolidColor;
            m_Sw.Restart();
            m_UICamera.Render();
            c.Add("render_ms", m_Sw.Elapsed.TotalMilliseconds);
            c.Add("dirtyLayout", m_DirtyLayout);
            c.Add("dirtyVerts", m_DirtyVerts);
            c.Add("dirtyMat", m_DirtyMat);
            m_DirtyLayout = m_DirtyVerts = m_DirtyMat = 0;

            // ④ 计数器给的是“上一帧”的值，所以用上一帧的标签记账
            var tag = c.Tag;
            c.Tag = m_PrevTag;
            c.Add("batches", m_Batches.LastValue);
            c.Add("setpass", m_SetPass.LastValue);
            c.Add("tris", m_Tris.LastValue);
            c.Add("buildbatch_ms", m_BuildBatch.LastValue * 1e-6);
            c.Add("updatebatches_ms", m_UpdateBatches.LastValue * 1e-6);
            c.Add("gc_kb", m_Gc.LastValue / 1024.0);
            long jobs = 0;
            for (int i = 0; i < m_Jobs.Length; i++) jobs += m_Jobs[i].LastValue;
            c.Add("canvasjobs_ms", jobs * 1e-6);
            c.Tag = tag;
            m_PrevTag = tag;

            m_Frame++;
            if (m_Warm && m_Frame >= WarmFrames) { m_Warm = false; m_Frame = 0; }
            else if (!m_Warm && m_Frame >= m_Case.Frames) FinishCase();
        }
        //#endregion

        bool NextCase()
        {
            m_Index++;
            if (m_Index >= m_Cases.Count) { Done(); enabled = false; return false; }
            m_Case = m_Cases[m_Index];
            GC.Collect();
            m_Ctx = new LabContext { Lab = this, UICam = m_UICamera, WorldCam = m_WorldCamera };
            var go = new GameObject("Case_" + m_Case.Id, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = m_UICamera;
            canvas.planeDistance = 100;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(RTWidth, RTHeight);
            go.AddComponent<GraphicRaycaster>();
            m_Ctx.Canvas = canvas;
            m_Ctx.Root = (RectTransform)go.transform;
            m_Case.Setup(m_Ctx);
            // 给本实验的所有 Graphic 挂上“被标脏”回调，统计每帧各类标脏次数
            foreach (var g in go.GetComponentsInChildren<Graphic>(true))
            {
                g.RegisterDirtyLayoutCallback(m_OnLayout);
                g.RegisterDirtyVerticesCallback(m_OnVerts);
                g.RegisterDirtyMaterialCallback(m_OnMat);
            }
            m_Warm = true; m_Frame = 0; m_PrevTag = null;
            return true;
        }

        void FinishCase()
        {
            var sb = new StringBuilder();
            sb.Append(m_Case.Group).Append('|').Append(m_Case.Id).Append('|').Append(m_Case.Title).Append('|');
            foreach (var kv in m_Ctx.Sums.OrderBy(k => k.Key.Item1).ThenBy(k => k.Key.Item2))
                sb.Append(kv.Key.Item1).Append(kv.Key.Item2 != null ? "@" + kv.Key.Item2 : "").Append('=')
                  .Append((kv.Value.sum / Math.Max(1, kv.Value.n)).ToString("0.####")).Append(';');
            foreach (var kv in m_Ctx.Once) sb.Append(kv.Key).Append('=').Append(kv.Value.ToString("0.####")).Append(';');
            var line = sb.ToString();
            m_Lines.Add(line);
            Debug.Log("LAB " + line);
            ShowOnScreen();

            Destroy(m_Ctx.Root.gameObject);
            foreach (var o in m_Ctx.Extra) if (o != null) Destroy(o);
            m_Case = null;
        }

        void ShowOnScreen()
        {
            if (m_Report == null) return;
            m_Screen.Clear();
            m_Screen.Append($"<b>性能实验室</b>  {m_Index + 1}/{m_Cases.Count}\n");
            foreach (var l in m_Lines.Skip(Math.Max(1, m_Lines.Count - 14)))
            {
                var p = l.Split('|');
                if (p.Length < 4) continue;
                var m = p[3].Split(';').Where(x => x.StartsWith("rebuild_ms=") || x.StartsWith("buildbatch_ms=") || x.StartsWith("batches="));
                m_Screen.Append(p[2]).Append("  <color=#7a8299>").Append(string.Join("  ", m)).Append("</color>\n");
            }
            m_Report.text = m_Screen.ToString();
        }

        void Done()
        {
            string path = Application.isEditor
                ? Path.Combine(Application.dataPath, "UGUIBook/Ch20/Results/perflab_results.txt")
                : Path.Combine(Application.persistentDataPath, "perflab_results.txt");
            // 只跑了某一组时，把结果合并进已有文件（同 id 覆盖）
            var merged = new List<string>();
            if (!string.IsNullOrEmpty(onlyGroup) && File.Exists(path))
            {
                var ids = new HashSet<string>(m_Lines.Select(l => l.Split('|')).Where(p => p.Length > 1).Select(p => p[1]));
                merged.AddRange(File.ReadAllLines(path).Where(l => !l.StartsWith("#") && !ids.Contains(l.Split('|').ElementAtOrDefault(1) ?? "")));
            }
            merged.InsertRange(0, m_Lines);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllLines(path, merged);
            Debug.Log("LAB done -> " + path);
            if (m_Report != null) m_Report.text += "\n全部完成，结果已写入 " + path;
#if UNITY_EDITOR
            if (exitWhenDone && Application.isBatchMode) UnityEditor.EditorApplication.Exit(0);
#endif
        }
    }
}
