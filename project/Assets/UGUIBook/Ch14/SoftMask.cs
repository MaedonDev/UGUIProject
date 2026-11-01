// 软遮罩：用一张贴图的 alpha 作遮罩形状，边缘可以是任意柔和的渐变（Mask 只能硬边，RectMask2D 只能矩形）。
// 子物体挂 SoftMaskable，它们会改用 SoftMasked 材质的一个共享副本——同一个软遮罩下的元素仍可合批。
using UnityEngine;

namespace UGUIBook.Ch14
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UGUIBook/Ch14/Soft Mask")]
    public class SoftMask : MonoBehaviour
    {
        //#region fields
        [SerializeField] Sprite m_MaskSprite;           // 遮罩形状（用 alpha）
        [SerializeField] Texture m_MaskTexture;         // 没有 Sprite 时也可以直接给一张贴图（整张使用）
        [SerializeField] Material m_MaskedMaterial;     // 使用 UGUIBook/Ch14/SoftMasked 的材质资源

        Material m_Instance;
        Rect m_LastRect;
        static readonly int s_Tex = Shader.PropertyToID("_SoftMaskTex");
        static readonly int s_Rect = Shader.PropertyToID("_SoftMaskRect");
        static readonly int s_UV = Shader.PropertyToID("_SoftMaskUV");
        //#endregion

        public Sprite maskSprite { get => m_MaskSprite; set { m_MaskSprite = value; Refresh(); } }
        public Texture maskTexture { get => m_MaskTexture; set { m_MaskTexture = value; Refresh(); } }
        public Material maskedMaterial { get => m_MaskedMaterial; set { m_MaskedMaterial = value; m_Instance = null; Refresh(); } }

        //#region material
        /// 子物体通过这里拿到“共享的”遮罩材质
        public Material GetMaterial()
        {
            if (m_MaskedMaterial == null) return null;
            if (m_Instance == null)
            {
                m_Instance = new Material(m_MaskedMaterial) { hideFlags = HideFlags.HideAndDontSave, name = m_MaskedMaterial.name + " (SoftMask)" };
                m_LastRect = default;
                UpdateProperties();
            }
            return m_Instance;
        }

        void LateUpdate()
        {
            // 遮罩自己移动、缩放了，就更新矩形（只改材质参数，不需要重建任何网格）
            if (m_Instance != null && CanvasRect() != m_LastRect) UpdateProperties();
        }

        public void Refresh()
        {
            UpdateProperties();
            foreach (var m in GetComponentsInChildren<SoftMaskable>()) m.Dirty();
        }

        void UpdateProperties()
        {
            if (m_Instance == null) return;
            m_LastRect = CanvasRect();
            m_Instance.SetVector(s_Rect, new Vector4(m_LastRect.xMin, m_LastRect.yMin, m_LastRect.xMax, m_LastRect.yMax));
            if (m_MaskSprite != null)
            {
                m_Instance.SetTexture(s_Tex, m_MaskSprite.texture);
                m_Instance.SetVector(s_UV, UnityEngine.Sprites.DataUtility.GetOuterUV(m_MaskSprite));   // 精灵可能在图集里
            }
            else if (m_MaskTexture != null)
            {
                m_Instance.SetTexture(s_Tex, m_MaskTexture);
                m_Instance.SetVector(s_UV, new Vector4(0, 0, 1, 1));
            }
        }

        // 遮罩矩形在根画布本地空间中的位置（与顶点坐标同一空间）
        Rect CanvasRect()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return default;
            var root = canvas.rootCanvas.transform;
            var c = new Vector3[4];
            ((RectTransform)transform).GetWorldCorners(c);
            Vector2 a = root.InverseTransformPoint(c[0]), b = root.InverseTransformPoint(c[2]);
            return Rect.MinMaxRect(a.x, a.y, b.x, b.y);
        }

        void OnDestroy() { if (m_Instance != null) DestroyImmediate(m_Instance); }
        //#endregion
    }
}
