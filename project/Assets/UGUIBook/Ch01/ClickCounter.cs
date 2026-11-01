// 案例 1.2 方式一：给 Button 的 OnClick 列表“拖”一个方法。
// 这里的方法必须是 public、返回 void、最多一个参数（int/float/string/bool/Object），Inspector 才能在下拉框里列出来。
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch01
{
    public class ClickCounter : MonoBehaviour
    {
        [SerializeField] TMP_Text label;   // 显示次数的文字，在 Inspector 里拖进来
        int count;

        //#region inspector
        // 在 Button 的 On Click () 列表里选中这个方法
        public void Increment()
        {
            count++;
            Refresh();
        }

        // 带一个 int 参数的版本：Inspector 里会多出一个数字输入框，可以做“+5”按钮
        public void Add(int amount)
        {
            count += amount;
            Refresh();
        }
        //#endregion

        public void SetCount(int value)
        {
            count = value;
            Refresh();
        }

        void Refresh()
        {
            if (label != null) label.text = count.ToString();
        }

        void Start() => Refresh();
    }
}
