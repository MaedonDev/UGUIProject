// 案例 11.4：点击弹窗外的空白处关闭弹窗。
// 结构：Popup(根) → Blocker(全屏半透明 Image + 本脚本) + Panel(弹窗本体)。Blocker 和 Panel 是兄弟，Blocker 在前（先画、在下面）。
// 点到 Panel 上：射线最先命中 Panel 的图片，事件沿 Panel 的父链冒泡，到不了 Blocker；
// 点到 Panel 外：命中 Blocker，关闭。顺带它还挡住了弹窗下面的所有按钮——这正是模态弹窗想要的。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    public class PopupBlocker : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] GameObject m_Popup;     // 要关闭的整个弹窗（通常是 Blocker 的父物体）
        public UnityEvent onClose = new UnityEvent();

        //#region click
        public void OnPointerClick(PointerEventData e)
        {
            // 保险：只有“按下和抬起都直接落在 Blocker 本身”才算点空白。
            // 如果有人把 Panel 错放成了 Blocker 的子物体，点 Panel 的点击会冒泡上来，这一行能把它挡掉
            if (e.pointerPressRaycast.gameObject != gameObject) return;
            m_Popup.SetActive(false);
            onClose.Invoke();
        }
        //#endregion
    }
}
