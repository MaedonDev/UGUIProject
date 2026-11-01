// World Space 头顶名牌：始终朝向相机，可选“屏幕上大小恒定”。
// 与第 17 章“Screen Space 画布 + WorldToScreenPoint 跟随”相比：名牌天然被墙挡住、有近大远小（或恒定大小），
// 不用每帧换算坐标；代价是每个名牌一个 World Space 画布（各自合批），数量多时要注意 Draw Call。
using UnityEngine;

namespace UGUIBook.Ch19
{
    //#region billboard
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        public Camera cam;
        [Tooltip("只绕 Y 轴转，名牌保持竖直（第三人称游戏常用）")]
        public bool keepUpright = true;
        [Tooltip("屏幕上的大小不随距离变化")]
        public bool constantScreenSize;
        [Tooltip("constantScreenSize 时：距离 1 米处的缩放")]
        public float scalePerMeter = 0.0015f;
        Vector3 m_BaseScale;

        void OnEnable() => m_BaseScale = transform.localScale;

        void LateUpdate() => Face();

        public void Face()
        {
            var c = cam != null ? cam : Camera.main;
            if (c == null) return;
            // 与相机“朝向”平行，而不是看向相机位置：屏幕边缘的名牌也不会歪
            var forward = c.transform.forward;
            if (keepUpright) { forward.y = 0; if (forward.sqrMagnitude < 1e-4f) return; }
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            if (constantScreenSize)
            {
                float dist = Vector3.Dot(transform.position - c.transform.position, c.transform.forward);
                transform.localScale = Vector3.one * Mathf.Max(0.01f, dist) * scalePerMeter;
            }
            else if (m_BaseScale != Vector3.zero) transform.localScale = m_BaseScale;
        }
    }
    //#endregion
}
