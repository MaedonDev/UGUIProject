// 任务追踪：标题 + 目标进度 + 方向距离；进度变化时闪一下
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch17
{
    //#region questdata
    [Serializable]
    public class QuestObjective
    {
        public string desc;
        public int current;
        public int required = 1;
        public bool Done => current >= required;
    }

    [Serializable]
    public class Quest
    {
        public string title;
        public bool main;                   // 主线 / 支线，颜色不同
        public Transform target;            // 可选：显示方向和距离
        public QuestObjective[] objectives;
    }
    //#endregion

    //#region tracker
    public class QuestTracker : MonoBehaviour
    {
        [SerializeField] QuestEntryView m_Template;
        [SerializeField] Transform m_Player;
        [SerializeField] Camera m_Camera;
        [SerializeField] List<Quest> m_Quests = new List<Quest>();

        readonly List<QuestEntryView> m_Views = new List<QuestEntryView>();
        readonly StringBuilder m_Sb = new StringBuilder();

        public List<Quest> Quests => m_Quests;

        void Start() => Rebuild();

        public void Rebuild()
        {
            m_Template.gameObject.SetActive(false);
            while (m_Views.Count < m_Quests.Count)
                m_Views.Add(Instantiate(m_Template, m_Template.transform.parent, false));
            for (int i = 0; i < m_Views.Count; i++)
            {
                bool used = i < m_Quests.Count;
                m_Views[i].gameObject.SetActive(used);
                if (used) RefreshEntry(i);
            }
            RefreshDirections();
        }

        /// 更新某个目标的进度，并让该任务闪一下
        public void SetProgress(int quest, int objective, int value)
        {
            var o = m_Quests[quest].objectives[objective];
            int v = Mathf.Clamp(value, 0, o.required);
            if (v == o.current) return;
            o.current = v;
            RefreshEntry(quest);
            m_Views[quest].Flash();
        }

        void RefreshEntry(int i)
        {
            var q = m_Quests[i];
            m_Sb.Clear();
            bool allDone = true;
            foreach (var o in q.objectives)
            {
                if (m_Sb.Length > 0) m_Sb.Append('\n');
                if (o.Done) m_Sb.Append("<color=#4CD16E>■ <s>").Append(o.desc).Append("</s></color>");
                else
                {
                    allDone = false;
                    m_Sb.Append("<color=#8A93AD>□</color> ").Append(o.desc);
                    if (o.required > 1) m_Sb.Append("  <b>").Append(o.current).Append('/').Append(o.required).Append("</b>");
                }
            }
            m_Views[i].Set(q.title, q.main, m_Sb.ToString(), allDone);
        }

        void LateUpdate()
        {
            RefreshDirections();
            foreach (var v in m_Views) v.Tick(Time.deltaTime);
        }

        /// 距离 + 方向箭头（相对镜头朝向，箭头朝上 = 正前方）
        public void RefreshDirections()
        {
            if (m_Player == null || m_Camera == null) return;
            Vector3 fwd = m_Camera.transform.forward; fwd.y = 0;
            for (int i = 0; i < m_Quests.Count && i < m_Views.Count; i++)
            {
                var t = m_Quests[i].target;
                if (t == null) { m_Views[i].SetDirection(false, 0, 0); continue; }
                Vector3 d = t.position - m_Player.position; d.y = 0;
                m_Views[i].SetDirection(true, Vector3.SignedAngle(fwd, d, Vector3.up), d.magnitude);
            }
        }
    }
    //#endregion
}
