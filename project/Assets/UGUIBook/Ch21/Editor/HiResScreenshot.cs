// 高清截图：
//   · Play 模式：ScreenCapture.CaptureScreenshot 的 superSize 参数，按 Game 视图分辨率的 N 倍截整屏（含 Overlay 画布）
//   · 编辑模式：把选中的 Canvas 用一台临时相机渲染到任意尺寸的 RenderTexture，可选透明背景——做商店图、宣传图、文档插图很方便
// 菜单：Tools › UGUIBook › 高清截图 › ……  截图保存在工程根目录的 Screenshots/ 文件夹
using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch21
{
    public static class HiResScreenshot
    {
        static string Folder { get { var d = Path.GetFullPath(Path.Combine(Application.dataPath, "../Screenshots")); Directory.CreateDirectory(d); return d; } }
        static string Stamp => DateTime.Now.ToString("yyyyMMdd_HHmmss");

        [MenuItem("Tools/UGUIBook/高清截图/Game 视图 ×2（Play 模式）")]
        static void Game2() => CaptureGame(2);
        [MenuItem("Tools/UGUIBook/高清截图/Game 视图 ×4（Play 模式）")]
        static void Game4() => CaptureGame(4);

        static void CaptureGame(int superSize)
        {
            var path = Path.Combine(Folder, $"game_{Stamp}_x{superSize}.png");
            // 截图在本帧渲染结束后才写盘，所以这里只能“预约”
            ScreenCapture.CaptureScreenshot(path, superSize);
            Debug.Log("[HiResScreenshot] 已预约截图（本帧结束写入）：" + path);
        }

        [MenuItem("Tools/UGUIBook/高清截图/选中 Canvas → 3840×2160 透明 PNG")]
        static void SelectedCanvas()
        {
            var canvas = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponentInParent<Canvas>() : null;
            if (canvas == null) { EditorUtility.DisplayDialog("高清截图", "先在 Hierarchy 里选中一个 Canvas（或它下面的物体）", "好"); return; }
            var path = Path.Combine(Folder, $"{canvas.rootCanvas.name}_{Stamp}.png");
            CaptureCanvas(canvas.rootCanvas, 3840, 2160, true, path);
            EditorUtility.RevealInFinder(path);
        }

        //#region capture
        /// 把一个根 Canvas 渲染成 width×height 的 PNG。transparent = true 时背景透明
        public static void CaptureCanvas(Canvas canvas, int width, int height, bool transparent, string path)
        {
            // 临时相机：只拍这个画布所在的层
            var camGo = new GameObject("HiResCaptureCam") { hideFlags = HideFlags.HideAndDontSave };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = transparent ? new Color(0, 0, 0, 0) : Color.white;
            cam.cullingMask = 1 << canvas.gameObject.layer;
            cam.transform.position = new Vector3(10000, 10000, -100);   // 远离场景，避免拍到别的东西

            // 记下画布原来的设置，临时改成 Screen Space - Camera 挂到临时相机上（Overlay 画布不经过相机，拍不到）
            var mode = canvas.renderMode; var oldCam = canvas.worldCamera; var dist = canvas.planeDistance;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            try
            {
                cam.targetTexture = rt;
                cam.aspect = (float)width / height;
                if (mode == RenderMode.ScreenSpaceOverlay || mode == RenderMode.ScreenSpaceCamera)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = cam;
                    canvas.planeDistance = 10;
                }
                // CanvasScaler 在编辑模式下不会马上按新尺寸重算缩放（它在 Update 里做），反射调一次它的 Handle()
                var scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                    typeof(CanvasScaler).GetMethod("Handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(scaler, null);
                Canvas.ForceUpdateCanvases();
                cam.Render();
                Canvas.ForceUpdateCanvases();
                cam.Render();

                var prev = RenderTexture.active;
                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                RenderTexture.active = prev;
                if (transparent)
                {
                    // UI/Default 输出的是“预乘 alpha”的颜色；PNG 存的是非预乘，半透明边缘要除回去，否则会发黑
                    var px = tex.GetPixels32();
                    for (int i = 0; i < px.Length; i++)
                    {
                        var c = px[i];
                        if (c.a == 0 || c.a == 255) continue;
                        float k = 255f / c.a;
                        px[i] = new Color32((byte)Mathf.Min(255, c.r * k), (byte)Mathf.Min(255, c.g * k), (byte)Mathf.Min(255, c.b * k), c.a);
                    }
                    tex.SetPixels32(px);
                }
                tex.Apply();
                File.WriteAllBytes(path, tex.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(tex);
                Debug.Log($"[HiResScreenshot] {width}×{height} → {path}");
            }
            finally
            {
                canvas.renderMode = mode; canvas.worldCamera = oldCam; canvas.planeDistance = dist;
                cam.targetTexture = null;
                rt.Release(); UnityEngine.Object.DestroyImmediate(rt);
                UnityEngine.Object.DestroyImmediate(camGo);
            }
        }
        //#endregion
    }
}
