// 所有界面的基类。生命周期由 UIManager 统一驱动，不依赖 Awake/Start，
// 这样“什么时候创建、什么时候显示、什么时候回收”全部掌握在框架手里。
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region panel
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] UILayer m_Layer = UILayer.Normal;
        [Tooltip("全屏不透明界面：打开后，栈里被它完全挡住的界面会被隐藏，省掉它们的渲染和重建")]
        [SerializeField] bool m_FullScreen;
        [SerializeField] UICacheMode m_CacheMode = UICacheMode.Cache;

        public UILayer Layer => m_Layer;
        public bool FullScreen => m_FullScreen;
        public UICacheMode CacheMode => m_CacheMode;
        public string Key { get; internal set; }          // 界面名，默认 = 类名 = 预制体名
        public bool IsOpen { get; private set; }
        public bool IsFocused { get; private set; }       // 是否位于栈顶（能接收返回键）

        // 打开期间注册的绑定、事件订阅，关闭时统一解除，杜绝“关了界面还在收事件”
        readonly List<IDisposable> m_OpenScope = new List<IDisposable>();

        // ---------- 由 UIManager 调用 ----------
        internal void InternalCreate()
        {
            BindUI();
            OnCreate();
        }

        internal void InternalOpen(object args)
        {
            if (IsOpen) DisposeOpenScope();   // 已打开时再次 Open：先解除上一次的绑定，避免重复订阅
            IsOpen = true;
            IsFocused = true;
            gameObject.SetActive(true);
            OnOpen(args);
        }

        internal void InternalPause()
        {
            if (!IsFocused) return;
            IsFocused = false;
            OnPause();
        }

        internal void InternalResume()
        {
            if (IsFocused || !IsOpen) return;
            IsFocused = true;
            OnResume();
        }

        internal void InternalClose()
        {
            IsFocused = false;
            IsOpen = false;
            OnClose();
            DisposeOpenScope();
            gameObject.SetActive(false);
        }

        void DisposeOpenScope()
        {
            for (int i = m_OpenScope.Count - 1; i >= 0; i--) m_OpenScope[i].Dispose();
            m_OpenScope.Clear();
        }

        internal void InternalRelease() => OnRelease();

        // ---------- 子类按需重写 ----------
        /// 取控件引用。一般由 UIBindGenerator 生成到 Xxx.Bind.cs 里，不用手写
        protected virtual void BindUI() { }
        /// 实例化后调用一次：AddListener、初始化静态内容
        protected virtual void OnCreate() { }
        /// 每次打开调用：args 是 Open 时传入的参数；在这里绑定数据、订阅事件
        protected virtual void OnOpen(object args) { }
        /// 被别的界面压在下面（失去焦点）
        protected virtual void OnPause() { }
        /// 上面的界面关掉了，自己重新回到栈顶
        protected virtual void OnResume() { }
        /// 关闭：停掉计时器、动画；OnOpen 里登记的绑定会被自动解除
        protected virtual void OnClose() { }
        /// 实例被销毁前调用：释放自己额外加载的资源
        protected virtual void OnRelease() { }

        /// 返回键 / Esc。返回 true 表示处理了；默认行为是关闭自己
        public virtual bool OnBack()
        {
            Close();
            return true;
        }

        // ---------- 工具方法 ----------
        public void Close()
        {
            if (UIManager.Instance != null) UIManager.Instance.Close(this);
        }

        /// 登记一个“关闭时自动释放”的对象（数据绑定、事件订阅……）
        protected void AddToOpenScope(IDisposable d)
        {
            if (d != null) m_OpenScope.Add(d);
        }

        /// 订阅事件中心的事件，界面关闭时自动退订
        protected void Listen<T>(Action<T> handler) => AddToOpenScope(EventCenter.Subscribe(handler));

        /// 生成的绑定代码通过它找控件；路径写错时给出清楚的报错而不是空引用
        protected T Bind<T>(string path) where T : Component
        {
            var t = transform.Find(path);
            var c = t != null ? t.GetComponent<T>() : null;
            if (c == null)
                Debug.LogError($"[UIPanel] {name} 找不到 {path} 上的 {typeof(T).Name}，预制体改过的话请重新生成绑定代码", this);
            return c;
        }

        protected GameObject BindGo(string path)
        {
            var t = transform.Find(path);
            if (t == null) Debug.LogError($"[UIPanel] {name} 找不到节点 {path}", this);
            return t != null ? t.gameObject : null;
        }
    }
    //#endregion
}
