// 第 18 章搭景：固定高度列表、原理透视图、性能对比、聊天、背包、排行榜
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch18
{
    static class Ch18Shots
    {
        static readonly Color Dark = UIKit.Hex("#1B1F2E");
        static readonly Color Card = UIKit.Hex("#262B40");
        static readonly Color Soft = UIKit.Hex("#A9B0C7");

        static void Set(Object o, string field, object v)
        {
            var f = o.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) throw new Exception($"{o.GetType().Name}.{field} 不存在");
            f.SetValue(o, v);
            EditorUtility.SetDirty(o);
        }

        static TextMeshProUGUI T(Transform p, string text, float size, Color c, TextAlignmentOptions al, bool bold, string name,
            Vector2 anchor, Vector2 pos, Vector2 sz)
        {
            var t = UIKit.Text(p, text, size, c, al, bold, name);
            t.rectTransform.Place(anchor, pos, sz);
            return t;
        }

        /// 滚动视图 + 循环列表组件。模板放在 ScrollView 下、视口外（隐藏）
        static LoopListView NewList(Transform parent, string name, float w, float h)
        {
            var sr = UIKit.ScrollView(parent, true, false, w, h);
            sr.name = name;
            sr.movementType = ScrollRect.MovementType.Elastic;
            return sr.gameObject.AddComponent<LoopListView>();
        }

        // ---------------- 邮件列表项 ----------------
        static RectTransform MailTemplate(Transform parent)
        {
            var root = UIKit.Image(parent, "round_16", Color.white, "MailItemTemplate");
            root.raycastTarget = false;
            root.rectTransform.sizeDelta = new Vector2(900, 132);
            var hl = UIKit.Image(root.transform, "round_outline_16", UISkin.Instance.accent, "Highlight");
            hl.raycastTarget = false; hl.rectTransform.Stretch(-3, -3, -3, -3);
            var ib = UIKit.Image(root.transform, "circle", UISkin.Instance.page, "IconBg");
            ib.raycastTarget = false;
            ib.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(96, 96));
            var icon = UIKit.Icon(ib.transform, "mail", 64, Color.white, "Icon");
            var title = T(root.transform, "标题", 34, UISkin.Instance.ink, TextAlignmentOptions.MidlineLeft, true, "Title",
                new Vector2(0, 1), new Vector2(140, -18), new Vector2(560, 50));
            title.rectTransform.anchorMax = new Vector2(1, 1); title.rectTransform.offsetMax = new Vector2(-200, -18);
            var sub = T(root.transform, "副标题", 26, UISkin.Instance.muted, TextAlignmentOptions.MidlineLeft, false, "Sub",
                new Vector2(0, 0), new Vector2(140, 18), new Vector2(560, 40));
            sub.rectTransform.anchorMax = new Vector2(1, 0); sub.rectTransform.offsetMax = new Vector2(-200, 58);
            var btn = UIKit.Button(root.transform, "领取", UISkin.Instance.primary, 150, 70, null, 30, "Claim");
            btn.Rt().Place(new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(150, 70));
            var cb = btn.colors; cb.disabledColor = new Color(0.75f, 0.75f, 0.8f, 0.6f); btn.colors = cb;
            var view = root.gameObject.AddComponent<MailItemView>();
            Set(view, "m_Icon", icon);
            Set(view, "m_Title", title);
            Set(view, "m_Sub", sub);
            Set(view, "m_Claim", btn);
            Set(view, "m_ClaimText", btn.GetComponentInChildren<TextMeshProUGUI>());
            Set(view, "m_Highlight", hl);
            root.gameObject.SetActive(false);
            return root.rectTransform;
        }

        static (FixedListDemo demo, LoopListView list, TextMeshProUGUI stats) BuildFixed(Shot s, float listH)
        {
            var card = UIKit.Card(s.Root, "ListCard", 24, true, UIKit.Hex("#E6E9F2"));
            card.Place(new Vector2(0.5f, 0.5f), new Vector2(-380, 0), new Vector2(1000, listH + 40));
            var list = NewList(card, "MailList", 960, listH);
            var tpl = MailTemplate(list.transform);
            Set(list, "m_Templates", new[] { tpl });
            Set(list, "m_DefaultHeight", 132f);
            Set(list, "m_Spacing", 12f);
            var side = UIKit.Node(s.Root, "Side");
            side.Place(new Vector2(0.5f, 0.5f), new Vector2(520, 0), new Vector2(700, 900));
            T(side, "固定高度循环列表", 52, UISkin.Instance.ink, TextAlignmentOptions.TopLeft, true, "Title", new Vector2(0, 1), new Vector2(0, -40), new Vector2(700, 80))
                .rectTransform.pivot = new Vector2(0, 1);
            var stats = T(side, "", 32, UISkin.Instance.ink, TextAlignmentOptions.TopLeft, false, "Stats", new Vector2(0, 1), new Vector2(0, -150), new Vector2(680, 160));
            stats.rectTransform.pivot = new Vector2(0, 1);
            stats.lineSpacing = 12;
            var input = UIKit.Input(side, "输入 1~10000", 400, 96);
            input.contentType = TMP_InputField.ContentType.IntegerNumber;
            input.Rt().Place(new Vector2(0, 1), new Vector2(0, -360), new Vector2(400, 96));
            var jump = UIKit.Button(side, "跳转", UISkin.Instance.accent, 220, 96, null, 36, "Jump");
            jump.Rt().Place(new Vector2(0, 1), new Vector2(430, -360), new Vector2(220, 96));
            T(side, "拖动列表、输入序号跳转、点「领取」，\n留意右侧“实例化的 Item”数字始终不变。", 30, UISkin.Instance.muted, TextAlignmentOptions.TopLeft, false, "Hint",
                new Vector2(0, 1), new Vector2(0, -500), new Vector2(700, 120)).rectTransform.pivot = new Vector2(0, 1);
            var demo = s.Root.gameObject.AddComponent<FixedListDemo>();
            Set(demo, "m_List", list);
            Set(demo, "m_JumpInput", input);
            Set(demo, "m_JumpButton", jump);
            Set(demo, "m_Stats", stats);
            return (demo, list, stats);
        }

        [BookShot("ch18-fixed", Scene = "Ch18_FixedList")]
        static void Fixed(Shot s)
        {
            var (demo, list, stats) = BuildFixed(s, 900);
            s.BeforeCapture(() =>
            {
                demo.Init();
                demo.JumpTo(4999);
                list.Refresh();
            });
        }

        // 原理透视：去掉视口的 RectMask2D，看看视口外到底有几个 Item
        [BookShot("ch18-principle")]
        static void Principle(Shot s)
        {
            var (demo, list, stats) = BuildFixed(s, 560);
            var card = (RectTransform)list.transform.parent;
            card.Find("Bg").GetComponent<Image>().color = new Color(1, 1, 1, 0);
            card.Find("Shadow").gameObject.SetActive(false);
            Object.DestroyImmediate(list.ScrollRect.viewport.GetComponent<RectMask2D>());
            var vpFrame = UIKit.Image(list.ScrollRect.viewport, "round_outline_16", UIKit.Hex("#1E88E5"), "ViewportFrame");
            vpFrame.raycastTarget = false; vpFrame.rectTransform.Stretch(-6, -6, -6, -6);
            s.BeforeCapture(() =>
            {
                demo.Init();
                list.ScrollY = 132 * 3 + 12 * 3 + 70;    // 停在第 4 项中间，让上下都有“半露”的项
                list.UpdateVisible();
            });
            s.LabelAt(list.ScrollRect.viewport, new Vector2(1, 0.5f), new Vector2(90, 0), "视口", UIKit.Hex("#1E88E5"));
            s.Label(new Vector2(520, -330), "去掉遮罩后：视口外只多出上下各 1 个缓冲项", UIKit.Hex("#E5484D"), 30);
        }

        // ---------------- 性能对比 ----------------
        [BookShot("ch18-bench", Scene = "Ch18_Benchmark")]
        static void Bench(Shot s)
        {
            var naive = UIKit.ScrollView(s.Root, true, false, 820, 500);
            naive.name = "NaiveList";
            naive.Rt().Place(new Vector2(0.5f, 1), new Vector2(-450, -170), new Vector2(820, 500));
            var vlg = naive.content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 12; vlg.padding = new RectOffset(16, 16, 10, 10);
            vlg.childControlWidth = true; vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false; vlg.childForceExpandHeight = false;
            naive.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var naiveTpl = MailTemplate(naive.transform);
            var loop = NewList(s.Root, "LoopList", 820, 500);
            loop.Rt().Place(new Vector2(0.5f, 1), new Vector2(450, -170), new Vector2(820, 500));
            Set(loop, "m_Templates", new[] { MailTemplate(loop.transform) });
            Set(loop, "m_DefaultHeight", 132f);
            Set(loop, "m_Spacing", 12f);
            foreach (var sr in new[] { naive, loop.ScrollRect })
            {
                var bg = UIKit.Image(sr.transform, "round_16", UIKit.Hex("#E1E5EE"), "Bg");
                bg.rectTransform.Stretch(); bg.transform.SetAsFirstSibling(); bg.raycastTarget = true;
            }
            T(s.Root, "普通列表（VerticalLayoutGroup）", 36, UISkin.Instance.ink, TextAlignmentOptions.Center, true, "NaiveTitle",
                new Vector2(0.5f, 1), new Vector2(-450, -100), new Vector2(820, 60));
            T(s.Root, "循环列表（LoopListView）", 36, UISkin.Instance.ink, TextAlignmentOptions.Center, true, "LoopTitle",
                new Vector2(0.5f, 1), new Vector2(450, -100), new Vector2(820, 60));
            var nb = UIKit.Button(s.Root, "生成 10000 条（普通）", UIKit.Hex("#E5484D"), 460, 90, null, 32, "RunNaive");
            nb.Rt().Place(new Vector2(0.5f, 1), new Vector2(-450, -690), new Vector2(460, 84));
            var lb = UIKit.Button(s.Root, "生成 10000 条（循环）", UISkin.Instance.success, 460, 90, null, 32, "RunLoop");
            lb.Rt().Place(new Vector2(0.5f, 1), new Vector2(450, -690), new Vector2(460, 84));
            var table = UIKit.Image(s.Root, "round_16", Color.white, "Result");
            table.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, 16), new Vector2(1720, 270));
            var output = UIKit.Text(table.transform, "点上面的按钮开始测量（普通列表 10000 条会卡顿几秒）", 30, UISkin.Instance.ink, TextAlignmentOptions.TopLeft, false, "Output");
            output.rectTransform.Stretch(30, 14, 30, 10);
            output.lineSpacing = 4;
            var bench = s.Root.gameObject.AddComponent<ListBenchmark>();
            Set(bench, "m_Naive", naive);
            Set(bench, "m_NaiveTemplate", naiveTpl);
            Set(bench, "m_Loop", loop);
            Set(bench, "m_Output", output);
            Set(bench, "m_NaiveButton", nb);
            Set(bench, "m_LoopButton", lb);
            s.BeforeCapture(() =>
            {
                bench.RunNaive(200); bench.RunLoop(200);   // 预热：让 JIT 编译、字体字形缓存先完成，避免污染第一组数据
                var rows = new List<ListBenchmark.Result>
                {
                    bench.RunNaive(1000), bench.RunLoop(1000), bench.RunLoop(10000), bench.RunNaive(10000),
                };
                var sb = new System.Text.StringBuilder();
                sb.Append("<b>方案<pos=18%>数据量<pos=30%>GameObject<pos=46%>Graphic<pos=60%>创建+绑定<pos=78%>首次布局与重建</b>");
                foreach (var r in rows)
                {
                    sb.Append($"\n{r.Name}<pos=18%>{r.Count}<pos=30%>{r.GameObjects}<pos=46%>{r.Graphics}<pos=60%>{r.BuildMs:0.0} ms<pos=78%>{r.LayoutMs:0.0} ms");
                    Debug.Log($"[Ch18-BENCH] {r.Name} n={r.Count} go={r.GameObjects} graphic={r.Graphics} build={r.BuildMs:0.00}ms layout={r.LayoutMs:0.00}ms");
                }
                output.text = sb.ToString();
            });
        }

        // ---------------- 聊天 ----------------
        static RectTransform BubbleTemplate(Transform parent, bool me)
        {
            float sign = me ? -1 : 1;
            var ax = me ? 1f : 0f;
            var root = UIKit.Node(parent, me ? "MeTemplate" : "OtherTemplate");
            root.sizeDelta = new Vector2(900, 150);
            var av = UIKit.Image(root, me ? "avatar_1" : "avatar_2", Color.white, "Avatar");
            av.raycastTarget = false;
            av.rectTransform.Place(new Vector2(ax, 1), Vector2.zero, new Vector2(ChatBubbleView.AvatarSize, ChatBubbleView.AvatarSize));
            float x = (ChatBubbleView.AvatarSize + ChatBubbleView.Gap) * sign;
            var name = T(root, "名字", 24, UISkin.Instance.muted, me ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft, false, "Name",
                new Vector2(ax, 1), new Vector2(x, 0), new Vector2(500, ChatBubbleView.NameHeight));
            var bubble = UIKit.Image(root, "round_24", me ? UIKit.Hex("#6C5CE7") : Color.white, "Bubble");
            bubble.raycastTarget = false;
            bubble.rectTransform.Place(new Vector2(ax, 1), new Vector2(x, -ChatBubbleView.NameHeight), new Vector2(300, 80));
            var text = UIKit.Text(bubble.transform, "", 32, me ? Color.white : UISkin.Instance.ink, TextAlignmentOptions.TopLeft, false, "Text");
            text.rectTransform.Stretch(ChatBubbleView.PadX, ChatBubbleView.PadY, ChatBubbleView.PadX, ChatBubbleView.PadY);
            text.enableWordWrapping = true;
            var view = root.gameObject.AddComponent<ChatBubbleView>();
            Set(view, "m_Avatar", av);
            Set(view, "m_NameText", name);
            Set(view, "m_Bubble", bubble.rectTransform);
            Set(view, "m_Text", text);
            root.gameObject.SetActive(false);
            return root;
        }

        static RectTransform TimeTemplate(Transform parent)
        {
            var root = UIKit.Node(parent, "TimeTemplate");
            root.sizeDelta = new Vector2(900, 56);
            var pill = UIKit.Image(root, "pill", new Color(0, 0, 0, 0.07f), "Pill");
            pill.raycastTarget = false;
            pill.rectTransform.Place(0, 0, 150, 42);
            var t = UIKit.Text(pill.transform, "14:30", 24, UISkin.Instance.muted, TextAlignmentOptions.Center, false, "Text");
            t.rectTransform.Stretch();
            root.gameObject.SetActive(false);
            return root;
        }

        [BookShot("ch18-chat", W = 405, H = 720, Panels = 2, Scene = "Ch18_Chat")]
        static void Chat(Shot s)
        {
            s.Ref(1080, 1920);
            s.Background(UIKit.Hex("#EDEFF5"));
            var header = UIKit.Image(s.Root, null, Card, "Header").rectTransform;
            header.anchorMin = new Vector2(0, 1); header.anchorMax = Vector2.one; header.pivot = new Vector2(0.5f, 1);
            header.sizeDelta = new Vector2(0, 150); header.anchoredPosition = Vector2.zero;
            T(header, "公会频道", 44, Color.white, TextAlignmentOptions.MidlineLeft, true, "Title", new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(400, 80))
                .rectTransform.pivot = new Vector2(0, 0.5f);
            var auto = UIKit.Toggle(header, "模拟群聊", false, 280, 64);
            auto.Rt().Place(new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(280, 64));
            auto.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;

            var list = NewList(s.Root, "ChatList", 1080, 1500);
            var lrt = list.Rt();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = new Vector2(0, 170); lrt.offsetMax = new Vector2(0, -150);
            var other = BubbleTemplate(list.transform, false);
            var me = BubbleTemplate(list.transform, true);
            var time = TimeTemplate(list.transform);
            Set(list, "m_Templates", new[] { other, me, time });
            Set(list, "m_Spacing", 18f);
            Set(list, "m_PaddingLeft", 28f);
            Set(list, "m_PaddingRight", 28f);
            Set(list, "m_PaddingTop", 24f);
            Set(list, "m_Buffer", 2);

            var bar = UIKit.Image(s.Root, null, Color.white, "InputBar").rectTransform;
            bar.anchorMin = Vector2.zero; bar.anchorMax = new Vector2(1, 0); bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 170); bar.anchoredPosition = Vector2.zero;
            var input = UIKit.Input(bar, "说点什么…（回车发送）", 760, 100);
            input.Rt().Place(new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(760, 100));
            var send = UIKit.Button(bar, "发送", UISkin.Instance.primary, 220, 100, null, 36, "Send");
            send.Rt().Place(new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(220, 100));

            var tip = UIKit.Button(s.Root, "3 条新消息", UISkin.Instance.accent, 300, 80, "arrow_down", 30, "NewTip");
            tip.Rt().Place(new Vector2(1, 0), new Vector2(-30, 200), new Vector2(300, 80));

            var chat = s.Root.gameObject.AddComponent<ChatPanel>();
            Set(chat, "m_List", list);
            Set(chat, "m_Measure", other.Find("Bubble/Text").GetComponent<TextMeshProUGUI>());
            Set(chat, "m_Input", input);
            Set(chat, "m_Send", send);
            Set(chat, "m_NewTip", tip);
            Set(chat, "m_NewTipText", tip.GetComponentInChildren<TextMeshProUGUI>());
            Set(chat, "m_AutoChat", auto);
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                UnityEngine.Random.InitState(5);
                chat.Init();
                chat.BotSay(); chat.BotSay(); chat.BotSay(); chat.BotSay();
                chat.Add(ChatKind.Me, "我", "avatar_1", "副本几点开？");
                chat.BotSay(); chat.BotSay();
                chat.Add(ChatKind.Me, "我", "avatar_1", "我来！等我换个装备");
                chat.BotSay(); chat.BotSay();
                chat.Add(ChatKind.Me, "我", "avatar_1", "第二阶段我来拉小怪，你们集中打 Boss。");
                chat.BotSay(); chat.BotSay(); chat.BotSay();
                chat.Add(ChatKind.Me, "我", "avatar_1", "好");
                if (panel == 1)
                {
                    chat.List.ScrollY = chat.List.MaxScrollY - 700;   // 用户往上翻看历史
                    chat.List.UpdateVisible();
                    chat.BotSay(); chat.BotSay(); chat.BotSay();       // 这时又来了三条
                }
            });
            s.Label(new Vector2(0, 740), panel == 0 ? "在底部：新消息自动滚到底" : "往上翻时：不打断，只提示新消息",
                panel == 0 ? UIKit.Hex("#2FB36A") : UIKit.Hex("#E5484D"), 44);
        }

        // ---------------- 背包 ----------------
        static RectTransform CellTemplate(Transform parent)
        {
            var root = UIKit.Image(parent, "round_16", UIKit.Hex("#151827"), "CellTemplate");
            root.rectTransform.sizeDelta = new Vector2(140, 140);
            var q = UIKit.Image(root.transform, "round_16", Color.gray, "Quality");
            q.raycastTarget = false; q.rectTransform.Stretch(4, 4, 4, 4);
            var shade = UIKit.Image(root.transform, "gradient_v", new Color(0, 0, 0, 0.35f), "Shade");
            shade.raycastTarget = false; shade.rectTransform.Stretch(4, 4, 4, 4);
            var icon = UIKit.Icon(root.transform, "item_sword", 100, Color.white, "Icon");
            var count = UIKit.Text(root.transform, "", 28, Color.white, TextAlignmentOptions.BottomRight, true, "Count");
            count.rectTransform.Stretch(8, 8, 12, 6);
            var sel = UIKit.Image(root.transform, "frame_select", Color.white, "Selected");
            sel.raycastTarget = false; sel.rectTransform.Stretch(-5, -5, -5, -5);
            var hint = UIKit.Image(root.transform, "round_16", new Color(1, 1, 1, 0.4f), "DropHint");
            hint.raycastTarget = false; hint.rectTransform.Stretch();
            var cell = root.gameObject.AddComponent<BagCell>();
            Set(cell, "m_Quality", q);
            Set(cell, "m_Icon", icon);
            Set(cell, "m_Count", count);
            Set(cell, "m_Selected", sel.gameObject);
            Set(cell, "m_DropHint", hint.gameObject);
            root.gameObject.SetActive(false);
            return root.rectTransform;
        }

        static Toggle Tab(Transform parent, string label, ToggleGroup group, bool on)
        {
            var bg = UIKit.Image(parent, "pill", UIKit.Hex("#2B314A"), "Tab_" + label);
            bg.rectTransform.sizeDelta = new Vector2(180, 72);
            var onImg = UIKit.Image(bg.transform, "pill", UISkin.Instance.primary, "On");
            onImg.raycastTarget = false; onImg.rectTransform.Stretch();
            var t = UIKit.Text(bg.transform, label, 32, Color.white, TextAlignmentOptions.Center, true, "Label");
            t.rectTransform.Stretch();
            var tg = bg.gameObject.AddComponent<Toggle>();
            tg.targetGraphic = bg;
            tg.graphic = onImg;
            tg.group = group;
            tg.isOn = on;
            return tg;
        }

        static BagView BuildBag(Shot s)
        {
            s.Background(Dark);
            var top = UIKit.Node(s.Root, "Toolbar");
            top.anchorMin = new Vector2(0, 1); top.anchorMax = Vector2.one; top.pivot = new Vector2(0.5f, 1);
            top.sizeDelta = new Vector2(-120, 120); top.anchoredPosition = new Vector2(0, -30);
            var tabs = UIKit.Node(top, "Tabs");
            tabs.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(800, 72));
            tabs.HLayout(16, 0, TextAnchor.MiddleLeft);
            var group = tabs.gameObject.AddComponent<ToggleGroup>();
            var tabList = new List<Toggle>();
            string[] names = { "全部", "装备", "消耗品", "材料" };
            for (int i = 0; i < names.Length; i++) tabList.Add(Tab(tabs, names[i], group, i == 0));
            var cap = T(top, "0/0", 30, Soft, TextAlignmentOptions.MidlineRight, false, "Capacity", new Vector2(1, 0.5f), new Vector2(-700, 0), new Vector2(240, 60));
            var sort = UIKit.Dropdown(top, new[] { "按品质", "按数量", "按名称" }, 260, 80, 30);
            sort.Rt().Place(new Vector2(1, 0.5f), new Vector2(-220, 0), new Vector2(260, 80));
            var arrange = UIKit.Button(top, "整理", UISkin.Instance.accent, 190, 80, null, 32, "Arrange");
            arrange.Rt().Place(new Vector2(1, 0.5f), Vector2.zero, new Vector2(190, 80));

            var sr = UIKit.ScrollView(s.Root, true, false, 1800, 840);
            sr.name = "BagGrid";
            var srt = sr.Rt();
            srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.one; srt.offsetMin = new Vector2(60, 50); srt.offsetMax = new Vector2(-60, -170);
            var gbg = UIKit.Image(sr.transform, "round_24", Card, "Bg");
            gbg.rectTransform.Stretch(); gbg.transform.SetAsFirstSibling(); gbg.raycastTarget = true;
            var grid = sr.gameObject.AddComponent<LoopGridView>();
            Set(grid, "m_Template", CellTemplate(sr.transform));
            Set(grid, "m_CellSize", new Vector2(140, 140));
            Set(grid, "m_Spacing", new Vector2(16, 16));
            Set(grid, "m_PaddingTop", 24f);
            Set(grid, "m_PaddingBottom", 24f);

            // Tips 层
            var tipsLayer = UIKit.Node(s.Root, "TipsLayer");
            tipsLayer.Stretch();
            var blocker = UIKit.Image(tipsLayer, null, new Color(0, 0, 0, 0.001f), "Blocker");
            blocker.rectTransform.Stretch();
            var blockerBtn = blocker.gameObject.AddComponent<Button>();
            blockerBtn.transition = Selectable.Transition.None;
            var panel = UIKit.Image(tipsLayer, "round_16", new Color(0.09f, 0.1f, 0.16f, 0.97f), "Panel").rectTransform;
            panel.sizeDelta = new Vector2(480, 300);
            var vl = panel.VLayout(10, 24, TextAnchor.UpperLeft, true);
            vl.childControlHeight = true;
            var fit = panel.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var tIcon = UIKit.Icon(panel, "item_sword", 96, Color.white, "Icon");
            tIcon.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            tIcon.rectTransform.Place(new Vector2(1, 1), new Vector2(-24, -24), new Vector2(96, 96));
            var tName = UIKit.Text(panel, "名字", 38, Color.white, TextAlignmentOptions.TopLeft, true, "Name");
            tName.margin = new Vector4(0, 0, 110, 0);
            var tType = UIKit.Text(panel, "类型", 26, Soft, TextAlignmentOptions.TopLeft, false, "Type");
            tType.margin = new Vector4(0, 0, 110, 0);
            var div = UIKit.Image(panel, null, new Color(1, 1, 1, 0.12f), "Divider");
            div.gameObject.AddComponent<LayoutElement>().preferredHeight = 2;
            var tDesc = UIKit.Text(panel, "描述", 28, UIKit.Hex("#DDE2EE"), TextAlignmentOptions.TopLeft, false, "Desc");
            tDesc.lineSpacing = 10;
            var tFoot = UIKit.Text(panel, "数量", 26, UIKit.Hex("#FFB547"), TextAlignmentOptions.TopLeft, false, "Footer");
            var tips = tipsLayer.gameObject.AddComponent<ItemTips>();
            Set(tips, "m_Panel", panel);
            Set(tips, "m_Icon", tIcon);
            Set(tips, "m_NameText", tName);
            Set(tips, "m_Type", tType);
            Set(tips, "m_Desc", tDesc);
            Set(tips, "m_Footer", tFoot);
            Set(tips, "m_Blocker", blockerBtn);
            tipsLayer.gameObject.SetActive(false);

            // 拖拽层
            var drag = UIKit.Node(s.Root, "DragLayer");
            drag.Stretch();
            var ghost = UIKit.Icon(drag, "item_sword", 130, new Color(1, 1, 1, 0.9f), "Ghost");
            ghost.gameObject.SetActive(false);

            var hint = T(s.Root, "点格子看详情 · 按住 0.3 秒再拖动可交换位置 · 直接拖动是滚动", 26, Soft, TextAlignmentOptions.Center, false, "Hint",
                new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(1400, 40));
            var view = s.Root.gameObject.AddComponent<BagView>();
            Set(view, "m_Grid", grid);
            Set(view, "m_Tips", tips);
            Set(view, "m_Tabs", tabList.ToArray());
            Set(view, "m_SortBy", sort);
            Set(view, "m_Arrange", arrange);
            Set(view, "m_Capacity", cap);
            Set(view, "m_DragLayer", drag);
            Set(view, "m_Ghost", ghost);
            return view;
        }

        [BookShot("ch18-bag", Scene = "Ch18_Bag")]
        static void Bag(Shot s)
        {
            var view = BuildBag(s);
            s.BeforeCapture(() =>
            {
                view.Init();
                view.Grid.ScrollY = 300;
                view.Grid.UpdateVisible();
                int idx = view.Grid.FirstVisible + view.Grid.Columns * 2 + 3;
                view.Select(idx, view.Grid.GetItem(idx));
            });
        }

        [BookShot("ch18-bag-drag")]
        static void BagDrag(Shot s)
        {
            var view = BuildBag(s);
            s.BeforeCapture(() =>
            {
                view.Init();
                view.Arrange();   // 按品质整理
                int from = view.Grid.Columns + 2, over = view.Grid.Columns * 2 + 5;
                var a = view.Grid.GetItem(from);
                var b = view.Grid.GetItem(over);
                var drag = (RectTransform)s.Root.Find("DragLayer");
                Vector2 pb = drag.InverseTransformPoint(b.TransformPoint(b.rect.center));
                view.PreviewDrag(from, over, pb + new Vector2(30, -20));
                a.Find("Icon").GetComponent<Image>().color = new Color(1, 1, 1, 0.3f);   // 原位置半透明
            });
            s.Label(new Vector2(0, -470), "按住 0.3 秒后拖动：图标跟手，目标格子高亮，松手交换", UIKit.Hex("#FF8A3D"), 34);
        }

        // ---------------- 排行榜 ----------------
        static RankRowView RowTemplate(Transform parent, string name)
        {
            var bg = UIKit.Image(parent, "round_16", Card, name);
            bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = new Vector2(1000, 120);
            var medal = UIKit.Image(bg.transform, "circle", Color.yellow, "Medal");
            medal.raycastTarget = false;
            medal.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(70, 70));
            var rank = T(bg.transform, "1", 34, Color.white, TextAlignmentOptions.Center, true, "Rank", new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(90, 60));
            var av = UIKit.Image(bg.transform, "avatar_2", Color.white, "Avatar");
            av.raycastTarget = false;
            av.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(84, 84));
            var nm = T(bg.transform, "名字", 34, Color.white, TextAlignmentOptions.MidlineLeft, true, "Name", new Vector2(0, 0.5f), new Vector2(240, 0), new Vector2(480, 60));
            var sc = T(bg.transform, "0", 34, UIKit.Hex("#FFC53D"), TextAlignmentOptions.MidlineRight, true, "Score", new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(260, 60));
            var view = bg.gameObject.AddComponent<RankRowView>();
            Set(view, "m_Bg", bg);
            Set(view, "m_Medal", medal);
            Set(view, "m_Rank", rank);
            Set(view, "m_Avatar", av);
            Set(view, "m_NameText", nm);
            Set(view, "m_Score", sc);
            return view;
        }

        [BookShot("ch18-rank", W = 360, H = 640, Panels = 3, Scene = "Ch18_Rank")]
        static void Rank(Shot s)
        {
            s.Ref(1080, 1920);
            s.Background(Dark);
            T(s.Root, "全服战力榜 · 1000 人", 52, Color.white, TextAlignmentOptions.Center, true, "Title", new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(800, 76));
            var list = NewList(s.Root, "RankList", 1080, 1500);
            var lrt = list.Rt();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = new Vector2(0, 170); lrt.offsetMax = new Vector2(0, -210);
            var tpl = RowTemplate(list.transform, "RowTemplate");
            tpl.gameObject.SetActive(false);
            Set(list, "m_Templates", new[] { (RectTransform)tpl.transform });
            Set(list, "m_DefaultHeight", 120f);
            Set(list, "m_Spacing", 12f);
            Set(list, "m_PaddingLeft", 30f);
            Set(list, "m_PaddingRight", 30f);
            // 固定行：放在一个和列表视口同样大小的层里，贴顶或贴底
            var pinArea = UIKit.Node(s.Root, "PinnedArea");
            pinArea.anchorMin = Vector2.zero; pinArea.anchorMax = Vector2.one; pinArea.offsetMin = new Vector2(0, 170); pinArea.offsetMax = new Vector2(0, -210);
            var pinned = RowTemplate(pinArea, "MyRankPinned");
            var prt = (RectTransform)pinned.transform;
            prt.anchorMin = new Vector2(0, 0); prt.anchorMax = new Vector2(1, 0); prt.pivot = new Vector2(0.5f, 0);
            prt.sizeDelta = new Vector2(-60, 130); prt.anchoredPosition = Vector2.zero;
            var sh = UIKit.Image(pinned.transform, "shadow_16", new Color(0, 0, 0, 0.5f), "Shadow");
            sh.raycastTarget = false; sh.rectTransform.Stretch(-18, -14, -18, -22); sh.transform.SetAsFirstSibling();
            var locate = UIKit.Button(s.Root, "定位我", UISkin.Instance.accent, 400, 100, "user", 36, "LocateMe");
            locate.Rt().Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(400, 100));
            var rank = s.Root.gameObject.AddComponent<RankPanel>();
            Set(rank, "m_List", list);
            Set(rank, "m_Pinned", pinned);
            Set(rank, "m_LocateMe", locate);
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                rank.Init();
                if (panel == 1) rank.List.ScrollTo(385, 0.5f);
                if (panel == 2) rank.List.ScrollTo(520, 0f);
                rank.List.UpdateVisible();
            });
            string[] cap = { "我在下方：固定行贴底", "我在视口内：固定行隐藏", "我在上方：固定行贴顶" };
            s.Label(new Vector2(0, 815), cap[panel], UISkin.Instance.accent, 46);
        }
    }
}
