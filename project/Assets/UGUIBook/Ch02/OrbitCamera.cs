// 配套场景用：按住鼠标左键拖动，相机绕目标旋转；滚轮缩放。用来观察 World Space 画布的效果。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch02
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField] Vector3 target = new Vector3(0, 1, 0);
        [SerializeField] float distance = 9f;
        [SerializeField] float yaw = 0f, pitch = 18f;

        void Start() => Apply();

        void Update()
        {
            // 鼠标在 UI 上时不转相机：IsPointerOverGameObject 是“UI 优先”的常用判断（第 11 章细讲）
            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Input.GetMouseButton(0) && !overUI)
            {
                yaw += Input.GetAxis("Mouse X") * 4f;
                pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 3f, -10f, 75f);
            }
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 0.5f, 3f, 20f);
            Apply();
        }

        public void Apply()
        {
            var rot = Quaternion.Euler(pitch, yaw, 0);
            transform.position = target - rot * Vector3.forward * distance;
            transform.rotation = rot;
        }
    }
}
