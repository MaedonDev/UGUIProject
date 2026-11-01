// 案例 2.3：让 World Space 画布始终正对相机（头顶名字、血条）。
using UnityEngine;

namespace UGUIBook.Ch02
{
    //#region billboard
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        [Tooltip("留空则用 Camera.main")]
        [SerializeField] Camera targetCamera;
        [Tooltip("只绕 Y 轴转：相机俯视时名字牌不会跟着“躺下”")]
        [SerializeField] bool lockY = false;

        // 放在 LateUpdate：等相机和角色这一帧都移动完了再转，避免抖动
        void LateUpdate() => Face();

        public void Face()
        {
            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null) return;
            // 与相机朝向一致（而不是“看向相机”）：画面里所有名字牌互相平行，不会因透视各自歪斜
            Vector3 forward = cam.transform.forward;
            if (lockY) { forward.y = 0; if (forward.sqrMagnitude < 1e-6f) return; }
            transform.rotation = Quaternion.LookRotation(forward, lockY ? Vector3.up : cam.transform.up);
        }
    }
    //#endregion
}
