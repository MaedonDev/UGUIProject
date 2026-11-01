// 血条演示场景里的两个按钮：受伤 / 治疗
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    public class HealthBarDemo : MonoBehaviour
    {
        [SerializeField] DelayedHealthBar m_Bar;
        [SerializeField] Button m_Hit, m_BigHit, m_Heal;

        void Awake()
        {
            m_Hit.onClick.AddListener(() => m_Bar.Damage(80));
            m_BigHit.onClick.AddListener(() => m_Bar.Damage(320));
            m_Heal.onClick.AddListener(() => m_Bar.Heal(250));
        }
    }
}
