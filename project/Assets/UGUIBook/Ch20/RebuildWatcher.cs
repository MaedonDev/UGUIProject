// 开发期诊断组件：某些元素连续很多帧都在重建（常见于 Animator、每帧赋值同一个文本/颜色），就把它们的名字打印出来
using UnityEngine;

namespace UGUIBook.Ch20
{
    //#region watcher
    /// 挂在场景里任意物体上：每帧在 LateUpdate 末尾检查重建队列，超过阈值就打印是谁
    [DefaultExecutionOrder(32000)]   // 尽量晚于其他脚本的 LateUpdate，看到的才是本帧的全部
    public class RebuildWatcher : MonoBehaviour
    {
        [Tooltip("每帧待重建元素超过这个数就打印一次清单")]
        public int threshold = 1;
        [Tooltip("连续多少帧都超过阈值才报（排除打开界面那一下的正常重建）")]
        public int consecutiveFrames = 30;
        int m_Hits;

        void LateUpdate()
        {
            int n = RebuildCounter.LayoutCount + RebuildCounter.GraphicCount;
            m_Hits = n >= threshold ? m_Hits + 1 : 0;
            if (m_Hits == consecutiveFrames)
                Debug.LogWarning($"[RebuildWatcher] 连续 {consecutiveFrames} 帧有元素在重建：\n{RebuildCounter.Dump()}", this);
        }
    }
    //#endregion
}
