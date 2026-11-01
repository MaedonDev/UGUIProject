// 技能冷却：按钮上盖一层 Filled / Radial 360 的半透明遮罩，fillAmount 从 1 走到 0。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    [RequireComponent(typeof(Button))]
    public class SkillCooldown : MonoBehaviour
    {
        //#region fields
        [SerializeField] Image m_Mask;            // Image Type = Filled，Fill Method = Radial 360，Origin = Top
        [SerializeField] TMP_Text m_Label;        // 剩余秒数
        [SerializeField] Image m_ReadyFlash;      // 冷却结束时闪一下（可选）
        [SerializeField] float m_Duration = 5f;
        //#endregion

        float m_Remain;
        float m_Flash;

        public bool IsCooling => m_Remain > 0f;
        public float Duration { get => m_Duration; set => m_Duration = Mathf.Max(0.01f, value); }

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Cast);
            SetRemain(0f);
            if (m_ReadyFlash != null) m_ReadyFlash.enabled = false;
        }

        //#region logic
        /// 点击释放技能：冷却中直接忽略
        public void Cast()
        {
            if (IsCooling) return;
            SetRemain(m_Duration);
        }

        void Update()
        {
            if (IsCooling)
            {
                SetRemain(Mathf.Max(0f, m_Remain - Time.deltaTime));
                if (!IsCooling) m_Flash = 0.25f;          // 刚好走完：触发一次“就绪”闪光
            }
            if (m_ReadyFlash != null)
            {
                m_Flash -= Time.deltaTime;
                m_ReadyFlash.enabled = m_Flash > 0f;
                if (m_Flash > 0f) m_ReadyFlash.color = new Color(1, 1, 1, m_Flash / 0.25f * 0.8f);
            }
        }

        /// 直接设置剩余时间（拍图、读档恢复冷却时也用它）
        public void SetRemain(float remain)
        {
            m_Remain = remain;
            float t = m_Duration > 0f ? remain / m_Duration : 0f;
            m_Mask.fillAmount = t;
            m_Mask.enabled = t > 0f;                          // 不冷却时关掉，省一次绘制
            if (m_Label == null) return;
            if (remain <= 0f) m_Label.text = string.Empty;
            else if (remain >= 1f) m_Label.SetText("{0}", Mathf.CeilToInt(remain));
            else m_Label.SetText("{0:1}", remain);            // 最后一秒显示一位小数
        }
        //#endregion
    }
}
