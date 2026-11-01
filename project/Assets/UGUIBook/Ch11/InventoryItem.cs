// 案例 11.2：拖放背包——可拖动的物品。
// 拖起时把自己挪到最上层的“拖拽层”（保证盖在所有格子上面），并关掉 CanvasGroup.blocksRaycasts，
// 让射线穿过自己打到下面的格子——否则 OnDrop 永远只会落在物品自己身上。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch11
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform m_DragLayer;      // 画布最上层的一个空节点
        public string itemId;

        CanvasGroup m_Group;
        InventorySlot m_From;                            // 拖起时所在的格子
        Vector3 m_GrabOffset;
        bool m_Dropped;

        public InventorySlot slot => GetComponentInParent<InventorySlot>();

        void Awake() => m_Group = GetComponent<CanvasGroup>();

        //#region drag
        public void OnBeginDrag(PointerEventData e)
        {
            m_From = slot;
            m_Dropped = false;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DragLayer, e.position, e.pressEventCamera, out var world);
            m_GrabOffset = transform.position - world;
            Lift();
        }

        public void OnDrag(PointerEventData e)
        {
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DragLayer, e.position, e.pressEventCamera, out var world))
                transform.position = world + m_GrabOffset;
        }

        public void OnEndDrag(PointerEventData e)
        {
            // OnDrop 比 OnEndDrag 先执行（见 11.3 节源码）。没有任何格子接收，就回到原位
            if (!m_Dropped && this != null) PlaceInto(m_From);
            m_Group.blocksRaycasts = true;
            m_Group.alpha = 1;
        }
        //#endregion

        /// 进入“拖拽中”的外观：挪到拖拽层、半透明、不挡射线
        public void Lift()
        {
            if (m_Group == null) m_Group = GetComponent<CanvasGroup>();
            transform.SetParent(m_DragLayer, true);
            m_Group.blocksRaycasts = false;
            m_Group.alpha = 0.85f;
        }

        /// 放进某个格子（null = 不动）
        public void PlaceInto(InventorySlot target)
        {
            if (target == null) return;
            m_Dropped = true;
            var rt = (RectTransform)transform;
            rt.SetParent(target.transform, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        /// 被扔进垃圾桶
        public void Discard()
        {
            m_Dropped = true;
            Destroy(gameObject);
        }

        public InventorySlot origin => m_From;
    }
}
