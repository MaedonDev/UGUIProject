// 案例 11.3：虚拟摇杆（动态位置）。挂在一块透明的触摸区域上（例如屏幕左半边，Image alpha=0 但 Raycast Target 打开）。
// 手指按下的位置就是摇杆中心；拖动时摇杆头被限制在半径以内；输出 value（-1~1）。
// 只认第一根按下的手指（pointerId），另一只手在右半屏按技能按钮不会干扰摇杆。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform m_Base;        // 底座（圆环）
        [SerializeField] RectTransform m_Knob;        // 摇杆头，是 m_Base 的子物体
        [SerializeField] float m_Radius = 140;
        [SerializeField, Range(0, 0.5f)] float m_DeadZone = 0.1f;
        [SerializeField] bool m_Dynamic = true;       // false = 固定位置的摇杆

        RectTransform m_Area;
        Vector2 m_HomePos;                            // 松手后底座回到的位置
        int m_PointerId = int.MinValue;               // 当前控制摇杆的手指；MinValue 表示没有

        /// 当前输出，长度 0~1
        public Vector2 value { get; private set; }
        public bool isPressed => m_PointerId != int.MinValue;

        void Awake()
        {
            m_Area = (RectTransform)transform;
            m_HomePos = m_Base.anchoredPosition;
        }

        //#region input
        public void OnPointerDown(PointerEventData e)
        {
            if (isPressed) return;                    // 已经有手指在控制了
            m_PointerId = e.pointerId;
            if (m_Dynamic && ToLocal(e, out var local))
                m_Base.localPosition = local;         // 底座跳到手指下面（m_Base 与触摸区同一父空间时可直接用 localPosition）
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != m_PointerId || !ToLocal(e, out var local)) return;
            Vector2 delta = local - (Vector2)m_Base.localPosition;
            SetKnob(Vector2.ClampMagnitude(delta, m_Radius) / m_Radius);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != m_PointerId) return;
            m_PointerId = int.MinValue;
            m_Base.anchoredPosition = m_HomePos;
            SetKnob(Vector2.zero);
        }

        bool ToLocal(PointerEventData e, out Vector2 local) =>
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)m_Base.parent, e.position, e.pressEventCamera, out local);
        //#endregion

        /// v：摇杆头相对位置（长度 0~1）。死区内输出 0，死区外重新映射到 0~1，手感更线性
        public void SetKnob(Vector2 v)
        {
            m_Knob.anchoredPosition = v * m_Radius;
            float m = v.magnitude;
            value = m < m_DeadZone ? Vector2.zero : v.normalized * ((m - m_DeadZone) / (1 - m_DeadZone));
        }
    }
}
