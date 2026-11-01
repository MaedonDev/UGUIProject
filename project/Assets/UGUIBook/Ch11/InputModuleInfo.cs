// 11.11 节：显示当前生效的输入模块；使用 InputSystemUIInputModule 时，点击会收到 ExtendedPointerEventData，
// 里面能拿到“是哪个设备点的”（鼠标 / 触屏 / 手柄虚拟光标 / 笔）。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace UGUIBook.Ch11
{
    public class InputModuleInfo : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] TMP_Text m_Text;
        string m_Last = "（点一下这块区域）";

        void Update()
        {
            var es = EventSystem.current;
            string module = es != null && es.currentInputModule != null ? es.currentInputModule.GetType().Name : "无";
            m_Text.text = $"当前输入模块：{module}\n{m_Last}";
        }

        //#region click
        public void OnPointerClick(PointerEventData e)
        {
#if ENABLE_INPUT_SYSTEM
            if (e is ExtendedPointerEventData x)
            {
                m_Last = $"设备：{(x.device != null ? x.device.displayName : "?")}   类型：{x.pointerType}   pointerId：{x.pointerId}";
                return;
            }
#endif
            m_Last = $"普通 PointerEventData   button：{e.button}   pointerId：{e.pointerId}";
        }
        //#endregion
    }
}
