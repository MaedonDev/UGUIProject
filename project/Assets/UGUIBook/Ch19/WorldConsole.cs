// World Space 控制台：3D 场景里的一块屏幕，按钮改灯的颜色，滑块调亮度。
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UGUIBook.Ch19
{
    public class WorldConsole : MonoBehaviour
    {
        [SerializeField] Light m_Lamp;
        [SerializeField] Renderer m_Bulb;
        [SerializeField] Button[] m_ColorButtons;
        [SerializeField] Color[] m_Colors;
        [SerializeField] Slider m_Intensity;
        [SerializeField] TMP_Text m_Status;

        //#region console
        void Awake()
        {
            for (int i = 0; i < m_ColorButtons.Length; i++)
            {
                int k = i;
                m_ColorButtons[i].onClick.AddListener(() => SetColor(k));
            }
            m_Intensity.onValueChanged.AddListener(SetIntensity);
        }

        public void SetColor(int index)
        {
            var c = m_Colors[index];
            m_Lamp.color = c;
            // renderer.material 会复制一份材质实例，不会改到资源文件
            if (Application.isPlaying) m_Bulb.material.color = c; else m_Bulb.sharedMaterial.color = c;
            if (m_Status != null) m_Status.text = $"灯光颜色：<color=#{ColorUtility.ToHtmlStringRGB(c)}>■</color>  亮度 {m_Lamp.intensity:0.0}";
        }

        //#endregion

        public void SetIntensity(float v)
        {
            m_Lamp.intensity = v;
            if (m_Status != null) m_Status.text = $"亮度 {v:0.0}";
        }
    }
}
