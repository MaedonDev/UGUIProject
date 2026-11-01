// 最简单的拖动：按住拖着 RectTransform 走（配套场景里拖动贝塞尔连线的节点）。拖拽事件的细节见第 11 章。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch13
{
    public class DragMove : MonoBehaviour, IDragHandler
    {
        public void OnDrag(PointerEventData e)
        {
            var rt = (RectTransform)transform;
            var canvas = GetComponentInParent<Canvas>();
            // delta 是屏幕像素，除以画布缩放系数换算成 UI 单位
            rt.anchoredPosition += e.delta / (canvas != null ? canvas.rootCanvas.scaleFactor : 1f);
        }
    }
}
