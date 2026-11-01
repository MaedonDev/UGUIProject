// 安全区适配（第 3 章 SafeArea 组件的加强版）：
//  · 可以只适配部分边（比如底栏只让开 Home 指示条，顶部背景延伸进刘海）
//  · 转屏、分辨率变化、安全区变化（折叠屏展开、Android 切换手势导航）都会重新计算，且只在变化时才改锚点
//  · 编辑器里可以模拟一个安全区，不装 Device Simulator 也能预览
// 挂在“铺满整个画布”的面板上（它的父节点必须与屏幕一样大），内容放在它下面。
using System;
using UnityEngine;

namespace UGUIBook.Ch19
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaPanel : MonoBehaviour
    {
        [Flags] public enum Edges { None = 0, Left = 1, Right = 2, Top = 4, Bottom = 8, All = 15 }

        [Tooltip("要让开哪些边")]
        public Edges conform = Edges.All;
        [Tooltip("在编辑器里用下面的模拟值代替 Screen.safeArea")]
        public bool simulateInEditor;
        [Tooltip("模拟的安全区内缩比例：x=左 y=下 z=右 w=上（0~1，相对屏幕宽/高）")]
        public Vector4 simulatedInsets = new Vector4(0, 0.04f, 0, 0.07f);
        [Tooltip("在安全区之外再额外留的边距（画布像素）")]
        public float extraPadding;

        RectTransform m_Rt;
        Rect m_LastSafe;
        Vector2Int m_LastScreen;
        Edges m_LastConform = (Edges)(-1);

        void OnEnable() { m_Rt = (RectTransform)transform; m_LastScreen = Vector2Int.zero; Update(); }

        //#region update
        void Update()
        {
            var screen = new Vector2Int(Screen.width, Screen.height);
            var safe = CurrentSafeArea(screen);
            // 只在真的变了才改 RectTransform：每帧赋值同样的锚点虽然不会重建，但没必要
            if (safe == m_LastSafe && screen == m_LastScreen && conform == m_LastConform) return;
            m_LastSafe = safe; m_LastScreen = screen; m_LastConform = conform;
            if (screen.x <= 0 || screen.y <= 0) return;
            ApplyNormalized(new Vector2(safe.xMin / screen.x, safe.yMin / screen.y),
                            new Vector2(safe.xMax / screen.x, safe.yMax / screen.y));
        }

        Rect CurrentSafeArea(Vector2Int screen)
        {
            if (Application.isEditor && simulateInEditor)
            {
                var v = simulatedInsets;
                return Rect.MinMaxRect(v.x * screen.x, v.y * screen.y, (1 - v.z) * screen.x, (1 - v.w) * screen.y);
            }
            return Screen.safeArea;   // 像素坐标，原点在左下角
        }
        //#endregion

        //#region apply
        /// 以“占屏幕的比例”设置安全区，并按 conform 决定哪些边真的让开
        public void ApplyNormalized(Vector2 min, Vector2 max)
        {
            if (m_Rt == null) m_Rt = (RectTransform)transform;
            if ((conform & Edges.Left) == 0) min.x = 0;
            if ((conform & Edges.Bottom) == 0) min.y = 0;
            if ((conform & Edges.Right) == 0) max.x = 1;
            if ((conform & Edges.Top) == 0) max.y = 1;
            m_Rt.anchorMin = min;
            m_Rt.anchorMax = max;
            // 锚点已经框出了安全区，偏移只剩额外边距
            m_Rt.offsetMin = new Vector2((conform & Edges.Left) != 0 ? extraPadding : 0, (conform & Edges.Bottom) != 0 ? extraPadding : 0);
            m_Rt.offsetMax = new Vector2((conform & Edges.Right) != 0 ? -extraPadding : 0, (conform & Edges.Top) != 0 ? -extraPadding : 0);
        }

        /// 预览/测试用：直接按内缩比例套用（x=左 y=下 z=右 w=上）
        public void ApplyInsets(Vector4 insets) =>
            ApplyNormalized(new Vector2(insets.x, insets.y), new Vector2(1 - insets.z, 1 - insets.w));
        //#endregion
    }
}
