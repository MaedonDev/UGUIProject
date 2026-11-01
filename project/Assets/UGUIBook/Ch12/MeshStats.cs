// IMeshModifier 示例（不借助 BaseMeshEffect，直接实现接口）：记录 Graphic 最终生成的网格——
// 顶点数、三角形数，并把三角形保存下来，供 WireOverlay 画线框。用来直观看 Image 各种 Type、文字到底生成了什么网格。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    //#region stats
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class MeshStats : UIBehaviour, IMeshModifier
    {
        public int vertexCount { get; private set; }
        public int triangleCount { get; private set; }
        public readonly List<Vector3> positions = new List<Vector3>();   // 本地坐标
        public readonly List<int> indices = new List<int>();

        static readonly List<UIVertex> s_Stream = new List<UIVertex>();

        public void ModifyMesh(VertexHelper vh)
        {
            vertexCount = vh.currentVertCount;
            triangleCount = vh.currentIndexCount / 3;
            // GetUIVertexStream 把“共享顶点 + 索引”展开成三角形列表（每 3 个一组）
            vh.GetUIVertexStream(s_Stream);
            positions.Clear(); indices.Clear();
            for (int i = 0; i < s_Stream.Count; i++) { positions.Add(s_Stream[i].position); indices.Add(i); }
            // 只是旁观，不修改 vh
        }

        // 旧接口（Mesh 版本）：useLegacyMeshGeneration 为 true 的 Graphic（Graphic 基类的默认值）会走这里
        public void ModifyMesh(Mesh mesh)
        {
            using (var vh = new VertexHelper(mesh)) ModifyMesh(vh);
        }

        protected override void OnEnable() { var g = GetComponent<Graphic>(); if (g != null) g.SetVerticesDirty(); }
    }
    //#endregion
}
