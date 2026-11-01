// 案例 2.1：在角落实时显示“屏幕多大、画布被缩放了多少、画布逻辑尺寸是多少”。
// 调适配时把它挂在画布上，改 Game 视图分辨率就能看到数字怎么变。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch02
{
    [ExecuteAlways]
    public class ScreenInfo : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] TMP_Text output;

        void Update() => Refresh();

        //#region info
        public void Refresh()
        {
            if (canvas == null || output == null) return;
            var scaler = canvas.GetComponent<CanvasScaler>();
            Vector2 screen = canvas.renderingDisplaySize;            // 画布实际渲染到的像素尺寸
            Vector2 logical = ((RectTransform)canvas.transform).rect.size; // 画布的逻辑尺寸（布局用的单位）
            output.text =
                $"屏幕 {screen.x:0}×{screen.y:0}（宽高比 {screen.x / screen.y:0.00}）\n" +
                $"scaleFactor {canvas.scaleFactor:0.000}   Match {scaler.matchWidthOrHeight:0.00}\n" +
                $"画布逻辑尺寸 {logical.x:0}×{logical.y:0}";
        }
        //#endregion
    }
}
