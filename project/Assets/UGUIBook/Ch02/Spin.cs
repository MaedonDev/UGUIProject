// 配套场景用：让物体匀速旋转（案例 2.4 夹在两层 UI 之间的 3D 物体）。
using UnityEngine;

namespace UGUIBook.Ch02
{
    public class Spin : MonoBehaviour
    {
        [SerializeField] Vector3 degreesPerSecond = new Vector3(0, 60, 0);
        void Update() => transform.Rotate(degreesPerSecond * Time.deltaTime, Space.World);
    }
}
