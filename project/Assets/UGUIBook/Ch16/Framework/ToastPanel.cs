using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region toast
    /// 飘字提示（Toast 层，常驻、不进栈）。同时最多显示 m_MaxCount 条，新的在下面，旧的往上顶
    public partial class ToastPanel : UIPanel
    {
        [SerializeField] float m_Duration = 2f;
        [SerializeField] float m_FadeTime = 0.3f;
        [SerializeField] int m_MaxCount = 3;

        class Entry
        {
            public CanvasGroup Group;
            public TMP_Text Text;
            public float Time;
        }

        readonly List<Entry> m_Active = new List<Entry>();
        readonly Stack<Entry> m_Pool = new Stack<Entry>();

        protected override void OnCreate() => go_Item.SetActive(false);   // go_Item 是条目模板

        public void Show(string message)
        {
            if (m_Active.Count >= m_MaxCount) Recycle(m_Active[0]);
            var e = m_Pool.Count > 0 ? m_Pool.Pop() : CreateEntry();
            e.Group.gameObject.SetActive(true);
            e.Group.transform.SetAsLastSibling();
            e.Group.alpha = 1;
            e.Text.text = message;
            e.Time = 0;
            m_Active.Add(e);
        }

        Entry CreateEntry()
        {
            var go = Instantiate(go_Item, go_Item.transform.parent, false);
            return new Entry { Group = go.GetComponent<CanvasGroup>(), Text = go.GetComponentInChildren<TMP_Text>() };
        }

        void Recycle(Entry e)
        {
            m_Active.Remove(e);
            e.Group.gameObject.SetActive(false);
            m_Pool.Push(e);
        }

        void Update()
        {
            // 用不受 timeScale 影响的时间：游戏暂停（timeScale=0）时提示也要能消失
            float dt = Time.unscaledDeltaTime;
            for (int i = m_Active.Count - 1; i >= 0; i--)
            {
                var e = m_Active[i];
                e.Time += dt;
                float left = m_Duration - e.Time;
                e.Group.alpha = Mathf.Clamp01(left / m_FadeTime);
                if (left <= 0) Recycle(e);
            }
        }

        public override bool OnBack() => false;   // 不进栈，本来也收不到；写明不处理返回键
    }
    //#endregion
}
