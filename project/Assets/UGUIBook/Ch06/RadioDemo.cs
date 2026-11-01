// 单选演示：选难度后在下方显示说明
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch06
{
    public class RadioDemo : MonoBehaviour
    {
        [SerializeField] RadioButtonGroup m_Group;
        [SerializeField] TMP_Text m_Desc;
        static readonly string[] Descs =
        {
            "简单：敌人血量 -30%，适合只想看剧情的玩家",
            "普通：推荐的游戏体验",
            "困难：敌人更聪明，掉落也更好",
        };

        void Start()
        {
            m_Group.onChanged.AddListener(i => m_Desc.text = Descs[i]);
            m_Desc.text = Descs[m_Group.Index];
        }
    }
}
