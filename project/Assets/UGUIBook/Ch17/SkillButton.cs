// 技能按钮：冷却遮罩、充能层数、长按蓄力，三种机制可以单独用也可以组合。
// 注意：真实项目里冷却时间应该由技能系统（战斗逻辑）计时，按钮只负责“显示”；
// 这里为了让组件能单独拿去用，把计时也放在了按钮里，接入时把 Tick 换成读技能系统的数据即可。
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region skill
    public class SkillButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("显示")]
        [SerializeField] Image m_Icon;
        [SerializeField] Image m_CooldownMask;     // Filled / Radial 360 / 顶部起点 / 逆时针，黑色半透明
        [SerializeField] TMP_Text m_CooldownText;
        [SerializeField] GameObject m_ChargeBadge; // 右下角层数徽标
        [SerializeField] TMP_Text m_ChargeText;
        [SerializeField] Image m_RechargeRing;     // 层数没满时：下一层的充能进度
        [SerializeField] Image m_HoldRing;         // 长按蓄力进度
        [SerializeField] TMP_Text m_KeyText;

        [Header("规则")]
        [SerializeField] float m_Cooldown = 5f;    // 每一层的充能时间
        [SerializeField] int m_MaxCharges = 1;
        [SerializeField] bool m_HoldToCharge;
        [SerializeField] float m_MaxHold = 1.2f;
        [SerializeField] KeyCode m_Hotkey = KeyCode.None;

        public event Action<SkillButton, float> Cast;   // 参数：蓄力比例 0~1（非蓄力技能恒为 0）

        int m_Charges;
        float m_Recharge;      // 当前这一层已经充了多久
        bool m_Holding;
        float m_Hold;
        bool m_Usable = true;  // 例如蓝不够
        bool m_Inited;

        public int Charges => m_Charges;
        public bool CanCast => m_Usable && m_Charges > 0;

        void Awake() => Init();

        public void Init()
        {
            if (m_Inited) return;
            m_Inited = true;
            m_Charges = m_MaxCharges;
            if (m_KeyText != null) m_KeyText.text = m_Hotkey == KeyCode.None ? "" : KeyName(m_Hotkey);
            Refresh();
        }

        void Update()
        {
            if (m_Hotkey != KeyCode.None)
            {
                if (Input.GetKeyDown(m_Hotkey)) Press();
                if (Input.GetKeyUp(m_Hotkey)) Release(false);
            }
            Tick(Time.deltaTime);
        }

        public void Tick(float dt)
        {
            if (m_Charges < m_MaxCharges)
            {
                m_Recharge += dt;
                if (m_Recharge >= m_Cooldown)
                {
                    m_Charges++;
                    m_Recharge = m_Charges < m_MaxCharges ? m_Recharge - m_Cooldown : 0;
                }
            }
            if (m_Holding) m_Hold = Mathf.Min(m_MaxHold, m_Hold + dt);
            Refresh();
        }

        public void OnPointerDown(PointerEventData e) => Press();
        public void OnPointerUp(PointerEventData e) => Release(false);
        public void OnPointerExit(PointerEventData e) => Release(true);   // 手指滑出按钮 = 取消蓄力

        void Press()
        {
            if (!CanCast) return;
            if (m_HoldToCharge) { m_Holding = true; m_Hold = 0; }
            else Fire(0);
        }

        void Release(bool cancel)
        {
            if (!m_Holding) return;
            m_Holding = false;
            if (!cancel) Fire(m_Hold / m_MaxHold);
            m_Hold = 0;
            Refresh();
        }

        void Fire(float charge)
        {
            m_Charges--;
            Cast?.Invoke(this, charge);
            Refresh();
        }

        public void SetUsable(bool usable)
        {
            m_Usable = usable;
            Refresh();
        }

        /// 直接设置状态（读档、联网同步、截图预览）
        public void SetState(int charges, float rechargeElapsed, float hold01 = 0)
        {
            Init();
            m_Charges = Mathf.Clamp(charges, 0, m_MaxCharges);
            m_Recharge = rechargeElapsed;
            m_Holding = hold01 > 0;
            m_Hold = hold01 * m_MaxHold;
            Refresh();
        }

        void Refresh()
        {
            bool empty = m_Charges == 0;
            float progress = m_Recharge / m_Cooldown;   // 下一层充了多少
            // 没有可用层数时：整块遮罩 + 倒计时数字
            m_CooldownMask.gameObject.SetActive(empty);
            if (empty)
            {
                m_CooldownMask.fillAmount = 1 - progress;
                float left = m_Cooldown - m_Recharge;
                m_CooldownText.text = left >= 1 ? Mathf.CeilToInt(left).ToString() : left.ToString("0.0");
            }
            m_CooldownText.gameObject.SetActive(empty);
            // 多层充能：显示层数；层数没满且不是空的时候，用外圈显示下一层的进度
            if (m_ChargeBadge != null)
            {
                m_ChargeBadge.SetActive(m_MaxCharges > 1);
                m_ChargeText.text = m_Charges.ToString();
            }
            if (m_RechargeRing != null)
            {
                bool showRing = m_MaxCharges > 1 && !empty && m_Charges < m_MaxCharges;
                m_RechargeRing.gameObject.SetActive(showRing);
                m_RechargeRing.fillAmount = progress;
            }
            if (m_HoldRing != null)
            {
                m_HoldRing.gameObject.SetActive(m_Holding);
                m_HoldRing.fillAmount = m_Hold / m_MaxHold;
            }
            m_Icon.color = m_Usable ? Color.white : new Color(0.45f, 0.45f, 0.5f);
        }

        static string KeyName(KeyCode k) => k >= KeyCode.Alpha0 && k <= KeyCode.Alpha9 ? ((int)(k - KeyCode.Alpha0)).ToString() : k.ToString();
    }
    //#endregion
}
