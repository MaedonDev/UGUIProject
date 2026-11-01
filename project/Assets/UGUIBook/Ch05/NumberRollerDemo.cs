// 数字跳动演示：点按钮领取随机金币
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch05
{
    public class NumberRollerDemo : MonoBehaviour
    {
        [SerializeField] NumberRoller m_Gold;
        [SerializeField] NumberRoller m_Power;
        [SerializeField] Button m_Claim;
        [SerializeField] Button m_Upgrade;

        void Start()
        {
            m_Gold.SetInstant(12800);
            m_Power.SetInstant(35210);
            m_Claim.onClick.AddListener(() => m_Gold.RollTo(m_Gold.Value + Random.Range(100, 5000)));
            m_Upgrade.onClick.AddListener(() => m_Power.RollTo(m_Power.Value + Random.Range(800, 3000)));
        }
    }
}
