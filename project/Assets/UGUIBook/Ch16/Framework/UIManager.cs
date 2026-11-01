// UI 框架的核心：层级、界面栈、返回键、加载、缓存、弹窗队列。
// 挂在 UIRoot（一个 Screen Space 根 Canvas）上，场景里只放一个。
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch16
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] string m_PathFormat = "UIPanels/{0}";
        [Tooltip("Cache 模式的界面最多缓存几个（LRU）；超过时销毁最久没用的")]
        [SerializeField] int m_CacheCapacity = 4;
        [SerializeField] bool m_HandleBackKey = true;

        public IUIAssetLoader Loader { get; set; }
        public event Action<UIPanel> PanelOpened;
        public event Action<UIPanel> PanelClosed;

        //#region fields
        readonly Dictionary<UILayer, RectTransform> m_Layers = new Dictionary<UILayer, RectTransform>();
        readonly Dictionary<string, UIPanel> m_Opened = new Dictionary<string, UIPanel>();   // 所有打开中的界面
        readonly List<UIPanel> m_Stack = new List<UIPanel>();                                // 界面栈，末尾是栈顶
        readonly Dictionary<string, UIPanel> m_Permanent = new Dictionary<string, UIPanel>(); // 常驻实例
        readonly LinkedList<UIPanel> m_Lru = new LinkedList<UIPanel>();                       // 缓存，表头最新
        readonly HashSet<string> m_Loading = new HashSet<string>();                          // 异步加载中
        //#endregion

        public UIPanel Top => m_Stack.Count > 0 ? m_Stack[m_Stack.Count - 1] : null;
        public int StackCount => m_Stack.Count;
        public int CachedCount => m_Lru.Count;
        public RectTransform GetLayer(UILayer layer) => m_Layers[layer];

        void Awake() => Init();

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            // Android 的返回键在 Unity 里映射为 KeyCode.Escape
            if (m_HandleBackKey && Input.GetKeyDown(KeyCode.Escape)) Back();
        }

        //#region init
        /// 建立（或找回）各层节点。运行时由 Awake 调用；编辑器里搭景时也可以手动调用
        public void Init()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("[UIManager] 场景里有多个 UIManager，后创建的会接管 Instance", this);
            Instance = this;
            if (Loader == null) Loader = new ResourcesUILoader(m_PathFormat);

            var rootCanvas = GetComponent<Canvas>();
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var node = transform.Find(layer.ToString()) as RectTransform;
                if (node == null)
                {
                    var go = new GameObject(layer.ToString(), typeof(RectTransform));
                    go.layer = gameObject.layer;
                    node = (RectTransform)go.transform;
                    node.SetParent(transform, false);
                    node.anchorMin = Vector2.zero;
                    node.anchorMax = Vector2.one;
                    node.offsetMin = node.offsetMax = Vector2.zero;
                    // 每层一个子 Canvas：层与层之间的重建互不影响，排序由 sortingOrder 决定，
                    // 和层级里的先后顺序无关，界面里带粒子特效时也好插排序
                    var c = go.AddComponent<Canvas>();
                    c.overrideSorting = true;
                    c.sortingOrder = rootCanvas.sortingOrder + (int)layer * 100;
                    go.AddComponent<GraphicRaycaster>();   // 子 Canvas 上的图形只会被自己 Canvas 上的 Raycaster 检测到
                }
                node.SetSiblingIndex((int)layer);
                m_Layers[layer] = node;
            }
        }
        //#endregion

        // 进栈的层：参与返回键与焦点管理
        static bool IsStacked(UILayer layer) => layer == UILayer.Normal || layer == UILayer.Popup;

        //#region open
        public T Open<T>(object args = null) where T : UIPanel => Open(typeof(T).Name, args) as T;

        public UIPanel Open(string key, object args = null)
        {
            if (m_Opened.TryGetValue(key, out var opened))
            {
                // 已经开着：把它上面的界面都关掉，让它回到栈顶（类似 Android 的 singleTask）
                if (IsStacked(opened.Layer))
                    while (Top != null && Top != opened) Close(Top);
                opened.InternalOpen(args);   // 再走一次 OnOpen，让它用新参数刷新
                return opened;
            }
            var panel = GetOrCreate(key);
            return panel != null ? Show(panel, args) : null;
        }

        /// 异步打开：适合大界面，加载期间可以显示转圈（Top 层）
        public void OpenAsync<T>(object args = null, Action<T> onOpened = null) where T : UIPanel
        {
            string key = typeof(T).Name;
            if (m_Opened.ContainsKey(key) || m_Permanent.ContainsKey(key) || FindCached(key) != null)
            {
                var p = Open<T>(args);
                onOpened?.Invoke(p);
                return;
            }
            if (!m_Loading.Add(key)) return;   // 正在加载，忽略重复点击
            Loader.LoadAsync(key, prefab =>
            {
                m_Loading.Remove(key);
                if (this == null || prefab == null) { onOpened?.Invoke(null); return; }
                var panel = Instantiate(prefab, key);
                onOpened?.Invoke(panel != null ? Show(panel, args) as T : null);
            });
        }

        UIPanel Show(UIPanel panel, object args)
        {
            var rt = (RectTransform)panel.transform;
            rt.SetParent(m_Layers[panel.Layer], false);
            rt.SetAsLastSibling();
            if (IsStacked(panel.Layer))
            {
                if (Top != null) Top.InternalPause();
                m_Stack.Add(panel);
            }
            m_Opened[panel.Key] = panel;
            panel.InternalOpen(args);
            RefreshVisibility();
            PanelOpened?.Invoke(panel);
            return panel;
        }
        //#endregion

        //#region close
        public void Close<T>() where T : UIPanel
        {
            if (m_Opened.TryGetValue(typeof(T).Name, out var p)) Close(p);
        }

        public void Close(UIPanel panel)
        {
            if (panel == null || !panel.IsOpen) return;
            int index = m_Stack.IndexOf(panel);
            bool wasTop = index >= 0 && index == m_Stack.Count - 1;
            if (index >= 0) m_Stack.RemoveAt(index);
            m_Opened.Remove(panel.Key);

            panel.InternalClose();
            Recycle(panel);

            RefreshVisibility();
            if (wasTop && Top != null) Top.InternalResume();
            PanelClosed?.Invoke(panel);
            TryShowQueuedPopup();
        }

        /// 关掉栈里所有界面（切换场景、断线回登录时用）
        public void CloseAll()
        {
            m_PopupQueue.Clear();
            while (Top != null) Close(Top);
            foreach (var p in new List<UIPanel>(m_Opened.Values)) Close(p);
        }
        //#endregion

        //#region visibility
        /// 从栈顶往下：遇到第一个全屏界面后，更下面的界面都看不见了，直接隐藏
        void RefreshVisibility()
        {
            bool covered = false;
            for (int i = m_Stack.Count - 1; i >= 0; i--)
            {
                var p = m_Stack[i];
                if (p.gameObject.activeSelf == covered) p.gameObject.SetActive(!covered);
                if (p.FullScreen) covered = true;
            }
        }
        //#endregion

        //#region back
        /// 返回键：交给栈顶界面处理。返回 false 表示没有界面响应
        public bool Back()
        {
            var top = Top;
            return top != null && top.OnBack();
        }
        //#endregion

        //#region cache
        UIPanel GetOrCreate(string key)
        {
            if (m_Permanent.TryGetValue(key, out var p)) return p;
            var cached = FindCached(key);
            if (cached != null)
            {
                m_Lru.Remove(cached);
                return cached.Value;
            }
            var prefab = Loader.Load(key);
            if (prefab == null)
            {
                Debug.LogError($"[UIManager] 找不到界面预制体 {key}（{string.Format(m_PathFormat, key)}）");
                return null;
            }
            return Instantiate(prefab, key);
        }

        UIPanel Instantiate(GameObject prefab, string key)
        {
            var go = Object.Instantiate(prefab, m_Layers[UILayer.Normal], false);
            go.name = key;
            var panel = go.GetComponent<UIPanel>();
            if (panel == null)
            {
                Debug.LogError($"[UIManager] 预制体 {key} 的根节点上没有 UIPanel 组件");
                DestroyGo(go);
                return null;
            }
            panel.Key = key;
            if (panel.CacheMode == UICacheMode.Permanent) m_Permanent[key] = panel;
            panel.InternalCreate();
            return panel;
        }

        LinkedListNode<UIPanel> FindCached(string key)
        {
            for (var n = m_Lru.First; n != null; n = n.Next)
                if (n.Value.Key == key) return n;
            return null;
        }

        void Recycle(UIPanel panel)
        {
            switch (panel.CacheMode)
            {
                case UICacheMode.Permanent:
                    break;   // 已经 SetActive(false)，留着下次直接用
                case UICacheMode.Cache:
                    m_Lru.AddFirst(panel);
                    while (m_Lru.Count > Mathf.Max(0, m_CacheCapacity))
                    {
                        var oldest = m_Lru.Last.Value;
                        m_Lru.RemoveLast();
                        DestroyPanel(oldest);
                    }
                    break;
                default:
                    DestroyPanel(panel);
                    break;
            }
        }

        /// 清空缓存（内存告警、切场景时调用）；常驻界面不受影响
        public void ClearCache()
        {
            while (m_Lru.Count > 0)
            {
                var p = m_Lru.Last.Value;
                m_Lru.RemoveLast();
                DestroyPanel(p);
            }
        }

        void DestroyPanel(UIPanel panel)
        {
            panel.InternalRelease();
            Loader.Release(panel.Key);
            DestroyGo(panel.gameObject);
        }

        static void DestroyGo(GameObject go)
        {
            if (Application.isPlaying) Object.Destroy(go);
            else Object.DestroyImmediate(go);   // 编辑器里搭景、单元测试时
        }
        //#endregion

        //#region queue
        struct QueuedPopup
        {
            public string Key;
            public object Args;
            public int Priority;
            public int Order;   // 同优先级先进先出
        }

        readonly List<QueuedPopup> m_PopupQueue = new List<QueuedPopup>();
        int m_QueueOrder;

        public int QueuedCount => m_PopupQueue.Count;
        public event Action QueueChanged;

        /// 排队弹窗：当前没有弹窗时立即打开，否则等前面的弹窗都关掉再依次弹出。priority 大的先弹
        public void EnqueuePopup<T>(object args = null, int priority = 0) where T : UIPanel
        {
            m_PopupQueue.Add(new QueuedPopup { Key = typeof(T).Name, Args = args, Priority = priority, Order = m_QueueOrder++ });
            QueueChanged?.Invoke();
            TryShowQueuedPopup();
        }

        void TryShowQueuedPopup()
        {
            if (m_PopupQueue.Count == 0) return;
            if (Top != null && Top.Layer == UILayer.Popup) return;   // 还有弹窗开着，继续等
            int best = 0;
            for (int i = 1; i < m_PopupQueue.Count; i++)
            {
                var a = m_PopupQueue[i];
                var b = m_PopupQueue[best];
                if (a.Priority > b.Priority || (a.Priority == b.Priority && a.Order < b.Order)) best = i;
            }
            var next = m_PopupQueue[best];
            m_PopupQueue.RemoveAt(best);
            QueueChanged?.Invoke();
            Open(next.Key, next.Args);
        }
        //#endregion

        //#region helpers
        /// 通用确认框。onOk/onCancel 在按钮点下、确认框关闭之后调用
        public static void Confirm(string message, Action onOk, Action onCancel = null,
            string title = "提示", string okText = "确定", string cancelText = "取消")
        {
            Instance.Open<ConfirmPanel>(new ConfirmArgs
            {
                Title = title, Message = message, OkText = okText, CancelText = cancelText,
                OnOk = onOk, OnCancel = onCancel,
            });
        }

        /// 只有一个按钮的提示框
        public static void Alert(string message, Action onOk = null, string title = "提示") =>
            Instance.Open<ConfirmPanel>(new ConfirmArgs { Title = title, Message = message, OkText = "知道了", OnOk = onOk });

        /// 飘字提示：不进栈、不抢焦点
        public static void Toast(string message)
        {
            var toast = Instance.Open<ToastPanel>();
            if (toast != null) toast.Show(message);
        }
        //#endregion

        /// 调试用：把当前栈打印成 “MainPanel > BagPanel > ConfirmPanel”
        public string DumpStack()
        {
            var names = new List<string>();
            foreach (var p in m_Stack) names.Add(p.Key + (p.gameObject.activeSelf ? "" : "(隐藏)"));
            return string.Join(" > ", names);
        }
    }
}
