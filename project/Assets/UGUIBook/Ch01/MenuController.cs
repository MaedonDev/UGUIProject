// 案例 1.3 / 1.4：主菜单 ↔ 设置页切换、退出确认框。
// 所有引用都用 [SerializeField] 在 Inspector 里连好，监听在 Awake 里加。
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch01
{
    public class MenuController : MonoBehaviour
    {
        [Header("页面")]
        [SerializeField] GameObject mainPage;
        [SerializeField] GameObject settingsPage;
        [SerializeField] GameObject quitDialog;

        [Header("主菜单按钮")]
        [SerializeField] Button startButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button quitButton;

        [Header("设置页 / 确认框按钮")]
        [SerializeField] Button backButton;
        [SerializeField] Button quitYesButton;
        [SerializeField] Button quitNoButton;

        [SerializeField] TMP_Text hint;

        //#region awake
        void Awake()
        {
            startButton.onClick.AddListener(OnStart);
            settingsButton.onClick.AddListener(() => ShowPage(settingsPage));
            backButton.onClick.AddListener(() => ShowPage(mainPage));
            quitButton.onClick.AddListener(() => SetDialog(true));
            quitNoButton.onClick.AddListener(() => SetDialog(false));
            quitYesButton.onClick.AddListener(QuitGame.Quit);

            // 运行时的初始状态由代码保证，不依赖场景里保存时“恰好”是什么样
            ShowPage(mainPage);
            SetDialog(false);
        }
        //#endregion

        //#region pages
        /// 显示一个页面、隐藏其它页面。页面多了可以改成数组或字典
        public void ShowPage(GameObject page)
        {
            mainPage.SetActive(page == mainPage);
            settingsPage.SetActive(page == settingsPage);
        }

        public void SetDialog(bool visible)
        {
            quitDialog.SetActive(visible);
        }
        //#endregion

        //#region back
        void Update()
        {
            // Android 的返回键在 Unity 里映射为 Escape；PC 上按 Esc 也能测试
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (quitDialog.activeSelf) SetDialog(false);           // 先关最上层的弹窗
            else if (settingsPage.activeSelf) ShowPage(mainPage);  // 再退回上一页
            else SetDialog(true);                                  // 已在首页：询问是否退出
        }
        //#endregion

        void OnStart()
        {
            // 真实项目里这里会 SceneManager.LoadScene("Game")；示例只给个提示
            hint.text = "开始游戏！（示例里不切换场景）";
        }
    }
}
