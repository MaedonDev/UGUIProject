// 配套场景 Ch08_Stepper：购买数量变化时刷新总价。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch08
{
    public class StepperDemo : MonoBehaviour
    {
        [SerializeField] NumberStepper m_Stepper;
        [SerializeField] TMP_Text m_Total;
        [SerializeField] int m_UnitPrice = 12;

        void Awake() => m_Stepper.onValueChanged.AddListener(Refresh);
        public void Refresh(int count) => m_Total.text = $"合计 <color=#E5484D><b>¥{count * m_UnitPrice}</b></color>";

        public void Setup(NumberStepper stepper, TMP_Text total, int price) { m_Stepper = stepper; m_Total = total; m_UnitPrice = price; }
    }
}
