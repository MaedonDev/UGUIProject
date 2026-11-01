// 配图用：在编辑器（不进 Play 模式）里驱动真实的 EventSystem / StandaloneInputModule / GraphicRaycaster。
// 这几个组件没有 [ExecuteAlways]，编辑器下不会自动调用 OnEnable，这里用反射补调，用完再调 OnDisable 复原。
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    class EventSim
    {
        public EventSystem es;
        public StandaloneInputModule module;
        public ScriptedInput input;
        readonly List<Behaviour> m_Enabled = new List<Behaviour>();
        int m_Frame;

        static void Call(Behaviour b, string method) =>
            b.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.Invoke(b, null);

        public static EventSim Begin()
        {
            var sim = new EventSim { es = Object.FindObjectOfType<EventSystem>() };
            sim.module = sim.es.GetComponent<StandaloneInputModule>();
            sim.input = sim.es.gameObject.AddComponent<ScriptedInput>();
            sim.module.inputOverride = sim.input;
            sim.Enable(sim.es);
            sim.Enable(sim.module);
            foreach (var r in Object.FindObjectsOfType<BaseRaycaster>()) sim.Enable(r);
            return sim;
        }

        void Enable(Behaviour b) { Call(b, "OnEnable"); m_Enabled.Add(b); }

        public void End()
        {
            for (int i = m_Enabled.Count - 1; i >= 0; i--) Call(m_Enabled[i], "OnDisable");
            m_Enabled.Clear();
            module.inputOverride = null;
            Object.DestroyImmediate(input);
        }

        /// 模拟一帧：设置指针位置和按键状态，然后跑一遍输入模块
        public void Frame(Vector2 screenPos, bool press = false, bool release = false)
        {
            EventLog.Frame = ++m_Frame;
            input.position = screenPos;
            input.pressedThisFrame = press;
            input.releasedThisFrame = release;
            input.held = press || (input.held && !release);
            module.UpdateModule();
            module.Process();
        }

        public void ResetFrames() => m_Frame = 0;

        public static Vector2 Screen(Camera cam, RectTransform rt, Vector2 normalized)
        {
            var c = new Vector3[4];
            rt.GetWorldCorners(c);
            var w = Vector3.Lerp(Vector3.Lerp(c[0], c[3], normalized.x), Vector3.Lerp(c[1], c[2], normalized.x), normalized.y);
            return RectTransformUtility.WorldToScreenPoint(cam, w);
        }
    }
}
