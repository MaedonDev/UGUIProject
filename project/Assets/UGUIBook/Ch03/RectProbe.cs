// 3.1 节配套：实时显示一个 RectTransform 的全部关键数值。编辑器里不运行也会刷新，
// 拖动 Scene 视图里的锚点、改 Inspector，右边的数字跟着变——最快的理解方式。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch03
{
    [ExecuteAlways]
    public class RectProbe : MonoBehaviour
    {
        [SerializeField] RectTransform target;
        [SerializeField] TMP_Text output;

        void Update() => Refresh();

        static string F(Vector2 v) => $"({v.x:0.##}, {v.y:0.##})";

        public void Refresh()
        {
            if (target == null || output == null) return;
            var r = target.rect;
            output.text =
                $"anchorMin        {F(target.anchorMin)}\n" +
                $"anchorMax        {F(target.anchorMax)}\n" +
                $"pivot            {F(target.pivot)}\n" +
                $"anchoredPosition {F(target.anchoredPosition)}\n" +
                $"sizeDelta        {F(target.sizeDelta)}\n" +
                $"offsetMin        {F(target.offsetMin)}\n" +
                $"offsetMax        {F(target.offsetMax)}\n" +
                $"<color=#FFB27A>rect   x {r.x:0.##}  y {r.y:0.##}  w {r.width:0.##}  h {r.height:0.##}</color>";
        }
    }
}
