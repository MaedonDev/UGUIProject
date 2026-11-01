// 类型安全的事件中心：事件用 struct 定义，按类型分发。
// 发送方和接收方互相不知道对方存在，界面之间、界面和玩法之间就此解耦。
using System;
using UnityEngine;

namespace UGUIBook.Ch16
{
    //#region eventcenter
    public static class EventCenter
    {
        // 泛型静态类：每种事件类型 T 各有一份独立的静态字段，
        // 相当于一张由编译器生成的“类型 → 委托”表，不需要字典查找，也没有装箱
        static class Channel<T>
        {
            public static Action<T> Handlers;
        }

        public static IDisposable Subscribe<T>(Action<T> handler)
        {
            Channel<T>.Handlers += handler;
            return new Disposer(() => Channel<T>.Handlers -= handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) => Channel<T>.Handlers -= handler;

        public static void Publish<T>(T evt)
        {
            var handlers = Channel<T>.Handlers;   // 委托是不可变的：派发途中有人增删订阅也不影响这一轮
            if (handlers == null) return;
            // 逐个调用并隔离异常：一个监听者出错，不能让后面的监听者都收不到
            foreach (Action<T> h in handlers.GetInvocationList())
            {
                try { h(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        /// 清空某类事件的所有订阅（切换场景、关闭“进入播放模式时不重载域”时防止残留）
        public static void Clear<T>() => Channel<T>.Handlers = null;
    }
    //#endregion
}
