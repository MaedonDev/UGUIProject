// 第 12 章实验：一个“会报数”的 Image。标脏只是登记，真正干活的是 Rebuild 里的 UpdateGeometry / UpdateMaterial，
// 这里把它们的执行次数记下来——被裁剪(cull)的图形即使标脏了也不会执行。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch12
{
    //#region probe
    public class ProbeImage : Image
    {
        public static int geometryUpdates, materialUpdates, populateCalls;

        protected override void UpdateGeometry() { geometryUpdates++; base.UpdateGeometry(); }
        protected override void UpdateMaterial() { materialUpdates++; base.UpdateMaterial(); }
        protected override void OnPopulateMesh(VertexHelper toFill) { populateCalls++; base.OnPopulateMesh(toFill); }

        public static void ResetCounters() { geometryUpdates = materialUpdates = populateCalls = 0; }
    }
    //#endregion
}
