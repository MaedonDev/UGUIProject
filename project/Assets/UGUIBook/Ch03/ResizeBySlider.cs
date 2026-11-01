// 配套场景用：拖滑块改变目标的宽高，观察锚在它身上的子元素怎么跟着走。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch03
{
    public class ResizeBySlider : MonoBehaviour
    {
        [SerializeField] RectTransform target;
        [SerializeField] Slider slider;
        [SerializeField] Vector2 minSize = new Vector2(200, 200);
        [SerializeField] Vector2 maxSize = new Vector2(600, 500);

        void Awake()
        {
            slider.onValueChanged.AddListener(t =>
            {
                var s = Vector2.Lerp(minSize, maxSize, t);
                target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, s.x);
                target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, s.y);
            });
        }
    }
}
