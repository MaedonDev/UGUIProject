// 案例 11.2：拖放背包——格子。接收 OnDrop：空格子直接放入；有东西就和来源格子交换。
// 拖着物品经过时高亮（OnPointerEnter/Exit 在拖拽期间照样会发，pointerDrag 告诉我们“手上拿着什么”）。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] GameObject m_Highlight;

        public InventoryItem item => GetComponentInChildren<InventoryItem>();

        //#region drop
        public void OnDrop(PointerEventData e)
        {
            var dragged = e.pointerDrag != null ? e.pointerDrag.GetComponent<InventoryItem>() : null;
            if (dragged == null) return;                         // 拖过来的不是物品（比如拖的是滚动列表）
            var mine = item;                                     // 拖起的物品已经挪到拖拽层，所以这里拿到的是“原住民”
            if (mine != null) mine.PlaceInto(dragged.origin);    // 交换：原住民搬去对方的格子
            dragged.PlaceInto(this);
            SetHighlight(false);
        }
        //#endregion

        public void OnPointerEnter(PointerEventData e)
        {
            if (e.dragging && e.pointerDrag != null && e.pointerDrag.GetComponent<InventoryItem>() != null) SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData e) => SetHighlight(false);

        public void SetHighlight(bool on) { if (m_Highlight != null) m_Highlight.SetActive(on); }
    }
}
