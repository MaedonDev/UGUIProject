// 2.1 节配套：运行时在三种 Render Mode 之间切换同一个画布，观察它和 3D 物体的关系。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch02
{
    public class RenderModeSwitcher : MonoBehaviour
    {
        [SerializeField] Canvas target;          // 被切换的画布
        [SerializeField] Camera uiCamera;        // Camera / World Space 模式用的相机
        [SerializeField] Button overlayButton, cameraButton, worldButton;
        [SerializeField] TMP_Text label;

        void Awake()
        {
            overlayButton.onClick.AddListener(() => SetMode(RenderMode.ScreenSpaceOverlay));
            cameraButton.onClick.AddListener(() => SetMode(RenderMode.ScreenSpaceCamera));
            worldButton.onClick.AddListener(() => SetMode(RenderMode.WorldSpace));
        }

        //#region switch
        public void SetMode(RenderMode mode)
        {
            var rt = (RectTransform)target.transform;
            target.renderMode = mode;
            switch (mode)
            {
                case RenderMode.ScreenSpaceCamera:
                    target.worldCamera = uiCamera;
                    target.planeDistance = 6f;      // 让画布平面落在 3D 物体后面一点，正方体就会挡住部分 UI
                    break;
                case RenderMode.WorldSpace:
                    target.worldCamera = uiCamera;  // World Space 下它是“事件相机”，点击检测要用
                    // World Space 下尺寸、位置、缩放都由我们自己定：1600×900 个单位 × 0.004 = 6.4×3.6 米
                    rt.sizeDelta = new Vector2(1600, 900);
                    rt.localScale = Vector3.one * 0.004f;
                    rt.position = new Vector3(-1.5f, 1.8f, 1.5f);
                    rt.rotation = Quaternion.Euler(0, -25, 0);
                    break;
            }
            label.text = "当前：" + mode;
        }
        //#endregion
    }
}
