// 平滑滚动到第 N 项：算出目标 normalizedPosition，再用缓动在若干帧内滚过去。
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch09
{
    [RequireComponent(typeof(ScrollRect))]
    public class SmoothScroller : MonoBehaviour
    {
        [SerializeField] float m_Duration = 0.35f;
        ScrollRect m_Scroll;
        Coroutine m_Running;

        ScrollRect Scroll => m_Scroll != null ? m_Scroll : (m_Scroll = GetComponent<ScrollRect>());

        //#region scroll
        public void ScrollTo(RectTransform item, float align, bool instant = false)
        {
            float target = ScrollRectUtil.VerticalPositionFor(Scroll, item, align);
            if (m_Running != null) StopCoroutine(m_Running);
            Scroll.StopMovement();               // 清掉惯性速度，否则松手后的惯性会和我们的动画“抢”位置
            if (instant || !Application.isPlaying) { Scroll.verticalNormalizedPosition = target; return; }
            m_Running = StartCoroutine(Animate(target));
        }

        IEnumerator Animate(float target)
        {
            float from = Scroll.verticalNormalizedPosition;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / m_Duration)
            {
                float e = 1 - (1 - t) * (1 - t) * (1 - t);          // EaseOutCubic：先快后慢
                Scroll.verticalNormalizedPosition = Mathf.LerpUnclamped(from, target, e);
                yield return null;
            }
            Scroll.verticalNormalizedPosition = target;
            m_Running = null;
        }

        // 用户一拖动就放弃自动滚动，把控制权还给手指
        void Update()
        {
            if (m_Running != null && Input.GetMouseButtonDown(0)) { StopCoroutine(m_Running); m_Running = null; }
        }
        //#endregion
    }
}
