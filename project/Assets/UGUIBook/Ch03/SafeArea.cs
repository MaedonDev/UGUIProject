// 案例 3.4：刘海屏 / 挖孔屏 / 圆角屏适配。挂在画布下一个铺满全屏的节点上，
// 它会把自己的锚点设成 Screen.safeArea 对应的比例，所有子节点就自动避开了不安全区域。
using UnityEngine;

namespace UGUIBook.Ch03
{
    //#region safearea
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        [Tooltip("编辑器里模拟安全区（归一化坐标），方便没有真机时调界面")]
        [SerializeField] bool simulate;
        [SerializeField] Rect simulatedArea = new Rect(0.056f, 0.054f, 0.888f, 0.946f);

        Rect lastArea;
        Vector2Int lastScreen;

        void OnEnable() => Apply(true);
        void Update() => Apply(false);   // 旋转屏幕、折叠屏展开时安全区会变，所以每帧检查（只比较，不重复设置）

        public void Apply(bool force)
        {
            Rect area;
            if (simulate)
                area = simulatedArea;
            else
            {
                var screen = new Vector2(Screen.width, Screen.height);
                Rect safe = Screen.safeArea;   // 单位是像素，原点在屏幕左下角
                area = new Rect(safe.x / screen.x, safe.y / screen.y, safe.width / screen.x, safe.height / screen.y);
            }
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (!force && area == lastArea && screenSize == lastScreen) return;
            lastArea = area; lastScreen = screenSize;

            var rt = (RectTransform)transform;
            rt.anchorMin = area.min;   // 锚点 = 安全区在屏幕中的比例
            rt.anchorMax = area.max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;   // 四边紧贴锚点
        }
    }
    //#endregion
}
