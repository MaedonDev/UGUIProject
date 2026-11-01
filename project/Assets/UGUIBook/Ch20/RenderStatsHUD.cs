// 运行时性能小面板：用 ProfilerRecorder 读 Unity 自己的计数器（Batches、SetPass、三角形数）
// 和 UGUI 相关的性能标记（Canvas.BuildBatch、Layout），再加上本帧的托管内存分配。
// 编辑器 Play 模式和 Development Build 里都能用；正式包（非 Development）里大部分计数器读不到，会显示 “-”。
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;

namespace UGUIBook.Ch20
{
    public class RenderStatsHUD : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Label;
        [Tooltip("多少帧刷新一次显示（刷新文字本身也会引起重建，别每帧刷）")]
        [SerializeField] int m_RefreshEvery = 15;

        //#region recorders
        ProfilerRecorder m_Batches, m_SetPass, m_Tris, m_BuildBatch, m_Layout, m_GcAlloc, m_MainThread;

        void OnEnable()
        {
            // 计数器（Counter）：值是“上一帧的总数”
            m_Batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            m_SetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            m_Tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            m_GcAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            // 标记（Marker）：值是该标记在一帧内的累计耗时，单位纳秒
            m_BuildBatch = ProfilerRecorder.StartNew(ProfilerCategory.Gui, "Canvas.BuildBatch", 30);
            m_Layout = ProfilerRecorder.StartNew(ProfilerCategory.Gui, "Layout", 30);
            m_MainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 30);
        }

        void OnDisable()
        {
            m_Batches.Dispose(); m_SetPass.Dispose(); m_Tris.Dispose(); m_GcAlloc.Dispose();
            m_BuildBatch.Dispose(); m_Layout.Dispose(); m_MainThread.Dispose();
        }
        //#endregion

        readonly StringBuilder m_Sb = new StringBuilder(256);

        void Update()
        {
            if (m_Label == null || Time.frameCount % m_RefreshEvery != 0) return;
            m_Sb.Clear();
            m_Sb.Append("Batches ").Append(Val(m_Batches)).Append("   SetPass ").Append(Val(m_SetPass))
                .Append("   Tris ").Append(Val(m_Tris)).Append('\n');
            m_Sb.Append("BuildBatch ").Append(Ms(m_BuildBatch)).Append(" ms   Layout ").Append(Ms(m_Layout))
                .Append(" ms   GC ").Append(m_GcAlloc.Valid ? (m_GcAlloc.LastValue / 1024f).ToString("0.0") : "-").Append(" KB");
            if (m_MainThread.Valid && m_MainThread.Count > 0)
                m_Sb.Append("\nMain Thread ").Append(Ms(m_MainThread)).Append(" ms");
            m_Label.SetText(m_Sb);
        }

        static string Val(ProfilerRecorder r) => r.Valid ? r.LastValue.ToString() : "-";

        //#region average
        // 标记的值逐帧抖动很大，取最近 N 帧的平均值再显示
        static string Ms(ProfilerRecorder r)
        {
            if (!r.Valid || r.Count == 0) return "-";
            int n = r.Count;
            double sum = 0;
            for (int i = 0; i < n; i++) sum += r.GetSample(i).Value;
            return (sum / n * 1e-6).ToString("0.000");
        }
        //#endregion
    }
}
