namespace UGUIBook.Ch16
{
    //#region layer
    /// 界面层级：数值越大越靠上。每层是 UIRoot 下的一个子 Canvas，sortingOrder = 层号 × 100
    public enum UILayer
    {
        Background = 0, // 场景背景、主城底图
        Normal = 1,     // 普通全屏/半屏界面：主界面、背包、商城（进界面栈，响应返回键）
        Popup = 2,      // 弹窗：确认框、奖励弹窗（进界面栈，响应返回键）
        Toast = 3,      // 飘字提示：不抢焦点、不进栈
        Guide = 4,      // 新手引导遮罩：要盖住所有普通界面
        Top = 5,        // 最顶层：断线重连、加载转圈、GM 面板
    }

    /// 界面关闭后怎么处理它的实例
    public enum UICacheMode
    {
        DestroyOnClose, // 关闭即销毁：低频的大界面（活动页、排行榜）
        Cache,          // 关闭后放进 LRU 缓存，超过容量时销毁最久没用的
        Permanent,      // 常驻：主界面、Toast、确认框这种随时要用的
    }
    //#endregion
}
