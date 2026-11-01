// 呼吸：按钮在 1 ~ 1.06 倍之间缓慢往返，提示“点我”。无限循环 + 往返（yoyo），InOutSine 让两端停顿自然。
using UnityEngine;

namespace UGUIBook.Ch15
{
    public class Breathing : MonoBehaviour
    {
        //#region breathing
        [SerializeField] float m_Scale = 1.06f;
        [SerializeField] float m_Period = 0.9f;     // 放大一次用时
        TweenBase m_Tween;

        public TweenBase Build() =>
            transform.Scale(m_Scale, m_Period).From(Vector3.one, false).SetEase(Ease.InOutSine).SetLoops(-1, true);

        void OnEnable() { if (Application.isPlaying) m_Tween = Build().Play(); }

        void OnDisable()
        {
            m_Tween?.Kill();
            transform.localScale = Vector3.one;      // 停下时恢复原样，不要停在 1.03 倍
        }
        //#endregion
    }
}
