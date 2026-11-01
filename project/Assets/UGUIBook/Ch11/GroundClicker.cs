// 案例 11.5 的另一半：游戏逻辑自己读输入（不走 EventSystem）时，怎么避免“点 UI 按钮的同时角色也走过去”。
// 关键是 EventSystem.current.IsPointerOverGameObject()：指针当前悬停在任何被射线命中的物体上就返回 true。
// 注意：场景里有 PhysicsRaycaster 时，悬停在 3D 物体上也算 true——所以这里额外判断命中的是不是 UI。
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UGUIBook.Ch11
{
    public class GroundClicker : MonoBehaviour
    {
        [SerializeField] Camera m_Camera;
        [SerializeField] Transform m_Marker;
        [SerializeField] LayerMask m_Ground = ~0;

        //#region guard
        void Update()
        {
            if (!PressedThisFrame(out var pos)) return;
            if (IsOverUI(pos)) return;                          // 点在 UI 上：交给 UI，游戏世界不响应
            if (Physics.Raycast(m_Camera.ScreenPointToRay(pos), out var hit, 100, m_Ground) && hit.collider.GetComponent<Clickable3D>() == null)
                m_Marker.position = hit.point + Vector3.up * 0.01f;
        }

        static readonly System.Collections.Generic.List<RaycastResult> s_Hits = new System.Collections.Generic.List<RaycastResult>();

        /// 只关心 UI：用 EventSystem 做一次 RaycastAll，看第一个结果是不是来自 GraphicRaycaster
        public static bool IsOverUI(Vector2 screenPos)
        {
            var es = EventSystem.current;
            if (es == null) return false;
            var data = new PointerEventData(es) { position = screenPos };
            es.RaycastAll(data, s_Hits);
            return s_Hits.Count > 0 && s_Hits[0].module is UnityEngine.UI.GraphicRaycaster;
        }
        //#endregion

        static bool PressedThisFrame(out Vector2 pos)
        {
#if ENABLE_INPUT_SYSTEM
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            { pos = Pointer.current.position.ReadValue(); return true; }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
#endif
            pos = default;
            return false;
        }
    }
}
