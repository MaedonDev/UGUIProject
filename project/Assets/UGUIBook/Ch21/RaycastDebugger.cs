// 运行时射线调试器：实时列出鼠标（或第一根手指）下面所有被射线命中的 UI 元素，按 EventSystem 的排序从上到下，
// 并告诉你“这一下点击最终会交给谁”。按 F3 显示/隐藏。“点了没反应”时先开它看一眼，八成问题当场暴露。
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    public class RaycastDebugger : MonoBehaviour
    {
        public KeyCode toggleKey = KeyCode.F3;
        public bool visible = true;
        [SerializeField] TMP_Text m_Label;          // 放在一个排序最高、没有 GraphicRaycaster 的画布上
        [SerializeField] RectTransform m_Highlight; // 一个描边框，套在最上面的命中物体上
        [Tooltip("最多列出几个命中")]
        public int maxRows = 8;

        readonly List<RaycastResult> m_Hits = new List<RaycastResult>();
        readonly StringBuilder m_Sb = new StringBuilder();
        PointerEventData m_Ped;
        readonly Vector3[] m_Corners = new Vector3[4];

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) visible = !visible;
            m_Label.gameObject.SetActive(visible);
            if (m_Highlight != null) m_Highlight.gameObject.SetActive(visible && m_Hits.Count > 0);
            if (!visible || EventSystem.current == null) return;
            Vector2 pos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
            Inspect(EventSystem.current, pos);
        }

        //#region inspect
        public void Inspect(EventSystem es, Vector2 screenPos)
        {
            if (m_Ped == null || m_Ped.currentInputModule == null) m_Ped = new PointerEventData(es);
            m_Ped.position = screenPos;
            m_Hits.Clear();
            es.RaycastAll(m_Ped, m_Hits);       // 与 InputModule 处理点击时用的是同一个方法、同一套排序
            Show(m_Hits, screenPos, es.currentSelectedGameObject);
        }

        public void Show(List<RaycastResult> hits, Vector2 screenPos, GameObject selected)
        {
            m_Sb.Clear();
            m_Sb.Append("<b>Raycast @ (").Append((int)screenPos.x).Append(", ").Append((int)screenPos.y).Append(")</b>  命中 ")
                .Append(hits.Count).Append(" 个\n");
            for (int i = 0; i < hits.Count && i < maxRows; i++)
            {
                var h = hits[i];
                var canvas = h.gameObject.GetComponentInParent<Canvas>();
                m_Sb.Append(i == 0 ? "<color=#FF8A3D>→ " : "   ").Append(i).Append(". ").Append(h.gameObject.name)
                    .Append("  <color=#9AA3BD>").Append(h.module != null ? h.module.GetType().Name : "?")
                    .Append(" · order ").Append(canvas != null ? canvas.sortingOrder : 0)
                    .Append(" · depth ").Append(h.depth).Append("</color>");
                if (i == 0) m_Sb.Append("</color>");
                m_Sb.Append('\n');
            }
            if (hits.Count > 0)
            {
                // 点击事件会从最上面的命中物体开始往父节点找第一个实现 IPointerClickHandler 的组件
                var clickTarget = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
                var downTarget = ExecuteEvents.GetEventHandler<IPointerDownHandler>(hits[0].gameObject);
                m_Sb.Append("点击将交给：<b>").Append(clickTarget != null ? clickTarget.name : "<color=#E5484D>没人接收</color>").Append("</b>");
                if (downTarget != clickTarget) m_Sb.Append("　按下交给：").Append(downTarget != null ? downTarget.name : "无");
                m_Sb.Append('\n');
            }
            m_Sb.Append("当前选中：").Append(selected != null ? selected.name : "无");
            m_Label.text = m_Sb.ToString();
            Highlight(hits.Count > 0 ? hits[0].gameObject.transform as RectTransform : null);
        }
        //#endregion

        // 把描边框对齐到目标的屏幕矩形（描边框所在画布是 Overlay，屏幕坐标直接换算）
        void Highlight(RectTransform target)
        {
            if (m_Highlight == null) return;
            if (target == null) { m_Highlight.gameObject.SetActive(false); return; }
            var canvas = target.GetComponentInParent<Canvas>();
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.rootCanvas.worldCamera : null;
            target.GetWorldCorners(m_Corners);
            var parent = (RectTransform)m_Highlight.parent;
            var hlCanvas = m_Highlight.GetComponentInParent<Canvas>();
            var hlCam = hlCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : hlCanvas.worldCamera;
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            foreach (var c in m_Corners)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(cam, c);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, sp, hlCam, out var lp);
                min = Vector2.Min(min, lp); max = Vector2.Max(max, lp);
            }
            m_Highlight.gameObject.SetActive(true);
            m_Highlight.anchorMin = m_Highlight.anchorMax = new Vector2(0.5f, 0.5f);
            m_Highlight.pivot = new Vector2(0.5f, 0.5f);
            m_Highlight.anchoredPosition = (min + max) * 0.5f + parent.rect.center * -1f;
            m_Highlight.sizeDelta = max - min + new Vector2(12, 12);
        }
    }
}
