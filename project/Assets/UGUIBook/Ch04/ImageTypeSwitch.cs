// 点按钮在 Simple / Sliced / Tiled 之间切换目标 Image 的类型，对比效果
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch04
{
    [RequireComponent(typeof(Button))]
    public class ImageTypeSwitch : MonoBehaviour
    {
        [SerializeField] Image m_Target;
        [SerializeField] TMP_Text m_Label;
        static readonly Image.Type[] Types = { Image.Type.Sliced, Image.Type.Simple, Image.Type.Tiled };
        int m_Index;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Next);
            Show();
        }

        void Next() { m_Index = (m_Index + 1) % Types.Length; Show(); }

        void Show()
        {
            m_Target.type = Types[m_Index];
            m_Label.text = "Image Type：" + Types[m_Index];
        }
    }
}
