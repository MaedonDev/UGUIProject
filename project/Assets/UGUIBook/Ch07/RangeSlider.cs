// 双滑块区间选择（价格区间、等级区间）。UGUI 没有内置，这里照着 Slider 的思路从零写一个：
// 值 → 归一化 → 改锚点（和 Slider.UpdateVisuals 一样），输入用 IPointerDown/IDrag 自己算。
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch07
{
    [ExecuteAlways]
    public class RangeSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IInitializePotentialDragHandler
    {
        [Serializable] public class RangeEvent : UnityEvent<float, float> { }

        [SerializeField] RectTransform m_SlideArea;     // 两个滑块的父节点，决定可滑动的范围
        [SerializeField] RectTransform m_Fill;          // 两个滑块之间的高亮条（也放在 SlideArea 下）
        [SerializeField] RectTransform m_LowHandle;
        [SerializeField] RectTransform m_HighHandle;
        [SerializeField] float m_MinLimit = 0, m_MaxLimit = 1000;
        [SerializeField] float m_Low = 200, m_High = 800;
        [SerializeField] float m_Step = 10;             // 吸附步长；0 = 连续
        [SerializeField] float m_MinGap = 50;           // 两个滑块至少隔多少

        public RangeEvent onValueChanged = new RangeEvent();
        public float Low => m_Low;
        public float High => m_High;

        RectTransform m_Active;    // 本次按下拖的是哪个滑块

        void OnEnable() => UpdateVisuals();
#if UNITY_EDITOR
        void OnValidate() { m_High = Mathf.Max(m_High, m_Low); UpdateVisuals(); }
#endif

        //#region set
        public void SetValues(float low, float high, bool notify = true)
        {
            low = Snap(Mathf.Clamp(low, m_MinLimit, m_MaxLimit));
            high = Snap(Mathf.Clamp(high, m_MinLimit, m_MaxLimit));
            if (high - low < m_MinGap) high = Mathf.Min(m_MaxLimit, low + m_MinGap);
            if (high - low < m_MinGap) low = Mathf.Max(m_MinLimit, high - m_MinGap);
            if (Mathf.Approximately(low, m_Low) && Mathf.Approximately(high, m_High)) return;
            m_Low = low; m_High = high;
            UpdateVisuals();
            if (notify) onValueChanged.Invoke(m_Low, m_High);
        }

        float Snap(float v) => m_Step > 0 ? m_MinLimit + Mathf.Round((v - m_MinLimit) / m_Step) * m_Step : v;

        float Normalize(float v) => Mathf.Approximately(m_MinLimit, m_MaxLimit) ? 0 : Mathf.InverseLerp(m_MinLimit, m_MaxLimit, v);
        //#endregion

        //#region visuals
        void UpdateVisuals()
        {
            if (m_SlideArea == null) return;
            float a = Normalize(m_Low), b = Normalize(m_High);
            // 和 Slider 一样：滑块的 x 锚点 = 归一化值，y 方向撑满；sizeDelta.x 就是滑块宽度
            if (m_LowHandle) { m_LowHandle.anchorMin = new Vector2(a, 0); m_LowHandle.anchorMax = new Vector2(a, 1); }
            if (m_HighHandle) { m_HighHandle.anchorMin = new Vector2(b, 0); m_HighHandle.anchorMax = new Vector2(b, 1); }
            // 高亮条左右锚点分别对齐两个滑块
            if (m_Fill) { m_Fill.anchorMin = new Vector2(a, m_Fill.anchorMin.y); m_Fill.anchorMax = new Vector2(b, m_Fill.anchorMax.y); }
        }
        //#endregion

        //#region input
        public void OnInitializePotentialDrag(PointerEventData e) => e.useDragThreshold = false;   // 按下即拖，不等阈值

        public void OnPointerDown(PointerEventData e)
        {
            if (e.button != PointerEventData.InputButton.Left || !TryNormalized(e, out float n)) return;
            float a = Normalize(m_Low), b = Normalize(m_High);
            // 选离按下点更近的滑块；两个滑块重叠时，按在左边就拖左边的、右边就拖右边的
            if (Mathf.Approximately(a, b)) m_Active = n < a ? m_LowHandle : m_HighHandle;
            else m_Active = Mathf.Abs(n - a) <= Mathf.Abs(n - b) ? m_LowHandle : m_HighHandle;
            MoveActive(n);
        }

        public void OnDrag(PointerEventData e)
        {
            if (m_Active != null && TryNormalized(e, out float n)) MoveActive(n);
        }

        void MoveActive(float n)
        {
            float v = Mathf.Lerp(m_MinLimit, m_MaxLimit, n);
            // 拖一个滑块时，另一个保持不动，当前滑块被限制在“对方 ± 最小间隔”之内
            if (m_Active == m_LowHandle) SetValues(Mathf.Min(v, m_High - m_MinGap), m_High);
            else SetValues(m_Low, Mathf.Max(v, m_Low + m_MinGap));
        }

        bool TryNormalized(PointerEventData e, out float n)
        {
            n = 0;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(m_SlideArea, e.position, e.pressEventCamera, out var local))
                return false;
            var r = m_SlideArea.rect;
            n = Mathf.Clamp01((local.x - r.xMin) / r.width);
            return true;
        }
        //#endregion

        public void Setup(RectTransform area, RectTransform fill, RectTransform low, RectTransform high,
            float min, float max, float step, float gap)
        {
            m_SlideArea = area; m_Fill = fill; m_LowHandle = low; m_HighHandle = high;
            m_MinLimit = min; m_MaxLimit = max; m_Step = step; m_MinGap = gap;
            UpdateVisuals();
        }
    }
}
