// 常用 UI 属性的补间“快捷方式”。每个方法只是 new Tween<T>(目标, 取值, 写值, 终点, 时长, 插值) 的包装。
// 都用 LerpUnclamped：OutBack、OutElastic 会超出 0~1，夹紧就没有回弹了。
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public static class Tw
    {
        //#region factory
        public static Tween<float> Float(UnityEngine.Object target, Func<float> get, Action<float> set, float to, float dur) =>
            new Tween<float>(target, get, set, to, dur, Mathf.LerpUnclamped);

        /// 从 from 到 to 的一个纯数值（常用来驱动自定义逻辑：贝塞尔路径、数字滚动……）
        public static Tween<float> Value(UnityEngine.Object target, float from, float to, float dur, Action<float> set) =>
            new Tween<float>(target, () => from, set, to, dur, Mathf.LerpUnclamped);

        public static Tween<Vector2> AnchorPos(this RectTransform rt, Vector2 to, float dur) =>
            new Tween<Vector2>(rt, () => rt.anchoredPosition, v => rt.anchoredPosition = v, to, dur, Vector2.LerpUnclamped);

        public static Tween<Vector3> Scale(this Transform tr, Vector3 to, float dur) =>
            new Tween<Vector3>(tr, () => tr.localScale, v => tr.localScale = v, to, dur, Vector3.LerpUnclamped);

        public static Tween<Vector3> Scale(this Transform tr, float to, float dur) => tr.Scale(Vector3.one * to, dur);

        public static Tween<float> RotateZ(this Transform tr, float to, float dur) =>
            new Tween<float>(tr, () => tr.localEulerAngles.z, z => tr.localEulerAngles = new Vector3(0, 0, z), to, dur, Mathf.LerpUnclamped);

        /// CanvasGroup 淡入淡出：只改 CanvasRenderer 的继承透明度，子物体不需要重建网格（比逐个改 Graphic.color 便宜）
        public static Tween<float> Alpha(this CanvasGroup cg, float to, float dur) =>
            new Tween<float>(cg, () => cg.alpha, a => cg.alpha = a, to, dur, Mathf.LerpUnclamped);

        /// 改 Graphic.color 会让这个元素的顶点重建（顶点色在网格里）
        public static Tween<Color> Color(this Graphic g, Color to, float dur) =>
            new Tween<Color>(g, () => g.color, c => g.color = c, to, dur, UnityEngine.Color.LerpUnclamped);

        public static Tween<float> Fill(this Image img, float to, float dur) =>
            new Tween<float>(img, () => img.fillAmount, f => img.fillAmount = f, to, dur, Mathf.LerpUnclamped);
        //#endregion

        //#region punch
        /// “弹一下”：缩放先放大再回到原值，用 sin 包络，不需要专门的缓动类型
        public static Tween<float> PunchScale(this Transform tr, float strength, float dur)
        {
            Vector3 baseScale = tr.localScale;
            return Value(tr, 0, 1, dur, k =>
            {
                float s = Mathf.Sin(k * Mathf.PI) * (1 - k * 0.5f);      // 0 → 峰 → 0
                tr.localScale = baseScale * (1 + strength * s);
            }).SetEase(Ease.Linear);
        }

        /// 左右抖动（输入错误、余额不足）：振幅随时间衰减
        public static Tween<float> Shake(this RectTransform rt, float strength, float dur, int vibrato = 6)
        {
            Vector2 origin = rt.anchoredPosition;
            return Value(rt, 0, 1, dur, k =>
            {
                rt.anchoredPosition = origin + new Vector2(Mathf.Sin(k * Mathf.PI * 2 * vibrato) * strength * (1 - k), 0);
            }).SetEase(Ease.Linear);
        }
        //#endregion
    }
}
