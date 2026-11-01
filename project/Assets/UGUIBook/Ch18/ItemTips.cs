using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region tips
    /// 物品详情 Tips：贴在格子旁边，右边放不下就放左边，上下不出屏幕。高度随描述长短自适应
    public class ItemTips : MonoBehaviour
    {
        [SerializeField] RectTransform m_Panel;      // VerticalLayoutGroup + ContentSizeFitter
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_NameText;
        [SerializeField] TMP_Text m_Type;
        [SerializeField] TMP_Text m_Desc;
        [SerializeField] TMP_Text m_Footer;
        [SerializeField] Button m_Blocker;           // 全屏透明按钮：点空白处关闭
        [SerializeField] float m_Gap = 12f;

        static readonly string[] s_Quality = { "普通", "优秀", "精良", "史诗", "传说" };
        static readonly string[] s_Category = { "装备", "消耗品", "材料" };
        static readonly Color[] s_NameColors =
        {
            Color.white, new Color32(0x5B, 0xE5, 0x84, 0xFF), new Color32(0x6F, 0xA8, 0xFF, 0xFF),
            new Color32(0xC0, 0x8B, 0xFF, 0xFF), new Color32(0xFF, 0xB5, 0x47, 0xFF),
        };

        void Awake() => m_Blocker.onClick.AddListener(Hide);

        public void Show(BagItem item, RectTransform cell)
        {
            gameObject.SetActive(true);
            m_Icon.sprite = UISkin.Sprite(item.Icon);
            m_NameText.text = item.Name;
            m_NameText.color = s_NameColors[item.Quality];
            m_Type.text = $"{s_Quality[item.Quality]} · {s_Category[item.Category]}";
            m_Desc.text = item.Desc;
            m_Footer.text = $"数量 {item.Count}　　出售 {item.Price} 金币";
            // 内容变了，先强制把布局算出来，才能拿到正确的高度去做边界判断
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_Panel);
            Place(cell);
        }

        void Place(RectTransform cell)
        {
            var parent = (RectTransform)m_Panel.parent;
            var corners = new Vector3[4];
            cell.GetWorldCorners(corners);                       // 左下、左上、右上、右下
            Vector2 left = parent.InverseTransformPoint(corners[1]);   // 格子左上角
            Vector2 right = parent.InverseTransformPoint(corners[2]);  // 格子右上角
            Rect area = parent.rect;
            Vector2 size = m_Panel.rect.size;
            m_Panel.pivot = new Vector2(0, 1);
            // 先试右边，放不下再放左边
            float x = right.x + m_Gap;
            if (x + size.x > area.xMax) x = left.x - m_Gap - size.x;
            // 顶部和格子对齐，再夹在屏幕上下边界内
            float y = Mathf.Clamp(right.y, area.yMin + size.y, area.yMax);
            m_Panel.localPosition = new Vector3(x, y, 0);   // 与锚点无关：直接设轴心在父节点里的位置
        }

        public void Hide() => gameObject.SetActive(false);
    }
    //#endregion
}
