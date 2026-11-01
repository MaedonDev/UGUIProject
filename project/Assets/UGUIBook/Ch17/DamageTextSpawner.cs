// 伤害飘字：对象池 + 每帧把世界坐标换算到屏幕。
// 一场团战每秒可能冒出几十个数字，Instantiate/Destroy 会带来 GC 和卡顿，必须复用。
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch17
{
    //#region spawner
    public class DamageTextSpawner : MonoBehaviour
    {
        [SerializeField] DamageText m_Template;   // 场景里一个隐藏的模板
        [SerializeField] Camera m_WorldCamera;
        [SerializeField] Canvas m_Canvas;
        [SerializeField] int m_Prewarm = 16;
        [SerializeField] int m_MaxActive = 60;    // 超过就回收最老的，防止爆量时把帧率拖垮

        readonly List<DamageText> m_Active = new List<DamageText>();
        readonly Stack<DamageText> m_Pool = new Stack<DamageText>();
        bool m_Inited;

        public int ActiveCount => m_Active.Count;
        public int PooledCount => m_Pool.Count;
        public int CreatedCount { get; private set; }

        void Awake() => Init();

        public void Init()
        {
            if (m_Inited) return;
            m_Inited = true;
            m_Template.gameObject.SetActive(false);
            for (int i = 0; i < m_Prewarm; i++) m_Pool.Push(Create());
        }

        DamageText Create()
        {
            CreatedCount++;
            var d = Instantiate(m_Template, transform, false);
            d.gameObject.SetActive(false);
            return d;
        }

        public DamageText Spawn(Vector3 worldPos, int amount, DamageKind kind, Vector2? drift = null)
        {
            Init();
            if (m_Active.Count >= m_MaxActive) Recycle(0);
            var d = m_Pool.Count > 0 ? m_Pool.Pop() : Create();
            // 随机横向漂移，连续多次命中时数字不会叠成一团
            var dir = drift ?? new Vector2(Random.Range(-60f, 60f), Random.Range(90f, 140f));
            d.Setup(worldPos, amount, kind, dir);
            d.transform.SetAsLastSibling();   // 新数字盖在旧数字上面
            d.gameObject.SetActive(true);
            m_Active.Add(d);
            return d;
        }

        void Recycle(int index)
        {
            var d = m_Active[index];
            m_Active.RemoveAt(index);
            d.gameObject.SetActive(false);
            m_Pool.Push(d);
        }

        void LateUpdate() => Tick(Time.deltaTime);

        public void Tick(float dt)
        {
            var space = (RectTransform)transform;
            var uiCam = HUDUtil.UICamera(m_Canvas);
            for (int i = m_Active.Count - 1; i >= 0; i--)
            {
                var d = m_Active[i];
                d.Age += dt;
                HUDUtil.WorldToLocal(m_WorldCamera, d.WorldPos, space, uiCam, out var local, out float depth);
                if (d.Age >= d.Life || depth <= 0) { Recycle(i); continue; }
                d.Evaluate(local);
            }
        }
    }
    //#endregion
}
