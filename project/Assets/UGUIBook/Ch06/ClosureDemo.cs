// 闭包陷阱演示：for 循环里直接用循环变量 i 注册监听，所有按钮都会打印同一个值
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public class ClosureDemo : MonoBehaviour
    {
        [SerializeField] Button[] m_Wrong;
        [SerializeField] Button[] m_Right;
        [SerializeField] TMP_Text m_Log;

        //#region closure
        void Awake()
        {
            // ✗ 错误：lambda 捕获的是变量 i 本身，循环结束时 i == 5，点哪个都显示 5
            for (int i = 0; i < m_Wrong.Length; i++)
                m_Wrong[i].onClick.AddListener(() => Log("错误写法", i));

            // ✓ 正确：每次循环声明一个新的局部变量，各自被捕获
            for (int i = 0; i < m_Right.Length; i++)
            {
                int index = i;
                m_Right[i].onClick.AddListener(() => Log("正确写法", index));
            }
        }
        //#endregion

        void Log(string tag, int value) => m_Log.text = $"{tag}：点击了第 {value} 个按钮";

        /// 给 Inspector 里的 OnClick 绑定用（UnityEvent 持久监听只支持 0 或 1 个参数）
        public void LogFromInspector(string message) => m_Log.text = "Inspector 绑定：" + message;
    }
}
