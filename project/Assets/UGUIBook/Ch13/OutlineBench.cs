// 实测 Shadow / Outline 的代价：同一段旧版 Text 分别挂 无 / Shadow / Outline / Outline+Shadow，
// 统计顶点数，并用 Stopwatch 计时“标脏 + 重建网格”N 次的耗时。配套场景运行后结果显示在屏幕上。
using System.Diagnostics;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    public class OutlineBench : MonoBehaviour
    {
        [SerializeField] Text m_Text;
        [SerializeField] TMP_Text m_Output;
        [SerializeField] int m_Iterations = 2000;

        void Start()
        {
            if (m_Output != null) m_Output.text = Run(m_Text, m_Iterations);
        }

        //#region bench
        public static string Run(Text text, int iterations)
        {
            var outline = text.GetComponent<Outline>();
            Shadow pureShadow = null;
            foreach (var s in text.GetComponents<Shadow>()) if (!(s is Outline)) pureShadow = s;
            var sb = new StringBuilder();
            string[] names = { "无效果", "Shadow", "Outline", "Outline + Shadow" };
            for (int cfg = 0; cfg < 4; cfg++)
            {
                if (pureShadow != null) pureShadow.enabled = cfg == 1 || cfg == 3;
                if (outline != null) outline.enabled = cfg == 2 || cfg == 3;
                MeshProbe.Count(text, out int verts, out int tris);
                for (int i = 0; i < 50; i++) Rebuild(text);     // 预热
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < iterations; i++) Rebuild(text);
                sw.Stop();
                double us = sw.Elapsed.TotalMilliseconds * 1000.0 / iterations;
                sb.AppendLine($"{names[cfg]}\t顶点 {verts}\t三角形 {tris}\t每次重建 {us:F1} μs");
            }
            if (pureShadow != null) pureShadow.enabled = true;
            if (outline != null) outline.enabled = true;
            return sb.ToString();
        }

        // 和 CanvasUpdateRegistry 每帧做的事一样：顶点标脏 → PreRender 阶段 Rebuild → UpdateGeometry
        static void Rebuild(Graphic g)
        {
            g.SetVerticesDirty();
            g.Rebuild(CanvasUpdate.PreRender);
        }
        //#endregion
    }
}
