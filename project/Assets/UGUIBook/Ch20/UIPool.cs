// 极简 UI 对象池：取出时只 SetActive(true)、回收时 SetActive(false) 并挂到池节点下。
// 列表项、飘字、弹窗这类“反复出现又反复消失”的东西都适合。
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch20
{
    //#region pool
    public class UIPool<T> where T : Component
    {
        readonly T m_Prefab;
        readonly Transform m_Root;          // 闲置对象放这里（建议挂在一个 inactive 节点下）
        readonly Stack<T> m_Free = new Stack<T>();

        public int CountAll { get; private set; }
        public int CountFree => m_Free.Count;

        public UIPool(T prefab, Transform root, int prewarm = 0)
        {
            m_Prefab = prefab; m_Root = root;
            for (int i = 0; i < prewarm; i++) Release(Create());
        }

        T Create()
        {
            CountAll++;
            return Object.Instantiate(m_Prefab, m_Root, false);
        }

        public T Get(Transform parent)
        {
            var item = m_Free.Count > 0 ? m_Free.Pop() : Create();
            // SetParent 的第二个参数传 false：保持 RectTransform 的锚点/尺寸数值不变，不做世界坐标换算
            if (item.transform.parent != parent) item.transform.SetParent(parent, false);
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            item.gameObject.SetActive(false);
            item.transform.SetParent(m_Root, false);
            m_Free.Push(item);
        }
    }
    //#endregion
}
