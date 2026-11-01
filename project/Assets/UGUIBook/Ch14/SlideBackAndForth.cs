// 演示用：让一排元素左右来回平移，观察它们在 RectMask2D 边缘被软裁剪。
using UnityEngine;

namespace UGUIBook.Ch14
{
    public class SlideBackAndForth : MonoBehaviour
    {
        [SerializeField] float m_Distance = 400, m_Period = 6;
        Vector2 m_Origin;
        void Start() => m_Origin = ((RectTransform)transform).anchoredPosition;
        void Update()
        {
            float x = Mathf.Sin(Time.time * Mathf.PI * 2 / m_Period) * m_Distance;
            ((RectTransform)transform).anchoredPosition = m_Origin + new Vector2(x, 0);
        }
    }
}
