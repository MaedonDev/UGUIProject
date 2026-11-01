// 演示用：几个按钮被点中时，在一行文字里显示是谁
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    public class HitLog : MonoBehaviour
    {
        [SerializeField] Button[] m_Buttons;
        [SerializeField] TMP_Text m_Output;
        int m_Count;

        void Awake()
        {
            foreach (var b in m_Buttons)
            {
                var name = b.name;   // foreach 的循环变量每次都是新的，闭包捕获安全（第 6 章细讲）
                b.onClick.AddListener(() => m_Output.text = $"第 {++m_Count} 次：点中了 <b>{name}</b>");
            }
        }
    }
}
