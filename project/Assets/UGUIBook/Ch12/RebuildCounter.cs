// 第 12 章实验工具：统计“标脏”次数、偷看 CanvasUpdateRegistry 的两个重建队列。
// 只用于学习和调试（读私有字段用了反射），不要留在正式版本里。
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    //#region counter
    /// 用 Graphic 自带的 RegisterDirty*Callback 钩子数“标脏”次数——不需要改任何 UGUI 源码，也不用继承
    public static class RebuildCounter
    {
        public static int layoutDirty, verticesDirty, materialDirty;

        public static void Watch(Graphic g)
        {
            g.RegisterDirtyLayoutCallback(OnLayout);
            g.RegisterDirtyVerticesCallback(OnVerts);
            g.RegisterDirtyMaterialCallback(OnMaterial);
        }

        public static void Unwatch(Graphic g)
        {
            g.UnregisterDirtyLayoutCallback(OnLayout);
            g.UnregisterDirtyVerticesCallback(OnVerts);
            g.UnregisterDirtyMaterialCallback(OnMaterial);
        }

        static void OnLayout() => layoutDirty++;
        static void OnVerts() => verticesDirty++;
        static void OnMaterial() => materialDirty++;

        public static void Reset() { layoutDirty = verticesDirty = materialDirty = 0; }
    }
    //#endregion

    //#region spy
    /// 读 CanvasUpdateRegistry 里两个私有队列的长度（m_LayoutRebuildQueue / m_GraphicRebuildQueue）
    public static class CanvasUpdateSpy
    {
        static readonly FieldInfo s_Layout = typeof(CanvasUpdateRegistry).GetField("m_LayoutRebuildQueue", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo s_Graphic = typeof(CanvasUpdateRegistry).GetField("m_GraphicRebuildQueue", BindingFlags.Instance | BindingFlags.NonPublic);

        // IndexedSet<T> 是 internal 类，但它实现了公开的 IList<T>，按接口取就行
        public static IList<ICanvasElement> LayoutQueue => (IList<ICanvasElement>)s_Layout.GetValue(CanvasUpdateRegistry.instance);
        public static IList<ICanvasElement> GraphicQueue => (IList<ICanvasElement>)s_Graphic.GetValue(CanvasUpdateRegistry.instance);

        public static int layoutCount => LayoutQueue.Count;
        public static int graphicCount => GraphicQueue.Count;
    }
    //#endregion
}
