// 让粒子跟着所在画布排序：粒子的 sortingOrder = 所属画布的 sortingOrder + offset。
// 适用于 Screen Space - Camera / World Space 画布（Overlay 画布不经过相机，粒子永远在它下面）。
using UnityEngine;

namespace UGUIBook.Ch19
{
    //#region order
    [ExecuteAlways]
    public class UIParticleOrder : MonoBehaviour
    {
        [Tooltip("相对所属画布的排序偏移：+1 = 盖在这个画布上，-1 = 垫在它下面")]
        public int offset = 1;

        void OnEnable() => Apply();
        void OnTransformParentChanged() => Apply();   // 被挪到别的界面下时重新算
        void OnValidate() => Apply();

        public void Apply()
        {
            var canvas = SortingCanvas(transform);
            if (canvas == null) return;
            foreach (var r in GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                r.sortingLayerID = canvas.sortingLayerID;
                r.sortingOrder = canvas.sortingOrder + offset;
            }
        }

        /// 真正决定排序的画布：最近的一个“根画布或开了 Override Sorting 的嵌套画布”
        public static Canvas SortingCanvas(Transform t)
        {
            for (var c = t.GetComponentInParent<Canvas>(); c != null; c = c.transform.parent ? c.transform.parent.GetComponentInParent<Canvas>() : null)
                if (c.isRootCanvas || c.overrideSorting) return c;
            return null;
        }
    }
    //#endregion
}
