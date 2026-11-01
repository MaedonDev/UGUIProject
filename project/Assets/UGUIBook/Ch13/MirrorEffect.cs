// 镜像：美术只画左半边，右半边由网格镜像出来，贴图内存省一半（对称的边框、徽章、标题底板很常见）。
// 做法：把原网格压到左半边，再复制一份水平翻转放到右半边。顶点数 ×2。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [AddComponentMenu("UGUIBook/Ch13/Mirror")]
    public class MirrorEffect : BaseMeshEffect
    {
        //#region mirror
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            Rect r = graphic.rectTransform.rect;
            float midX = r.center.x;
            List<UIVertex> stream = ListPool<UIVertex>.Get();
            vh.GetUIVertexStream(stream);          // 取出“三角形流”：每 3 个顶点一个三角形
            int n = stream.Count;
            for (int i = 0; i < n; i++)
            {
                var v = stream[i];
                v.position.x = r.xMin + (v.position.x - r.xMin) * 0.5f;   // 压到左半边
                stream[i] = v;
                var m = v;
                m.position.x = midX + (midX - v.position.x);             // 关于中线翻转
                stream.Add(m);
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(stream);
            ListPool<UIVertex>.Release(stream);
        }
        //#endregion
    }
}
