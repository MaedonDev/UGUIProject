// 案例 3.6：点击区域内任意位置，在点击处放一个标记，并显示屏幕坐标与本地坐标的换算。
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch03
{
    public class ClickMarker : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] RectTransform area;            // 点击区域（本例故意把轴心设在左下角）
        [SerializeField] RectTransform markerTemplate;  // 隐藏的标记模板
        [SerializeField] TMP_Text info;
        [SerializeField] int maxMarkers = 8;

        public void OnPointerClick(PointerEventData eventData)
        {
            // pressEventCamera：按下时负责这个画布的相机；Overlay 画布下它是 null，正好就是 API 要的值
            Mark(eventData.position, eventData.pressEventCamera);
        }

        //#region mark
        public RectTransform Mark(Vector2 screenPos, Camera cam)
        {
            // 屏幕坐标 → area 的本地坐标（原点 = area 的轴心）
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screenPos, cam, out Vector2 local))
                return null;   // 射线和 area 所在平面不相交（World Space 斜着看时可能发生）

            if (area.childCount > maxMarkers) Destroy(area.GetChild(1).gameObject);   // 第 0 个是模板
            var m = Instantiate(markerTemplate, area);
            m.gameObject.SetActive(true);
            m.localPosition = local;   // 用 localPosition：它和 local 一样以父节点轴心为原点，与锚点无关
            m.GetComponentInChildren<TMP_Text>().text = $"({local.x:0}, {local.y:0})";

            // 反过来：标记的世界坐标 → 屏幕坐标，应当回到点击的位置
            Vector2 back = RectTransformUtility.WorldToScreenPoint(cam, m.position);
            info.text = $"屏幕坐标  ({screenPos.x:0}, {screenPos.y:0})\n" +
                        $"本地坐标  ({local.x:0}, {local.y:0})\n" +
                        $"反算屏幕  ({back.x:0}, {back.y:0})";
            return m;
        }
        //#endregion
    }
}
