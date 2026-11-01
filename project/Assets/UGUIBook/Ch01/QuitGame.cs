// 案例 1.4：退出游戏。Application.Quit 在编辑器里不起作用，所以编辑器下改为停止 Play 模式。
using UnityEngine;

namespace UGUIBook.Ch01
{
    //#region quit
    public static class QuitGame
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            // 编辑器里 Application.Quit 会被忽略，这里直接退出 Play 模式，方便测试
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            // WebGL 没有“退出”的概念，常见做法是回到首页或什么都不做
            Debug.Log("WebGL 平台不支持退出");
#else
            Application.Quit();
#endif
        }
    }
    //#endregion
}
