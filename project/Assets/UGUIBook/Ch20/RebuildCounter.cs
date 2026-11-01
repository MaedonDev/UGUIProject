// 读取 CanvasUpdateRegistry 的两个重建队列：这一帧有多少元素等着做布局重建 / 图形重建，分别是谁。
// 队列是 UGUI 的私有字段，只能用反射读；仅用于开发期诊断，别在正式逻辑里依赖它。
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    //#region counter
    public static class RebuildCounter
    {
        static readonly FieldInfo s_LayoutQueue = typeof(CanvasUpdateRegistry)
            .GetField("m_LayoutRebuildQueue", BindingFlags.Instance | BindingFlags.NonPublic);
        static readonly FieldInfo s_GraphicQueue = typeof(CanvasUpdateRegistry)
            .GetField("m_GraphicRebuildQueue", BindingFlags.Instance | BindingFlags.NonPublic);

        // IndexedSet<ICanvasElement> 是 internal 类，但它实现了 IList<ICanvasElement>
        static IList<ICanvasElement> Queue(FieldInfo f) =>
            f?.GetValue(CanvasUpdateRegistry.instance) as IList<ICanvasElement>;

        /// 当前排队等待布局重建的元素数（在 LateUpdate 里读，正好是本帧攒下的）
        public static int LayoutCount => Queue(s_LayoutQueue)?.Count ?? -1;

        /// 当前排队等待图形重建（顶点/材质）的元素数
        public static int GraphicCount => Queue(s_GraphicQueue)?.Count ?? -1;

        /// 列出队列里的元素名字，找“是谁每帧在标脏”时很好用
        public static string Dump(int max = 20)
        {
            var sb = new StringBuilder();
            void List(string title, IList<ICanvasElement> q)
            {
                sb.Append(title).Append(' ').Append(q?.Count ?? 0).Append('\n');
                if (q == null) return;
                for (int i = 0; i < q.Count && i < max; i++)
                {
                    var t = q[i]?.transform;
                    sb.Append("  ").Append(t != null ? Path(t) : "(null)")
                      .Append("  <").Append(q[i]?.GetType().Name).Append(">\n");
                }
            }
            List("Layout", Queue(s_LayoutQueue));
            List("Graphic", Queue(s_GraphicQueue));
            return sb.ToString();
        }

        static string Path(Transform t) => t.parent == null ? t.name : Path(t.parent) + "/" + t.name;
    }
    //#endregion

}
