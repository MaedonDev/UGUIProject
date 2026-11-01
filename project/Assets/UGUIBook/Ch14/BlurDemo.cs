// 配套场景 Ch14_Blur：点“打开弹窗”时先抓模糊快照再显示弹窗，点“关闭”收起。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    public class BlurDemo : MonoBehaviour
    {
        [SerializeField] BlurSnapshot m_Blur;
        [SerializeField] GameObject m_Popup;
        [SerializeField] Button m_Open, m_Close;

        void Start()
        {
            m_Open.onClick.AddListener(Open);
            m_Close.onClick.AddListener(() => m_Popup.SetActive(false));
            m_Popup.SetActive(false);
        }

        void Open()
        {
            m_Popup.SetActive(true);
            m_Blur.Capture();          // Capture 内部会临时隐藏弹窗自己，只拍背景
        }
    }
}
