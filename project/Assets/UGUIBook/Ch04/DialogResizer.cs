// 拖动对话框右下角的把手改变大小——用来直观感受 Sliced 九宫格“四角不变、边和中间拉伸”。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch04
{
    public class DialogResizer : MonoBehaviour, IDragHandler
    {
        [SerializeField] RectTransform m_Target;          // 对话框（轴心在左上角，这样往右下拖就是变大）
        [SerializeField] Vector2 m_MinSize = new Vector2(160, 120);
        [SerializeField] Vector2 m_MaxSize = new Vector2(1600, 900);

        public void OnDrag(PointerEventData e)
        {
            var canvas = m_Target.GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            var size = m_Target.sizeDelta + new Vector2(e.delta.x, -e.delta.y) / scale;   // 屏幕像素 → 画布单位
            m_Target.sizeDelta = Vector2.Max(m_MinSize, Vector2.Min(m_MaxSize, size));
        }
    }
}
