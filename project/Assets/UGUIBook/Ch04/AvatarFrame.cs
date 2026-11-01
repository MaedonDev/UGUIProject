// 头像框：圆形头像（Mask 裁剪）+ 品质色环 + 等级角标 + 红点。点击换下一个头像。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    public class AvatarFrame : MonoBehaviour
    {
        //#region fields
        [SerializeField] Image m_Avatar;          // 放在带 Mask 的圆形节点下面
        [SerializeField] Image m_Ring;            // 品质色环（ring 精灵，用 color 染色）
        [SerializeField] TMP_Text m_Level;
        [SerializeField] GameObject m_RedDot;
        [SerializeField] Sprite[] m_Avatars;      // 点击轮换（演示用）
        //#endregion

        public static readonly Color[] RarityColors =
        {
            new Color32(0x9A, 0xA3, 0xB5, 255),   // 普通：灰
            new Color32(0x2F, 0xB3, 0x6A, 255),   // 优秀：绿
            new Color32(0x3B, 0x82, 0xF6, 255),   // 稀有：蓝
            new Color32(0x9B, 0x51, 0xE0, 255),   // 史诗：紫
            new Color32(0xFF, 0x8A, 0x3D, 255),   // 传说：橙
        };

        int m_Index;

        void Awake()
        {
            var b = GetComponent<Button>();
            if (b != null) b.onClick.AddListener(Next);
        }

        //#region setdata
        public void SetData(Sprite avatar, int level, int rarity, bool hasNews)
        {
            m_Avatar.sprite = avatar;
            m_Ring.color = RarityColors[Mathf.Clamp(rarity, 0, RarityColors.Length - 1)];
            m_Level.SetText("{0}", level);
            m_RedDot.SetActive(hasNews);
        }
        //#endregion

        void Next()
        {
            if (m_Avatars == null || m_Avatars.Length == 0) return;
            m_Index = (m_Index + 1) % m_Avatars.Length;
            SetData(m_Avatars[m_Index], 10 + m_Index * 7, m_Index % RarityColors.Length, m_Index % 2 == 0);
        }
    }
}
