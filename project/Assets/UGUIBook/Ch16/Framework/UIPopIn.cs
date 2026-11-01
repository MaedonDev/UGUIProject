using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region popin
    /// 弹窗出现时的回弹缩放 + 淡入。挂在弹窗的卡片节点上（不要挂在全屏遮罩上）。
    /// 每次 SetActive(true) 自动播放一次；用 unscaledDeltaTime，暂停游戏时也能弹出
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPopIn : MonoBehaviour
    {
        [SerializeField] float m_Duration = 0.22f;
        [SerializeField] float m_FromScale = 0.85f;

        CanvasGroup m_Group;
        float m_T = 1;

        void OnEnable()
        {
            if (!Application.isPlaying) return;   // 编辑器里保持最终状态
            m_Group = GetComponent<CanvasGroup>();
            m_T = 0;
            Apply(0);
        }

        void Update()
        {
            if (m_T >= 1) return;
            m_T = Mathf.Min(1, m_T + Time.unscaledDeltaTime / m_Duration);
            Apply(m_T);
        }

        void Apply(float t)
        {
            // EaseOutBack：先冲过 1 再落回 1，有“弹”一下的感觉
            const float c1 = 1.70158f, c3 = c1 + 1;
            float e = 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(m_FromScale, 1, e);
            m_Group.alpha = Mathf.Clamp01(t * 2);
        }
    }
    //#endregion
}
