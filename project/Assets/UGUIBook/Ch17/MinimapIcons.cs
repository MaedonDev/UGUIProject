// 小地图的两种做法：
//   MinimapIcons  —— 图标方案：把世界坐标 XZ 按比例映射到 UI 上，只画图标（便宜，风格统一）
//   MinimapCamera —— RenderTexture 方案：一台俯视正交相机把场景渲染到 RT，用 RawImage 显示（所见即所得，但多一次场景渲染）
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region icons
    public class MinimapIcons : MonoBehaviour
    {
        [Serializable]
        public class Marker
        {
            public Transform target;
            public RectTransform icon;
            [Tooltip("超出地图范围时贴在边缘显示（任务目标），否则直接隐藏（普通敌人）")]
            public bool clampToEdge;
        }

        [SerializeField] RectTransform m_Map;          // 圆形地图区域（轴心居中）
        [SerializeField] RectTransform m_Background;   // 地图底图：旋转模式下跟着转
        [SerializeField] RectTransform m_PlayerIcon;
        [SerializeField] Transform m_Player;
        [SerializeField] float m_WorldRadius = 30f;    // 地图半径对应多少米
        [SerializeField] bool m_RotateWithPlayer;      // true：玩家朝向永远朝上，地图转
        [SerializeField] List<Marker> m_Markers = new List<Marker>();

        public List<Marker> Markers => m_Markers;
        public bool RotateWithPlayer { get => m_RotateWithPlayer; set => m_RotateWithPlayer = value; }

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            if (m_Player == null) return;
            float mapRadius = m_Map.rect.width * 0.5f;
            float scale = mapRadius / m_WorldRadius;
            float yaw = m_Player.eulerAngles.y;
            // 旋转模式：所有相对位置绕玩家反向转 yaw，玩家箭头固定朝上
            var rot = m_RotateWithPlayer ? Quaternion.Euler(0, 0, yaw) : Quaternion.identity;
            m_PlayerIcon.localRotation = m_RotateWithPlayer ? Quaternion.identity : Quaternion.Euler(0, 0, -yaw);
            if (m_Background != null) m_Background.localRotation = rot;

            foreach (var m in m_Markers)
            {
                if (m.target == null || !m.target.gameObject.activeInHierarchy) { SetActive(m.icon, false); continue; }
                Vector3 d = m.target.position - m_Player.position;
                Vector2 p = rot * new Vector2(d.x, d.z) * scale;   // 世界 X → 地图 x，世界 Z → 地图 y
                float r = mapRadius - m.icon.rect.width * 0.5f;
                if (p.magnitude > r)
                {
                    if (!m.clampToEdge) { SetActive(m.icon, false); continue; }
                    p = p.normalized * r;
                }
                SetActive(m.icon, true);
                m.icon.localPosition = p;
            }
        }

        static void SetActive(Component c, bool v)
        {
            if (c.gameObject.activeSelf != v) c.gameObject.SetActive(v);
        }
    }
    //#endregion
}
