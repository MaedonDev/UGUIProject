// 案例 1.5：点哪张卡片，就把它挪到同级的最后——也就是最后绘制、显示在最上面。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch01
{
    //#region front
    public class BringToFront : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            // 同一个父节点下，越靠后的子节点越晚绘制，所以“置顶”= 移到最后
            transform.SetAsLastSibling();
        }
    }
    //#endregion
}
