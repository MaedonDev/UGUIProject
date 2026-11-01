// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class TransitionDemo : MonoBehaviour
    {
        [SerializeField] ScreenTransition m_Transition;
        [SerializeField] Button[] m_Buttons;      // 依次对应 Wipe / Iris / Dissolve
        [SerializeField] GameObject[] m_Pages;    // 盖住后切换页面
        int m_Page;
        void Start()
        {
            m_Transition.progress = 0;
            for (int i = 0; i < m_Buttons.Length; i++)
            {
                var shape = (ScreenTransition.Shape)i;
                var btn = (RectTransform)m_Buttons[i].transform;
                m_Buttons[i].onClick.AddListener(() =>
                {
                    m_Transition.shape = shape;
                    // 圆形收缩的圆心放在被点的按钮上
                    var canvas = (RectTransform)m_Transition.transform;
                    Vector2 local = canvas.InverseTransformPoint(btn.position);
                    var r = canvas.rect;
                    m_Transition.center = new Vector2((local.x - r.xMin) / r.width, (local.y - r.yMin) / r.height);
                    m_Transition.Cover(() =>
                    {
                        m_Pages[m_Page].SetActive(false);
                        m_Page = (m_Page + 1) % m_Pages.Length;
                        m_Pages[m_Page].SetActive(true);
                        m_Transition.Reveal();
                    });
                });
            }
        }
    }
}
