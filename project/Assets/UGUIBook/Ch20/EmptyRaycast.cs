// 只接收射线、不画任何东西的 Graphic。
// 常见用法：全屏遮罩的点击区域、按钮放大点击范围。比“透明度为 0 的 Image”省：
// 透明 Image 照样生成网格、参与合批、在 GPU 上逐像素混合（只是混合结果看不出来）。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    //#region empty
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyRaycast : Graphic
    {
        // 不生成任何顶点：CanvasRenderer 拿到空网格，不会产生 Draw Call，也没有 Overdraw
        protected override void OnPopulateMesh(VertexHelper vh) => vh.Clear();

        // 不需要材质/贴图，颜色变化也没意义，直接跳过这些标脏
        public override void SetMaterialDirty() { }
        public override void SetVerticesDirty() { }
    }
    //#endregion
}
