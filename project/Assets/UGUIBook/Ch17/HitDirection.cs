// 受击方向指示：在屏幕中心周围画一段红色弧，指向伤害来源。玩家转身时弧会跟着转（记的是来源的世界位置）
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region hitdir
    public class HitDirection : MonoBehaviour
    {
        [SerializeField] RectTransform m_Template;   // 轴心在屏幕中心的节点，子节点 Arc 是一段 Filled 圆环
        [SerializeField] Transform m_Player;
        [SerializeField] Camera m_Camera;
        [SerializeField] float m_Life = 1.4f;
        [SerializeField] float m_ArcDegrees = 50f;

        class Entry
        {
            public RectTransform Root;
            public Image Arc;
            public Vector3 Source;
            public float Age;
        }

        readonly List<Entry> m_Active = new List<Entry>();
        readonly Stack<Entry> m_Pool = new Stack<Entry>();

        public void Show(Vector3 sourceWorldPos)
        {
            // 同一方向（±20°）已经有指示就刷新它，避免叠一堆
            float angle = AngleTo(sourceWorldPos);
            foreach (var a in m_Active)
                if (Mathf.Abs(Mathf.DeltaAngle(AngleTo(a.Source), angle)) < 20) { a.Source = sourceWorldPos; a.Age = 0; return; }

            var e = m_Pool.Count > 0 ? m_Pool.Pop() : Create();
            e.Source = sourceWorldPos;
            e.Age = 0;
            e.Root.gameObject.SetActive(true);
            m_Active.Add(e);
            Apply(e);
        }

        Entry Create()
        {
            var rt = Instantiate(m_Template, m_Template.parent, false);
            var arc = rt.GetComponentInChildren<Image>();
            arc.fillAmount = m_ArcDegrees / 360f;
            // Filled 从顶部开始顺时针填充，把弧往回转半个弧长，让弧的中点对准正上方
            arc.rectTransform.localRotation = Quaternion.Euler(0, 0, m_ArcDegrees * 0.5f);
            return new Entry { Root = rt, Arc = arc };
        }

        /// 来源相对“镜头前方”的水平角度：0 = 正前方，90 = 右边，180 = 身后
        float AngleTo(Vector3 source)
        {
            Vector3 fwd = m_Camera.transform.forward; fwd.y = 0;
            Vector3 dir = source - m_Player.position; dir.y = 0;
            return Vector3.SignedAngle(fwd, dir, Vector3.up);
        }

        void Update() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            for (int i = m_Active.Count - 1; i >= 0; i--)
            {
                var e = m_Active[i];
                e.Age += dt;
                if (e.Age >= m_Life)
                {
                    e.Root.gameObject.SetActive(false);
                    m_Active.RemoveAt(i);
                    m_Pool.Push(e);
                    continue;
                }
                Apply(e);
            }
        }

        void Apply(Entry e)
        {
            // 世界里顺时针为正（俯视），UI 的 z 旋转逆时针为正，所以取负
            e.Root.localRotation = Quaternion.Euler(0, 0, -AngleTo(e.Source));
            var c = e.Arc.color;
            c.a = Mathf.Clamp01((m_Life - e.Age) / (m_Life * 0.5f));   // 后半段淡出
            e.Arc.color = c;
        }

        public void SetAge(int index, float age)   // 截图用
        {
            if (index < m_Active.Count) { m_Active[index].Age = age; Apply(m_Active[index]); }
        }
    }
    //#endregion
}
