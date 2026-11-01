// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class PopupDemo : MonoBehaviour
    {
        [SerializeField] PopupAnim m_Popup;
        [SerializeField] Button m_Open, m_Close;
        void Start()
        {
            m_Open.onClick.AddListener(m_Popup.Show);
            m_Close.onClick.AddListener(m_Popup.Hide);
            m_Popup.gameObject.SetActive(false);
        }
    }
}
