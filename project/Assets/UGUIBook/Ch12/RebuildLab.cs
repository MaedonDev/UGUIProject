// 第 12 章实验台：对一批 Image / Text / TextMeshPro 做某种修改，统计这一帧里
//   ① 触发了哪种“标脏”（layout / vertices / material），② 两个重建队列里排了多少元素，
//   ③ 真正执行了多少次 UpdateGeometry / UpdateMaterial，④ Layout、图形重建、Canvas 合批各花了多少时间。
// 场景里可以点按钮手动试；“跑一遍基准”会把每种操作连续执行 60 帧取平均，结果表显示在右侧。
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    public class RebuildLab : MonoBehaviour
    {
        public enum Op
        {
            None, ImageColor, ImageMove, ImageSize, ImageScale, SpriteSameTexture, SpriteOtherTexture, SpriteOtherSize,
            ImageMaterial, GroupAlpha, LegacyText, TmpTextInLayout, TmpColor, ToggleActive,
        }

        public static readonly string[] OpNames =
        {
            "无操作（基线）", "Image.color", "移动 anchoredPosition", "改 sizeDelta", "改 localScale", "换 sprite：同纹理同尺寸",
            "换 sprite：不同纹理同尺寸", "换 sprite：尺寸不同", "换 material", "CanvasGroup.alpha", "Text.text（旧版）",
            "TMP.text（在 VLG 里）", "TMP.color", "SetActive 关/开",
        };

        [SerializeField] RectTransform m_ImageRoot;     // 400 个 ProbeImage，挂 CanvasGroup
        [SerializeField] RectTransform m_LegacyRoot;    // 旧版 Text
        [SerializeField] RectTransform m_TmpRoot;       // TextMeshPro，挂 VerticalLayoutGroup
        [SerializeField] Material m_AltMaterial;
        [SerializeField] TMP_Text m_Readout;
        [SerializeField] RectTransform m_ButtonRoot;    // 按 Op 顺序排列的按钮
        [SerializeField] Toggle m_Continuous;
        [SerializeField] Button m_Benchmark;

        ProbeImage[] m_Images;
        Text[] m_Texts;
        TMP_Text[] m_Tmps;
        CanvasGroup m_Group;
        Sprite m_SpriteA, m_SpriteA2, m_SpriteB, m_SpriteSmall;
        Op m_Op;
        bool m_ApplyOnce;
        int m_Tick;
        bool m_Benchmarking;

        ProfilerRecorder m_LayoutRec, m_RenderRec, m_BatchRec;

        /// 最近一次基准的结果（制表符分隔，便于粘进表格）
        public string lastReport { get; private set; }
        public bool benchmarkDone { get; private set; }

        public struct FrameStats
        {
            public int layoutDirty, vertsDirty, materialDirty, layoutQueue, graphicQueue, geometry, material;
            public double layoutMs, renderMs, batchMs;
        }

        void Awake()
        {
            m_Images = m_ImageRoot.GetComponentsInChildren<ProbeImage>(true);
            m_Texts = m_LegacyRoot.GetComponentsInChildren<Text>(true);
            m_Tmps = m_TmpRoot.GetComponentsInChildren<TMP_Text>(true);
            m_Group = m_ImageRoot.GetComponent<CanvasGroup>();
            MakeSprites();
            foreach (var img in m_Images) img.sprite = m_SpriteA;
            foreach (var g in m_Images) RebuildCounter.Watch(g);
            foreach (var g in m_Texts) RebuildCounter.Watch(g);
            foreach (var g in m_Tmps) RebuildCounter.Watch(g);

            for (int i = 0; i < m_ButtonRoot.childCount && i < OpNames.Length; i++)
            {
                var op = (Op)i;
                var b = m_ButtonRoot.GetChild(i).GetComponent<Button>();
                if (b != null) b.onClick.AddListener(() => { m_Op = op; m_ApplyOnce = true; });
            }
            if (m_Benchmark != null) m_Benchmark.onClick.AddListener(() => { if (!m_Benchmarking) StartCoroutine(RunBenchmark()); });

            m_LayoutRec = Recorder("Layout");                // CanvasUpdateRegistry.PerformUpdate 的布局部分（UISystemProfilerApi 采样）
            m_RenderRec = Recorder("Render", "UI Render");   // 同上，图形重建部分
            m_BatchRec = Recorder("Canvas.BuildBatch");      // 原生层：把画布里的网格合成批次
        }

        void OnDestroy() { m_LayoutRec.Dispose(); m_RenderRec.Dispose(); m_BatchRec.Dispose(); }

        //#region recorder
        /// 按名字找到 Profiler 采样点并开始录制（每帧把同名采样的耗时加总）
        static ProfilerRecorder Recorder(string name, string category = null)
        {
            var list = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(list);
            foreach (var h in list)
            {
                var d = ProfilerRecorderHandle.GetDescription(h);
                if (d.Name == name && (category == null || d.Category.Name == category))
                {
                    var r = new ProfilerRecorder(h, 1, ProfilerRecorderOptions.Default);
                    r.Start();
                    return r;
                }
            }
            return default;
        }

        static double Ms(ProfilerRecorder r) => r.Valid ? r.LastValue / 1e6 : -1;
        //#endregion

        // 程序生成 3 张贴图：A（含两个同尺寸的子图）、B（与 A 同尺寸的另一张纹理）
        void MakeSprites()
        {
            var texA = Gen(128, 64, new Color(0.36f, 0.29f, 0.84f), new Color(1f, 0.54f, 0.24f));
            var texB = Gen(64, 64, new Color(0.18f, 0.7f, 0.42f), Color.white);
            m_SpriteA = Sprite.Create(texA, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
            m_SpriteA2 = Sprite.Create(texA, new Rect(64, 0, 64, 64), Vector2.one * 0.5f);
            m_SpriteB = Sprite.Create(texB, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
            m_SpriteSmall = Sprite.Create(texA, new Rect(16, 16, 32, 32), Vector2.one * 0.5f);
        }

        static Texture2D Gen(int w, int h, Color a, Color b)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontSave };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) px[y * w + x] = ((x / 8 + y / 8) & 1) == 0 ? a : b;
            t.SetPixels(px); t.Apply();
            return t;
        }

        //#region apply
        /// 对所有对象执行一次修改；odd 让相邻两帧的值来回切换，保证每帧都是“真的改了”
        public void Apply(Op op, bool odd)
        {
            switch (op)
            {
                case Op.ImageColor: foreach (var i in m_Images) i.color = odd ? Color.white : new Color(0.9f, 0.9f, 1f); break;
                case Op.ImageMove: foreach (var i in m_Images) i.rectTransform.anchoredPosition += new Vector2(odd ? 1 : -1, 0); break;
                case Op.ImageSize: foreach (var i in m_Images) i.rectTransform.sizeDelta += new Vector2(odd ? 1 : -1, 0); break;
                case Op.ImageScale: foreach (var i in m_Images) i.rectTransform.localScale = Vector3.one * (odd ? 1.05f : 1f); break;
                case Op.SpriteSameTexture: foreach (var i in m_Images) i.sprite = odd ? m_SpriteA2 : m_SpriteA; break;
                case Op.SpriteOtherTexture: foreach (var i in m_Images) i.sprite = odd ? m_SpriteB : m_SpriteA; break;
                case Op.SpriteOtherSize: foreach (var i in m_Images) i.sprite = odd ? m_SpriteSmall : m_SpriteA; break;
                case Op.ImageMaterial: foreach (var i in m_Images) i.material = odd ? m_AltMaterial : null; break;
                case Op.GroupAlpha: m_Group.alpha = odd ? 0.8f : 1f; break;
                case Op.LegacyText: for (int k = 0; k < m_Texts.Length; k++) m_Texts[k].text = $"得分 {(odd ? 100 : 200) + k}"; break;
                case Op.TmpTextInLayout: for (int k = 0; k < m_Tmps.Length; k++) m_Tmps[k].text = $"第 {k} 行：{(odd ? "胜利" : "失败")}"; break;
                case Op.TmpColor: foreach (var t in m_Tmps) t.color = odd ? Color.white : new Color(0.8f, 0.85f, 1f); break;
                case Op.ToggleActive: foreach (var i in m_Images) i.gameObject.SetActive(!odd); break;
            }
        }
        //#endregion

        //#region frame
        // 时间线：Update(第 N 帧)改数据 → LateUpdate 记下队列长度 → 帧末 Canvas.willRenderCanvases 执行重建、合批
        //        → Update(第 N+1 帧)开头读出第 N 帧的执行次数与耗时
        FrameStats m_Pending;

        void Update()
        {
            m_Pending.geometry = ProbeImage.geometryUpdates;
            m_Pending.material = ProbeImage.materialUpdates;
            m_Pending.layoutDirty = RebuildCounter.layoutDirty;
            m_Pending.vertsDirty = RebuildCounter.verticesDirty;
            m_Pending.materialDirty = RebuildCounter.materialDirty;
            m_Pending.layoutMs = Ms(m_LayoutRec);
            m_Pending.renderMs = Ms(m_RenderRec);
            m_Pending.batchMs = Ms(m_BatchRec);
            lastFrame = m_Pending;
            OnFrameMeasured?.Invoke(m_Pending);

            ProbeImage.ResetCounters();
            RebuildCounter.Reset();
            m_Pending = default;

            bool continuous = m_Continuous != null && m_Continuous.isOn;
            if (m_Benchmarking || continuous || m_ApplyOnce)
            {
                Apply(m_Op, (m_Tick++ & 1) == 1);
                m_ApplyOnce = false;
            }
            if (!m_Benchmarking && Time.frameCount % 20 == 0) ShowFrame(lastFrame);
        }

        void LateUpdate()
        {
            m_Pending.layoutQueue = CanvasUpdateSpy.layoutCount;
            m_Pending.graphicQueue = CanvasUpdateSpy.graphicCount;
        }

        public FrameStats lastFrame { get; private set; }
        public event Action<FrameStats> OnFrameMeasured;
        //#endregion

        void ShowFrame(FrameStats f)
        {
            if (m_Readout == null) return;
            m_Readout.text =
                $"<b>{OpNames[(int)m_Op]}</b>\n" +
                $"标脏  layout {f.layoutDirty}  vertices {f.vertsDirty}  material {f.materialDirty}\n" +
                $"队列  layout {f.layoutQueue}  graphic {f.graphicQueue}\n" +
                $"执行  UpdateGeometry {f.geometry}  UpdateMaterial {f.material}\n" +
                $"耗时  Layout {f.layoutMs:0.000}  Render {f.renderMs:0.000}  BuildBatch {f.batchMs:0.000} ms";
        }

        //#region bench
        public IEnumerator RunBenchmark(int warmup = 10, int frames = 60)
        {
            m_Benchmarking = true;
            benchmarkDone = false;
            var sb = new StringBuilder("操作\t标脏L\t标脏V\t标脏M\t队列L\t队列G\t几何\t材质\tLayout\tRender\tBuildBatch\n");
            var samples = new List<FrameStats>();
            Action<FrameStats> collect = f => samples.Add(f);
            for (int o = 0; o < OpNames.Length; o++)
            {
                m_Op = (Op)o;
                OnFrameMeasured -= collect;
                for (int i = 0; i < warmup; i++) yield return null;
                samples.Clear();
                OnFrameMeasured += collect;
                for (int i = 0; i < frames; i++) yield return null;
                OnFrameMeasured -= collect;
                Apply(m_Op, false);   // 复原
                sb.Append(OpNames[o]).Append('\t').Append(Summarize(samples)).Append('\n');
            }
            m_Op = Op.None;
            m_Benchmarking = false;
            lastReport = sb.ToString();
            benchmarkDone = true;
            if (m_Readout != null) m_Readout.text = "<size=70%>" + lastReport.Replace('\t', ' ') + "</size>";
        }

        static string Summarize(List<FrameStats> s)
        {
            double Avg(Func<FrameStats, double> f) { double t = 0; foreach (var x in s) t += f(x); return s.Count > 0 ? t / s.Count : 0; }
            double Med(Func<FrameStats, double> f) { var l = new List<double>(); foreach (var x in s) l.Add(f(x)); l.Sort(); return l.Count > 0 ? l[l.Count / 2] : 0; }
            return $"{Avg(x => x.layoutDirty):0.#}\t{Avg(x => x.vertsDirty):0.#}\t{Avg(x => x.materialDirty):0.#}\t" +
                   $"{Avg(x => x.layoutQueue):0.#}\t{Avg(x => x.graphicQueue):0.#}\t{Avg(x => x.geometry):0.#}\t{Avg(x => x.material):0.#}\t" +
                   $"{Med(x => x.layoutMs):0.000}\t{Med(x => x.renderMs):0.000}\t{Med(x => x.batchMs):0.000}";
        }
        //#endregion
    }
}
