using System;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch16
{
    /// 背包格子：只负责显示一个 ItemData，点击时把自己交给外面
    public class BagSlot : MonoBehaviour
    {
        [SerializeField] Image m_Quality;
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_Count;
        [SerializeField] GameObject m_Selected;
        [SerializeField] Button m_Button;

        static readonly Color[] s_QualityColors =
        {
            new Color32(0xFF, 0xFF, 0xFF, 0x10), new Color32(0x2F, 0xB3, 0x6A, 0x70),
            new Color32(0x40, 0x80, 0xFF, 0x70), new Color32(0xA0, 0x5A, 0xFF, 0x80),
        };

        public ItemData Item { get; private set; }
        Action<BagSlot> m_OnClick;

        void Awake() => m_Button.onClick.AddListener(() => m_OnClick?.Invoke(this));

        public void Set(ItemData item, bool selected, Action<BagSlot> onClick)
        {
            Item = item;
            m_OnClick = onClick;
            m_Quality.color = s_QualityColors[Mathf.Clamp(item.Quality, 0, 3)];
            m_Icon.sprite = UISkin.Sprite(item.Icon);
            m_Count.text = item.Count > 1 ? item.Count.ToString() : "";
            m_Selected.SetActive(selected);
        }

        public void SetSelected(bool selected) => m_Selected.SetActive(selected);
    }
}
