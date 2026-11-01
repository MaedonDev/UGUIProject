// 弹窗模糊背景（快照方案）：打开弹窗的那一刻，把相机画面渲染到一张降采样的 RenderTexture，
// 来回做几次高斯模糊，交给 RawImage 显示。之后每帧零开销——背景是静止的快照。
// 适用于 Screen Space - Camera / World Space 画布（它们由相机绘制）；Overlay 画布不经过相机，
// 需要改用 ScreenCapture.CaptureScreenshotIntoRenderTexture 在帧末抓屏。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    [AddComponentMenu("UGUIBook/Ch14/Blur Snapshot")]
    public class BlurSnapshot : MonoBehaviour
    {
        //#region fields
        [SerializeField] Camera m_Camera;              // 画面来源
        [SerializeField] RawImage m_Target;            // 显示模糊结果
        [SerializeField] GameObject m_HideWhileCapture; // 抓图时先隐藏的东西（弹窗本身）
        [SerializeField] Material m_BlurMaterial;      // UGUIBook/Ch14/Blur
        [SerializeField, Range(1, 8)] int m_Downsample = 4;
        [SerializeField, Range(0, 6)] int m_Iterations = 3;
        [SerializeField] float m_BlurSize = 1.2f;

        RenderTexture m_Result;
        //#endregion

        public int downsample { get => m_Downsample; set => m_Downsample = Mathf.Clamp(value, 1, 8); }
        public int iterations { get => m_Iterations; set => m_Iterations = Mathf.Clamp(value, 0, 6); }

        //#region capture
        public void Capture()
        {
            int w = Mathf.Max(1, m_Camera.pixelWidth), h = Mathf.Max(1, m_Camera.pixelHeight);
            // 1) 全分辨率渲染一帧（不含弹窗）
            bool wasActive = m_HideWhileCapture != null && m_HideWhileCapture.activeSelf;
            if (wasActive) m_HideWhileCapture.SetActive(false);
            m_Target.enabled = false;
            Canvas.ForceUpdateCanvases();
            var full = RenderTexture.GetTemporary(w, h, 24);
            var prevTarget = m_Camera.targetTexture;
            m_Camera.targetTexture = full;
            m_Camera.Render();
            m_Camera.targetTexture = prevTarget;
            if (wasActive) m_HideWhileCapture.SetActive(true);
            m_Target.enabled = true;

            // 2) 降采样：缩小本身就是一次“免费”的模糊，也让后面每次模糊的像素数少 downsample² 倍
            int dw = w / m_Downsample, dh = h / m_Downsample;
            var a = RenderTexture.GetTemporary(dw, dh, 0);
            var b = RenderTexture.GetTemporary(dw, dh, 0);
            a.filterMode = b.filterMode = FilterMode.Bilinear;
            Graphics.Blit(full, a);
            RenderTexture.ReleaseTemporary(full);

            // 3) 横、竖两遍为一次迭代；每次迭代把采样间距放大一点，模糊范围增长更快
            for (int i = 0; i < m_Iterations; i++)
            {
                m_BlurMaterial.SetFloat("_BlurSize", m_BlurSize * (1 + i * 0.5f));
                Graphics.Blit(a, b, m_BlurMaterial, 0);
                Graphics.Blit(b, a, m_BlurMaterial, 1);
            }

            // 4) 结果保存在自己持有的 RT 里（临时 RT 可能被别人复用，不能直接给 RawImage）
            if (m_Result == null || m_Result.width != dw || m_Result.height != dh)
            {
                if (m_Result != null) m_Result.Release();
                m_Result = new RenderTexture(dw, dh, 0) { name = "BlurSnapshot" };
            }
            Graphics.Blit(a, m_Result);
            RenderTexture.ReleaseTemporary(a);
            RenderTexture.ReleaseTemporary(b);
            m_Target.texture = m_Result;
        }

        void OnDestroy()
        {
            if (m_Result != null) { m_Result.Release(); if (Application.isPlaying) Destroy(m_Result); else DestroyImmediate(m_Result); }
        }
        //#endregion
    }
}
