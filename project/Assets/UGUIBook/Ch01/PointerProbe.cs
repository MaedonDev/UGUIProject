// 案例 1.6：显示“鼠标下面都有谁”。点按钮没反应时，先看看是谁挡在了最上面。
// EventSystem.RaycastAll 会询问场景里所有 Raycaster，按“谁在最上面”排好序返回。
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UGUIBook.Ch01
{
    public class PointerProbe : MonoBehaviour
    {
        [SerializeField] TMP_Text output;
        readonly List<RaycastResult> results = new List<RaycastResult>();

        void Update()
        {
            if (EventSystem.current == null)
            {
                output.text = "<color=#E5484D>场景里没有 EventSystem！</color>";
                return;
            }
            Probe(Input.mousePosition);
        }

        //#region probe
        public void Probe(Vector2 screenPos)
        {
            results.Clear();
            var es = EventSystem.current;
            if (es != null)
            {
                var data = new PointerEventData(es) { position = screenPos };
                es.RaycastAll(data, results);   // 结果已排序：第 0 个就是会收到点击的那个
            }
            else
            {
                CollectInEditMode(screenPos);
            }

            var sb = new StringBuilder("鼠标下的物体（上 → 下）：\n");
            for (int i = 0; i < results.Count && i < 6; i++)
            {
                var go = results[i].gameObject;
                sb.Append(i == 0 ? "<color=#E5484D><b>→ " : "   ")
                  .Append(go.name)
                  .Append(i == 0 ? "  ← 收到点击</b></color>" : "")
                  .Append('\n');
            }
            if (results.Count == 0) sb.Append("（没有任何 UI）");
            output.text = sb.ToString();
        }
        //#endregion

        // 编辑器非运行状态下 EventSystem 没有启用（配图就是这样拍的），这里用 GraphicRaycaster 的核心判断手工模拟：
        // 开着 Raycast Target、矩形包含该点的 Graphic，按层级顺序越靠后越在上面。只用于截图/调试，不考虑多画布排序
        void CollectInEditMode(Vector2 screenPos)
        {
            var canvas = GetComponentInParent<Canvas>().rootCanvas;
            var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var graphics = canvas.GetComponentsInChildren<Graphic>();
            for (int i = graphics.Length - 1; i >= 0; i--)
            {
                var g = graphics[i];
                if (!g.raycastTarget || !g.isActiveAndEnabled) continue;
                if (!RectTransformUtility.RectangleContainsScreenPoint(g.rectTransform, screenPos, cam)) continue;
                results.Add(new RaycastResult { gameObject = g.gameObject });
            }
        }
    }
}
