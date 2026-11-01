// 列表依次入场：每一项从右侧滑入并淡入，相邻两项错开 stagger 秒。
// 注意：列表项本身的位置归 LayoutGroup 管，动画要做在它的子节点（Body）上，否则布局一刷新就被拉回去。
using UnityEngine;

namespace UGUIBook.Ch15
{
    public class ListEntrance : MonoBehaviour
    {
        //#region fields
        [SerializeField] float m_Offset = 160;      // 从多远的地方滑进来
        [SerializeField] float m_Duration = 0.4f;
        [SerializeField] float m_Stagger = 0.07f;   // 相邻两项的间隔
        [SerializeField] bool m_PlayOnEnable = true;
        //#endregion

        void OnEnable() { if (m_PlayOnEnable && Application.isPlaying) Play(); }

        //#region build
        public Sequence Build()
        {
            var seq = new Sequence();
            int i = 0;
            foreach (RectTransform item in transform)
            {
                if (!item.gameObject.activeSelf || item.childCount == 0) continue;
                var body = (RectTransform)item.GetChild(0);          // 约定：第一个子节点是可动的 Body
                var cg = body.GetComponent<CanvasGroup>();
                if (cg == null) cg = body.gameObject.AddComponent<CanvasGroup>();
                float start = i * m_Stagger;
                seq.Insert(start, body.AnchorPos(Vector2.zero, m_Duration).From(new Vector2(m_Offset, 0)).SetEase(Ease.OutCubic));
                seq.Insert(start, cg.Alpha(1, m_Duration * 0.8f).From(0).SetEase(Ease.OutQuad));
                i++;
            }
            return seq;
        }
        //#endregion

        public void Play() => Build().Play();
    }
}
