// 最小可用的数据绑定：可观察属性 + 几个常用的绑定扩展方法。
// 数据变了自动刷新界面，界面不用再到处写 “改完数据记得调 Refresh”。
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch16
{
    //#region observable
    public class Observable<T>
    {
        T m_Value;
        public event Action<T> Changed;

        public Observable(T initial = default) { m_Value = initial; }

        public T Value
        {
            get => m_Value;
            set
            {
                // 值没变不通知，避免“设同一个值也触发整片刷新”
                if (EqualityComparer<T>.Default.Equals(m_Value, value)) return;
                m_Value = value;
                Changed?.Invoke(value);
            }
        }

        /// 改值但不通知（初始化、批量修改时用）
        public void SetWithoutNotify(T value) => m_Value = value;

        /// 值是引用类型、内部字段变了时手动通知一次
        public void Notify() => Changed?.Invoke(m_Value);

        /// 绑定：返回的 IDisposable 调用 Dispose 即解绑。invokeNow=true 时立刻用当前值刷一次界面
        public IDisposable Bind(Action<T> onChanged, bool invokeNow = true)
        {
            Changed += onChanged;
            if (invokeNow) onChanged(m_Value);
            return new Disposer(() => Changed -= onChanged);
        }

        public static implicit operator T(Observable<T> o) => o.m_Value;
        public override string ToString() => m_Value?.ToString() ?? "null";
    }

    /// 把一个 Action 包成 IDisposable，Dispose 多次也只执行一次
    public sealed class Disposer : IDisposable
    {
        Action m_OnDispose;
        public Disposer(Action onDispose) { m_OnDispose = onDispose; }
        public void Dispose()
        {
            m_OnDispose?.Invoke();
            m_OnDispose = null;
        }
    }
    //#endregion

    //#region bindext
    public static class BindingExtensions
    {
        /// 文字绑定：format 为空时直接 ToString
        public static IDisposable BindText<T>(this TMP_Text text, Observable<T> source, Func<T, string> format = null) =>
            source.Bind(v => text.text = format != null ? format(v) : v?.ToString());

        public static IDisposable BindActive(this GameObject go, Observable<bool> source, bool invert = false) =>
            source.Bind(v => go.SetActive(v != invert));

        public static IDisposable BindFill(this Image image, Observable<float> source) =>
            source.Bind(v => image.fillAmount = Mathf.Clamp01(v));

        public static IDisposable BindInteractable(this Selectable sel, Observable<bool> source) =>
            source.Bind(v => sel.interactable = v);
    }
    //#endregion
}
