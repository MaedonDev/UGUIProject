// 统计“这一帧有多少 Graphic 在等待重建”的小探针。
// CanvasUpdateRegistry.TryRegisterCanvasElementForGraphicRebuild 内部是 AddUnique：已经在队列里就返回 false。
// 所以对一个 Graphic 调用它，返回 false = 它本帧已经被标脏（SetVerticesDirty / SetMaterialDirty）。
// 没被标脏的会被顺手加进队列，但它的 Rebuild 发现没有脏标记就直接返回，几乎没有代价。
using System.Collections.Generic;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public static class RebuildProbe
    {
        //#region probe
        public static int CountDirty(IList<Graphic> graphics)
        {
            int dirty = 0;
            for (int i = 0; i < graphics.Count; i++)
                if (!CanvasUpdateRegistry.TryRegisterCanvasElementForGraphicRebuild(graphics[i])) dirty++;
            return dirty;
        }
        //#endregion
    }
}
