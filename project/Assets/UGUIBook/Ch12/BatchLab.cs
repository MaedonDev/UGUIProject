// 第 12 章合批实验台：m_Cases 下每个子物体是一种摆法，一次只显示一种，
// 右上角实时显示本帧的 Batches / SetPass / Draw Calls（ProfilerRecorder 读的是和 Stats 窗口同一份渲染统计）。
// “测量全部”依次切换每种摆法，各取 10 帧中位数，减去“空画布”基线后列表显示。
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    public class BatchLab : MonoBehaviour
    {
        [SerializeField] RectTransform m_Cases;
        [SerializeField] TMP_Text m_Title;
        [SerializeField] TMP_Text m_Hud;
        [SerializeField] Button m_Prev, m_Next, m_MeasureAll;

        int m_Index;
        bool m_Measuring;
        ProfilerRecorder m_Batches, m_SetPass, m_Draws;

        public string lastReport { get; private set; }
        public bool done { get; private set; }
        public int caseCount => m_Cases.childCount;

        //#region rec
        void OnEnable()
        {
            m_Batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            m_SetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
            m_Draws = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        }

        void OnDisable() { m_Batches.Dispose(); m_SetPass.Dispose(); m_Draws.Dispose(); }
        //#endregion

        void Start()
        {
            if (m_Prev != null) m_Prev.onClick.AddListener(() => SetCase((m_Index + caseCount - 1) % caseCount));
            if (m_Next != null) m_Next.onClick.AddListener(() => SetCase((m_Index + 1) % caseCount));
            if (m_MeasureAll != null) m_MeasureAll.onClick.AddListener(() => { if (!m_Measuring) StartCoroutine(MeasureAll()); });
            SetCase(0);
        }

        public void SetCase(int index)
        {
            m_Index = index;
            for (int i = 0; i < m_Cases.childCount; i++) m_Cases.GetChild(i).gameObject.SetActive(i == index);
            if (m_Title != null) m_Title.text = $"{index + 1}/{caseCount}  {m_Cases.GetChild(index).name}";
        }

        void Update()
        {
            if (m_Hud != null && !m_Measuring && Time.frameCount % 10 == 0)
                m_Hud.text = $"Batches {m_Batches.LastValue}   SetPass {m_SetPass.LastValue}   Draw Calls {m_Draws.LastValue}";
        }

        //#region measure
        public IEnumerator MeasureAll()
        {
            m_Measuring = true; done = false;
            var rows = new List<(string name, long b, long s, long d)>();
            for (int i = 0; i < caseCount; i++)
            {
                SetCase(i);
                for (int k = 0; k < 5; k++) yield return null;           // 等布局、重建、合批稳定
                var b = new List<long>(); var s = new List<long>(); var d = new List<long>();
                for (int k = 0; k < 10; k++)
                {
                    yield return null;
                    b.Add(m_Batches.LastValue); s.Add(m_SetPass.LastValue); d.Add(m_Draws.LastValue);
                }
                b.Sort(); s.Sort(); d.Sort();
                rows.Add((m_Cases.GetChild(i).name, b[5], s[5], d[5]));
            }
            var sb = new StringBuilder("摆法\tBatches\tSetPass\tDrawCalls\t(减基线后 Batches)\n");
            var baseline = rows[0];
            foreach (var r in rows) sb.Append($"{r.name}\t{r.b}\t{r.s}\t{r.d}\t{r.b - baseline.b}\n");
            lastReport = sb.ToString();
            m_Measuring = false; done = true;
            SetCase(0);
            if (m_Hud != null) m_Hud.text = "<size=60%>" + lastReport.Replace('\t', ' ') + "</size>";
        }
        //#endregion
    }
}
