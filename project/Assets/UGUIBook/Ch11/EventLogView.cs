// 把 EventLog 里的内容显示到一个 TMP 文本上（最新的在最下面），并用帧号区分同一帧内的事件。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch11
{
    public class EventLogView : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Text;
        [SerializeField] int m_MaxLines = 18;
        [SerializeField] Button m_Clear;

        void OnEnable() { EventLog.Changed += Refresh; Refresh(); }
        void OnDisable() => EventLog.Changed -= Refresh;
        void Start() { if (m_Clear != null) m_Clear.onClick.AddListener(EventLog.Clear); }
        void Update() => EventLog.Frame = Time.frameCount;

        public void Refresh()
        {
            var lines = EventLog.Lines;
            int start = Mathf.Max(0, lines.Count - m_MaxLines);
            m_Text.text = string.Join("\n", lines.GetRange(start, lines.Count - start));
        }
    }
}
