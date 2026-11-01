// 小地图的两种做法：
//   MinimapIcons  —— 图标方案：把世界坐标 XZ 按比例映射到 UI 上，只画图标（便宜，风格统一）
//   MinimapCamera —— RenderTexture 方案：一台俯视正交相机把场景渲染到 RT，用 RawImage 显示（所见即所得，但多一次场景渲染）
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region camera
    /// 俯视小地图相机：跟随玩家，输出到 RenderTexture（相机的 Target Texture）
    [RequireComponent(typeof(Camera))]
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] Transform m_Player;
        [SerializeField] float m_Height = 40f;
        [SerializeField] bool m_RotateWithPlayer;
        [Tooltip("每隔几帧渲染一次：小地图不需要 60 帧，隔帧渲染能省一半开销")]
        [SerializeField, Range(1, 4)] int m_RenderInterval = 2;

        Camera m_Camera;

        void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_Camera.enabled = false;   // 关掉自动渲染，自己控制频率
        }

        void LateUpdate()
        {
            Follow();
            if (Time.frameCount % m_RenderInterval == 0) m_Camera.Render();
        }

        public void Follow()
        {
            if (m_Player == null) return;
            transform.position = m_Player.position + Vector3.up * m_Height;
            float yaw = m_RotateWithPlayer ? m_Player.eulerAngles.y : 0;
            transform.rotation = Quaternion.Euler(90, yaw, 0);
        }

        /// 编辑器预览、截图时手动渲染一帧
        public void RenderNow()
        {
            if (m_Camera == null) m_Camera = GetComponent<Camera>();
            Follow();
            m_Camera.Render();
        }
    }
    //#endregion
}
