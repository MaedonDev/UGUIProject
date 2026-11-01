// 事件日志：把挂上它的物体收到的每一个 EventSystem 事件记下来，直观看到“谁、在什么时候、收到了什么”。
// 用法：给想观察的 UI 挂 EventLogger，场景里放一个 EventLogView 显示日志。
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    /// 全局日志缓冲（运行时和编辑器下的模拟都往这里写）
    public static class EventLog
    {
        public static readonly List<string> Lines = new List<string>();
        public static event Action Changed;
        public static int Frame;   // 由使用者标记“第几帧”，便于看出哪些事件在同一帧里发生
        public static bool Verbose = true;   // 是否附带 pointerId / pointerPress / pointerDrag

        public static void Write(string line)
        {
            Lines.Add(line);
            if (Lines.Count > 200) Lines.RemoveAt(0);
            Changed?.Invoke();
        }

        public static void Clear() { Lines.Clear(); Changed?.Invoke(); }
    }

    //#region logger
    public class EventLogger : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
        IScrollHandler, ISelectHandler, IDeselectHandler
    {
        [Tooltip("OnDrag 每帧都会来，默认只记第一次，免得刷屏")]
        [SerializeField] bool m_LogEveryDrag = false;
        bool m_DragLogged;

        void Log(string evt, BaseEventData e)
        {
            string extra = "";
            if (EventLog.Verbose && e is PointerEventData p)
                extra = $"  (id {p.pointerId}, 按下物体 {Name(p.pointerPress)}, 拖拽物体 {Name(p.pointerDrag)})";
            EventLog.Write($"[{EventLog.Frame}] {name}.{evt}{extra}");
        }

        static string Name(GameObject go) => go != null ? go.name : "-";

        public void OnPointerEnter(PointerEventData e) => Log("OnPointerEnter", e);
        public void OnPointerExit(PointerEventData e) => Log("OnPointerExit", e);
        public void OnPointerDown(PointerEventData e) => Log("OnPointerDown", e);
        public void OnPointerUp(PointerEventData e) => Log("OnPointerUp", e);
        public void OnPointerClick(PointerEventData e) => Log($"OnPointerClick(clickCount={e.clickCount})", e);
        public void OnInitializePotentialDrag(PointerEventData e) => Log("OnInitializePotentialDrag", e);
        public void OnBeginDrag(PointerEventData e) { m_DragLogged = false; Log("OnBeginDrag", e); }
        public void OnDrag(PointerEventData e)
        {
            if (m_DragLogged && !m_LogEveryDrag) return;
            m_DragLogged = true;
            Log("OnDrag", e);
        }
        public void OnEndDrag(PointerEventData e) => Log("OnEndDrag", e);
        public void OnDrop(PointerEventData e) => Log("OnDrop", e);
        public void OnScroll(PointerEventData e) => Log("OnScroll", e);
        public void OnSelect(BaseEventData e) => Log("OnSelect", e);
        public void OnDeselect(BaseEventData e) => Log("OnDeselect", e);
    }
    //#endregion
}
