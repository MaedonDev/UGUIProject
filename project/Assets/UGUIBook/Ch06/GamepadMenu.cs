// 手柄/键盘菜单：进入时自动选中第一项；鼠标点空白处导致“没有选中项”时，下一次按方向键自动找回；
// 一个高亮框跟随当前选中的按钮平滑移动。
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch06
{
    public class GamepadMenu : MonoBehaviour
    {
        //#region fields
        [SerializeField] Selectable m_First;            // 默认选中项
        [SerializeField] RectTransform m_Frame;         // 跟随选中项的高亮框（和按钮同一个父节点）
        [SerializeField] float m_FollowSpeed = 20f;
        [SerializeField] TMPro.TMP_Text m_Log;           // 演示用：显示确认了哪一项
        //#endregion

        GameObject m_LastSelected;

        void Awake()
        {
            if (m_Log == null) return;
            foreach (var b in GetComponentsInChildren<Button>())
            {
                var label = b.GetComponentInChildren<TMPro.TMP_Text>().text;
                b.onClick.AddListener(() => m_Log.text = "确认：" + label);   // 鼠标点击、回车、手柄 A 键都会走 onClick
            }
        }

        //#region logic
        void OnEnable()
        {
            // 界面打开时必须主动选中一项，否则方向键/手柄没有起点
            if (EventSystem.current != null && m_First != null)
                EventSystem.current.SetSelectedGameObject(m_First.gameObject);
        }

        void Update()
        {
            var es = EventSystem.current;
            if (es == null) return;
            var cur = es.currentSelectedGameObject;
            if (cur != null && cur.transform.IsChildOf(transform)) m_LastSelected = cur;
            // 鼠标点了空白 → 选中项被清空；此时一有方向输入就把上一次的选中项找回来
            else if (cur == null && m_LastSelected != null && HasNavigationInput())
                es.SetSelectedGameObject(m_LastSelected);
            if (m_LastSelected != null) FollowTo((RectTransform)m_LastSelected.transform, Time.unscaledDeltaTime);
        }

        static bool HasNavigationInput() =>
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.5f;

        /// 高亮框向目标按钮靠拢（dt 传一个大数 = 直接贴上去，拍图时用）
        public void FollowTo(RectTransform target, float dt)
        {
            float t = 1f - Mathf.Exp(-m_FollowSpeed * dt);
            m_Frame.anchoredPosition = Vector2.Lerp(m_Frame.anchoredPosition, target.anchoredPosition, t);
            m_Frame.sizeDelta = Vector2.Lerp(m_Frame.sizeDelta, target.sizeDelta + new Vector2(24, 24), t);
        }
        //#endregion
    }
}
