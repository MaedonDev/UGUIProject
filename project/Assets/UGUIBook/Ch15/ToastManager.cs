// Toast 提示：从底部滑入、停留、淡出；同时出现多条时，旧的往上让位。
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch15
{
    public class ToastManager : MonoBehaviour
    {
        //#region fields
        [SerializeField] RectTransform m_Template;     // 隐藏的模板：CanvasGroup + 背景 + TMP 文字
        [SerializeField] float m_Spacing = 110;        // 相邻两条的垂直间距
        [SerializeField] float m_Stay = 1.6f;
        [SerializeField] int m_Max = 4;

        class Entry { public RectTransform rt; public CanvasGroup cg; public float slot; public Sequence seq; }
        readonly List<Entry> m_Live = new List<Entry>();
        //#endregion

        public void Setup(RectTransform template) => m_Template = template;

        //#region show
        /// 弹出一条提示。返回它的完整生命周期动画（入场 → 停留 → 离场）
        public Sequence Show(string message, bool play = true)
        {
            // 已有的每条往上挪一格
            foreach (var e in m_Live) { e.slot += 1; e.rt.AnchorPos(new Vector2(0, e.slot * m_Spacing), 0.25f).SetEase(Ease.OutCubic).PlayIf(play); }
            if (m_Live.Count >= m_Max) Remove(m_Live[0]);

            var rt = Instantiate(m_Template, m_Template.parent);
            rt.gameObject.SetActive(true);
            rt.GetComponentInChildren<TMP_Text>().text = message;
            var entry = new Entry { rt = rt, cg = rt.GetComponent<CanvasGroup>() };
            m_Live.Add(entry);

            var seq = new Sequence();
            seq.Append(rt.AnchorPos(Vector2.zero, 0.3f).From(new Vector2(0, -60)).SetEase(Ease.OutBack));
            seq.Join(entry.cg.Alpha(1, 0.2f).From(0));
            seq.AppendInterval(m_Stay);
            // 起点必须显式写 1：上面的 From(0) 已经把 alpha 设成了 0，不写的话这里会“从 0 淡到 0”
            seq.Append(entry.cg.Alpha(0, 0.35f).From(1, false).SetEase(Ease.InQuad));
            seq.AppendCallback(() => Remove(entry));
            entry.seq = seq;
            if (play) seq.Play();
            return seq;
        }

        void Remove(Entry e)
        {
            if (!m_Live.Remove(e)) return;
            e.seq?.Kill();
            if (e.rt != null) Destroy(e.rt.gameObject);
        }
        //#endregion

        /// 截图用：手动摆出“第 i 条处于第 slot 格”的状态
        public void Place(Sequence seq, float time, int slot)
        {
            seq.Evaluate(time);
            var e = m_Live.Find(x => x.seq == seq);
            if (e != null) e.rt.anchoredPosition += new Vector2(0, slot * m_Spacing);
        }
    }

    static class TweenPlayIf
    {
        public static T PlayIf<T>(this T t, bool play) where T : TweenBase => play ? t.Play() : t;
    }
}
