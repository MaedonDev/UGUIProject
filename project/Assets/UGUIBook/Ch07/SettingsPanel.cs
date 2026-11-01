// 设置界面：两个音量滑块 + 画质分段选择 + 两个开关，读写 PlayerPrefs。
// 要点：读档时用 SetValueWithoutNotify / SetIsOnWithoutNotify，避免“读出来的值又触发保存和音效”。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class SettingsPanel : MonoBehaviour
    {
        [Header("音量")]
        [SerializeField] Slider m_Music;
        [SerializeField] TMP_Text m_MusicValue;
        [SerializeField] Slider m_Sfx;
        [SerializeField] TMP_Text m_SfxValue;
        [Header("画质（同一 ToggleGroup 下的几个 Toggle）")]
        [SerializeField] Toggle[] m_Quality;
        [Header("开关")]
        [SerializeField] Toggle m_Vibration;
        [SerializeField] Toggle m_Notify;
        [Header("按钮")]
        [SerializeField] Button m_Reset;
        [SerializeField] Button m_Save;
        [SerializeField] TMP_Text m_Hint;

        const string kMusic = "ugui.music", kSfx = "ugui.sfx", kQuality = "ugui.quality",
                     kVibration = "ugui.vibration", kNotify = "ugui.notify";

        //#region bind
        void Awake()
        {
            // 拖动过程中 onValueChanged 每帧都会来；这里只更新数字和实际音量，保存留给“保存”按钮
            m_Music.onValueChanged.AddListener(v => { m_MusicValue.text = Mathf.RoundToInt(v).ToString(); ApplyVolume(); MarkDirty(); });
            m_Sfx.onValueChanged.AddListener(v => { m_SfxValue.text = Mathf.RoundToInt(v).ToString(); MarkDirty(); });
            for (int i = 0; i < m_Quality.Length; i++)
            {
                int level = i;
                m_Quality[i].onValueChanged.AddListener(on => { if (on) { ApplyQuality(level); MarkDirty(); } });
            }
            m_Vibration.onValueChanged.AddListener(_ => MarkDirty());
            m_Notify.onValueChanged.AddListener(_ => MarkDirty());
            m_Reset.onClick.AddListener(ResetToDefault);
            m_Save.onClick.AddListener(Save);
        }

        void Start() => Load();
        //#endregion

        //#region load
        /// 从 PlayerPrefs 读出设置并显示到控件上（不触发任何 onValueChanged）
        public void Load()
        {
            ShowValues(PlayerPrefs.GetFloat(kMusic, 80), PlayerPrefs.GetFloat(kSfx, 65),
                       PlayerPrefs.GetInt(kQuality, 2), PlayerPrefs.GetInt(kVibration, 1) == 1,
                       PlayerPrefs.GetInt(kNotify, 0) == 1);
            ApplyVolume();
            m_Hint.text = "";
        }

        public void ShowValues(float music, float sfx, int quality, bool vibration, bool notify)
        {
            m_Music.SetValueWithoutNotify(music);
            m_Sfx.SetValueWithoutNotify(sfx);
            m_MusicValue.text = Mathf.RoundToInt(m_Music.value).ToString();
            m_SfxValue.text = Mathf.RoundToInt(m_Sfx.value).ToString();
            quality = Mathf.Clamp(quality, 0, m_Quality.Length - 1);
            // ToggleGroup 里只需“打开目标”：组会用 SetIsOnWithoutNotify(false) 关掉其他成员。
            // 反过来先关当前项是行不通的——allowSwitchOff = false 时，组里最后一个打开的 Toggle 关不掉
            m_Quality[quality].SetIsOnWithoutNotify(true);
            m_Vibration.SetIsOnWithoutNotify(vibration);
            m_Notify.SetIsOnWithoutNotify(notify);
        }
        //#endregion

        //#region save
        public void Save()
        {
            PlayerPrefs.SetFloat(kMusic, m_Music.value);
            PlayerPrefs.SetFloat(kSfx, m_Sfx.value);
            PlayerPrefs.SetInt(kQuality, SelectedQuality());
            PlayerPrefs.SetInt(kVibration, m_Vibration.isOn ? 1 : 0);
            PlayerPrefs.SetInt(kNotify, m_Notify.isOn ? 1 : 0);
            PlayerPrefs.Save();
            m_Hint.text = "已保存";
        }

        void ResetToDefault()
        {
            ShowValues(80, 65, 2, true, false);
            ApplyVolume();
            ApplyQuality(2);
            MarkDirty();
        }

        int SelectedQuality()
        {
            for (int i = 0; i < m_Quality.Length; i++) if (m_Quality[i].isOn) return i;
            return 0;
        }

        void ApplyVolume() => AudioListener.volume = m_Music.value / 100f;   // 真实项目一般改 AudioMixer 的分组音量

        void ApplyQuality(int level)
        {
            // 工程里的画质档位数量不一定等于按钮数量，按比例映射一下
            int count = QualitySettings.names.Length;
            int mapped = Mathf.RoundToInt(level / (float)Mathf.Max(1, m_Quality.Length - 1) * (count - 1));
            QualitySettings.SetQualityLevel(mapped, true);
        }

        void MarkDirty() => m_Hint.text = "有未保存的修改";
        //#endregion

        public void Setup(Slider music, TMP_Text musicValue, Slider sfx, TMP_Text sfxValue, Toggle[] quality,
            Toggle vibration, Toggle notify, Button reset, Button save, TMP_Text hint)
        {
            m_Music = music; m_MusicValue = musicValue; m_Sfx = sfx; m_SfxValue = sfxValue; m_Quality = quality;
            m_Vibration = vibration; m_Notify = notify; m_Reset = reset; m_Save = save; m_Hint = hint;
        }
    }
}
