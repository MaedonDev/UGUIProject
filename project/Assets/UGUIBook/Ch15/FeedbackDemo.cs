// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class FeedbackDemo : MonoBehaviour
    {
        [SerializeField] NumberRoller m_Gold;
        [SerializeField] Button m_Earn, m_Spend;
        [SerializeField] RectTransform m_SpendRect;
        void Start()
        {
            m_Gold.SetImmediate(1280);
            m_Earn.onClick.AddListener(() => m_Gold.Add(Random.Range(200, 3000)));
            m_Spend.onClick.AddListener(() =>
            {
                if (m_Gold.value < 5000) m_SpendRect.Shake(18, 0.4f).Play();   // 余额不足：抖一下
                else m_Gold.Add(-5000, false);
            });
        }
    }
}
