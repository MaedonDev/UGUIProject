// 案例 11.2：把物品拖到垃圾桶丢弃。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch11
{
    public class TrashZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Image m_Icon;
        [SerializeField] TMP_Text m_Hint;
        [SerializeField] Color m_Normal = Color.gray, m_Hot = new Color32(0xE5, 0x48, 0x4D, 0xFF);

        public void OnDrop(PointerEventData e)
        {
            var item = e.pointerDrag != null ? e.pointerDrag.GetComponent<InventoryItem>() : null;
            if (item == null) return;
            if (m_Hint != null) m_Hint.text = "丢弃了 " + item.itemId;
            item.Discard();
            m_Icon.color = m_Normal;
        }

        public void OnPointerEnter(PointerEventData e) { if (e.dragging) m_Icon.color = m_Hot; }
        public void OnPointerExit(PointerEventData e) => m_Icon.color = m_Normal;
    }
}
