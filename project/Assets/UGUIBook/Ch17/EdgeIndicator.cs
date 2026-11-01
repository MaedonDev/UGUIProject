// 屏幕边缘目标指示：目标在屏幕外（包括身后）时，在屏幕边上画一个指向它的箭头 + 距离
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region edge
    public class EdgeIndicator : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public Transform target;
            public RectTransform view;     // 指示器根节点：位置跟着边缘走
            public RectTransform arrow;    // 只有箭头旋转，图标和文字保持正立
            public TMP_Text distance;
        }

        [SerializeField] Camera m_WorldCamera;
        [SerializeField] Transform m_Player;          // 距离从玩家算起
        [SerializeField] float m_EdgeMargin = 70f;    // 离屏幕边多远（画布单位）
        [SerializeField] List<Entry> m_Entries = new List<Entry>();

        public List<Entry> Entries => m_Entries;

        void LateUpdate() => Refresh();

        public void Refresh()
        {
            var space = (RectTransform)transform;           // 铺满画布、轴心在中心的节点
            Vector2 half = space.rect.size * 0.5f - Vector2.one * m_EdgeMargin;
            foreach (var e in m_Entries)
            {
                bool show = false;
                if (e.target != null && e.target.gameObject.activeInHierarchy)
                {
                    Vector3 vp = m_WorldCamera.WorldToViewportPoint(e.target.position);
                    bool behind = vp.z < 0;
                    bool onScreen = !behind && vp.x > 0 && vp.x < 1 && vp.y > 0 && vp.y < 1;
                    if (!onScreen)
                    {
                        show = true;
                        // 以屏幕中心为原点的方向；在身后时投影是反的，要翻转
                        Vector2 dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
                        if (behind) dir = -dir;
                        if (dir.sqrMagnitude < 1e-6f) dir = Vector2.down;
                        dir.x *= space.rect.width;            // 换到画布单位，保证宽高比正确
                        dir.y *= space.rect.height;
                        // 沿 dir 射线和内缩后的屏幕矩形求交：取先碰到的那条边
                        float k = Mathf.Min(half.x / Mathf.Abs(dir.x + 1e-5f), half.y / Mathf.Abs(dir.y + 1e-5f));
                        e.view.localPosition = dir * k;
                        e.arrow.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
                        if (e.distance != null && m_Player != null)
                            e.distance.text = Mathf.RoundToInt(Vector3.Distance(m_Player.position, e.target.position)) + "m";
                    }
                }
                if (e.view.gameObject.activeSelf != show) e.view.gameObject.SetActive(show);   // 只在变化时切换
            }
        }
    }
    //#endregion
}
