// 飞金币：从来源处炸开一圈，再沿二次贝塞尔曲线依次飞向顶栏的金币图标；每到一枚，图标弹一下、数字涨一截。
// 整段动画是一个 Sequence，Evaluate(t) 能直接拍出任意时刻（随机数用固定种子，同一时刻结果固定）。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class CoinFly : MonoBehaviour
    {
        //#region fields
        [SerializeField] Image m_CoinTemplate;          // 隐藏的模板，运行时复制（数量多时换成对象池）
        [SerializeField] RectTransform m_Layer;         // 金币飞行的图层（放在最上层，与模板同一父节点）
        [SerializeField] RectTransform m_Target;        // 飞向的目标（顶栏金币图标）
        [SerializeField] NumberRoller m_Counter;
        [SerializeField] int m_Count = 12;
        [SerializeField] float m_BurstRadius = 160;
        [SerializeField] float m_BurstTime = 0.3f;      // 炸开阶段
        [SerializeField] float m_FlyTime = 0.55f;       // 单枚飞行时间
        [SerializeField] float m_Stagger = 0.05f;
        [SerializeField] int m_Seed = 7;

        readonly List<Image> m_Coins = new List<Image>();
        //#endregion

        public void Setup(Image template, RectTransform layer, RectTransform target, NumberRoller counter)
        { m_CoinTemplate = template; m_Layer = layer; m_Target = target; m_Counter = counter; }

        //#region build
        /// from：起点（m_Layer 本地坐标）；reward：本次总奖励
        public Sequence Build(Vector2 from, long reward)
        {
            var rnd = new System.Random(m_Seed);
            Vector2 to = LocalOf(m_Target);
            var seq = new Sequence();
            long startValue = m_Counter != null ? m_Counter.value : 0;
            for (int i = 0; i < m_Count; i++)
            {
                var coin = GetCoin(i);
                var rt = coin.rectTransform;
                // 炸开点：均匀分布在圆周附近，带一点随机
                float ang = (i / (float)m_Count + (float)rnd.NextDouble() * 0.08f) * Mathf.PI * 2;
                float rad = m_BurstRadius * (0.6f + 0.4f * (float)rnd.NextDouble());
                Vector2 burst = from + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rad;
                // 控制点：起终点中点往侧上方偏，飞行轨迹是一条弧
                Vector2 ctrl = (burst + to) * 0.5f + new Vector2(-(to.y - burst.y), to.x - burst.x).normalized * 220f * (rnd.NextDouble() < 0.5 ? 1 : -1);

                // 阶段 1：炸开（OutCubic，快出慢停），同时从 0 放大
                seq.Insert(0, rt.AnchorPos(burst, m_BurstTime).From(from).SetEase(Ease.OutCubic));
                seq.Insert(0, rt.Scale(1f, m_BurstTime * 0.7f).From(Vector3.zero).SetEase(Ease.OutBack));
                // 阶段 2：沿贝塞尔飞向目标（InCubic，越飞越快，“吸”进去的感觉），逐枚错开
                float start = m_BurstTime + 0.1f + i * m_Stagger;
                seq.Insert(start, Tw.Value(rt, 0, 1, m_FlyTime, k =>
                {
                    float u = 1 - k;
                    rt.anchoredPosition = u * u * burst + 2 * u * k * ctrl + k * k * to;
                    rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.55f, k);
                }).SetEase(Ease.InCubic));
                // 到达：隐藏金币、目标弹一下
                float arrive = start + m_FlyTime;
                seq.Insert(arrive, Tw.Value(rt, 1, 0, 0.01f, a => coin.enabled = a > 0.5f).SetEase(Ease.Linear));
                seq.Insert(arrive, m_Target.PunchScale(0.25f, 0.2f));
            }
            // 数字从第一枚到达开始滚，到最后一枚到达时滚完
            if (m_Counter != null)
            {
                float first = m_BurstTime + 0.1f + m_FlyTime, last = first + (m_Count - 1) * m_Stagger;
                seq.Insert(first, Tw.Value(m_Counter, startValue, startValue + reward, last - first + 0.15f,
                    v => m_Counter.SetImmediate((long)v)).SetEase(Ease.Linear));
            }
            return seq;
        }
        //#endregion

        public void Play(RectTransform source, long reward) => Build(LocalOf(source), reward).Play();

        Image GetCoin(int i)
        {
            while (m_Coins.Count <= i)
            {
                var c = Instantiate(m_CoinTemplate, m_Layer);
                c.gameObject.SetActive(true);
                c.name = "Coin" + m_Coins.Count;
                m_Coins.Add(c);
            }
            m_Coins[i].enabled = true;
            return m_Coins[i];
        }

        // 任意 RectTransform 的中心换算到 m_Layer 的本地坐标（金币的 anchoredPosition 就在这个空间，锚点在中心）
        Vector2 LocalOf(RectTransform rt) =>
            m_Layer.InverseTransformPoint(rt.TransformPoint(rt.rect.center));
    }
}
