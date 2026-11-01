// 案例 11.5：让 3D 物体响应 UI 事件。相机上挂 PhysicsRaycaster，物体有 Collider，
// 就能像 UI 一样实现 IPointerEnter/Exit/Click 接口——而且自动被前面的 UI 挡住，不会“点穿”。
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    [RequireComponent(typeof(Collider))]
    public class Clickable3D : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] Color m_Color = Color.white;
        [SerializeField] Color m_HoverColor = new Color(1f, 0.85f, 0.4f);
        public UnityEvent<string> onClicked = new UnityEvent<string>();

        static readonly int ColorId = Shader.PropertyToID("_Color");
        MaterialPropertyBlock m_Block;
        Vector3 m_BaseScale;
        float m_Punch;

        void Awake() { m_BaseScale = transform.localScale; ApplyColor(m_Color); }

        //#region events
        public void OnPointerEnter(PointerEventData e) => ApplyColor(m_HoverColor);
        public void OnPointerExit(PointerEventData e) => ApplyColor(m_Color);

        public void OnPointerClick(PointerEventData e)
        {
            m_Punch = 1;
            // pointerCurrentRaycast 里有 3D 命中点和法线（PhysicsRaycaster 填的）
            var hit = e.pointerCurrentRaycast;
            onClicked.Invoke($"{name}  命中点 {hit.worldPosition:F2}  距离 {hit.distance:0.00}");
        }
        //#endregion

        void Update()
        {
            if (m_Punch <= 0) return;
            m_Punch = Mathf.Max(0, m_Punch - Time.deltaTime * 4);
            transform.localScale = m_BaseScale * (1 + 0.25f * Mathf.Sin(m_Punch * Mathf.PI));
            transform.Rotate(0, 360 * Time.deltaTime, 0, Space.World);
        }

        /// 用 MaterialPropertyBlock 改颜色：多个物体共用一个材质也互不影响，且不会生成材质实例
        public void ApplyColor(Color c)
        {
            if (m_Block == null) m_Block = new MaterialPropertyBlock();
            var r = GetComponent<Renderer>();
            r.GetPropertyBlock(m_Block);
            m_Block.SetColor(ColorId, c);
            r.SetPropertyBlock(m_Block);
        }

        public void SetBaseColor(Color c) { m_Color = c; ApplyColor(c); }
    }
}
