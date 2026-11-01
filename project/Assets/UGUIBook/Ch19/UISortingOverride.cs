// 把某个 UI 元素“抬”到粒子上面：给它加一个开启 Override Sorting 的嵌套画布，order = 所在画布 + offset。
// 典型用法：按钮上冒粒子，但按钮上的文字/红点要压在粒子上面。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch19
{
    //#region override
    public class UISortingOverride : MonoBehaviour
    {
        public int offset = 2;

        void Awake() => Apply();

        public void Apply()
        {
            var parent = transform.parent != null ? UIParticleOrder.SortingCanvas(transform.parent) : null;
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            if (parent != null)
            {
                canvas.sortingLayerID = parent.sortingLayerID;
                canvas.sortingOrder = parent.sortingOrder + offset;
            }
            // 嵌套画布要自带 GraphicRaycaster，否则它下面的按钮点不到
            if (GetComponent<GraphicRaycaster>() == null) gameObject.AddComponent<GraphicRaycaster>();
        }
    }
    //#endregion
}
