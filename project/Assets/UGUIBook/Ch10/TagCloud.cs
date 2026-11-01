// 案例 10.4 的演示脚本：用 FlowLayoutGroup 排标签。点击标签切换选中，输入框回车添加新标签。
// 每个标签自己是“Image + HorizontalLayoutGroup(带 padding) + TMP 文字”，所以它的 preferred 宽度
// = 文字宽度 + 左右 padding，FlowLayoutGroup 直接拿来用，不需要任何 ContentSizeFitter。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    public class TagCloud : MonoBehaviour
    {
        [SerializeField] RectTransform m_Container;    // 挂 FlowLayoutGroup 的节点
        [SerializeField] Button m_Template;            // 隐藏的标签模板
        [SerializeField] TMP_InputField m_Input;
        [SerializeField] Color m_Normal = new Color(0.93f, 0.94f, 0.98f);
        [SerializeField] Color m_Selected = new Color32(0x5B, 0x4B, 0xD6, 0xFF);
        [SerializeField] string[] m_Initial = { "动作", "角色扮演", "开放世界", "像素风", "多人联机", "Roguelike", "策略", "剧情丰富", "休闲", "二次元", "卡牌", "生存建造", "解谜", "音乐节奏", "独立游戏" };

        void Start()
        {
            if (m_Container.childCount == 0) Populate(m_Initial);   // 场景里已经放好标签就不重复生成
            else foreach (var b in m_Container.GetComponentsInChildren<Button>()) Hook(b);
            if (m_Input != null) m_Input.onSubmit.AddListener(s => { if (s.Trim().Length > 0) AddTag(s.Trim(), false); m_Input.text = ""; });
        }

        public void Populate(string[] tags, params int[] selected)
        {
            for (int i = 0; i < tags.Length; i++) AddTag(tags[i], System.Array.IndexOf(selected, i) >= 0);
        }

        public Button AddTag(string label, bool selected)
        {
            var b = Instantiate(m_Template, m_Container);
            b.gameObject.SetActive(true);
            b.name = "Tag_" + label;
            b.GetComponentInChildren<TMP_Text>().text = label;
            SetSelected(b, selected);
            if (Application.isPlaying) Hook(b);
            return b;
        }

        void Hook(Button b) => b.onClick.AddListener(() => SetSelected(b, !IsSelected(b)));

        bool IsSelected(Button b) => b.image.color == m_Selected;

        void SetSelected(Button b, bool on)
        {
            b.image.color = on ? m_Selected : m_Normal;
            b.GetComponentInChildren<TMP_Text>().color = on ? Color.white : new Color32(0x1F, 0x24, 0x36, 0xFF);
        }
    }
}
