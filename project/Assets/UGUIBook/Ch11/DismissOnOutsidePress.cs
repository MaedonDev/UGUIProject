// 案例 11.4 的第二种做法：不放遮罩，只要“按在我外面”就收起（下拉菜单、气泡提示常用，不挡别的按钮）。
// 在 Update 里轮询指针，用 RectTransformUtility 判断按下点是否落在自己的矩形内。
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UGUIBook.Ch11
{
    public class DismissOnOutsidePress : MonoBehaviour
    {
        [SerializeField] Camera m_UICamera;   // Overlay 画布留空

        //#region poll
        void Update()
        {
            if (!PressedThisFrame(out var pos)) return;
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.RectangleContainsScreenPoint(rt, pos, m_UICamera))
                gameObject.SetActive(false);
        }

        // 同时兼容旧 Input Manager 和新 Input System（工程的 Active Input Handling = Both）
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
        //#endregion
    }
}
