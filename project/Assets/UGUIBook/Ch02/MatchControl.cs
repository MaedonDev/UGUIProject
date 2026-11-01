// 案例 2.1：运行时用滑块调 Canvas Scaler 的 Match，直观感受 0（按宽）→ 1（按高）的变化。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch02
{
    public class MatchControl : MonoBehaviour
    {
        [SerializeField] CanvasScaler scaler;
        [SerializeField] Slider slider;

        void Awake()
        {
            slider.value = scaler.matchWidthOrHeight;
            // CanvasScaler 在每次画布渲染前（Canvas.preWillRenderCanvases）重新计算 scaleFactor，改完值当帧就生效
            slider.onValueChanged.AddListener(v => scaler.matchWidthOrHeight = v);
        }
    }
}
