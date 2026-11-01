using UnityEngine;

namespace UGUIBook.Ch17
{
    /// 对话示例：选项会推进任务；对话结束后按 T 重新开始
    public class DialogueDemo : MonoBehaviour
    {
        [SerializeField] DialogueBox m_Box;
        [SerializeField] QuestTracker m_Quest;
        [SerializeField] DialogueLine[] m_Lines;

        void Start()
        {
            m_Box.OptionChosen += (line, option) =>
            {
                // 第 2 句的第 1 个选项 = 接下任务
                if (line == 1 && option == 0 && m_Quest != null) m_Quest.SetProgress(1, 0, 1);
            };
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.T) && !m_Box.gameObject.activeSelf) m_Box.Play(m_Lines);
        }
    }
}
