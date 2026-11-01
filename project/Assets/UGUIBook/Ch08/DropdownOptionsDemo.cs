// 配套场景 Ch08_Dropdown：运行时增删选项，并显示 onValueChanged 收到的值。
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class DropdownOptionsDemo : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown m_Dropdown;
        [SerializeField] Button m_Add, m_RemoveLast, m_Reset;
        [SerializeField] TMP_Text m_Log;
        int m_Counter;

        //#region options
        void Awake()
        {
            m_Dropdown.onValueChanged.AddListener(i => m_Log.text = $"onValueChanged({i})  →  {m_Dropdown.options[i].text}");
            m_Add.onClick.AddListener(() =>
            {
                // options 是普通 List：改完之后要 RefreshShownValue 才会刷新标题文字
                m_Dropdown.options.Add(new TMP_Dropdown.OptionData($"新服务器 {++m_Counter}"));
                m_Dropdown.RefreshShownValue();
                m_Log.text = $"共 {m_Dropdown.options.Count} 个选项";
            });
            m_RemoveLast.onClick.AddListener(() =>
            {
                var list = m_Dropdown.options;
                if (list.Count <= 1) return;
                list.RemoveAt(list.Count - 1);
                // 删掉的恰好是当前选中项时，value 不会自动修正，要手动夹回有效范围
                if (m_Dropdown.value >= list.Count) m_Dropdown.SetValueWithoutNotify(list.Count - 1);
                m_Dropdown.RefreshShownValue();
                m_Log.text = $"共 {list.Count} 个选项";
            });
            m_Reset.onClick.AddListener(ResetOptions);
        }

        public void ResetOptions()
        {
            m_Dropdown.ClearOptions();      // 清空并把 value 归零
            m_Dropdown.AddOptions(new List<string> { "华东一区", "华东二区", "华南一区", "华北一区", "海外区" });
            m_Counter = 0;
            m_Log.text = "已重置为 5 个选项";
        }
        //#endregion

        public void Setup(TMP_Dropdown dd, Button add, Button removeLast, Button reset, TMP_Text log)
        {
            m_Dropdown = dd; m_Add = add; m_RemoveLast = removeLast; m_Reset = reset; m_Log = log;
        }
    }
}
