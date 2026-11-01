// 普通列表（每条数据一个 GameObject + VerticalLayoutGroup）和循环列表的对比测量。
// 场景里点按钮即可自测；书中的数字由同一套方法在编辑器批处理里测得。
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region bench
    public class ListBenchmark : MonoBehaviour
    {
        public struct Result
        {
            public string Name;
            public int Count;
            public double BuildMs;       // 创建 + 绑定数据
            public double LayoutMs;      // 第一次 Canvas.ForceUpdateCanvases（布局 + 网格重建）
            public int GameObjects;      // 列表 Content 下的 GameObject 总数
            public int Graphics;         // 其中 Graphic 组件（Image/Text…）的数量
        }

        [SerializeField] ScrollRect m_Naive;          // Content 上挂 VerticalLayoutGroup + ContentSizeFitter
        [SerializeField] RectTransform m_NaiveTemplate;
        [SerializeField] LoopListView m_Loop;
        [SerializeField] TMP_Text m_Output;
        [SerializeField] Button m_NaiveButton, m_LoopButton;
        [SerializeField] int m_Count = 10000;

        readonly List<MailData> m_Data = new List<MailData>();

        void Start()
        {
            m_NaiveButton.onClick.AddListener(() => Show(RunNaive(m_Count)));
            m_LoopButton.onClick.AddListener(() => Show(RunLoop(m_Count)));
        }

        void EnsureData(int n)
        {
            while (m_Data.Count < n)
                m_Data.Add(new MailData { title = $"第 {m_Data.Count + 1} 封邮件", sub = "来自系统 · 附件 2 件", icon = "mail" });
        }

        public Result RunNaive(int n)
        {
            EnsureData(n);
            ClearNaive();
            var content = m_Naive.content;
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < n; i++)
            {
                var item = Instantiate(m_NaiveTemplate, content, false);
                item.gameObject.SetActive(true);
                item.GetComponent<MailItemView>().Set(i, m_Data[i], false, null);
            }
            double build = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            Canvas.ForceUpdateCanvases();
            double layout = sw.Elapsed.TotalMilliseconds;
            return Measure("普通列表", n, build, layout, content);
        }

        public Result RunLoop(int n)
        {
            EnsureData(n);
            m_Loop.SetCount(0);
            m_Loop.onBind = (i, item) => item.GetComponent<MailItemView>().Set(i, m_Data[i], false, null);
            var sw = Stopwatch.StartNew();
            m_Loop.SetCount(n);
            double build = sw.Elapsed.TotalMilliseconds;
            sw.Restart();
            Canvas.ForceUpdateCanvases();
            double layout = sw.Elapsed.TotalMilliseconds;
            return Measure("循环列表", n, build, layout, m_Loop.ScrollRect.content);
        }

        static Result Measure(string name, int n, double build, double layout, RectTransform content) => new Result
        {
            Name = name, Count = n, BuildMs = build, LayoutMs = layout,
            GameObjects = content.GetComponentsInChildren<Transform>(true).Length - 1,
            Graphics = content.GetComponentsInChildren<Graphic>(true).Length,
        };

        /// 清掉普通列表的所有项
        public void ClearNaive()
        {
            var content = m_Naive.content;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var c = content.GetChild(i).gameObject;
                if (Application.isPlaying) Destroy(c); else DestroyImmediate(c);
            }
        }

        void Show(Result r)
        {
            if (m_Output == null) return;
            m_Output.text = $"{r.Name} {r.Count} 条：创建 {r.BuildMs:0.0} ms，首次布局与重建 {r.LayoutMs:0.0} ms，" +
                            $"GameObject {r.GameObjects} 个，Graphic {r.Graphics} 个";
            UnityEngine.Debug.Log("[ListBenchmark] " + m_Output.text);
        }
    }
    //#endregion
}
