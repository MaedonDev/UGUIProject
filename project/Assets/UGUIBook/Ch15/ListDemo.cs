// 第 15 章配套场景的按钮接线（在 Start 里 AddListener，场景存盘后照样能用）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class ListDemo : MonoBehaviour
    {
        [SerializeField] ListEntrance m_List;
        [SerializeField] Button m_Replay;
        void Start() => m_Replay.onClick.AddListener(m_List.Play);
    }
}
