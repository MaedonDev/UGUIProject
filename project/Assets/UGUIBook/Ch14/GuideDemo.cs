// 配套场景 Ch14_Guide：依次引导玩家点三个按钮。只有洞里的按钮点得到，点洞外没有反应。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    public class GuideDemo : MonoBehaviour
    {
        [SerializeField] GuideMask m_Mask;
        [SerializeField] Button[] m_Steps;
        [SerializeField] string[] m_Tips;
        [SerializeField] TMP_Text m_Tip;
        [SerializeField] RectTransform m_Hand;     // 指引小手
        int m_Index;

        void Start()
        {
            for (int i = 0; i < m_Steps.Length; i++)
            {
                int k = i;                                  // 闭包陷阱：循环变量要先拷贝
                m_Steps[i].onClick.AddListener(() => OnClicked(k));
            }
            Show(0);
        }

        void OnClicked(int i)
        {
            if (i != m_Index) return;                       // 理论上点不到，防御一下
            Show(i + 1);
        }

        public void Show(int i)
        {
            m_Index = i;
            bool done = i >= m_Steps.Length;
            m_Mask.gameObject.SetActive(!done);
            m_Hand.gameObject.SetActive(!done);
            if (done) { m_Tip.text = "引导完成，所有按钮恢复可点"; return; }
            var target = (RectTransform)m_Steps[i].transform;
            m_Mask.SetTarget(target, Application.isPlaying);
            m_Tip.text = m_Tips[i];
            // 小手放在目标右下角
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            m_Hand.position = corners[3];
        }
    }
}
