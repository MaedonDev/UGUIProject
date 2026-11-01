// 用 RawImage 的 UV Rect 做无限滚动背景：网格不动，只移动采样坐标，纹理 Wrap Mode 必须是 Repeat。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    //#region main
    [RequireComponent(typeof(RawImage))]
    public class UVScroller : MonoBehaviour
    {
        [SerializeField] Vector2 m_Speed = new Vector2(0.05f, 0f);   // 每秒移动多少个“纹理宽度”
        [SerializeField] bool m_KeepAspect = true;                     // 按控件宽高比调整 UV 宽度，避免纹理被拉伸
        [SerializeField] bool m_UnscaledTime;                          // 暂停（timeScale=0）时背景是否继续动

        RawImage m_Raw;
        RawImage Raw => m_Raw != null ? m_Raw : (m_Raw = GetComponent<RawImage>());

        void OnEnable() => FitAspect();
        void OnRectTransformDimensionsChange() => FitAspect();

        void Update() => Scroll(m_UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);

        public void Scroll(float dt)
        {
            var r = Raw.uvRect;
            r.x = Mathf.Repeat(r.x + m_Speed.x * dt, 1f);   // 取模防止浮点数越加越大、精度变差
            r.y = Mathf.Repeat(r.y + m_Speed.y * dt, 1f);
            Raw.uvRect = r;
        }

        /// UV 宽度 = 控件宽高比 / 纹理宽高比：控件比纹理“更宽”时，就多重复几次
        public void FitAspect()
        {
            if (!m_KeepAspect || Raw.texture == null) return;
            var size = ((RectTransform)transform).rect.size;
            if (size.x <= 0 || size.y <= 0) return;
            float texAspect = (float)Raw.texture.width / Raw.texture.height;
            var r = Raw.uvRect;
            r.width = (size.x / size.y) / texAspect * r.height;
            Raw.uvRect = r;
        }
    }
    //#endregion
}
