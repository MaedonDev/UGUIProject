// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class ToastDemo : MonoBehaviour
    {
        [SerializeField] ToastManager m_Toast;
        [SerializeField] Button m_Button;
        static readonly string[] s_Msgs = { "金币 +100", "背包已满", "任务完成：初入江湖", "网络不稳定，正在重连", "获得称号：收藏家" };
        int m_I;
        void Start() => m_Button.onClick.AddListener(() => m_Toast.Show(s_Msgs[m_I++ % s_Msgs.Length]));
    }
}
