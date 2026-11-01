// 让 TMP 文本里的 <link="id">…</link> 可以悬停变色、点击回调。
// 注意：文本组件的 Raycast Target 必须勾上，否则收不到任何指针事件。
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch05
{
    [RequireComponent(typeof(TMP_Text))]
    public class LinkHandler : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
    {
        //#region fields
        [Serializable] public class LinkEvent : UnityEvent<string, Vector2> { }   // 参数：link id、点击的屏幕坐标

        [SerializeField] Color32 m_HoverColor = new Color32(0xFF, 0xE0, 0x66, 0xFF);
        public LinkEvent onLinkClick = new LinkEvent();
        //#endregion

        TMP_Text m_Text;
        int m_Hover = -1;
        readonly Color32[] m_Saved = new Color32[256 * 4];    // 悬停前的原始顶点色，用于还原

        TMP_Text Text => m_Text != null ? m_Text : (m_Text = GetComponent<TMP_Text>());

        //#region events
        public void OnPointerClick(PointerEventData e)
        {
            int i = TMP_TextUtilities.FindIntersectingLink(Text, e.position, e.pressEventCamera);
            if (i < 0) return;
            onLinkClick.Invoke(Text.textInfo.linkInfo[i].GetLinkID(), e.position);
        }

        public void OnPointerMove(PointerEventData e) =>
            SetHover(TMP_TextUtilities.FindIntersectingLink(Text, e.position, e.enterEventCamera));

        public void OnPointerExit(PointerEventData e) => SetHover(-1);
        //#endregion

        //#region hover
        /// 改顶点色而不是改文字：不触发重新排版，只上传一次颜色数据
        public void SetHover(int linkIndex)
        {
            Hook(true);
            if (linkIndex == m_Hover) return;
            if (m_Hover >= 0) Paint(m_Hover, null);
            m_Hover = linkIndex;
            if (m_Hover >= 0) Paint(m_Hover, m_HoverColor);
            Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        // 文本因为改字、改尺寸等原因重新生成网格后，顶点色会被重置，这时把悬停色补回去
        bool m_Hooked;
        void OnEnable() => Hook(true);
        void OnDisable() { Hook(false); m_Hover = -1; }

        void Hook(bool on)
        {
            if (on == m_Hooked) return;
            m_Hooked = on;
            if (on) TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            else TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        void OnTextChanged(Object obj)
        {
            if (this == null || obj != Text || m_Hover < 0) return;   // 组件已销毁（编辑器下没走 OnDisable）时直接忽略
            Paint(m_Hover, m_HoverColor);          // 网格是新的，Paint 会顺便重新记录原始颜色
            Text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        void Paint(int linkIndex, Color32? color)
        {
            var info = Text.textInfo;
            if (linkIndex >= info.linkCount) return;
            var link = info.linkInfo[linkIndex];
            for (int j = 0; j < link.linkTextLength && j < 256; j++)
            {
                var ch = info.characterInfo[link.linkTextfirstCharacterIndex + j];
                if (!ch.isVisible) continue;                       // 空格没有顶点
                var colors = info.meshInfo[ch.materialReferenceIndex].colors32;
                for (int v = 0; v < 4; v++)
                {
                    int vi = ch.vertexIndex + v;
                    if (color.HasValue) { m_Saved[j * 4 + v] = colors[vi]; colors[vi] = color.Value; }
                    else colors[vi] = m_Saved[j * 4 + v];
                }
            }
        }
        //#endregion
    }
}
