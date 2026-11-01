// 配套场景 Ch07_ToggleGroup：左边多选、右边单选，下方实时显示当前选择，并可切换 allowSwitchOff。
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class ToggleGroupDemo : MonoBehaviour
    {
        [SerializeField] Toggle[] m_Checks;          // 多选：各自独立
        [SerializeField] ToggleGroup m_Group;        // 单选：同组互斥
        [SerializeField] Toggle m_AllowSwitchOff;    // 切换 m_Group.allowSwitchOff
        [SerializeField] TMP_Text m_Status;

        //#region read
        void Awake()
        {
            foreach (var t in m_Checks) t.onValueChanged.AddListener(_ => Refresh());
            // ToggleGroup 自己没有“选中项变化”事件，只能监听组里每个 Toggle
            foreach (var t in m_Group.GetComponentsInChildren<Toggle>()) t.onValueChanged.AddListener(_ => Refresh());
            m_AllowSwitchOff.onValueChanged.AddListener(on => { m_Group.allowSwitchOff = on; Refresh(); });
        }

        void Start() => Refresh();

        public void Refresh()
        {
            var picked = m_Checks.Where(t => t.isOn).Select(Label);
            // GetFirstActiveToggle：组里当前勾上的那个（allowSwitchOff 时可能为 null）
            var one = m_Group.GetFirstActiveToggle();
            m_Status.text = $"多选：{(picked.Any() ? string.Join("、", picked) : "（无）")}    " +
                            $"单选：{(one != null ? Label(one) : "（无）")}    allowSwitchOff = {m_Group.allowSwitchOff}";
        }
        //#endregion

        static string Label(Toggle t) => t.GetComponentInChildren<TMP_Text>().text;

        public void Setup(Toggle[] checks, ToggleGroup group, Toggle allowSwitchOff, TMP_Text status)
        {
            m_Checks = checks; m_Group = group; m_AllowSwitchOff = allowSwitchOff; m_Status = status;
        }
    }
}
