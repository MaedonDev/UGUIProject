// 案例 11.3 的演示：用摇杆移动一个“角色”图标（在 UI 里模拟游戏场景），并显示摇杆输出。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch11
{
    public class JoystickPlayer : MonoBehaviour
    {
        [SerializeField] VirtualJoystick m_Stick;
        [SerializeField] RectTransform m_Player;
        [SerializeField] RectTransform m_Arena;
        [SerializeField] TMP_Text m_Readout;
        [SerializeField] float m_Speed = 600;

        void Update()
        {
            Vector2 v = m_Stick.value;
            if (v != Vector2.zero)
            {
                Vector2 p = m_Player.anchoredPosition + v * m_Speed * Time.deltaTime;
                Vector2 half = m_Arena.rect.size * 0.5f - m_Player.rect.size * 0.5f;
                m_Player.anchoredPosition = new Vector2(Mathf.Clamp(p.x, -half.x, half.x), Mathf.Clamp(p.y, -half.y, half.y));
                m_Player.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg - 90);
            }
            if (m_Readout != null) m_Readout.text = $"value = ({v.x:0.00}, {v.y:0.00})";
        }
    }
}
