// 界面预制体从哪里来，是 UIManager 唯一不该关心的事。抽成接口后，
// 开发期用 Resources，上线前换成 Addressables / AssetBundle，只需要换一个实现类。
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region loader
    public interface IUIAssetLoader
    {
        /// 同步加载界面预制体，找不到返回 null
        GameObject Load(string key);
        /// 异步加载，完成后回调（失败时回调 null）
        void LoadAsync(string key, Action<GameObject> done);
        /// 界面实例被销毁后调用，告诉加载器这份预制体可以释放了
        void Release(string key);
    }

    /// 基于 Resources 的实现：预制体放在任意 Resources/UIPanels/ 目录下，文件名 = 界面名
    public class ResourcesUILoader : IUIAssetLoader
    {
        readonly string m_PathFormat;
        readonly Dictionary<string, GameObject> m_Loaded = new Dictionary<string, GameObject>();

        public ResourcesUILoader(string pathFormat = "UIPanels/{0}") { m_PathFormat = pathFormat; }

        public GameObject Load(string key)
        {
            if (m_Loaded.TryGetValue(key, out var prefab) && prefab != null) return prefab;
            prefab = Resources.Load<GameObject>(string.Format(m_PathFormat, key));
            if (prefab != null) m_Loaded[key] = prefab;
            return prefab;
        }

        public void LoadAsync(string key, Action<GameObject> done)
        {
            if (m_Loaded.TryGetValue(key, out var cached) && cached != null) { done(cached); return; }
            var req = Resources.LoadAsync<GameObject>(string.Format(m_PathFormat, key));
            req.completed += _ =>
            {
                var prefab = req.asset as GameObject;
                if (prefab != null) m_Loaded[key] = prefab;
                done(prefab);
            };
        }

        public void Release(string key)
        {
            // Resources 加载的预制体不能单独 Unload（Resources.UnloadAsset 只接受非 GameObject 资源），
            // 去掉引用后，等切场景或内存紧张时由 Resources.UnloadUnusedAssets 统一回收
            m_Loaded.Remove(key);
        }
    }
    //#endregion
}
