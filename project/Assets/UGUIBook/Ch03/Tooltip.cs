// 案例 3.3：跟随鼠标、永远不出屏幕的 Tooltip。
// 要点：屏幕坐标 → 画布本地坐标；放不下就翻到鼠标另一侧；最后再夹（Clamp）在画布范围内。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch03
{
    public class Tooltip : MonoBehaviour
    {
        public static Tooltip Instance { get; private set; }

        [SerializeField] RectTransform panel;     // 提示框本体，必须是画布的直接子节点
        [SerializeField] TMP_Text text;
        [SerializeField] float width = 460;
        [SerializeField] float padding = 24;
        [SerializeField] Vector2 offset = new Vector2(28, -28);   // 默认出现在鼠标右下方

        Canvas canvas;
        RectTransform canvasRect;

        void Awake()
        {
            Instance = this;
            Init();
            Hide();
        }

        void Init()
        {
            if (canvas != null) return;
            canvas = panel.GetComponentInParent<Canvas>().rootCanvas;
            canvasRect = (RectTransform)canvas.transform;
            // 锚点放在画布中心，这样 anchoredPosition 就等于“相对画布中心的本地坐标”
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        }

        void Update()
        {
            if (panel.gameObject.activeSelf) MoveTo(Input.mousePosition);
        }

        public void Show(string message)
        {
            Init();
            text.text = message;
            // 宽度固定，高度按文字内容算出来
            float h = text.GetPreferredValues(message, width - padding * 2, 0).y + padding * 2;
            panel.sizeDelta = new Vector2(width, h);
            panel.gameObject.SetActive(true);
        }

        public void Hide() => panel.gameObject.SetActive(false);

        //#region move
        public void MoveTo(Vector2 screenPos)
        {
            Init();
            // Overlay 画布传 null；Camera / World Space 画布传它的相机
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 local);

            Vector2 size = panel.rect.size;
            Rect bounds = canvasRect.rect;          // 画布自己的矩形（以画布中心为原点）
            Vector2 pivot = new Vector2(0, 1);       // 默认：框的左上角挨着鼠标
            Vector2 pos = local + offset;

            // 右边放不下 → 翻到鼠标左边；下面放不下 → 翻到鼠标上面
            if (pos.x + size.x > bounds.xMax) { pivot.x = 1; pos.x = local.x - offset.x; }
            if (pos.y - size.y < bounds.yMin) { pivot.y = 0; pos.y = local.y - offset.y; }

            // 翻转后仍可能越界（比如提示框比半个屏幕还宽），最后兜底夹一下
            pos.x = Mathf.Clamp(pos.x, bounds.xMin + size.x * pivot.x, bounds.xMax - size.x * (1 - pivot.x));
            pos.y = Mathf.Clamp(pos.y, bounds.yMin + size.y * pivot.y, bounds.yMax - size.y * (1 - pivot.y));

            panel.pivot = pivot;
            panel.anchoredPosition = pos;
        }
        //#endregion
    }
}
