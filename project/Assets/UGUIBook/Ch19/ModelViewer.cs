// RenderTexture 角色展示：一台专用相机把模型渲染到 RenderTexture，RawImage 显示它；在 RawImage 上拖动旋转模型，滚轮缩放。
// · RT 在 OnEnable 时按 RawImage 的实际像素尺寸创建，界面关闭时释放并关掉相机（不显示就不渲染）
// · 相机背景清成透明（alpha = 0），界面的背景图能透出来
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch19
{
    [RequireComponent(typeof(RawImage))]
    public class ModelViewer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public Camera modelCamera;
        public Transform target;                // 被旋转的模型根节点
        [Tooltip("拖动 1 个屏幕像素转多少度")]
        public float degreesPerPixel = 0.35f;
        [Tooltip("松手后的惯性衰减（越大停得越快）")]
        public float damping = 5f;
        public Vector2 fovRange = new Vector2(18, 40);
        [Tooltip("RT 分辨率相对 RawImage 实际像素的倍率：1 = 一比一，0.5 = 省一半显存但会糊")]
        [Range(0.25f, 2f)] public float resolutionScale = 1f;
        [Range(1, 8)] public int msaa = 4;

        RawImage m_Image;
        RenderTexture m_RT;
        float m_Velocity;               // 度/秒
        bool m_Dragging;

        //#region rt
        void OnEnable()
        {
            m_Image = GetComponent<RawImage>();
            CreateTexture();
            if (modelCamera != null) modelCamera.enabled = true;
        }

        void OnDisable()
        {
            if (modelCamera != null) { modelCamera.enabled = false; modelCamera.targetTexture = null; }
            ReleaseTexture();
        }

        // RawImage 尺寸变了（转屏、适配）就按新尺寸重建 RT
        void OnRectTransformDimensionsChange() { if (isActiveAndEnabled && m_RT != null) CreateTexture(); }

        public void CreateTexture()
        {
            if (m_Image == null) m_Image = GetComponent<RawImage>();
            var canvas = m_Image.canvas;
            // rect 是画布单位，乘 scaleFactor 才是屏幕像素
            var size = m_Image.rectTransform.rect.size * (canvas != null ? canvas.scaleFactor : 1f) * resolutionScale;
            int w = Mathf.Clamp(Mathf.RoundToInt(size.x), 16, 2048), h = Mathf.Clamp(Mathf.RoundToInt(size.y), 16, 2048);
            if (m_RT != null && m_RT.width == w && m_RT.height == h) return;
            ReleaseTexture();
            m_RT = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32) { name = "ModelViewerRT", antiAliasing = msaa };
            m_RT.Create();
            if (modelCamera != null)
            {
                modelCamera.targetTexture = m_RT;
                modelCamera.clearFlags = CameraClearFlags.SolidColor;
                modelCamera.backgroundColor = new Color(0, 0, 0, 0);   // 透明背景
            }
            m_Image.texture = m_RT;
        }

        void ReleaseTexture()
        {
            if (m_RT == null) return;
            if (m_Image != null && m_Image.texture == m_RT) m_Image.texture = null;
            m_RT.Release();
            Destroy(m_RT);
            m_RT = null;
        }
        //#endregion

        //#region drag
        public void OnBeginDrag(PointerEventData e) { m_Dragging = true; m_Velocity = 0; }

        public void OnDrag(PointerEventData e)
        {
            // 向右拖，模型向右转（绕 Y 轴负方向）
            float deg = -e.delta.x * degreesPerPixel;
            Rotate(deg);
            if (Time.unscaledDeltaTime > 0) m_Velocity = Mathf.Lerp(m_Velocity, deg / Time.unscaledDeltaTime, 0.5f);
        }

        public void OnEndDrag(PointerEventData e) => m_Dragging = false;

        public void OnScroll(PointerEventData e)
        {
            if (modelCamera == null) return;
            modelCamera.fieldOfView = Mathf.Clamp(modelCamera.fieldOfView - e.scrollDelta.y * 2f, fovRange.x, fovRange.y);
        }

        void Update()
        {
            if (m_Dragging || Mathf.Abs(m_Velocity) < 1f) return;
            Rotate(m_Velocity * Time.unscaledDeltaTime);          // 界面常在暂停时打开，用不受 timeScale 影响的时间
            m_Velocity = Mathf.Lerp(m_Velocity, 0, 1 - Mathf.Exp(-damping * Time.unscaledDeltaTime));
        }
        //#endregion

        public void Rotate(float degrees) { if (target != null) target.Rotate(0, degrees, 0, Space.World); }

        /// 立即渲染一帧（编辑器里预览、截图用）
        public void RenderNow() { if (modelCamera != null && m_RT != null) modelCamera.Render(); }
    }
}
