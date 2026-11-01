// 本章所有自定义 Graphic 共用的“画网格”小工具：加顶点、铺三角形带、画粗线、画圆点、生成圆弧。
// 它们只是对 VertexHelper.AddVert / AddTriangle 的组合，没有任何魔法。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    public static class UIMeshUtil
    {
        //#region vert
        /// 加一个顶点并返回它的索引（索引 = 加之前的顶点数）
        public static int Vert(VertexHelper vh, Vector2 pos, Color32 color, Vector2 uv)
        {
            int index = vh.currentVertCount;
            vh.AddVert(pos, color, uv);
            return index;
        }

        /// 四个顶点按 左下→左上→右上→右下 的顺序加入，拆成两个三角形（和 Graphic 默认的 OnPopulateMesh 一样）
        public static void Quad(VertexHelper vh, Vector2 bl, Vector2 tl, Vector2 tr, Vector2 br, Color32 color)
        {
            int i = vh.currentVertCount;
            vh.AddVert(bl, color, new Vector2(0, 0));
            vh.AddVert(tl, color, new Vector2(0, 1));
            vh.AddVert(tr, color, new Vector2(1, 1));
            vh.AddVert(br, color, new Vector2(1, 0));
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i + 2, i + 3, i);
        }
        //#endregion

        //#region strip
        /// 两排顶点（各 count 个，起始索引 a、b）之间铺三角形带。closed = 首尾也连起来（环形）
        public static void Strip(VertexHelper vh, int a, int b, int count, bool closed)
        {
            int n = closed ? count : count - 1;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % count;
                vh.AddTriangle(a + i, b + i, b + j);
                vh.AddTriangle(b + j, a + j, a + i);
            }
        }

        /// 扇形：中心点 c 和一圈顶点（起始索引 first，共 count 个）
        public static void Fan(VertexHelper vh, int c, int first, int count, bool closed)
        {
            int n = closed ? count : count - 1;
            for (int i = 0; i < n; i++)
                vh.AddTriangle(c, first + i, first + (i + 1) % count);
        }
        //#endregion

        /// 圆弧上的点：圆心 c、半径 r，从 a0 到 a1（弧度），包含两端共 seg+1 个点
        public static void Arc(List<Vector2> outPts, Vector2 c, float r, float a0, float a1, int seg)
        {
            for (int i = 0; i <= seg; i++)
            {
                float a = Mathf.Lerp(a0, a1, (float)i / seg);
                outPts.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
            }
        }

        /// 实心圆点（扇形），可带 1 像素左右的羽化边
        public static void Dot(VertexHelper vh, Vector2 c, float r, Color32 color, int seg = 16, float feather = 1f)
        {
            int center = Vert(vh, c, color, new Vector2(0.5f, 0.5f));
            int first = vh.currentVertCount;
            float rin = Mathf.Max(0, r - feather * 0.5f);
            for (int i = 0; i < seg; i++)
            {
                float a = i * Mathf.PI * 2 / seg;
                vh.AddVert(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rin, color, new Vector2(0.5f, 0.5f));
            }
            Fan(vh, center, first, seg, true);
            if (feather <= 0) return;
            var clear = color; clear.a = 0;
            int outer = vh.currentVertCount;
            for (int i = 0; i < seg; i++)
            {
                float a = i * Mathf.PI * 2 / seg;
                vh.AddVert(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (rin + feather), clear, new Vector2(0.5f, 0.5f));
            }
            Strip(vh, first, outer, seg, true);
        }

        //#region polyline
        /// 粗折线（斜接 Miter 拐角）。每个点生成左右两个顶点，相邻点之间铺两个三角形。
        /// 拐角很尖时斜接会变得极长，用 miterLimit 限制，超出就退化成普通偏移（略有缺口，但不会“飞刺”）
        public static void PolyLine(VertexHelper vh, IList<Vector2> pts, float width, Color32 color,
            bool closed = false, float feather = 1f, float miterLimit = 3f)
        {
            int n = pts.Count;
            if (n < 2) return;
            float half = Mathf.Max(0.01f, (width - feather) * 0.5f);
            var clear = color; clear.a = 0;
            int baseIndex = vh.currentVertCount;
            for (int i = 0; i < n; i++)
            {
                // 前后两段的方向；开放折线的两端只有一段
                Vector2 prev = (i > 0 || closed) ? pts[i] - pts[(i - 1 + n) % n] : pts[1] - pts[0];
                Vector2 next = (i < n - 1 || closed) ? pts[(i + 1) % n] - pts[i] : pts[n - 1] - pts[n - 2];
                Vector2 n0 = new Vector2(-prev.y, prev.x).normalized;
                Vector2 n1 = new Vector2(-next.y, next.x).normalized;
                Vector2 miter = (n0 + n1).normalized;
                if (miter.sqrMagnitude < 1e-6f) miter = n1;
                float len = 1f / Mathf.Max(0.0001f, Vector2.Dot(miter, n1));   // 斜接长度系数 = 1/cos(半角)
                len = Mathf.Min(len, miterLimit);
                Vector2 off = miter * len;
                // 每个点 4 个顶点：外羽化、左、右、外羽化（羽化顶点 alpha=0，让边缘在 1 像素内过渡，起抗锯齿作用）
                vh.AddVert(pts[i] + off * (half + feather), clear, Vector2.zero);
                vh.AddVert(pts[i] + off * half, color, Vector2.zero);
                vh.AddVert(pts[i] - off * half, color, Vector2.zero);
                vh.AddVert(pts[i] - off * (half + feather), clear, Vector2.zero);
            }
            int segs = closed ? n : n - 1;
            for (int i = 0; i < segs; i++)
            {
                int a = baseIndex + i * 4, b = baseIndex + ((i + 1) % n) * 4;
                for (int k = 0; k < 3; k++)
                {
                    vh.AddTriangle(a + k, b + k, b + k + 1);
                    vh.AddTriangle(b + k + 1, a + k + 1, a + k);
                }
            }
        }
        //#endregion

        /// 两点之间的一段粗线（不处理拐角），画网格线、坐标轴用
        public static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color32 color)
        {
            Vector2 d = b - a;
            if (d.sqrMagnitude < 1e-6f) return;
            Vector2 n = new Vector2(-d.y, d.x).normalized * width * 0.5f;
            Quad(vh, a - n, a + n, b + n, b - n, color);
        }
    }
}
