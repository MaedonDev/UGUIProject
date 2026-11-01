// 按“宽高比”在横屏 / 竖屏两套布局之间切换。
// 做法：给每个需要变化的 RectTransform 记两份“快照”（锚点、轴心、位置、尺寸），比例越过阈值时整体套用另一份。
// 判断用的是本节点自己的 rect（通常挂在 Canvas 或全屏面板上），所以在编辑器里拖 Game 视图、在嵌套的“预览框”里都能正确响应。
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch19
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AspectLayout : UIBehaviour
    {
        public enum Mode { Landscape, Portrait }

        //#region state
        [Serializable]
        public struct RectState
        {
            public Vector2 anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta;

            public static RectState Capture(RectTransform rt) => new RectState
            {
                anchorMin = rt.anchorMin, anchorMax = rt.anchorMax, pivot = rt.pivot,
                anchoredPosition = rt.anchoredPosition, sizeDelta = rt.sizeDelta,
            };

            public void Apply(RectTransform rt)
            {
                rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
                rt.anchoredPosition = anchoredPosition; rt.sizeDelta = sizeDelta;
            }
        }

        [Serializable]
        public class Entry
        {
            public RectTransform target;
            public RectState landscape;
            public RectState portrait;
        }
        //#endregion

        [Tooltip("宽 / 高 小于这个值算竖屏")]
        public float portraitBelow = 1f;
        [Tooltip("滞回区间：比例在阈值 ± 这个值之间时保持当前模式，避免在临界比例来回跳")]
        public float hysteresis = 0.05f;
        [Tooltip("编辑模式下也实时切换（会覆盖你在场景里手动改的位置，平时建议关掉）")]
        public bool previewInEditMode;
        public List<Entry> entries = new List<Entry>();
        public GameObject[] landscapeOnly;
        public GameObject[] portraitOnly;
        public ModeEvent onModeChanged = new ModeEvent();

        [Serializable] public class ModeEvent : UnityEvent<Mode> { }

        public Mode Current { get; private set; }
        bool m_Applied;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Applied = false;
            Refresh();
        }

        protected override void OnRectTransformDimensionsChange() => Refresh();

        //#region refresh
        public void Refresh(bool force = false)
        {
            if (!Application.isPlaying && !previewInEditMode && !force) return;
            var r = ((RectTransform)transform).rect;
            if (r.width <= 0 || r.height <= 0) return;
            float aspect = r.width / r.height;
            var mode = Current;
            if (!m_Applied || force) mode = aspect < portraitBelow ? Mode.Portrait : Mode.Landscape;
            else if (Current == Mode.Landscape && aspect < portraitBelow - hysteresis) mode = Mode.Portrait;
            else if (Current == Mode.Portrait && aspect > portraitBelow + hysteresis) mode = Mode.Landscape;
            if (m_Applied && mode == Current && !force) return;
            Apply(mode);
        }

        public void Apply(Mode mode)
        {
            Current = mode;
            m_Applied = true;
            foreach (var e in entries)
                if (e.target != null) (mode == Mode.Portrait ? e.portrait : e.landscape).Apply(e.target);
            foreach (var go in landscapeOnly) if (go != null) go.SetActive(mode == Mode.Landscape);
            foreach (var go in portraitOnly) if (go != null) go.SetActive(mode == Mode.Portrait);
            // 我们是在 OnRectTransformDimensionsChange 里改的子节点，布局组要在下一次布局计算时重新排
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
            onModeChanged.Invoke(mode);
        }
        //#endregion

        //#region capture
        // 用法：先把场景摆成横屏的样子 → 右键组件标题选“记录当前为横屏布局”；再摆成竖屏的样子 → “记录当前为竖屏布局”
        [ContextMenu("记录当前为横屏布局")]
        public void CaptureLandscape() { foreach (var e in entries) if (e.target) e.landscape = RectState.Capture(e.target); }

        [ContextMenu("记录当前为竖屏布局")]
        public void CapturePortrait() { foreach (var e in entries) if (e.target) e.portrait = RectState.Capture(e.target); }
        //#endregion
    }
}
