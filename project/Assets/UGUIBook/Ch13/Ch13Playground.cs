// 配套场景 Ch13_Playground 的控制脚本：用滑块和开关实时调整本章的几个自定义 Graphic，观察网格怎么变。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    public class Ch13Playground : MonoBehaviour
    {
        [SerializeField] RoundedRect m_Rect;
        [SerializeField] WireframeView m_RectWire;
        [SerializeField] RingProgress m_Ring;
        [SerializeField] TMP_Text m_RingLabel;
        [SerializeField] RadarChart m_Radar;
        [SerializeField] LineChart m_Line;
        [SerializeField] Slider m_Radius, m_Segments, m_Fill;
        [SerializeField] Toggle m_Smooth, m_Wire;
        [SerializeField] Button m_Random;
        [SerializeField] TMP_Text m_Info;

        void Start()
        {
            m_Radius.onValueChanged.AddListener(v => { m_Rect.radius = v; Refresh(); });
            m_Segments.onValueChanged.AddListener(v => { m_Rect.segments = Mathf.RoundToInt(v); Refresh(); });
            m_Fill.onValueChanged.AddListener(v => { m_Ring.fill = v; m_RingLabel.text = Mathf.RoundToInt(v * 100) + "%"; });
            m_Smooth.onValueChanged.AddListener(on => m_Line.smooth = on);
            m_Wire.onValueChanged.AddListener(on => m_RectWire.gameObject.SetActive(on));
            m_Random.onClick.AddListener(Randomize);
            Refresh();
        }

        void Randomize()
        {
            var v = new float[m_Radar.axisCount];
            for (int i = 0; i < v.Length; i++) v[i] = Random.Range(0.25f, 1f);
            m_Radar.SetValues(v);
            var line = new float[m_Line.count];
            for (int i = 0; i < line.Length; i++) line[i] = Random.Range(1f, 9.5f);
            m_Line.SetValues(line);
        }

        void Refresh()
        {
            m_RectWire.SetVerticesDirty();
            MeshProbe.Count(m_Rect, out int v, out int t);
            m_Info.text = $"圆角 {m_Rect.radius:F0}  每角 {m_Rect.segments} 段  →  顶点 {v} · 三角形 {t}";
        }
    }
}
