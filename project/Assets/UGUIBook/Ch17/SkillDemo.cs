using UnityEngine;

namespace UGUIBook.Ch17
{
    /// 技能按钮示例场景：放技能时往 Buff 栏里加点东西，方便观察两边的效果
    public class SkillDemo : MonoBehaviour
    {
        [SerializeField] SkillButton[] m_Skills;
        [SerializeField] BuffBar m_Buffs;

        static readonly string[] s_Icons = { "item_sword", "star", "item_sword", "item_bow", "item_potion_blue" };

        void Start()
        {
            foreach (var s in m_Skills) s.Cast += OnCast;
        }

        void OnCast(SkillButton skill, float charge)
        {
            int i = System.Array.IndexOf(m_Skills, skill);
            Debug.Log($"[SkillDemo] 释放 {skill.name}，蓄力 {charge:P0}");
            m_Buffs.Add("skill" + i, s_Icons[i], 6f + 4f * charge, false, 5);
        }
    }
}
