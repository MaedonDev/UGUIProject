using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region main
    /// 主界面：栈底的界面，返回键弹“退出游戏”确认
    public partial class MainPanel : UIPanel
    {
        protected override void OnCreate()
        {
            btn_Bag.onClick.AddListener(() => UIManager.Instance.Open<BagPanel>());
            btn_Mail.onClick.AddListener(ClaimMails);
            btn_Settings.onClick.AddListener(() => UIManager.Toast("设置界面留作练习：照着 BagPanel 做一个 Popup 层的界面"));
            btn_Battle.onClick.AddListener(() => UIManager.Toast("战斗玩法未开放，敬请期待"));
        }

        protected override void OnOpen(object args)
        {
            var m = PlayerModel.Instance;
            // 绑定登记到 OpenScope，界面关闭时自动解绑
            AddToOpenScope(txt_Name.BindText(m.Name));
            AddToOpenScope(txt_Level.BindText(m.Level, v => "Lv." + v));
            AddToOpenScope(txt_Gold.BindText(m.Gold, v => v.ToString("N0")));
            AddToOpenScope(txt_Diamond.BindText(m.Diamond, v => v.ToString("N0")));
        }

        // 演示弹窗队列：一次塞进三个弹窗，它们会按优先级依次弹出
        void ClaimMails()
        {
            var ui = UIManager.Instance;
            ui.EnqueuePopup<RewardPanel>(new RewardArgs { Title = "每日签到", Icon = "item_coin", Desc = "第 3 天签到奖励：金币 ×200", Gold = 200 });
            ui.EnqueuePopup<RewardPanel>(new RewardArgs { Title = "系统公告", Icon = "item_scroll", Desc = "今晚 22:00 停服维护 30 分钟" });
            ui.EnqueuePopup<RewardPanel>(new RewardArgs { Title = "首充礼包", Icon = "item_gem", Desc = "首充任意金额送蓝宝石 ×2" }, priority: 10);
        }

        public override bool OnBack()
        {
            UIManager.Confirm("确定要退出游戏吗？", Quit, title: "退出");
            return true;
        }

        static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    //#endregion
}
