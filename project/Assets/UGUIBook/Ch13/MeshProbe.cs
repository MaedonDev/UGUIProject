// 调试工具：在不渲染的情况下，取出任意 Graphic “将要提交”的网格（OnPopulateMesh + 全部 IMeshModifier），
// 用来统计顶点数/三角形数、画线框。OnPopulateMesh 是 protected 的，这里用反射调用——只在调试和教学时这么干。
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    public static class MeshProbe
    {
        //#region probe
        static readonly MethodInfo s_Populate = typeof(Graphic).GetMethod("OnPopulateMesh",
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(VertexHelper) }, null);

        /// 复刻 Graphic.DoMeshGeneration 的流程：先 OnPopulateMesh，再依次调用同物体上的 IMeshModifier
        public static void Build(Graphic g, VertexHelper vh)
        {
            vh.Clear();
            s_Populate.Invoke(g, new object[] { vh });      // 虚方法：会调到子类（Image/Text/自定义）的实现
            var mods = new List<Component>();
            g.GetComponents(typeof(IMeshModifier), mods);
            foreach (var m in mods)
                if (((Behaviour)m).isActiveAndEnabled) ((IMeshModifier)m).ModifyMesh(vh);
        }

        public static void Count(Graphic g, out int verts, out int tris)
        {
            using (var vh = new VertexHelper())
            {
                Build(g, vh);
                verts = vh.currentVertCount;
                tris = vh.currentIndexCount / 3;
            }
        }
        //#endregion
    }
}
