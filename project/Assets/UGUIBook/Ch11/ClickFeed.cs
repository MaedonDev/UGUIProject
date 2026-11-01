// 案例 11.5 的小工具：把字符串显示到一个文本上（给 UnityEvent<string> 在 Inspector 里绑定用）。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch11
{
    public class ClickFeed : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Text;
        public void Show(string message) => m_Text.text = message;
    }
}
