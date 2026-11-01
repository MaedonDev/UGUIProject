// 缓动函数：输入 0~1 的线性时间，输出“加了节奏”的进度。公式与 easings.net 上的标准写法一致。
// 注意 Back / Elastic 的输出会超出 0~1（回弹、过冲），所以插值必须用 LerpUnclamped。
using UnityEngine;

namespace UGUIBook.Ch15
{
    public enum Ease
    {
        Linear,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InSine, OutSine, InOutSine,
        InExpo, OutExpo,
        InBack, OutBack, InOutBack,
        OutElastic, OutBounce,
    }

    public static class EaseFunc
    {
        //#region ease
        const float C1 = 1.70158f, C2 = C1 * 1.525f, C3 = C1 + 1, C4 = 2 * Mathf.PI / 3;

        public static float Evaluate(Ease ease, float t)
        {
            switch (ease)
            {
                case Ease.InQuad: return t * t;
                case Ease.OutQuad: return 1 - (1 - t) * (1 - t);
                case Ease.InOutQuad: return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
                case Ease.InCubic: return t * t * t;
                case Ease.OutCubic: return 1 - Mathf.Pow(1 - t, 3);
                case Ease.InOutCubic: return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
                case Ease.InSine: return 1 - Mathf.Cos(t * Mathf.PI / 2);
                case Ease.OutSine: return Mathf.Sin(t * Mathf.PI / 2);
                case Ease.InOutSine: return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
                case Ease.InExpo: return t <= 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
                case Ease.OutExpo: return t >= 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
                case Ease.InBack: return C3 * t * t * t - C1 * t * t;
                case Ease.OutBack: return 1 + C3 * Mathf.Pow(t - 1, 3) + C1 * Mathf.Pow(t - 1, 2);
                case Ease.InOutBack:
                    return t < 0.5f
                        ? Mathf.Pow(2 * t, 2) * ((C2 + 1) * 2 * t - C2) / 2
                        : (Mathf.Pow(2 * t - 2, 2) * ((C2 + 1) * (t * 2 - 2) + C2) + 2) / 2;
                case Ease.OutElastic:
                    return t <= 0 ? 0 : t >= 1 ? 1 : Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * C4) + 1;
                case Ease.OutBounce: return Bounce(t);
                default: return t;
            }
        }

        static float Bounce(float t)
        {
            const float n1 = 7.5625f, d1 = 2.75f;
            if (t < 1 / d1) return n1 * t * t;
            if (t < 2 / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
            if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
            return n1 * (t -= 2.625f / d1) * t + 0.984375f;
        }
        //#endregion
    }
}
