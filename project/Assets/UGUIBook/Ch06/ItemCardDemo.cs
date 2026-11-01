// 双击演示：单击选中物品，双击使用物品
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch06
{
    public class ItemCardDemo : MonoBehaviour
    {
        [SerializeField] DoubleClickHandler[] m_Cards;
        [SerializeField] GameObject[] m_Frames;
        [SerializeField] TMP_Text m_Log;

        void Awake()
        {
            for (int i = 0; i < m_Cards.Length; i++)
            {
                int index = i;                            // 先拷贝一份，闭包才捕获到正确的下标
                m_Cards[i].onSingleClick.AddListener(() => Select(index));
                m_Cards[i].onDoubleClick.AddListener(() => Use(index));
            }
            Select(-1);
        }

        public void Select(int index)
        {
            for (int i = 0; i < m_Frames.Length; i++) m_Frames[i].SetActive(i == index);
            if (index >= 0) m_Log.text = $"单击：选中了 {m_Cards[index].name}";
        }

        void Use(int index)
        {
            Select(index);
            m_Log.text = $"<color=#FF8A3D>双击：使用了 {m_Cards[index].name}</color>";
        }
    }
}
