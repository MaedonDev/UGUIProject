// 隐藏/显示一整个界面最省的做法：关掉它自己的 Canvas（几何数据仍缓存在原生层），
// 同时关掉 GraphicRaycaster，避免隐藏的界面还在参与射线检测。
// 要求：界面根节点上有 Canvas（可以是嵌套 Canvas）+ GraphicRaycaster。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    //#region visibility
    [RequireComponent(typeof(Canvas))]
    public class CanvasVisibility : MonoBehaviour
    {
        Canvas m_Canvas;
        GraphicRaycaster m_Raycaster;

        void Awake()
        {
            m_Canvas = GetComponent<Canvas>();
            m_Raycaster = GetComponent<GraphicRaycaster>();
        }

        public bool Visible
        {
            get => m_Canvas != null && m_Canvas.enabled;
            set
            {
                if (m_Canvas == null) Awake();
                if (m_Canvas.enabled == value) return;
                m_Canvas.enabled = value;          // 不再渲染，但不会让子物体走 OnDisable/OnEnable
                if (m_Raycaster != null) m_Raycaster.enabled = value;
            }
        }

        public void Show() => Visible = true;
        public void Hide() => Visible = false;
    }
    //#endregion
}
