using System;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region boot
    /// 游戏入口：初始化数据、打开主界面、挂上全局的事件监听
    public class Ch16Bootstrap : MonoBehaviour
    {
        [SerializeField] TMP_Text m_StackLabel;   // 可选：把界面栈实时显示出来，方便观察
        IDisposable m_SoldListener;

        void Start() => Boot();

        public void Boot()
        {
            PlayerModel.ResetDemo();
            var ui = UIManager.Instance;
            ui.PanelOpened += _ => RefreshStackLabel();
            ui.PanelClosed += _ => RefreshStackLabel();
            // 卖东西的是背包，提示的是这里：两边通过事件中心解耦，互不引用
            m_SoldListener = EventCenter.Subscribe<ItemSoldEvent>(e => UIManager.Toast($"出售成功，获得 {e.Gold} 金币"));
            ui.Open<MainPanel>();
        }

        void OnDestroy() => m_SoldListener?.Dispose();

        public void RefreshStackLabel()
        {
            if (m_StackLabel != null && UIManager.Instance != null)
                m_StackLabel.text = "界面栈：" + UIManager.Instance.DumpStack();
        }
    }
    //#endregion
}
