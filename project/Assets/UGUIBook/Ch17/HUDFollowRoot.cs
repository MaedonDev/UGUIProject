// 让 UI 跟随 3D 物体：头顶血条、名字、交互提示……
// 所有跟随者由一个 HUDFollowRoot 在 LateUpdate 里统一刷新（等角色和相机都移动完再算位置，避免抖一帧）。
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region root
    /// 挂在一个铺满画布的节点上，所有 HUDFollower 都放在它下面
    public class HUDFollowRoot : MonoBehaviour
    {
        public static HUDFollowRoot Instance { get; private set; }

        [SerializeField] Camera m_WorldCamera;
        [SerializeField] Canvas m_Canvas;
        [Tooltip("近的血条盖住远的：按深度调整层级顺序（只在顺序变化时才改）")]
        [SerializeField] bool m_SortByDepth = true;
        [Tooltip("超出屏幕多少画布单位以内仍然显示（血条刚出屏幕边时不要突然消失）")]
        [SerializeField] float m_Margin = 100f;

        readonly List<HUDFollower> m_Followers = new List<HUDFollower>();
        static readonly System.Comparison<HUDFollower> s_ByDepth = (a, b) => b.Depth.CompareTo(a.Depth);

        public Camera WorldCamera { get => m_WorldCamera; set => m_WorldCamera = value; }

        void Awake() => Collect();

        /// 把子节点里的 HUDFollower 全部登记（运行时由 Awake 调用；编辑器里预览时手动调用）
        public void Collect()
        {
            Instance = this;
            m_Followers.Clear();
            GetComponentsInChildren(true, m_Followers);
        }

        public void Register(HUDFollower f) { if (!m_Followers.Contains(f)) m_Followers.Add(f); }
        public void Unregister(HUDFollower f) => m_Followers.Remove(f);

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (m_WorldCamera == null) return;
            var space = (RectTransform)transform;
            var uiCam = HUDUtil.UICamera(m_Canvas);
            var r = space.rect;
            var visible = new Rect(r.xMin - m_Margin, r.yMin - m_Margin, r.width + m_Margin * 2, r.height + m_Margin * 2);
            foreach (var f in m_Followers) f.UpdatePosition(m_WorldCamera, space, uiCam, visible);
            if (m_SortByDepth) SortByDepth();
        }

        void SortByDepth()
        {
            m_Followers.Sort(s_ByDepth);   // 远的在前（先画），近的在后（后画、盖在上面）
            for (int i = 0; i < m_Followers.Count; i++)
                if (m_Followers[i].transform.GetSiblingIndex() != i) m_Followers[i].transform.SetSiblingIndex(i);
        }
    }
    //#endregion
}
