// 带国旗图标的语言选择：TMP_Dropdown 的每个 OptionData 可以带一张 Sprite，
// 只要模板里设置了 Item Image、主体上设置了 Caption Image，图标就会自动显示。
// 国旗用代码画（配套工程不带任何外部图片素材）；真实项目里直接用图集里的精灵即可。
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch08
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class LanguageDropdown : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Status;

        static readonly (string code, string name)[] kLanguages =
        {
            ("zh", "简体中文"), ("en", "英语"), ("ja", "日语"), ("fr", "法语"), ("de", "德语"), ("it", "意大利语"),
        };

        public string CurrentCode { get; private set; }

        //#region options
        void Awake() => Init();

        public void Init()
        {
            var dd = GetComponent<TMP_Dropdown>();
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var (code, name) in kLanguages)
                options.Add(new TMP_Dropdown.OptionData(name, MakeFlag(code)));
            dd.ClearOptions();
            dd.AddOptions(options);
            // 读取上次的选择；用 WithoutNotify 避免启动时就触发“切换语言”
            int saved = Mathf.Clamp(PlayerPrefs.GetInt("ugui.lang", 0), 0, options.Count - 1);
            dd.SetValueWithoutNotify(saved);
            CurrentCode = kLanguages[saved].code;
            dd.onValueChanged.RemoveListener(OnChanged);
            dd.onValueChanged.AddListener(OnChanged);
            if (m_Status != null) m_Status.text = $"当前语言：{kLanguages[saved].name}（{CurrentCode}）";
        }

        void OnChanged(int index)
        {
            CurrentCode = kLanguages[index].code;
            PlayerPrefs.SetInt("ugui.lang", index);
            // 真实项目在这里通知本地化系统刷新所有文字（见第 19 章）
            if (m_Status != null) m_Status.text = $"已切换为：{kLanguages[index].name}（{CurrentCode}）";
        }
        //#endregion

        //#region flags
        // ---- 程序化国旗：90×60 像素，逐像素着色 ----
        const int W = 90, H = 60;
        static readonly Dictionary<string, Sprite> s_Cache = new Dictionary<string, Sprite>();

        static Sprite MakeFlag(string code)
        {
            if (s_Cache.TryGetValue(code, out var sp) && sp != null) return sp;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { name = "Flag_" + code, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    px[y * W + x] = x == 0 || y == 0 || x == W - 1 || y == H - 1
                        ? new Color32(0xC7, 0xCC, 0xE0, 0xFF)                                   // 1 像素灰边：白色旗面放在白底上也看得清
                        : FlagColor(code, (x + 0.5f) / W, 1f - (y + 0.5f) / H);                 // v 从上往下 0→1
            tex.SetPixels32(px);
            tex.Apply();
            sp = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100);
            sp.name = tex.name;
            s_Cache[code] = sp;
            return sp;
        }
        //#endregion

        static Color32 Hex(uint rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        // u: 0→1 从左到右；v: 0→1 从上到下（3:2 的旗面）
        static Color32 FlagColor(string code, float u, float v)
        {
            switch (code)
            {
                case "fr": return u < 1 / 3f ? Hex(0x0055A4) : u < 2 / 3f ? Hex(0xFFFFFF) : Hex(0xEF4135);
                case "it": return u < 1 / 3f ? Hex(0x009246) : u < 2 / 3f ? Hex(0xFFFFFF) : Hex(0xCE2B37);
                case "de": return v < 1 / 3f ? Hex(0x000000) : v < 2 / 3f ? Hex(0xDD0000) : Hex(0xFFCE00);
                case "ja":
                {
                    float dx = (u - 0.5f) * 1.5f, dy = v - 0.5f;          // 换算到以旗高为单位
                    return dx * dx + dy * dy < 0.3f * 0.3f ? Hex(0xBC002D) : Hex(0xFFFFFF);
                }
                case "en": return UnionJack(u, v);
                default: return China(u, v);
            }
        }

        // 英国国旗（简化版）：蓝底 + 白色斜十字 + 红色斜十字 + 白色正十字 + 红色正十字
        static Color32 UnionJack(float u, float v)
        {
            float x = (u - 0.5f) * 2f, y = (v - 0.5f) * 2f;           // -1~1
            float ax = Mathf.Abs(x), ay = Mathf.Abs(y);
            // 旗面宽高比 3:2，x 方向的 1 相当于 y 方向的 1.5，下面统一换算成“半旗高”为单位
            if (ax * 1.5f < 0.12f || ay < 0.12f) return Hex(0xC8102E);   // 红色正十字
            if (ax * 1.5f < 0.2f || ay < 0.2f) return Hex(0xFFFFFF);     // 白色正十字
            float diag = Mathf.Abs(ax - ay) * 0.83f;                   // 到对角线（角到角）的距离
            if (diag < 0.07f) return Hex(0xC8102E);
            if (diag < 0.17f) return Hex(0xFFFFFF);
            return Hex(0x012169);
        }

        // 中国国旗：红底，左上角一颗大五角星 + 四颗小五角星（小星的一个角指向大星中心）
        static Color32 China(float u, float v)
        {
            float x = u * 30f, y = v * 20f;                          // 旗面按 30×20 网格
            if (InStar(x, y, 5, 5, 3f, -90)) return Hex(0xFFDE00);
            Vector2[] small = { new Vector2(10, 2), new Vector2(12, 4), new Vector2(12, 7), new Vector2(10, 9) };
            foreach (var c in small)
            {
                float ang = Mathf.Atan2(5 - c.y, 5 - c.x) * Mathf.Rad2Deg;
                if (InStar(x, y, c.x, c.y, 1f, ang)) return Hex(0xFFDE00);
            }
            return Hex(0xEE1C25);
        }

        /// 点 (x,y) 是否在中心 (cx,cy)、外接圆半径 r、第一个尖角朝 angleDeg 方向的五角星内
        static bool InStar(float x, float y, float cx, float cy, float r, float angleDeg)
        {
            var pts = new Vector2[10];
            for (int i = 0; i < 10; i++)
            {
                float a = (angleDeg + i * 36f) * Mathf.Deg2Rad;
                float rr = i % 2 == 0 ? r : r * 0.382f;
                pts[i] = new Vector2(cx + Mathf.Cos(a) * rr, cy + Mathf.Sin(a) * rr);
            }
            bool inside = false;   // 射线法判断点是否在多边形内
            for (int i = 0, j = 9; i < 10; j = i++)
                if ((pts[i].y > y) != (pts[j].y > y) && x < (pts[j].x - pts[i].x) * (y - pts[i].y) / (pts[j].y - pts[i].y) + pts[i].x)
                    inside = !inside;
            return inside;
        }

        public void Setup(TMP_Text status) => m_Status = status;
    }
}
