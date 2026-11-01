// 第 11 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UGUIBook.Ch11
{
    static class Ch11Shots
    {
        static UISkin Skin => UISkin.Instance;

        static TextMeshProUGUI T(Transform p, string s, float size, Color? c = null, TextAlignmentOptions a = TextAlignmentOptions.Center, bool bold = false, string name = "Text")
            => UIKit.Text(p, s, size, c ?? Skin.ink, a, bold, name);

        static void Set(Object o, string prop, Object value)
        {
            var so = new SerializedObject(o);
            so.FindProperty(prop).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetBool(Object o, string prop, bool v)
        {
            var so = new SerializedObject(o);
            so.FindProperty(prop).boolValue = v;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Image Box(Transform p, string name, Color c, Vector2 pos, Vector2 size, string sprite = "round_16")
        {
            var img = UIKit.Image(p, sprite, c, name);
            img.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, size);
            return img;
        }

        // ------------------------------------------------------------------ 11.3 一次点击 / 一次拖放的真实事件序列
        [BookShot("ch11-order", W = 960, H = 470, Scene = "Ch11_EventLog")]
        static void Order(Shot s)
        {
            var panel = Box(s.Root, "Panel", Color.white, new Vector2(-560, 36), new Vector2(680, 760));
            panel.gameObject.AddComponent<EventLogger>();
            T(panel.transform, "Panel（父物体，也挂了 EventLogger）", 28, Skin.muted).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(640, 40));
            var a = Box(panel.transform, "A", Skin.primary, new Vector2(0, 170), new Vector2(360, 160));
            a.gameObject.AddComponent<EventLogger>();
            T(a.transform, "A（可点击、可拖）", 34, Color.white, bold: true).rectTransform.Stretch();
            var b = Box(panel.transform, "B", UIKit.Hex("#FFE7D6"), new Vector2(0, -170), new Vector2(460, 220), "round_outline_16");
            b.color = Skin.accent;
            var bFill = UIKit.Image(b.transform, "round_16", new Color(1, 0.54f, 0.24f, 0.12f), "Fill"); bFill.rectTransform.Stretch(); bFill.raycastTarget = false;
            b.gameObject.AddComponent<EventLogger>();
            T(b.transform, "B（放置区）", 34, Skin.accent, bold: true).rectTransform.Stretch();

            var logBg = Box(s.Root, "LogPanel", Skin.dark, new Vector2(370, 0), new Vector2(1100, 860));
            var logText = T(logBg.transform, "在左边点击、拖动，事件会显示在这里", 26, Color.white, TextAlignmentOptions.TopLeft, name: "Log");
            logText.rectTransform.Stretch(30, 30, 30, 120);
            var clear = UIKit.Button(logBg.transform, "清空", UIKit.Hex("#3A3F57"), 200, 80, fontSize: 30);
            clear.Rt().Place(new Vector2(1, 0), new Vector2(-30, 30), new Vector2(200, 80)); clear.Rt().pivot = new Vector2(1, 0);
            var view = logBg.gameObject.AddComponent<EventLogView>();
            Set(view, "m_Text", logText); Set(view, "m_Clear", clear);
            var hint = T(s.Root, "提示：A 和 B、Panel 都挂了 EventLogger", 28, Skin.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(-560, 18), new Vector2(700, 44));
            foreach (var l in Object.FindObjectsOfType<EventLogger>()) SetBool(l, "m_LogEveryDrag", true);

            s.BeforeCapture(() =>
            {
                s.Cam.Render();                                    // 让 Graphic.depth 有效（-1 的图形会被 GraphicRaycaster 跳过）
                EventLog.Verbose = false;
                var sim = EventSim.Begin();
                Vector2 pa = EventSim.Screen(s.Cam, a.rectTransform, new Vector2(0.5f, 0.5f));
                Vector2 pb = EventSim.Screen(s.Cam, b.rectTransform, new Vector2(0.5f, 0.5f));
                Vector2 outside = EventSim.Screen(s.Cam, s.Root, new Vector2(0.98f, 0.02f));

                EventLog.Clear();
                sim.Frame(outside);
                sim.ResetFrames();
                sim.Frame(pa);                                     // 1 移入 A
                sim.Frame(pa, press: true);                        // 2 按下
                sim.Frame(pa, release: true);                      // 3 抬起
                sim.Frame(outside);                                // 4 移出
                var click = new List<string>(EventLog.Lines);

                EventLog.Clear(); sim.ResetFrames();
                sim.Frame(pa);                                     // 1 移入 A
                sim.Frame(pa, press: true);                        // 2 按下
                sim.Frame(pa + new Vector2(0, -24));               // 3 移动超过阈值（默认 10 像素）→ 开始拖
                sim.Frame(pb);                                     // 4 拖到 B 上
                sim.Frame(pb, release: true);                      // 5 在 B 上松手
                var drag = new List<string>(EventLog.Lines);
                sim.End();
                EventLog.Verbose = true;

                logBg.gameObject.SetActive(false);
                Column(s.Root, "一次点击（A）", click, new Vector2(140, 0));
                Column(s.Root, "一次拖放（A → B）", drag, new Vector2(640, 0));
            });
        }

        static void Column(RectTransform root, string title, List<string> lines, Vector2 pos)
        {
            var bg = Box(root, "Col", Skin.dark, pos, new Vector2(480, 860));
            T(bg.transform, title, 32, UIKit.Hex("#FFC9A3"), TextAlignmentOptions.TopLeft, true).rectTransform.Stretch(24, 24, 24, 0);
            var sb = new StringBuilder();
            int lastFrame = -1;
            foreach (var l in lines)
            {
                int f = int.Parse(l.Substring(1, l.IndexOf(']') - 1));
                if (f != lastFrame) { sb.Append(lastFrame >= 0 ? "\n" : "").Append($"<color=#9AA3C7>— 第 {f} 帧 —</color>\n"); lastFrame = f; }
                sb.Append(l.Substring(l.IndexOf(']') + 2)).Append('\n');
            }
            var t = T(bg.transform, sb.ToString(), 25, Color.white, TextAlignmentOptions.TopLeft);
            t.rectTransform.Stretch(24, 80, 16, 16);
            t.lineSpacing = -6;
        }

        // ------------------------------------------------------------------ 11.4 RaycastAll 的真实结果与排序
        [BookShot("ch11-raycast", W = 960, H = 460)]
        static void Raycast(Shot s)
        {
            var card = Box(s.Root, "Card", Color.white, new Vector2(-420, 0), new Vector2(760, 640));
            var icon = UIKit.Icon(card.transform, "item_gem", 160, Color.white, "Icon(raycast off)"); icon.rectTransform.anchoredPosition = new Vector2(-200, 150);
            var title = T(card.transform, "限时礼包", 48, Skin.ink, TextAlignmentOptions.Left, true, "Title(raycast off)"); title.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(90, 150), new Vector2(360, 70));
            var btn = UIKit.Button(card.transform, "购买", null, 420, 120); btn.Rt().anchoredPosition = new Vector2(0, -150); btn.name = "Button";
            var wm = UIKit.Image(card.transform, "stripes", new Color(0.9f, 0.3f, 0.3f, 0.25f), "Watermark(CanvasGroup)");
            wm.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(60, -120), new Vector2(400, 200));
            var cg = wm.gameObject.AddComponent<CanvasGroup>(); cg.blocksRaycasts = false;

            var top = UIKit.Canvas("TopCanvas(order 10)");
            top.sortingOrder = 10;
            top.GetComponent<CanvasScaler>().referenceResolution = s.Scaler.referenceResolution;
            var toast = UIKit.Image(top.transform, "round_24", new Color(0.12f, 0.14f, 0.21f, 0.88f), "Toast");
            toast.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-300, -190), new Vector2(420, 120));
            T(toast.transform, "已领取！", 36, Color.white, bold: true).rectTransform.Stretch();

            var p = UIKit.Node(s.Root, "P"); p.Place(-260, -160, 2, 2);
            var list = Box(s.Root, "Result", Skin.dark, new Vector2(470, 0), new Vector2(900, 640));
            s.BeforeCapture(() =>
            {
                s.Cam.Render();
                var sim = EventSim.Begin();
                var data = new PointerEventData(sim.es) { position = EventSim.Screen(s.Cam, p, new Vector2(0.5f, 0.5f)) };
                var results = new List<RaycastResult>();
                sim.es.RaycastAll(data, results);
                sim.End();
                var sb = new StringBuilder("<b><color=#FFC9A3>EventSystem.RaycastAll(P) 的结果</color></b>\n\n");
                for (int i = 0; i < results.Count; i++)
                {
                    var r = results[i];
                    var cv = r.module.GetComponent<Canvas>();
                    sb.Append($"#{i}  <b>{r.gameObject.name}</b>\n     sortingOrder {r.sortingOrder} · depth {r.depth}\n");
                }
                sb.Append("\n<color=#9AA3C7>没出现的：Icon / Title（Raycast Target 关）\nWatermark（CanvasGroup.blocksRaycasts = false）</color>");
                var t = T(list.transform, sb.ToString(), 30, Color.white, TextAlignmentOptions.TopLeft);
                t.rectTransform.Stretch(36, 30, 24, 24);
            });
            s.Badge(p, "P", new Vector2(0.5f, 0.5f));
        }

        // ------------------------------------------------------------------ 案例 11.1 可拖拽窗口
        static RectTransform Window(Transform parent, string title, Vector2 pos, Vector2 size, Color bar)
        {
            var win = UIKit.Image(parent, "round_16", Color.white, "Window_" + title);
            win.rectTransform.Place(new Vector2(0.5f, 0.5f), pos, size);
            var sh = UIKit.Image(win.transform, "shadow_16", new Color(0.1f, 0.12f, 0.25f, 0.25f), "Shadow"); sh.rectTransform.Stretch(-20, -14, -20, -28); sh.raycastTarget = false; sh.transform.SetAsFirstSibling();
            var tb = UIKit.Image(win.transform, "round_16", bar, "TitleBar");
            tb.rectTransform.anchorMin = new Vector2(0, 1); tb.rectTransform.anchorMax = new Vector2(1, 1); tb.rectTransform.pivot = new Vector2(0.5f, 1);
            tb.rectTransform.sizeDelta = new Vector2(0, 80); tb.rectTransform.anchoredPosition = Vector2.zero;
            T(tb.transform, title, 32, Color.white, TextAlignmentOptions.MidlineLeft, true).rectTransform.Stretch(28, 0, 80, 0);
            var close = UIKit.Icon(tb.transform, "close", 36, Color.white, "Close");
            close.rectTransform.anchorMin = close.rectTransform.anchorMax = new Vector2(1, 0.5f); close.rectTransform.anchoredPosition = new Vector2(-40, 0);
            var dw = tb.gameObject.AddComponent<DraggableWindow>();
            Set(dw, "m_Window", win.rectTransform);
            var body = T(win.transform, "按住标题栏拖动窗口。\n窗口不会被拖出屏幕；\n点一下就会被置顶。", 30, Skin.muted, TextAlignmentOptions.TopLeft, name: "Body");
            body.rectTransform.Stretch(32, 110, 32, 30);
            return win.rectTransform;
        }

        [BookShot("ch11-window", W = 960, H = 480, Scene = "Ch11_DragWindow")]
        static void DragWindow(Shot s)
        {
            var desk = UIKit.Image(s.Root, "bg_landscape", Color.white, "Desktop"); desk.rectTransform.Stretch();
            var w1 = Window(s.Root, "背包", new Vector2(-380, 40), new Vector2(620, 460), Skin.primary);
            var w2 = Window(s.Root, "任务", new Vector2(80, -70), new Vector2(620, 460), Skin.accent);
            var w3 = Window(s.Root, "设置", new Vector2(520, 120), new Vector2(560, 400), Skin.success);
            s.Arrow(() => s.At(w2, new Vector2(0.3f, 1.02f)), () => s.At(w2, new Vector2(0.3f, 1.02f)) + new Vector2(-170, 130), Skin.danger);
            s.LabelAt(w2, new Vector2(0.3f, 1.02f), new Vector2(-190, 170), "拖动标题栏", Skin.danger);
        }

        // ------------------------------------------------------------------ 案例 11.2 拖放背包
        [BookShot("ch11-inventory", W = 960, H = 540, Scene = "Ch11_Inventory")]
        static void Inventory(Shot s)
        {
            s.Background(UIKit.Hex("#2A2F45"));
            var bag = UIKit.Image(s.Root, "round_24", UIKit.Hex("#3A4060"), "Bag");
            bag.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-180, 0), new Vector2(1000, 800));
            T(bag.transform, "背包（拖动物品：交换 / 丢弃）", 36, Color.white, TextAlignmentOptions.TopLeft, true).rectTransform.Stretch(40, 30, 40, 0);
            var grid = UIKit.Node(bag.transform, "Grid");
            grid.Stretch(40, 110, 40, 40);
            grid.Grid(new Vector2(200, 200), new Vector2(20, 20)).childAlignment = TextAnchor.UpperCenter;
            var dragLayer = UIKit.Node(s.Root, "DragLayer"); dragLayer.Stretch();
            string[] items = { "item_sword", "item_shield", "", "item_potion_red", "item_gem", "", "item_key", "", "item_bow", "item_coin", "", "item_helmet" };
            var slots = new List<InventorySlot>();
            for (int i = 0; i < items.Length; i++)
            {
                var slotImg = UIKit.Image(grid, "slot", Color.white, "Slot" + i);
                var hl = UIKit.Image(slotImg.transform, "frame_select", Skin.accent, "Highlight"); hl.rectTransform.Stretch(-6, -6, -6, -6); hl.raycastTarget = false;
                hl.gameObject.SetActive(false);
                var slot = slotImg.gameObject.AddComponent<InventorySlot>();
                Set(slot, "m_Highlight", hl.gameObject);
                slots.Add(slot);
                if (items[i] == "") continue;
                var it = UIKit.Image(slotImg.transform, null, new Color(0, 0, 0, 0), "Item_" + items[i]);
                it.rectTransform.sizeDelta = new Vector2(170, 170);
                it.gameObject.AddComponent<CanvasGroup>();
                var ic = UIKit.Icon(it.transform, items[i], 130, Color.white); ic.rectTransform.anchoredPosition = new Vector2(0, 6);
                var item = it.gameObject.AddComponent<InventoryItem>();
                item.itemId = items[i].Replace("item_", "");
                Set(item, "m_DragLayer", dragLayer);
                var cnt = T(it.transform, (i % 3 + 1).ToString(), 30, Color.white, TextAlignmentOptions.BottomRight, true);
                cnt.rectTransform.Stretch(0, 0, 14, 8);
            }
            var trash = UIKit.Image(s.Root, "round_24", UIKit.Hex("#3A4060"), "Trash");
            trash.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(620, -160), new Vector2(360, 360));
            var trashIcon = UIKit.Icon(trash.transform, "close", 140, Color.gray, "TrashIcon"); trashIcon.rectTransform.anchoredPosition = new Vector2(0, 30);
            var hint = T(trash.transform, "拖到这里丢弃", 30, UIKit.Hex("#C9CDE0")); hint.rectTransform.anchoredPosition = new Vector2(0, -110);
            var tz = trash.gameObject.AddComponent<TrashZone>();
            Set(tz, "m_Icon", trashIcon); Set(tz, "m_Hint", hint);
            dragLayer.SetAsLastSibling();

            s.BeforeCapture(() =>
            {
                // 模拟“正拖着宝石，悬停在第 3 格上”的那一刻
                var gem = slots[4].item;
                gem.Lift();
                var target = (RectTransform)slots[2].transform;
                gem.transform.position = target.TransformPoint(new Vector3(40, -30, 0));
                slots[2].SetHighlight(true);
            });
            s.LabelAt((RectTransform)slots[4].transform, new Vector2(0.5f, 0.5f), Vector2.zero, "原位置（已空）", Skin.accent, 26);
        }

        // ------------------------------------------------------------------ 案例 11.3 虚拟摇杆
        [BookShot("ch11-joystick", W = 960, H = 480, Scene = "Ch11_Joystick")]
        static void Joystick(Shot s)
        {
            var arena = UIKit.Image(s.Root, "checker", UIKit.Hex("#DDE3F0"), "Arena"); arena.rectTransform.Stretch(); arena.type = Image.Type.Tiled;
            arena.raycastTarget = false;
            var player = UIKit.Image(arena.transform, "circle", Skin.primary, "Player"); player.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(380, 40), new Vector2(150, 150));
            var pArrow = UIKit.Icon(player.transform, "arrow_up", 90, Color.white, "Arrow"); pArrow.rectTransform.anchoredPosition = new Vector2(0, 8);
            var area = UIKit.Image(s.Root, null, new Color(0, 0, 0, 0), "TouchArea");
            area.rectTransform.anchorMin = Vector2.zero; area.rectTransform.anchorMax = new Vector2(0.5f, 1); area.rectTransform.offsetMin = area.rectTransform.offsetMax = Vector2.zero;
            var areaHint = UIKit.Image(area.transform, "round_24", new Color(0.36f, 0.29f, 0.84f, 0.06f), "AreaTint"); areaHint.rectTransform.Stretch(20, 20, 10, 20); areaHint.raycastTarget = false;
            var stickBase = UIKit.Image(area.transform, "ring", new Color(1, 1, 1, 0.9f), "Base");
            stickBase.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(-120, -200), new Vector2(320, 320)); stickBase.raycastTarget = false;
            var baseFill = UIKit.Image(stickBase.transform, "circle", new Color(0.12f, 0.14f, 0.21f, 0.25f), "Fill"); baseFill.rectTransform.Stretch(); baseFill.raycastTarget = false;
            var knob = UIKit.Image(stickBase.transform, "circle", Color.white, "Knob"); knob.rectTransform.sizeDelta = new Vector2(140, 140); knob.raycastTarget = false;
            var knobDot = UIKit.Image(knob.transform, "circle", Skin.primary, "Dot"); knobDot.rectTransform.Stretch(30, 30, 30, 30); knobDot.raycastTarget = false;
            var stick = area.gameObject.AddComponent<VirtualJoystick>();
            Set(stick, "m_Base", stickBase.rectTransform); Set(stick, "m_Knob", knob.rectTransform);
            var readout = T(s.Root, "value = (0.00, 0.00)", 34, Skin.ink, TextAlignmentOptions.TopRight, name: "Readout");
            readout.rectTransform.Place(new Vector2(1, 1), new Vector2(-40, -40), new Vector2(600, 50));
            var hint = T(s.Root, "在左半屏任意位置按下拖动\n（多点触控：右半屏可以同时按别的按钮）", 28, Skin.muted);
            hint.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(480, 40), new Vector2(900, 90));
            var pl = s.Root.gameObject.AddComponent<JoystickPlayer>();
            Set(pl, "m_Stick", stick); Set(pl, "m_Player", player.rectTransform); Set(pl, "m_Arena", arena.rectTransform); Set(pl, "m_Readout", readout);
            s.BeforeCapture(() =>
            {
                stickBase.rectTransform.anchoredPosition = new Vector2(-80, -140);
                stick.SetKnob(new Vector2(0.62f, 0.5f));
                readout.text = $"value = ({stick.value.x:0.00}, {stick.value.y:0.00})";
                player.rectTransform.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(0.5f, 0.62f) * Mathf.Rad2Deg - 90);
            });
            s.Box(area.rectTransform, Skin.primary, "TouchArea（透明 Image，挂 VirtualJoystick）", 0, true);
        }

        // ------------------------------------------------------------------ 案例 11.4 点击空白关闭
        [BookShot("ch11-popup", W = 960, H = 480, Scene = "Ch11_ClickOutside")]
        static void Popup(Shot s)
        {
            var menu = UIKit.Node(s.Root, "Menu"); menu.Stretch();
            var open = UIKit.Button(menu, "打开弹窗", null, 400, 110); open.Rt().anchoredPosition = new Vector2(-300, 120);
            var more = UIKit.Button(menu, "更多…", UIKit.Hex("#E6E8F0"), 300, 110); more.Rt().anchoredPosition = new Vector2(250, 120);
            var other = UIKit.Button(menu, "被遮住的按钮", Skin.success, 400, 110); other.Rt().anchoredPosition = new Vector2(-300, -120);

            // 下拉小菜单：DismissOnOutsidePress，不挡别的按钮
            var drop = UIKit.Image(menu, "round_16", Color.white, "MoreMenu");
            drop.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(250, -60), new Vector2(300, 240));
            var dv = UIKit.Node(drop.transform, "List"); dv.Stretch(10, 10, 10, 10); dv.VLayout(4);
            foreach (var l in new[] { "分享", "收藏", "举报" }) { var it = UIKit.Button(dv, l, new Color(1, 1, 1, 0), 280, 70, fontSize: 30); }
            drop.gameObject.AddComponent<DismissOnOutsidePress>();
            drop.gameObject.SetActive(false);
            UnityEventTools.AddBoolPersistentListener(more.onClick, drop.gameObject.SetActive, true);

            // 模态弹窗：Popup → Blocker（兄弟，在前）+ Panel
            var popup = UIKit.Node(s.Root, "Popup"); popup.Stretch();
            var blocker = UIKit.Image(popup, null, new Color(0.08f, 0.1f, 0.18f, 0.55f), "Blocker"); blocker.rectTransform.Stretch();
            var pb = blocker.gameObject.AddComponent<PopupBlocker>();
            Set(pb, "m_Popup", popup.gameObject);
            var panel = UIKit.Image(popup, "round_24", Color.white, "Panel");
            panel.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(760, 460));
            T(panel.transform, "领取每日奖励", 44, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(700, 60));
            T(panel.transform, "点弹窗外的灰色区域关闭；\n点弹窗内部不会关闭。", 32, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(700, 120));
            var ok = UIKit.Button(panel.transform, "领取", null, 300, 96); ok.Rt().anchoredPosition = new Vector2(0, -150);
            UnityEventTools.AddBoolPersistentListener(ok.onClick, popup.gameObject.SetActive, false);
            UnityEventTools.AddBoolPersistentListener(open.onClick, popup.gameObject.SetActive, true);
            s.LabelAt(blocker.rectTransform, new Vector2(0.2f, 0.12f), Vector2.zero, "Blocker：全屏半透明 Image + PopupBlocker", Skin.accent);
            s.LabelAt(panel.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 40), "Panel：Blocker 的兄弟（在它后面）", Skin.primary);
        }

        // ------------------------------------------------------------------ 案例 11.5 点击 3D 物体
        [BookShot("ch11-click3d", W = 960, H = 540, Scene = "Ch11_Click3D")]
        static void Click3D(Shot s)
        {
            s.Cam.orthographic = false; s.Cam.fieldOfView = 40;
            s.Cam.transform.position = new Vector3(0, 5.2f, -9.5f); s.Cam.transform.rotation = Quaternion.Euler(25, 0, 0);
            s.Cam.clearFlags = CameraClearFlags.SolidColor; s.Cam.backgroundColor = UIKit.Hex("#C9D3EA");
            s.Cam.gameObject.AddComponent<PhysicsRaycaster>();
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional; light.transform.rotation = Quaternion.Euler(50, -30, 0); light.intensity = 1.1f; light.shadows = LightShadows.Soft;
            RenderSettings.ambientLight = new Color(0.55f, 0.58f, 0.66f);
            var mat = s.MatAsset("Ch11", "Clickable", "Standard");
            var groundMat = s.MatAsset("Ch11", "Ground", "Standard");
            groundMat.color = UIKit.Hex("#EEF0F6");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Ground"; ground.transform.localScale = new Vector3(2, 1, 2);
            ground.GetComponent<Renderer>().sharedMaterial = groundMat;

            var feedBg = UIKit.Image(s.Root, "round_16", new Color(0.12f, 0.14f, 0.21f, 0.85f), "Feed");
            feedBg.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1400, 90));
            var feedText = T(feedBg.transform, "点击场景里的物体（UI 按钮会挡住后面的 3D 物体）", 30, Color.white); feedText.rectTransform.Stretch(20, 0, 20, 0);
            var feed = feedBg.gameObject.AddComponent<ClickFeed>(); Set(feed, "m_Text", feedText);

            var shapes = new[] { PrimitiveType.Cube, PrimitiveType.Sphere, PrimitiveType.Cylinder, PrimitiveType.Capsule };
            var colors = new[] { Skin.primary, Skin.accent, Skin.success, UIKit.Hex("#1E88E5") };
            var objs = new List<Clickable3D>();
            for (int i = 0; i < 4; i++)
            {
                var go = GameObject.CreatePrimitive(shapes[i]); go.name = shapes[i].ToString();
                go.transform.position = new Vector3(-3.3f + i * 2.2f, shapes[i] == PrimitiveType.Capsule || shapes[i] == PrimitiveType.Cylinder ? 1f : 0.5f, 0);
                if (shapes[i] == PrimitiveType.Cube) go.transform.rotation = Quaternion.Euler(0, 30, 0);
                go.GetComponent<Renderer>().sharedMaterial = mat;
                var c = go.AddComponent<Clickable3D>();
                c.SetBaseColor(colors[i]);
                UnityEventTools.AddPersistentListener(c.onClicked, feed.Show);
                objs.Add(c);
            }
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder); marker.name = "Marker";
            Object.DestroyImmediate(marker.GetComponent<Collider>());
            marker.transform.localScale = new Vector3(0.5f, 0.01f, 0.5f); marker.transform.position = new Vector3(1.2f, 0.01f, -2.2f);
            marker.GetComponent<Renderer>().sharedMaterial = mat;
            var gc = new GameObject("GroundClicker").AddComponent<GroundClicker>();
            Set(gc, "m_Camera", s.Cam); Set(gc, "m_Marker", marker.transform);

            var uiBtn = UIKit.Button(s.Root, "UI 按钮", null, 300, 100);
            uiBtn.Rt().Place(new Vector2(0.5f, 0.5f), new Vector2(240, 30), new Vector2(300, 100));
            s.BeforeCapture(() =>
            {
                s.Canvas.planeDistance = 1;                                  // 截图时画布被临时改成 Camera 模式，挪到 3D 物体前面
                objs[1].ApplyColor(new Color(1f, 0.85f, 0.4f));            // 鼠标悬停在球上
                var mb = new MaterialPropertyBlock(); mb.SetColor("_Color", Skin.danger); marker.GetComponent<Renderer>().SetPropertyBlock(mb);
                // 用真实的 EventSystem.RaycastAll 取一次球上的命中结果，显示在底部
                Physics.SyncTransforms();                                    // 编辑器下不跑物理，手动把挪过的 Collider 同步到物理场景
                var sim = EventSim.Begin();
                var data = new PointerEventData(sim.es) { position = s.Cam.WorldToScreenPoint(objs[1].transform.position + new Vector3(0.15f, 0.2f, -0.3f)) };
                var res = new List<RaycastResult>();
                sim.es.RaycastAll(data, res);
                sim.End();
                var hits = res.Where(r => r.module is PhysicsRaycaster).ToList();
                if (hits.Count > 0)
                {
                    var h = hits[0];
                    feedText.text = $"{h.gameObject.name}  命中点 {h.worldPosition:F2}  距离 {h.distance:0.00}  模块 {h.module.GetType().Name}";
                }
            });
        }

        // ------------------------------------------------------------------ 案例 11.6 双指缩放
        [BookShot("ch11-pinch", W = 960, H = 480, Scene = "Ch11_Pinch")]
        static void Pinch(Shot s)
        {
            var vp = UIKit.Image(s.Root, null, Color.black, "Viewport");
            vp.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(1500, 780));
            vp.gameObject.AddComponent<RectMask2D>();
            var map = UIKit.Image(vp.transform, "bg_landscape", Color.white, "Map");
            map.rectTransform.Place(new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 800));
            map.raycastTarget = false;
            var pins = new[] { new Vector2(-400, 100), new Vector2(150, -120), new Vector2(520, 160) };
            string[] names = { "王城", "港口", "雪山" };
            for (int i = 0; i < pins.Length; i++)
            {
                var pin = UIKit.Icon(map.transform, "pointer", 46, Skin.danger, "Pin"); pin.rectTransform.anchoredPosition = pins[i];
                var lb = T(map.transform, names[i], 26, Color.white, bold: true); lb.rectTransform.anchoredPosition = pins[i] + new Vector2(0, -44);
            }
            var pz = vp.gameObject.AddComponent<PinchZoom>();
            Set(pz, "m_Content", map.rectTransform);
            T(s.Root, "双指缩放 / 单指拖动 / 鼠标滚轮缩放", 32, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -36), new Vector2(1600, 50));
            var f1 = UIKit.Node(s.Root, "Finger1"); f1.Place(-180, 60, 2, 2);
            var f2 = UIKit.Node(s.Root, "Finger2"); f2.Place(260, -150, 2, 2);
            s.BeforeCapture(() =>
            {
                var mid = EventSimScreen(s, f1, f2);
                pz.ZoomAt(mid, 1.7f, s.Cam);
            });
            foreach (var f in new[] { f1, f2 }) s.Badge(f, "●", new Vector2(0.5f, 0.5f), new Color(1, 1, 1, 0.9f));
            s.Arrow(() => s.At(f1, Vector2.one * 0.5f) + new Vector2(-30, 30), () => s.At(f1, Vector2.one * 0.5f) + new Vector2(-150, 120), Color.white);
            s.Arrow(() => s.At(f2, Vector2.one * 0.5f) + new Vector2(30, -30), () => s.At(f2, Vector2.one * 0.5f) + new Vector2(150, -120), Color.white);
        }

        static Vector2 EventSimScreen(Shot s, RectTransform a, RectTransform b) =>
            (EventSim.Screen(s.Cam, a, Vector2.one * 0.5f) + EventSim.Screen(s.Cam, b, Vector2.one * 0.5f)) * 0.5f;

        // ------------------------------------------------------------------ 案例 11.7 左滑删除（拦截与转发）
        [BookShot("ch11-swipe", W = 405, H = 720, Scene = "Ch11_SwipeList")]
        static void Swipe(Shot s)
        {
            s.Ref(1080, 1920, 1);
            T(s.Root, "邮件（左滑删除，上下拖滚动）", 44, Skin.ink, bold: true).rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1000, 70));
            var sr = UIKit.ScrollView(s.Root, true, false, 1080, 1600);
            sr.Rt().Stretch(0, 150, 0, 0);
            var content = sr.content;
            var vl = content.VLayout(2); vl.childControlHeight = true;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var rows = new List<SwipeRow>();
            for (int i = 0; i < 12; i++)
            {
                var row = UIKit.Image(content, null, Skin.danger, "Row" + i);
                row.gameObject.AddComponent<LayoutElement>().preferredHeight = 180;
                var del = UIKit.Button(row.transform, "删除", Skin.danger, 220, 180);
                del.Rt().anchorMin = new Vector2(1, 0); del.Rt().anchorMax = new Vector2(1, 1); del.Rt().pivot = new Vector2(1, 0.5f);
                del.Rt().sizeDelta = new Vector2(220, 0); del.Rt().anchoredPosition = Vector2.zero; del.image.sprite = null;
                var front = UIKit.Image(row.transform, null, Color.white, "Front"); front.rectTransform.Stretch();
                var line = UIKit.Image(front.transform, null, UIKit.Hex("#E8EAF2"), "Line"); line.rectTransform.anchorMin = Vector2.zero; line.rectTransform.anchorMax = new Vector2(1, 0); line.rectTransform.sizeDelta = new Vector2(0, 2); line.rectTransform.pivot = new Vector2(0.5f, 0);
                var av = UIKit.Image(front.transform, "avatar_" + (i % 6 + 1), Color.white, "Avatar"); av.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(110, 110)); av.raycastTarget = false;
                string[] titles = { "系统通知", "公会邀请", "活动奖励", "好友消息", "竞技场结算", "版本更新" };
                var tt = T(front.transform, titles[i % 6], 38, Skin.ink, TextAlignmentOptions.TopLeft, true); tt.rectTransform.Stretch(180, 34, 40, 90);
                var pv = T(front.transform, "点击查看详情，附件将在 7 天后过期……", 30, Skin.muted, TextAlignmentOptions.TopLeft); pv.rectTransform.Stretch(180, 92, 40, 30);
                var sw = row.gameObject.AddComponent<SwipeRow>();
                Set(sw, "m_Front", front.rectTransform); Set(sw, "m_Delete", del); Set(sw, "m_Scroll", sr);
                rows.Add(sw);
            }
            s.BeforeCapture(() => { rows[1].SetOpen(true); rows[3].SetOffset(-110); });
            s.LabelAt((RectTransform)rows[3].transform, new Vector2(0.5f, 0), new Vector2(0, -20), "横向拖：行自己处理", Skin.accent, 30);
        }

        // ------------------------------------------------------------------ 11.11 Input System 的 UI 输入模块
        [BookScene("Ch11_InputSystem")]
        static void InputSystemScene(Shot s)
        {
            var es = Object.FindObjectOfType<EventSystem>();
            Object.DestroyImmediate(es.GetComponent<StandaloneInputModule>());
            Object.DestroyImmediate(es.GetComponent<BaseInput>());
            es.gameObject.AddComponent<InputSystemUIInputModule>();   // 运行时 OnEnable 发现没配动作会自动 AssignDefaultActions()

            var area = UIKit.Image(s.Root, "round_24", Color.white, "ClickArea");
            area.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(0, 120), new Vector2(1400, 420));
            var t = T(area.transform, "", 36, Skin.ink); t.rectTransform.Stretch(40, 40, 40, 40);
            var info = area.gameObject.AddComponent<InputModuleInfo>(); Set(info, "m_Text", t);
            var b = UIKit.Button(s.Root, "普通按钮", null, 360, 110); b.Rt().anchoredPosition = new Vector2(-260, -260);
            var sl = UIKit.Slider(s.Root, 0.4f, 500, 50); sl.Rt().anchoredPosition = new Vector2(300, -260);
            T(s.Root, "EventSystem 上是 InputSystemUIInputModule：鼠标、触屏、手柄（方向键导航）都能用", 30, Skin.muted).rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(1800, 44));
            es.firstSelectedGameObject = b.gameObject;
        }
    }
}
