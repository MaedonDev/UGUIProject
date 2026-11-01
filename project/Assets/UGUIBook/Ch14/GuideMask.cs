// 新手引导遮罩：全屏压暗，只在目标控件处挖一个洞；洞里的点击“穿透”给下面的目标，洞外的点击被吃掉。
// 洞的位置通过 IMaterialModifier 写进材质副本（和 Mask 组件改 Stencil 的套路一样），Shader 用 SDF 画圆角洞。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UGUIBook/Ch14/Guide Mask")]
    public class GuideMask : MonoBehaviour, IMaterialModifier, ICanvasRaycastFilter
    {
        //#region fields
        [SerializeField] RectTransform m_Target;
        [SerializeField] Vector2 m_Padding = new Vector2(16, 16);  // 洞比目标大一圈
        [SerializeField] float m_Radius = 24;
        [SerializeField] float m_Feather = 10;
        [SerializeField, Range(0, 1)] float m_Progress = 1;         // 0 = 洞覆盖整屏，1 = 洞收缩到目标
        [SerializeField] float m_AnimDuration = 0.45f;

        Graphic m_Graphic;
        Material m_Instance;
        Rect m_Hole;                 // 当前洞（画布空间）
        float m_AnimTime = -1;
        static readonly int s_HoleRect = Shader.PropertyToID("_HoleRect");
        static readonly int s_HoleRadius = Shader.PropertyToID("_HoleRadius");
        static readonly int s_Feather = Shader.PropertyToID("_Feather");
        //#endregion

        public RectTransform target => m_Target;
        public float progress { get => m_Progress; set { m_Progress = Mathf.Clamp01(value); Refresh(); } }

        Graphic graphic => m_Graphic != null ? m_Graphic : (m_Graphic = GetComponent<Graphic>());

        //#region api
        /// 切换引导目标，并播放“洞从全屏收缩到目标”的动画
        public void SetTarget(RectTransform target, bool animate = true)
        {
            m_Target = target;
            m_AnimTime = animate ? 0 : -1;
            m_Progress = animate ? 0 : 1;
            Refresh();
        }

        void Update()
        {
            if (m_AnimTime >= 0 && Application.isPlaying)
            {
                m_AnimTime += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(m_AnimTime / m_AnimDuration);
                m_Progress = 1 - Mathf.Pow(1 - t, 3);          // OutCubic
                if (t >= 1) m_AnimTime = -1;
                Refresh();
            }
            else if (m_Target != null && m_Target.hasChanged)   // 目标被布局移动了也要跟上
            {
                m_Target.hasChanged = false;
                Refresh();
            }
        }

        /// 重新计算洞的位置并让材质刷新
        public void Refresh()
        {
            m_Hole = ComputeHole();
            graphic.SetMaterialDirty();
        }
        //#endregion

        //#region compute
        Rect ComputeHole()
        {
            var canvas = graphic.canvas;
            if (canvas == null || m_Target == null) return Rect.zero;
            // 与 RectMask2D 一致：把目标的四个角变换到根画布的本地空间（Shader 里顶点坐标就在这个空间）
            var root = canvas.rootCanvas.transform;
            var corners = new Vector3[4];
            m_Target.GetWorldCorners(corners);
            Vector2 min = root.InverseTransformPoint(corners[0]), max = root.InverseTransformPoint(corners[2]);
            var target = Rect.MinMaxRect(min.x - m_Padding.x, min.y - m_Padding.y, max.x + m_Padding.x, max.y + m_Padding.y);
            // 动画起点：以目标为中心、刚好盖住整个画布的大洞（再往外扩一点羽化宽度）
            var full = ((RectTransform)root).rect;
            Vector2 c = target.center;
            Vector2 half = new Vector2(Mathf.Max(c.x - full.xMin, full.xMax - c.x), Mathf.Max(c.y - full.yMin, full.yMax - c.y))
                           + Vector2.one * (m_Feather + m_Radius);
            var start = new Rect(c - half, half * 2);
            return new Rect(Vector2.Lerp(start.position, target.position, m_Progress), Vector2.Lerp(start.size, target.size, m_Progress));
        }
        //#endregion

        //#region modifier
        // Graphic 在 UpdateMaterial 时依次调用所有 IMaterialModifier；我们返回一个带洞参数的材质副本
        public Material GetModifiedMaterial(Material baseMaterial)
        {
            if (!isActiveAndEnabled || m_Target == null) return baseMaterial;
            if (m_Instance == null || m_Instance.shader != baseMaterial.shader)
            {
                if (m_Instance != null) DestroyImmediate(m_Instance);
                m_Instance = new Material(baseMaterial) { hideFlags = HideFlags.HideAndDontSave };
            }
            m_Instance.CopyPropertiesFromMaterial(baseMaterial);   // 保留 Mask 写进去的 Stencil 参数
            m_Instance.SetVector(s_HoleRect, new Vector4(m_Hole.xMin, m_Hole.yMin, m_Hole.xMax, m_Hole.yMax));
            m_Instance.SetFloat(s_HoleRadius, m_Radius);
            m_Instance.SetFloat(s_Feather, m_Feather);
            return m_Instance;
        }

        void OnEnable() { Refresh(); }
        void OnDisable() { if (graphic != null) graphic.SetMaterialDirty(); }
        void OnDestroy() { if (m_Instance != null) DestroyImmediate(m_Instance); }
#if UNITY_EDITOR
        void OnValidate() { if (isActiveAndEnabled) Refresh(); }
#endif
        //#endregion

        //#region raycast
        // 返回 false = “这一点不算我的”，GraphicRaycaster 就会把它交给下面的元素——点击就穿透了
        public bool IsRaycastLocationValid(Vector2 sp, Camera cam)
        {
            if (m_Target == null) return true;
            return !RectTransformUtility.RectangleContainsScreenPoint(m_Target, sp, cam);
        }
        //#endregion
    }
}
