// TMP 逐字顶点动画：波浪、抖动、逐字弹出、彩虹。
// TextMeshPro 不走 UGUI 的 IMeshModifier，要改它的网格，就改 textInfo.meshInfo[].vertices / colors32，再 UpdateVertexData 上传。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch13
{
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("UGUIBook/Ch13/TMP Vertex Anim")]
    public class TMPVertexAnim : MonoBehaviour
    {
        //#region fields
        public enum Mode { Wave, Jitter, PopIn, Rainbow }

        [SerializeField] Mode m_Mode = Mode.Wave;
        [SerializeField] float m_Amplitude = 12;     // 波浪/抖动幅度（本地单位）
        [SerializeField] float m_Frequency = 0.6f;   // 波浪：相邻字的相位差（弧度）
        [SerializeField] float m_Speed = 6;          // 波浪/彩虹：速度；PopIn：每秒弹出几个字
        [SerializeField] bool m_Loop = true;         // PopIn 播完后是否从头再来
        [SerializeField] float m_Time;               // 当前时间。运行时由 Update 推进；也可以直接 Evaluate(t)

        TMP_Text m_Text;
        Vector3[][] m_BaseVerts;                     // 每个材质（子网格）一份原始顶点
        Color32[][] m_BaseColors;
        bool m_Hooked;
        //#endregion

        public Mode mode { get => m_Mode; set { m_Mode = value; Evaluate(m_Time); } }
        public float speed { get => m_Speed; set => m_Speed = value; }
        public float amplitude { get => m_Amplitude; set => m_Amplitude = value; }
        public float frequency { get => m_Frequency; set => m_Frequency = value; }

        //#region hook
        void OnEnable()
        {
            m_Text = GetComponent<TMP_Text>();
            Hook();
            m_Text.ForceMeshUpdate();     // 让 OnPreRenderText 先跑一次，缓存原始顶点
        }

        void OnDisable()
        {
            if (m_Text != null && m_Hooked) m_Text.OnPreRenderText -= OnPreRenderText;
            m_Hooked = false;
            if (m_Text != null) m_Text.ForceMeshUpdate();   // 关掉后恢复成原样
        }

        void Hook()
        {
            if (m_Hooked) return;
            if (m_Text == null) m_Text = GetComponent<TMP_Text>();
            m_Text.OnPreRenderText += OnPreRenderText;
            m_Hooked = true;
        }

        // TMP 每次重新生成网格（改文字、改字号、布局变化……）后、上传之前调用：
        // 这时 textInfo 里是“干净”的原始顶点，先存一份，再按当前时间把动画套上去
        void OnPreRenderText(TMP_TextInfo info)
        {
            int mc = info.meshInfo.Length;
            if (m_BaseVerts == null || m_BaseVerts.Length != mc) { m_BaseVerts = new Vector3[mc][]; m_BaseColors = new Color32[mc][]; }
            for (int m = 0; m < mc; m++)
            {
                var src = info.meshInfo[m];
                if (src.vertices == null) continue;
                if (m_BaseVerts[m] == null || m_BaseVerts[m].Length != src.vertices.Length)
                {
                    m_BaseVerts[m] = new Vector3[src.vertices.Length];
                    m_BaseColors[m] = new Color32[src.colors32.Length];
                }
                System.Array.Copy(src.vertices, m_BaseVerts[m], src.vertices.Length);
                System.Array.Copy(src.colors32, m_BaseColors[m], src.colors32.Length);
            }
            Apply(info);
        }
        //#endregion

        //#region tick
        void Update()
        {
            if (!Application.isPlaying || m_Text == null || m_BaseVerts == null) return;
            m_Time += Time.deltaTime;
            Apply(m_Text.textInfo);
            // 只上传顶点和颜色，不重新排版——这是逐字动画便宜的关键
            m_Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices | TMP_VertexDataUpdateFlags.Colors32);
        }

        /// 直接跳到时间 t（截图、调试、和别的动画同步时用）
        public void Evaluate(float t)
        {
            m_Time = t;
            Hook();
            m_Text.ForceMeshUpdate();
        }
        //#endregion

        //#region apply
        void Apply(TMP_TextInfo info)
        {
            if (m_BaseVerts == null) return;
            int visibleIndex = 0;
            for (int i = 0; i < info.characterCount; i++)
            {
                var ch = info.characterInfo[i];
                if (!ch.isVisible) continue;                    // 空格等不可见字符没有顶点
                int m = ch.materialReferenceIndex, v0 = ch.vertexIndex;
                var dst = info.meshInfo[m].vertices;
                var dstC = info.meshInfo[m].colors32;
                var src = m_BaseVerts[m];
                var srcC = m_BaseColors[m];
                if (src == null || v0 + 3 >= src.Length) continue;
                Vector3 center = (src[v0] + src[v0 + 2]) * 0.5f;  // 0=左下 1=左上 2=右上 3=右下
                Vector3 offset = Vector3.zero; float scale = 1, alpha = 1;
                Color32? tint = null;

                switch (m_Mode)
                {
                    case Mode.Wave:
                        offset.y = Mathf.Sin(m_Time * m_Speed + i * m_Frequency) * m_Amplitude;
                        break;
                    case Mode.Jitter:
                        // 每 0.05 秒换一次随机偏移；用 (字序号, 时间片) 做种子，保证同一时刻结果固定
                        int slice = Mathf.FloorToInt(m_Time * 20);
                        offset = new Vector3(Hash(i * 73 + slice * 151) - 0.5f, Hash(i * 37 + slice * 97 + 11) - 0.5f) * m_Amplitude * 0.6f;
                        break;
                    case Mode.PopIn:
                        float total = (info.characterCount + 3) / Mathf.Max(0.01f, m_Speed);
                        float tt = m_Loop ? Mathf.Repeat(m_Time, total + 1f) : m_Time;
                        float p = Mathf.Clamp01(tt * m_Speed - visibleIndex);   // 每个字用 1/speed 秒弹出
                        scale = OutBack(p);
                        alpha = Mathf.Clamp01(p * 2);
                        offset.y = (1 - p) * -m_Amplitude;
                        break;
                    case Mode.Rainbow:
                        tint = Color.HSVToRGB(Mathf.Repeat(m_Time * m_Speed * 0.05f + i * 0.07f, 1f), 0.75f, 1f);
                        break;
                }

                for (int k = 0; k < 4; k++)
                {
                    dst[v0 + k] = center + (src[v0 + k] - center) * scale + offset;
                    Color32 c = tint ?? srcC[v0 + k];
                    c.a = (byte)(srcC[v0 + k].a * alpha);
                    dstC[v0 + k] = c;
                }
                visibleIndex++;
            }
        }

        static float Hash(int n)
        {
            n = (n << 13) ^ n;
            return ((n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 2147483648f;
        }

        static float OutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }
        //#endregion
    }
}
