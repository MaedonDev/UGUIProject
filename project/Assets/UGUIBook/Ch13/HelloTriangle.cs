// 最小的自定义 Graphic：三个顶点、一个三角形，每个顶点一种颜色。GPU 会在三角形内部对顶点色做插值。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch13
{
    [RequireComponent(typeof(CanvasRenderer))]   // 和 Image/Text 一样：保证先有 CanvasRenderer
    [AddComponentMenu("UGUIBook/Ch13/Hello Triangle")]
    public class HelloTriangle : MaskableGraphic
    {
        //#region hello
        [SerializeField] Color m_Top = new Color(0.36f, 0.29f, 0.84f);
        [SerializeField] Color m_Left = new Color(1f, 0.54f, 0.24f);
        [SerializeField] Color m_Right = new Color(0.18f, 0.7f, 0.42f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();                                   // VertexHelper 是全局共用的，先清空
            Rect r = GetPixelAdjustedRect();              // 本地坐标下的矩形（以轴心为原点）
            // AddVert(位置, 颜色, UV)：顶点按加入顺序编号 0、1、2……
            vh.AddVert(new Vector3(r.center.x, r.yMax), m_Top * color, new Vector2(0.5f, 1));
            vh.AddVert(new Vector3(r.xMax, r.yMin), m_Right * color, new Vector2(1, 0));
            vh.AddVert(new Vector3(r.xMin, r.yMin), m_Left * color, new Vector2(0, 0));
            // AddTriangle 记录的是顶点“索引”，三个一组
            vh.AddTriangle(0, 1, 2);
        }
        //#endregion
    }
}
