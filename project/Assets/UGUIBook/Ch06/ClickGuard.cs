// 全局防抖：同一个“操作键”在 interval 秒内只放行一次。适合网络请求、场景切换这种跨按钮的保护。
using System.Collections.Generic;
using UnityEngine;

namespace UGUIBook.Ch06
{
    //#region main
    public static class ClickGuard
    {
        static readonly Dictionary<string, float> s_Last = new Dictionary<string, float>();

        /// 用法：if (!ClickGuard.TryPass("buy_" + itemId, 1f)) return;
        public static bool TryPass(string key, float interval = 0.5f)
        {
            float now = Time.unscaledTime;
            if (s_Last.TryGetValue(key, out var last) && now - last < interval) return false;
            s_Last[key] = now;
            return true;
        }
    }
    //#endregion
}
