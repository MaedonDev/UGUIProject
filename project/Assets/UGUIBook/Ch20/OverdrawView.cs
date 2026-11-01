// 运行时 UI Overdraw 视图：按 F2 把场景里所有 UI 图形的材质换成 Overdraw 材质（每画一层加一点亮度），再按一次恢复。
// 越亮 = 同一个像素被画的次数越多。Scene 视图的 Overdraw 模式只在编辑器里有，这个在真机 Development 包里也能看。
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch20
{
    public class OverdrawView : MonoBehaviour
    {
        [Tooltip("Shader 用 UGUIBook/Ch20/Overdraw")]
        public Material overdrawMaterial;
        public KeyCode toggleKey = KeyCode.F2;
        [Tooltip("打开时显示的黑色底板（放在一个排序最低的 Overlay 画布上，挡住 3D 场景）")]
        public GameObject blackBackdrop;

        //#region apply
        readonly Dictionary<Graphic, Material> m_Saved = new Dictionary<Graphic, Material>();

        public bool On
        {
            get => m_Saved.Count > 0;
            set { if (value != On) { if (value) Apply(); else Restore(); } }
        }

        void Apply()
        {
            foreach (var g in FindObjectsOfType<Graphic>())
            {
                if (blackBackdrop != null && g.transform.IsChildOf(blackBackdrop.transform)) continue;
                if (g is TMP_Text t)
                {
                    // TMP 的材质走 fontSharedMaterial；换成 Overdraw 材质后每个字的四边形都会被计数
                    m_Saved[g] = t.fontSharedMaterial;
                    t.fontSharedMaterial = overdrawMaterial;
                }
                else
                {
                    // 没有自定义材质时 material 返回的是 defaultMaterial，恢复时设回 null 即可
                    m_Saved[g] = g.material == g.defaultMaterial ? null : g.material;
                    g.material = overdrawMaterial;
                }
            }
            if (blackBackdrop != null) blackBackdrop.SetActive(true);
        }

        void Restore()
        {
            foreach (var kv in m_Saved)
            {
                if (kv.Key == null) continue;
                if (kv.Key is TMP_Text t) t.fontSharedMaterial = kv.Value;
                else kv.Key.material = kv.Value;
            }
            m_Saved.Clear();
            if (blackBackdrop != null) blackBackdrop.SetActive(false);
        }
        //#endregion

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) On = !On;
        }

        void OnDisable() => On = false;
    }
}
