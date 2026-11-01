// 案例 1.2 方式二：在代码里 AddListener。按钮、文字都通过 [SerializeField] 字段引用，
// 监听在 Awake 里加、在 OnDestroy 里移除——这是最常见、也最不容易出错的写法。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch01
{
    public class ClickCounterByCode : MonoBehaviour
    {
        [SerializeField] Button addButton;
        [SerializeField] Button resetButton;
        [SerializeField] TMP_Text label;
        int count;

        //#region code
        void Awake()
        {
            addButton.onClick.AddListener(OnAddClicked);
            // 简单逻辑也可以直接写成 Lambda
            resetButton.onClick.AddListener(() => SetCount(0));
        }

        void OnDestroy()
        {
            // 按钮和本脚本同生共死时可以不移除；但养成习惯：谁加的监听，谁负责移除
            addButton.onClick.RemoveListener(OnAddClicked);
            resetButton.onClick.RemoveAllListeners();
        }

        void OnAddClicked()
        {
            SetCount(count + 1);
        }
        //#endregion

        public void SetCount(int value)
        {
            count = value;
            label.text = count.ToString();
        }

        void Start() => SetCount(count);
    }
}
