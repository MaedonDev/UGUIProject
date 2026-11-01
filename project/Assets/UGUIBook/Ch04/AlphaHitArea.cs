// 让 Image 只在“不透明的像素”上响应点击。
// alphaHitTestMinimumThreshold 不会被序列化（Image.cs 源码注释：Not serialized），所以必须在运行时用代码设置。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    //#region main
    [RequireComponent(typeof(Image))]
    public class AlphaHitArea : MonoBehaviour
    {
        [Range(0f, 1f)]
        [SerializeField] float m_Threshold = 0.5f;   // alpha 低于它的像素点不中

        void Awake() => Apply();

        public void Apply()
        {
            var img = GetComponent<Image>();
            var tex = img.sprite != null ? img.sprite.texture : null;
            if (tex == null || !tex.isReadable)
            {
                // 纹理没勾 Read/Write 时 setter 会直接抛异常，这里提前给出可读的提示
                Debug.LogWarning($"{name}: 纹理 {tex?.name} 没有开启 Read/Write，像素级点击检测无法使用", this);
                return;
            }
            img.alphaHitTestMinimumThreshold = m_Threshold;
        }
    }
    //#endregion
}
