// Buff 栏：图标 + 剩余时间扇形 + 层数 + 快结束时闪烁。同一个 Buff 再次获得时刷新时间、叠层
using System.Collections.Generic;
using TMPro;
using UGUIBook.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region buff
    public class BuffBar : MonoBehaviour
    {
        [SerializeField] BuffIcon m_Template;
        [SerializeField] int m_MaxShown = 8;

        class Buff
        {
            public string Id;
            public float Duration, Left;
            public int Stacks, MaxStacks;
            public BuffIcon View;
        }

        readonly List<Buff> m_Buffs = new List<Buff>();
        readonly Stack<BuffIcon> m_Pool = new Stack<BuffIcon>();

        public void Add(string id, string icon, float duration, bool debuff, int maxStacks = 1)
        {
            var b = m_Buffs.Find(x => x.Id == id);
            if (b != null)
            {
                b.Left = b.Duration = duration;
                b.Stacks = Mathf.Min(b.MaxStacks, b.Stacks + 1);
                b.View.SetStacks(b.Stacks);
                return;
            }
            if (m_Buffs.Count >= m_MaxShown) Remove(0);
            var view = m_Pool.Count > 0 ? m_Pool.Pop() : Instantiate(m_Template, m_Template.transform.parent, false);
            view.gameObject.SetActive(true);
            view.transform.SetAsLastSibling();
            view.Setup(UISkin.Sprite(icon), debuff);
            b = new Buff { Id = id, Duration = duration, Left = duration, Stacks = 1, MaxStacks = maxStacks, View = view };
            view.SetStacks(1);
            m_Buffs.Add(b);
            view.SetTime(b.Left, b.Duration);
        }

        void Remove(int i)
        {
            var v = m_Buffs[i].View;
            v.gameObject.SetActive(false);
            m_Pool.Push(v);
            m_Buffs.RemoveAt(i);
        }

        void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            for (int i = m_Buffs.Count - 1; i >= 0; i--)
            {
                var b = m_Buffs[i];
                b.Left -= dt;
                if (b.Left <= 0) { Remove(i); continue; }
                b.View.SetTime(b.Left, b.Duration);
            }
        }
    }
    //#endregion
}
