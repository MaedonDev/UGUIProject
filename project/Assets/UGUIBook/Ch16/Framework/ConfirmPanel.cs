using System;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region confirm
    public class ConfirmArgs
    {
        public string Title = "提示";
        public string Message;
        public string OkText = "确定";
        public string CancelText;          // 为空 = 只有一个按钮
        public Action OnOk;
        public Action OnCancel;
    }

    /// 通用确认框（Popup 层，常驻）。用法：UIManager.Confirm("确定出售吗？", () => Sell());
    public partial class ConfirmPanel : UIPanel
    {
        ConfirmArgs m_Args;

        protected override void OnCreate()
        {
            btn_Ok.onClick.AddListener(() => Finish(true));
            btn_Cancel.onClick.AddListener(() => Finish(false));
        }

        protected override void OnOpen(object args)
        {
            m_Args = args as ConfirmArgs ?? new ConfirmArgs { Message = "（没有传入 ConfirmArgs）" };
            txt_Title.text = m_Args.Title;
            txt_Message.text = m_Args.Message;
            txt_Ok.text = m_Args.OkText;
            bool hasCancel = !string.IsNullOrEmpty(m_Args.CancelText);
            btn_Cancel.gameObject.SetActive(hasCancel);
            if (hasCancel) txt_Cancel.text = m_Args.CancelText;
        }

        // 返回键 = 点取消
        public override bool OnBack()
        {
            Finish(false);
            return true;
        }

        void Finish(bool ok)
        {
            var args = m_Args;
            m_Args = null;
            if (args == null) return;     // 防止同一帧连点两次
            Close();                      // 先关自己，回调里再弹新的确认框也不会冲突
            if (ok) args.OnOk?.Invoke();
            else args.OnCancel?.Invoke();
        }
    }
    //#endregion
}
