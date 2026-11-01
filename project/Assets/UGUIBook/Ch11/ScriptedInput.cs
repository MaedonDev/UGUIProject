// 用脚本“假装”成鼠标的 BaseInput。把它赋给输入模块的 inputOverride，输入模块就会从这里读指针位置和按键，
// 适合做 UI 自动化测试，或者像本书配图那样，在不进入 Play 模式的情况下驱动真实的 StandaloneInputModule。
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch11
{
    //#region input
    public class ScriptedInput : BaseInput
    {
        public Vector2 position;
        public bool pressedThisFrame, releasedThisFrame, held;
        public Vector2 scroll;

        public override bool mousePresent => true;
        public override Vector2 mousePosition => position;
        public override Vector2 mouseScrollDelta => scroll;
        public override bool GetMouseButtonDown(int button) => button == 0 && pressedThisFrame;
        public override bool GetMouseButtonUp(int button) => button == 0 && releasedThisFrame;
        public override bool GetMouseButton(int button) => button == 0 && held;
        public override bool touchSupported => false;
        public override int touchCount => 0;
        public override float GetAxisRaw(string axisName) => 0;
        public override bool GetButtonDown(string buttonName) => false;
    }
    //#endregion
}
