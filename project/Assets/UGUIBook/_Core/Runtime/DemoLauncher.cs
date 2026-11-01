// 配套工程的总入口：列出全书所有配套场景，点一下就打开；每个场景左上角有“返回目录”按钮。
// 场景列表由菜单 UGUIBook › Build Demo Launcher 生成（同时写入 Build Settings）。
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGUIBook.Core
{
    public class DemoLauncher : MonoBehaviour
    {
        [System.Serializable]
        public class Entry
        {
            public string chapter;   // 例如 "第 6 章 Button 与 Selectable 体系"
            public string scene;     // 场景名（Build Settings 里的名字）
        }

        [SerializeField] List<Entry> m_Entries = new List<Entry>();
        [SerializeField] RectTransform m_Content;

        public List<Entry> Entries => m_Entries;
        public RectTransform Content { get => m_Content; set => m_Content = value; }

        const string LauncherScene = "Demos";

        void Start()
        {
            BackButton.Ensure();
            Build();
        }

        /// 按章节分组生成按钮（运行时生成，所以改了列表不用重新搭场景）
        public void Build()
        {
            if (m_Content == null) return;
            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(m_Content.GetChild(i).gameObject);
                else DestroyImmediate(m_Content.GetChild(i).gameObject);
            }
            // 每章一个标题 + 每行三个按钮（每行一个 HorizontalLayoutGroup 平分宽度，不需要知道 Content 多宽）
            const int Columns = 3; const float CellH = 76, Gap = 16;
            foreach (var group in m_Entries.GroupBy(e => e.chapter))
            {
                var h = UIKit.Text(m_Content, group.Key, 34, UIKit.Skin.primary, TextAlignmentOptions.BottomLeft, true, "Header");
                h.rectTransform.sizeDelta = new Vector2(0, 72);
                var list = group.ToList();
                for (int r = 0; r < list.Count; r += Columns)
                {
                    var row = UIKit.Node(m_Content, "Row").Size(0, CellH);
                    var hl = row.HLayout(Gap, 0, TextAnchor.MiddleLeft);
                    hl.childControlWidth = hl.childForceExpandWidth = true;
                    hl.childControlHeight = hl.childForceExpandHeight = true;
                    for (int c = r; c < r + Columns; c++)
                    {
                        if (c >= list.Count) { Equal(UIKit.Node(row, "Empty")); continue; }   // 占位，保持三等分
                        var e = list[c];
                        var b = UIKit.Button(row, e.scene.Replace('_', ' '), Color.white, 0, CellH, null, 28, e.scene);
                        var label = b.GetComponentInChildren<TextMeshProUGUI>();
                        label.alignment = TextAlignmentOptions.MidlineLeft;
                        ((RectTransform)label.transform).offsetMin = new Vector2(28, 0);
                        Equal(b.transform);
                        var name = e.scene;
                        b.onClick.AddListener(() => SceneManager.LoadScene(name));
                    }
                }
            }
        }

        static void Equal(Transform t)
        {
            var le = t.GetComponent<LayoutElement>();   // 不能用 ??：编辑器里缺失组件返回的是“假 null”
            if (le == null) le = t.gameObject.AddComponent<LayoutElement>();
            le.minWidth = 0; le.preferredWidth = 0; le.flexibleWidth = 1;
        }

        /// 跨场景常驻的“返回目录”按钮（放在左下角，尽量不挡各场景自己的界面）
        public static class BackButton
        {
            static GameObject s_Canvas;

            public static void Ensure()
            {
                if (s_Canvas != null) return;
                var canvas = UIKit.Canvas("DemoBackCanvas");
                canvas.sortingOrder = 32000;
                s_Canvas = canvas.gameObject;
                DontDestroyOnLoad(s_Canvas);
                var b = UIKit.Button(canvas.transform, "返回目录", new Color(0.12f, 0.14f, 0.21f, 0.7f), 180, 56, "back", 24);
                b.Rt().Place(new Vector2(0, 0), new Vector2(16, 16), new Vector2(180, 56));
                b.onClick.AddListener(() => SceneManager.LoadScene(LauncherScene));
                SceneManager.activeSceneChanged += (_, s) => { if (s_Canvas != null) s_Canvas.SetActive(s.name != LauncherScene); };
                s_Canvas.SetActive(SceneManager.GetActiveScene().name != LauncherScene);
            }
        }
    }
}
