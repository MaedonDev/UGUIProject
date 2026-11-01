// 对话框演示：点一下——正在打字就跳过，打完了就下一句
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch05
{
    public class DialogBox : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Typewriter m_Writer;
        [SerializeField] GameObject m_NextArrow;      // 打完一句后闪烁的“继续”箭头
        [SerializeField, TextArea] string[] m_Lines;
        int m_Index;

        void Start()
        {
            m_Writer.onFinished.AddListener(() => m_NextArrow.SetActive(true));
            PlayLine(0);
        }

        void PlayLine(int i)
        {
            m_Index = i % m_Lines.Length;
            m_NextArrow.SetActive(false);
            m_Writer.Play(m_Lines[m_Index]);
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (m_Writer.IsPlaying) m_Writer.Skip();
            else PlayLine(m_Index + 1);
        }

        void Update()
        {
            if (!m_NextArrow.activeSelf) return;
            // 箭头上下轻轻浮动
            var p = m_NextArrow.transform.localPosition;
            p.y = -6f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f));
            m_NextArrow.transform.localPosition = p;
        }
    }
}
