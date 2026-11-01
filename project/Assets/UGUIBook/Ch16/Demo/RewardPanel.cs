using UGUIBook.Core;

namespace UGUIBook.Ch16
{
    public class RewardArgs
    {
        public string Title;
        public string Icon;
        public string Desc;
        public int Gold;
    }

    /// 奖励/公告弹窗（Popup 层）：用来演示弹窗队列
    public partial class RewardPanel : UIPanel
    {
        RewardArgs m_Args;

        protected override void OnCreate() => btn_Claim.onClick.AddListener(Claim);

        protected override void OnOpen(object args)
        {
            m_Args = args as RewardArgs ?? new RewardArgs { Title = "奖励", Icon = "gift", Desc = "" };
            txt_Title.text = m_Args.Title;
            img_Icon.sprite = UISkin.Sprite(m_Args.Icon);
            txt_Desc.text = m_Args.Desc;
            // 打开之后队列里可能还会继续进新弹窗，所以订阅队列变化，而不是只在打开时读一次
            var ui = UIManager.Instance;
            ui.QueueChanged += RefreshQueueLabel;
            AddToOpenScope(new Disposer(() => ui.QueueChanged -= RefreshQueueLabel));
            RefreshQueueLabel();
            txt_Claim.text = m_Args.Gold > 0 ? "领取" : "知道了";
        }

        void RefreshQueueLabel()
        {
            int left = UIManager.Instance.QueuedCount;
            txt_Queue.text = left > 0 ? $"后面还有 {left} 个弹窗排队" : "这是最后一个";
        }

        void Claim()
        {
            var args = m_Args;
            Close();   // 关闭时 UIManager 会自动弹出队列里的下一个
            if (args != null && args.Gold > 0)
            {
                PlayerModel.Instance.Gold.Value += args.Gold;
                UIManager.Toast($"获得 {args.Gold} 金币");
            }
        }
    }
}
