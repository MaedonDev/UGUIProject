// 10.8 节“布局一帧延迟”的演示：往带 VerticalLayoutGroup + ContentSizeFitter 的列表里加一项，
// 分别在“刚加完”“ForceRebuildLayoutImmediate 之后”读列表高度，把结果显示在屏幕上。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch10
{
    public class LayoutTimingDemo : MonoBehaviour
    {
        [SerializeField] RectTransform m_List;        // VerticalLayoutGroup + ContentSizeFitter
        [SerializeField] RectTransform m_ItemTemplate;
        [SerializeField] TMP_Text m_Report;
        [SerializeField] Button m_AddButton;

        void Awake() => m_AddButton.onClick.AddListener(() => AddAndMeasure());

        //#region measure
        public void AddAndMeasure()
        {
            float before = m_List.rect.height;

            var item = Instantiate(m_ItemTemplate, m_List);
            item.gameObject.SetActive(true);
            item.GetComponentInChildren<TMP_Text>().text = "第 " + m_List.childCount + " 项";

            // ① 刚加完：布局只是被“标脏”了，真正的计算要等到本帧渲染前（Canvas.willRenderCanvases）
            float justAdded = m_List.rect.height;

            // ② 立即重建以 m_List 为根的布局（只算这一棵子树，不碰图形重建）
            LayoutRebuilder.ForceRebuildLayoutImmediate(m_List);
            float forced = m_List.rect.height;

            m_Report.text = $"加之前：{before:0}    刚加完：{justAdded:0}    强制重建后：{forced:0}";
        }
        //#endregion
    }
}
