// 轻量补间系统（不依赖任何第三方库）。
// 设计要点：每个补间都是“时间 → 状态”的纯函数，Evaluate(time) 可以跳到任意时刻；
// 运行时由 TweenRunner 在 Update 里累加时间再调用同一个 Evaluate。所以同一套代码既能播放，也能直接拍关键帧。
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch15
{
    //#region base
    public abstract class TweenBase
    {
        public float duration { get; protected set; }
        public float delay { get; internal set; }
        public Ease ease { get; internal set; } = Ease.OutQuad;
        public int loops { get; internal set; } = 1;         // -1 = 无限循环
        public bool yoyo { get; internal set; }               // 往返：奇数次循环倒着播
        public bool unscaledTime { get; internal set; }       // 用真实时间（暂停时 timeScale = 0 也照样播）
        public bool isPlaying { get; internal set; }
        public bool isComplete { get; private set; }
        public Object target { get; protected set; }          // 目标被销毁时自动停止

        internal float elapsed;
        internal Action onComplete;
        internal AnimationCurve curve;

        /// 总时长（含延迟和全部循环；无限循环返回 +∞）
        public float totalDuration => loops < 0 ? float.PositiveInfinity : delay + duration * loops;


        /// 跳到第 time 秒的状态（time 从 0 开始，包含 delay）。不触发回调，可以随意来回跳
        public void Evaluate(float time)
        {
            float local = time - delay;
            if (local < 0) { Apply(0); return; }                      // 延迟期间停在起点
            if (duration <= 0) { Apply(1); return; }
            int loop = Mathf.FloorToInt(local / duration);
            float t = (local - loop * duration) / duration;
            if (loops >= 0 && loop >= loops) { loop = loops - 1; t = 1; } // 播完了，停在终点
            if (yoyo && (loop & 1) == 1) t = 1 - t;
            Apply(Ease01(t));
        }

        float Ease01(float t) => curve != null ? curve.Evaluate(t) : EaseFunc.Evaluate(ease, t);

        /// 子类实现：k 是缓动后的进度（可能略超出 0~1）
        protected abstract void Apply(float k);

        /// 目标是否已被 Destroy（Unity 的 == null 重载）；Sequence 会检查它的每个子补间
        internal virtual bool targetDestroyed => target is Object && target == null;

        /// 推进 dt 秒（TweenRunner 调用）。返回 true 表示已经结束
        internal virtual bool Tick(float dt)
        {
            if (targetDestroyed) return true;   // 目标被 Destroy 了（比如切场景），直接结束
            elapsed += dt;
            Evaluate(elapsed);
            if (elapsed >= totalDuration)
            {
                isComplete = true;
                onComplete?.Invoke();
                return true;
            }
            return false;
        }

        internal void Restart() { elapsed = 0; isComplete = false; }

        public void Kill() => TweenRunner.Remove(this);

        /// 立刻跳到结尾（也会触发 OnComplete）
        public void Complete()
        {
            if (float.IsInfinity(totalDuration)) { Kill(); return; }
            elapsed = totalDuration; Evaluate(elapsed);
            isComplete = true; onComplete?.Invoke(); Kill();
        }
    }
    //#endregion

    //#region fluent
    /// 链式设置写成泛型扩展方法：返回的还是原来的类型，Tween<T>.From、Sequence.Append 可以继续接着点
    public static class TweenFluent
    {
        public static T SetEase<T>(this T t, Ease e) where T : TweenBase { t.ease = e; t.curve = null; return t; }
        public static T SetEase<T>(this T t, AnimationCurve c) where T : TweenBase { t.curve = c; return t; }  // 美术调的曲线
        public static T SetDelay<T>(this T t, float d) where T : TweenBase { t.delay = d; return t; }
        public static T SetLoops<T>(this T t, int n, bool yoyo = false) where T : TweenBase { t.loops = n; t.yoyo = yoyo; return t; }
        public static T SetUnscaled<T>(this T t, bool on = true) where T : TweenBase { t.unscaledTime = on; return t; }
        public static T OnComplete<T>(this T t, Action a) where T : TweenBase { t.onComplete += a; return t; }
        public static T Play<T>(this T t) where T : TweenBase { t.Restart(); TweenRunner.Add(t); return t; }
    }
    //#endregion

    //#region generic
    /// 对任意类型 T 做插值：起点、终点、写回函数、插值函数
    public class Tween<T> : TweenBase
    {
        T m_From, m_To;
        readonly Action<T> m_Setter;
        readonly Func<T, T, float, T> m_Lerp;

        public Tween(Object target, Func<T> getter, Action<T> setter, T to, float duration, Func<T, T, float, T> lerp)
        {
            this.target = target;
            m_From = getter();               // 起点默认取“创建时的当前值”
            m_To = to;
            m_Setter = setter;
            m_Lerp = lerp;
            this.duration = duration;
        }

        /// 指定起点（并立刻应用，避免第一帧闪一下终点状态）
        public Tween<T> From(T from, bool applyNow = true)
        {
            m_From = from;
            if (applyNow) m_Setter(from);
            return this;
        }

        protected override void Apply(float k) => m_Setter(m_Lerp(m_From, m_To, k));
    }
    //#endregion

    //#region sequence
    /// 把多个补间排进一条时间线：Append 接在后面，Join 与上一个同时开始，Insert 放到指定时刻
    public class Sequence : TweenBase
    {
        struct Item { public float start; public TweenBase tween; }
        struct Callback { public float time; public Action action; }
        readonly List<Item> m_Items = new List<Item>();
        readonly List<Callback> m_Callbacks = new List<Callback>();
        float m_LastStart;

        public Sequence() { duration = 0; ease = Ease.Linear; }

        public Sequence Append(TweenBase t) => Insert(duration, t);
        public Sequence Join(TweenBase t) => Insert(m_LastStart, t);
        public Sequence AppendInterval(float seconds) { duration += seconds; return this; }
        public Sequence AppendCallback(Action a) { m_Callbacks.Add(new Callback { time = duration, action = a }); return this; }
        public Sequence InsertCallback(float time, Action a) { m_Callbacks.Add(new Callback { time = time, action = a }); return this; }

        public Sequence Insert(float time, TweenBase t)
        {
            m_Items.Add(new Item { start = time, tween = t });
            m_LastStart = time;
            duration = Mathf.Max(duration, time + t.totalDuration);
            return this;
        }

        // 时间线上的 k 就是秒数（Sequence 本身用 Linear，不再额外加缓动）
        protected override void Apply(float k)
        {
            float time = k * duration;
            // 先把还没开始的补间按“开始时间倒序”放到起点：同一属性上最早的那个起点最终生效；
            // 再按顺序推进已经开始的补间：后开始的覆盖先开始的。这样任意时刻跳转结果都正确
            for (int i = m_Items.Count - 1; i >= 0; i--)
                if (time < m_Items[i].start) m_Items[i].tween.Evaluate(0);
            foreach (var it in m_Items)
                if (time >= it.start) it.tween.Evaluate(Mathf.Min(time - it.start, it.tween.totalDuration));
        }

        internal override bool targetDestroyed => m_Items.Exists(it => it.tween.targetDestroyed);

        internal override bool Tick(float dt)
        {
            // 回调只在“真正播放”时触发（Evaluate 跳转不触发）：本帧走过的时间段 (before, after] 里的都调一次
            float before = elapsed - delay, after = before + dt;
            bool first = elapsed == 0;
            foreach (var c in m_Callbacks)
                if ((first ? c.time >= before : c.time > before) && c.time <= after) c.action?.Invoke();
            return base.Tick(dt);
        }
    }
    //#endregion

    //#region runner
    /// 全局驱动器：第一次 Play 时自动创建，挂在一个 DontDestroyOnLoad 的物体上
    public class TweenRunner : MonoBehaviour
    {
        static TweenRunner s_Instance;
        readonly List<TweenBase> m_Active = new List<TweenBase>();
        readonly List<TweenBase> m_Pending = new List<TweenBase>();   // Update 过程中新加入的，下一轮再并入

        internal static void Add(TweenBase t)
        {
            if (!Application.isPlaying) { Debug.LogWarning("Tween.Play 只能在运行时调用；编辑器里请用 Evaluate(time)"); return; }
            if (s_Instance == null)
            {
                var go = new GameObject("[TweenRunner]");
                DontDestroyOnLoad(go);
                s_Instance = go.AddComponent<TweenRunner>();
            }
            t.isPlaying = true;
            // Kill 之后同一帧又 Play：它可能还在列表里（只是被标记了），不能重复加入
            if (!s_Instance.m_Active.Contains(t) && !s_Instance.m_Pending.Contains(t)) s_Instance.m_Pending.Add(t);
        }

        internal static void Remove(TweenBase t) => t.isPlaying = false;   // 标记即可，Update 里统一移除

        public static int activeCount => s_Instance != null ? s_Instance.m_Active.Count : 0;

        void Update()
        {
            m_Active.AddRange(m_Pending);
            m_Pending.Clear();
            float dt = Time.deltaTime, udt = Time.unscaledDeltaTime;
            for (int i = 0; i < m_Active.Count; i++)
            {
                var t = m_Active[i];
                bool done = !t.isPlaying;
                if (!done)
                {
                    try { done = t.Tick(t.unscaledTime ? udt : dt); }
                    catch (Exception e) { Debug.LogException(e); done = true; }   // 一个补间出错不影响其他
                }
                if (done) { t.isPlaying = false; m_Active.RemoveAt(i--); }
            }
        }
    }
    //#endregion
}
