// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class CoinDemo : MonoBehaviour
    {
        [SerializeField] CoinFly m_Fly;
        [SerializeField] RectTransform m_Source;
        [SerializeField] Button m_Claim;
        void Start() => m_Claim.onClick.AddListener(() => m_Fly.Play(m_Source, 1200));
    }
}
