// 让 UI 跟随 3D 物体：头顶血条、名字、交互提示……
// 所有跟随者由一个 HUDFollowRoot 在 LateUpdate 里统一刷新（等角色和相机都移动完再算位置，避免抖一帧）。
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region util
    public static class HUDUtil
    {
        /// Overlay 画布不需要相机，其余模式用画布的 worldCamera
        public static Camera UICamera(Canvas canvas) =>
            canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        /// 世界坐标 → space 节点下的本地坐标。depth 是到相机的距离（沿视线方向），小于 0 表示在相机背后
        public static bool WorldToLocal(Camera worldCam, Vector3 world, RectTransform space, Camera uiCam,
            out Vector2 local, out float depth)
        {
            Vector3 sp = worldCam.WorldToScreenPoint(world);
            depth = sp.z;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(space, sp, uiCam, out local);
        }
    }
    //#endregion

    //#region follower
    /// 挂在要跟随的 UI 节点上（节点本身负责移动，子节点 Content 负责显示/隐藏）
    public class HUDFollower : MonoBehaviour
    {
        public Transform target;
        [Tooltip("世界空间偏移，比如头顶 = (0, 2.2, 0)")]
        public Vector3 worldOffset = new Vector3(0, 2.2f, 0);
        [Tooltip("屏幕空间偏移（画布单位）")]
        public Vector2 screenOffset;
        [Tooltip("要显示/隐藏的子节点；为空时只移动不隐藏")]
        public GameObject content;
        public float maxDistance = 40f;

        [Header("按距离缩放")]
        public bool scaleByDistance = true;
        public float nearDistance = 6f, farDistance = 30f;
        public float nearScale = 1.1f, farScale = 0.55f;

        public float Depth { get; private set; }
        public bool Visible { get; private set; } = true;

        void OnEnable() { if (HUDFollowRoot.Instance != null) HUDFollowRoot.Instance.Register(this); }
        void OnDisable() { if (HUDFollowRoot.Instance != null) HUDFollowRoot.Instance.Unregister(this); }

        /// 由 HUDFollowRoot 调用；也可以在编辑器里直接调用来预览
        public void UpdatePosition(Camera worldCam, RectTransform space, Camera uiCam, Rect visibleRect)
        {
            bool show = target != null && target.gameObject.activeInHierarchy;
            if (show)
            {
                HUDUtil.WorldToLocal(worldCam, target.position + worldOffset, space, uiCam, out var local, out float depth);
                Depth = depth;
                local += screenOffset;
                // 背后（depth<0）的点投影出来会出现在屏幕上的“镜像”位置，必须隐藏
                show = depth > worldCam.nearClipPlane && depth < maxDistance && visibleRect.Contains(local);
                if (show)
                {
                    var rt = (RectTransform)transform;
                    rt.localPosition = new Vector3(local.x, local.y, 0);
                    if (scaleByDistance)
                    {
                        float t = Mathf.InverseLerp(nearDistance, farDistance, depth);
                        rt.localScale = Vector3.one * Mathf.Lerp(nearScale, farScale, t);
                    }
                }
            }
            SetVisible(show);
        }

        void SetVisible(bool v)
        {
            // 只在状态变化时切换：SetActive 会触发重建，每帧切是浪费
            if (Visible == v) return;
            Visible = v;
            if (content != null) content.SetActive(v);
        }
    }
    //#endregion
}
