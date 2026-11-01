using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    public class RankEntry
    {
        public int Rank;
        public string Name;
        public string Avatar;
        public int Level;
        public int Score;
        public bool IsMe;
    }

    //#region rankrow
    public class RankRowView : MonoBehaviour
    {
        [SerializeField] Image m_Bg;
        [SerializeField] Image m_Medal;
        [SerializeField] TMP_Text m_Rank;
        [SerializeField] Image m_Avatar;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] TMP_Text m_Score;

        static readonly Color[] s_Medal =
        {
            new Color32(0xFF, 0xC5, 0x3D, 0xFF), new Color32(0xC3, 0xCB, 0xD9, 0xFF), new Color32(0xE0, 0x93, 0x5C, 0xFF),
        };
        static readonly Color s_Normal = new Color32(0x2B, 0x31, 0x4A, 0xFF);
        static readonly Color s_Me = new Color32(0x5B, 0x4B, 0xD6, 0xFF);

        public void Set(RankEntry e)
        {
            bool top3 = e.Rank <= 3;
            m_Medal.enabled = top3;
            if (top3) m_Medal.color = s_Medal[e.Rank - 1];
            m_Rank.text = e.Rank.ToString();
            m_Rank.color = top3 ? new Color32(0x3A, 0x2A, 0x10, 0xFF) : Color.white;
            m_Avatar.sprite = UISkin.Sprite(e.Avatar);
            m_NameText.text = $"{e.Name}  <size=70%><color=#A9B0C7>Lv.{e.Level}</color></size>";
            m_Score.text = e.Score.ToString("N0");
            m_Bg.color = e.IsMe ? s_Me : s_Normal;
        }
    }
    //#endregion
}
