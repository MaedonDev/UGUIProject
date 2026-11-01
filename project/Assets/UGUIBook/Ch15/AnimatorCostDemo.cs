// 场景 Ch15_AnimatorCost：300 个挂着 Animator 的图标，动画曲线是常量（看起来一动不动）。
// 开关 Animator，看“本帧被标脏的 Graphic 数量”，再打开 Profiler 对比 Canvas 的开销。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch15
{
    public class AnimatorCostDemo : MonoBehaviour
    {
        //#region demo
        [SerializeField] Toggle m_Toggle;
        [SerializeField] TMP_Text m_Info;
        [SerializeField] RectTransform m_Grid;
        Animator[] m_Animators;
        Graphic[] m_Graphics;

        void Start()
        {
            m_Animators = m_Grid.GetComponentsInChildren<Animator>(true);
            m_Graphics = m_Grid.GetComponentsInChildren<Graphic>(true);
            m_Toggle.onValueChanged.AddListener(on => { foreach (var a in m_Animators) a.enabled = on; });
        }

        // Animator 在 Update 之后、LateUpdate 之前写属性；这里数一下有多少 Graphic 因此进了重建队列
        void LateUpdate()
        {
            int dirty = RebuildProbe.CountDirty(m_Graphics);
            m_Info.text = $"Animator {(m_Toggle.isOn ? "开" : "关")}：本帧 {dirty} / {m_Graphics.Length} 个 Graphic 等待重建";
        }
        //#endregion
    }
}
