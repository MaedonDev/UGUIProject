using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch18
{
    //#region cell
    /// 背包格子。难点在于它同时在 ScrollRect 里：
    /// 直接拖 = 滚动列表（把拖拽事件转交给 ScrollRect）；按住 0.3 秒不动再拖 = 拖动物品
    public class BagCell : MonoBehaviour, IPointerDownHandler, IPointerClickHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] Image m_Quality;
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_Count;
        [SerializeField] GameObject m_Selected;
        [SerializeField] GameObject m_DropHint;
        [SerializeField] float m_LongPress = 0.3f;

        public int Index { get; private set; }
        BagView m_View;
        float m_DownTime;
        bool m_DraggingItem, m_ForwardToScroll;

        public static readonly Color[] QualityColors =
        {
            new Color32(0x4A, 0x52, 0x6E, 0xFF), new Color32(0x2F, 0x9E, 0x5E, 0xFF),
            new Color32(0x3A, 0x78, 0xE0, 0xFF), new Color32(0x96, 0x55, 0xE8, 0xFF), new Color32(0xE6, 0x8A, 0x2E, 0xFF),
        };

        public void Set(BagView view, int index, BagItem item, bool selected)
        {
            m_View = view;
            Index = index;
            m_Quality.color = QualityColors[item.Quality];
            m_Icon.sprite = UISkin.Sprite(item.Icon);
            m_Count.text = item.Count > 1 ? item.Count.ToString() : "";
            m_Selected.SetActive(selected);
            m_DropHint.SetActive(false);
        }

        public void SetDropHint(bool on) => m_DropHint.SetActive(on);

        public void OnPointerDown(PointerEventData e) => m_DownTime = Time.unscaledTime;

        public void OnPointerClick(PointerEventData e)
        {
            if (!e.dragging) m_View.Select(Index, (RectTransform)transform);
        }

        public void OnBeginDrag(PointerEventData e)
        {
            m_DraggingItem = Time.unscaledTime - m_DownTime >= m_LongPress;
            m_ForwardToScroll = !m_DraggingItem;
            if (m_ForwardToScroll) m_View.ScrollRect.OnBeginDrag(e);   // 交给 ScrollRect：正常滚动
            else m_View.BeginItemDrag(Index, e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (m_ForwardToScroll) m_View.ScrollRect.OnDrag(e);
            else if (m_DraggingItem) m_View.DragItem(e);
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (m_ForwardToScroll) m_View.ScrollRect.OnEndDrag(e);
            else if (m_DraggingItem) m_View.EndItemDrag();
            m_DraggingItem = m_ForwardToScroll = false;
        }

        // 别的格子拖过来、在我这里松手
        public void OnDrop(PointerEventData e) => m_View.DropOn(Index);
    }
    //#endregion
}
