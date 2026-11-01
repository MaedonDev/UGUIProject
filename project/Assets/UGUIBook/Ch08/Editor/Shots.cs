// 第 8 章配图与配套场景（搭景代码，不进书）
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    static class Kit
    {
        public static UISkin Skin => UISkin.Instance;

        public static TextMeshProUGUI T(Transform p, string s, float size, Color c, TextAlignmentOptions a = TextAlignmentOptions.MidlineLeft, bool bold = false, string name = "Text")
        {
            var t = UIKit.Text(p, s, size, c, a, bold, name);
            t.enableWordWrapping = false;
            return t;
        }

        public static RectTransform Pin(this RectTransform rt, Vector2 anchor, Vector2 pivot, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = anchor; rt.pivot = pivot;
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Card(Transform p, float x, float y, float w, float h, string name = "Card")
        {
            var c = UIKit.Card(p, name);
            c.Place(x, y, w, h);
            return c;
        }

        /// 截图专用：在 Text Area 里放一个“光标 + 选区”的静态示意（真正的 Caret 只在 Play 模式由 TMP_InputField 动态创建）
        public static (RectTransform sel, RectTransform bar) FakeCaretNodes(TMP_InputField f)
        {
            var root = UIKit.Node(f.textViewport, "Caret");
            root.Stretch(); root.SetAsFirstSibling();
            var sel = UIKit.Image(root, null, f.selectionColor, "sel"); sel.raycastTarget = false;
            var bar = UIKit.Image(root, null, Skin.ink, "bar"); bar.raycastTarget = false;
            return (sel.rectTransform, bar.rectTransform);
        }

        /// 在 BeforeCapture 里调用：按 TMP 的字符信息把选区 [from, to) 和光标（在 caret 处）摆到正确位置
        public static void PlaceCaret(TMP_InputField f, RectTransform sel, RectTransform bar, int from, int to, int caret)
        {
            var t = f.textComponent;
            t.ForceMeshUpdate();
            var info = t.textInfo;
            var line = info.lineInfo[0];
            float X(int i) => i < info.characterCount ? info.characterInfo[i].origin : info.characterInfo[info.characterCount - 1].xAdvance;
            // Text 和 Caret 节点都撑满 Text Area，本地坐标一致
            var off = (Vector2)t.rectTransform.localPosition;
            foreach (var r in new[] { sel, bar }) { r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f); r.pivot = new Vector2(0, 0); }
            sel.anchoredPosition = new Vector2(X(from), line.descender) + off;
            sel.sizeDelta = new Vector2(X(to) - X(from), line.ascender - line.descender);
            sel.gameObject.SetActive(to > from);
            bar.anchoredPosition = new Vector2(X(caret) - 1.5f, line.descender) + off;
            bar.sizeDelta = new Vector2(3, line.ascender - line.descender);
        }

        /// 把 UIKit.Dropdown 的选项行调整到适合大字号（默认模板的勾选标记只有 20×20）
        public static void DressDropdown(TMP_Dropdown dd, float h, bool withImage)
        {
            var item = (RectTransform)dd.template.Find("Viewport/Content/Item");
            var ck = (RectTransform)item.Find("Item Checkmark");
            ck.anchoredPosition = new Vector2(h * 0.42f, 0); ck.sizeDelta = new Vector2(h * 0.34f, h * 0.34f);
            ck.GetComponent<Image>().color = Skin.primary;
            var lb = (RectTransform)item.Find("Item Label");
            lb.offsetMin = new Vector2(h * (withImage ? 1.55f : 0.8f), 0); lb.offsetMax = new Vector2(-16, 0);
            lb.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
            item.Find("Item Background").GetComponent<Image>().color = Color.white;
            var tpl = dd.template.GetComponent<Image>(); tpl.sprite = Shot.Sp("round_16"); tpl.color = Color.white;
            var outline = UIKit.Image(dd.template, "round_outline_16", new Color(0.78f, 0.8f, 0.88f), "Outline");
            outline.rectTransform.Stretch(); outline.raycastTarget = false;
            dd.template.anchoredPosition = new Vector2(0, -8);
            dd.captionText.alignment = TextAlignmentOptions.MidlineLeft;
            if (!withImage) return;
            // Item Image：选项行里的小图（OptionData.image）
            var ii = UIKit.Image(item, null, Color.white, "Item Image");
            ii.rectTransform.Pin(new Vector2(0, 0.5f), new Vector2(0, 0.5f), h * 0.75f, 0, h * 0.6f, h * 0.4f);
            ii.raycastTarget = false; ii.preserveAspect = true;
            dd.itemImage = ii;
            // Caption Image：收起时标题左边的小图
            var ci = UIKit.Image(dd.transform, null, Color.white, "Caption Image");
            ci.rectTransform.Pin(new Vector2(0, 0.5f), new Vector2(0, 0.5f), 28, 0, h * 0.6f, h * 0.4f);
            ci.raycastTarget = false; ci.preserveAspect = true;
            dd.captionImage = ci;
            ((RectTransform)dd.captionText.transform).offsetMin = new Vector2(28 + h * 0.6f + 20, 6);
        }

        /// 截图专用：批处理不进 Play 模式，TMP_Dropdown.Start 没跑过（淡入用的 TweenRunner 还没建），
        /// 先反射补调 Start，再调真正的 Show()，最后把淡入中的 CanvasGroup 直接设成不透明
        public static RectTransform Expand(TMP_Dropdown dd)
        {
            typeof(TMP_Dropdown).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(dd, null);
            dd.Show();
            var list = (RectTransform)dd.transform.Find("Dropdown List");
            if (list != null) list.GetComponent<CanvasGroup>().alpha = 1;
            // 场景关闭时 OnDisable 会用 Destroy() 销毁列表和 Blocker，编辑模式下这会报错；
            // 把这两个私有引用清空，让它们随场景一起被卸载即可
            foreach (var f in new[] { "m_Dropdown", "m_Blocker" })
                typeof(TMP_Dropdown).GetField(f, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(dd, null);
            return list;
        }
    }

    static class Shots
    {
        static UISkin Skin => UISkin.Instance;
        static readonly Vector2 L = new Vector2(0, 0.5f), C = new Vector2(0.5f, 0.5f), Rt = new Vector2(1, 0.5f);

        // ---------------------------------------------------------------- 8.1 结构
        [BookShot("ch08-input-anatomy", W = 960, H = 420)]
        static void InputAnatomy(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1640, 640);
            var a = UIKit.Input(card, "请输入昵称…", 700, 110, 40); a.name = "InputField_空";
            a.Rt().Place(-400, 20, 700, 110);
            var b = UIKit.Input(card, "请输入昵称…", 700, 110, 40); b.name = "InputField (TMP)";
            b.Rt().Place(400, 20, 700, 110);
            b.text = "麦冬的背包";
            b.transform.Find("Outline").GetComponent<Image>().color = Skin.primary;   // 获得焦点的样子
            var (sel, bar) = Kit.FakeCaretNodes(b);
            s.BeforeCapture(() => Kit.PlaceCaret(b, sel, bar, 3, 5, 5));

            Kit.T(card, "未输入：显示 Placeholder", 30, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(-400, -200, 700, 50);
            Kit.T(card, "编辑中：Text + 光标 + 选区（“背包”被选中）", 30, Skin.muted, TextAlignmentOptions.Center).rectTransform.Place(400, -200, 760, 50);
            s.Box(a.placeholder.rectTransform, Skin.accent, "Placeholder", 4, true);
            s.Box(a.textViewport, UIKit.Hex("#1E88E5"), "Text Area（RectMask2D）", 14);
            s.Box(b.textViewport, UIKit.Hex("#1E88E5"), "Text Area", 14);
            s.Box(b.textComponent.rectTransform, Skin.success, "Text（TextMeshProUGUI）", 4, true);
            s.LabelAt(b.Rt(), new Vector2(0.62f, 1), new Vector2(0, 150), "Caret（Play 时动态创建，绘制光标和选区）", Skin.danger, 26);
            s.Arrow(() => s.At(b.Rt(), new Vector2(0.55f, 1)) + new Vector2(0, 128), () => s.At(b.textViewport, new Vector2(0.24f, 0.85f)), Skin.danger, 5);
        }

        // ---------------------------------------------------------------- 8.2 Content Type
        [BookShot("ch08-content-types", W = 960, H = 540, Scene = "Ch08_ContentTypes")]
        static void ContentTypes(Shot s)
        {
            (TMP_InputField.ContentType type, string note, string text)[] rows =
            {
                (TMP_InputField.ContentType.Standard, "不限制字符", "你好，Unity UI"),
                (TMP_InputField.ContentType.IntegerNumber, "只收数字和开头的负号", "-128"),
                (TMP_InputField.ContentType.DecimalNumber, "数字、负号和一个小数点", "3.14159"),
                (TMP_InputField.ContentType.EmailAddress, "字母数字 + 邮箱允许的符号", "maidong@example.com"),
                (TMP_InputField.ContentType.Password, "显示成星号，内容不限", "secret123"),
                (TMP_InputField.ContentType.Pin, "只收数字，并显示成星号", "8848"),
            };
            for (int i = 0; i < rows.Length; i++)
            {
                int col = i % 2, row = i / 2;
                float x = col == 0 ? -470 : 470, y = 300 - row * 300;
                var card = Kit.Card(s.Root, x, y, 880, 260, "Card_" + rows[i].type);
                Kit.T(card, rows[i].type.ToString(), 34, Skin.primary, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 40, -45, 400, 50);
                Kit.T(card, rows[i].note, 28, Skin.muted, TextAlignmentOptions.MidlineRight).rectTransform.Pin(new Vector2(1, 1), Rt, -40, -45, 460, 50);
                var f = UIKit.Input(card, "在这里输入试试", 800, 100, 38);
                f.Rt().Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0), 0, 36, 800, 100);
                f.contentType = rows[i].type;
                string txt = rows[i].text;
                s.BeforeCapture(() => f.text = txt);   // 截图时才填示例文字，场景里保持空白给读者自己试
            }
        }

        // ---------------------------------------------------------------- 8.3 三种校验方式
        [BookShot("ch08-validation", W = 960, H = 460, Scene = "Ch08_Validation")]
        static void Validation(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 20, 1700, 780);
            (string title, string how, string typed, string result)[] rows =
            {
                ("年龄", "① Content Type = Integer Number", "键入：28abc", "28"),
                ("兑换码", "② onValidateInput：只收字母数字，自动大写", "键入：vip-8888a", "VIP8888A"),
                ("手机号", "③ Input Validator = PhoneValidator 资源", "键入：13812345678", "138 1234 5678"),
            };
            var fields = new TMP_InputField[3];
            for (int i = 0; i < 3; i++)
            {
                float y = 250 - i * 200;
                Kit.T(card, rows[i].title, 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(L, L, 70, y, 200, 60);
                var f = UIKit.Input(card, "", 520, 100, 40);
                f.Rt().Pin(L, L, 260, y, 520, 100);
                fields[i] = f;
                Kit.T(card, rows[i].how, 32, Skin.ink).rectTransform.Pin(L, L, 840, y + 24, 820, 50);
                Kit.T(card, rows[i].typed, 28, Skin.muted).rectTransform.Pin(L, L, 840, y - 26, 820, 44);
                string r = rows[i].result; var ff = f;
                s.BeforeCapture(() => ff.text = r);
            }
            fields[0].contentType = TMP_InputField.ContentType.IntegerNumber;
            fields[0].characterLimit = 3;
            const string dir = "Assets/UGUIBook/Ch08/Validators", path = dir + "/PhoneValidator.asset";
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch08", "Validators");
            var v = AssetDatabase.LoadAssetAtPath<PhoneValidator>(path);
            if (v == null) { v = ScriptableObject.CreateInstance<PhoneValidator>(); AssetDatabase.CreateAsset(v, path); }
            fields[2].inputValidator = v;            // 会自动把 Content Type 改成 Custom、Character Validation 改成 Custom Validator
            fields[2].keyboardType = TouchScreenKeyboardType.PhonePad;
            var log = Kit.T(card, "依次在三个输入框里随便敲字试试", 30, Skin.accent, TextAlignmentOptions.Center, false, "Log");
            log.rectTransform.Pin(new Vector2(0.5f, 0), new Vector2(0.5f, 0), 0, 40, 1400, 50);
            card.gameObject.AddComponent<ValidationDemo>().Setup(fields[0], fields[1], fields[2], log);
        }

        // ---------------------------------------------------------------- 案例 8.1 登录表单
        [BookShot("ch08-login", W = 640, H = 540, Panels = 2, Scene = "Ch08_Login")]
        static void Login(Shot s)
        {
            s.Background(UIKit.Hex("#E7E9F4"));
            var card = Kit.Card(s.Root, 0, 0, 1000, 980, "LoginPanel");
            var logo = UIKit.Image(card, "circle", Skin.primary, "Logo"); logo.rectTransform.Pin(new Vector2(0.5f, 1), C, 0, -110, 120, 120);
            UIKit.Icon(logo.transform, "user", 64, Color.white).rectTransform.Place(0, 0, 64, 64);
            Kit.T(card, "欢迎登录", 52, Skin.ink, TextAlignmentOptions.Center, true).rectTransform.Pin(new Vector2(0.5f, 1), C, 0, -225, 600, 70);

            var top = new Vector2(0.5f, 1);
            Kit.T(card, "用户名", 32, Skin.ink).rectTransform.Pin(top, C, 0, -310, 820, 50);
            var user = UIKit.Input(card, "4～16 位字母、数字或下划线", 820, 100, 38); user.name = "Input_User";
            user.Rt().Pin(top, C, 0, -380, 820, 100);
            var userErr = Kit.T(card, "", 28, Skin.danger, TextAlignmentOptions.MidlineLeft, false, "UserError");
            userErr.rectTransform.Pin(top, C, 0, -452, 820, 40);

            Kit.T(card, "密码", 32, Skin.ink).rectTransform.Pin(top, C, 0, -510, 820, 50);
            var pwd = UIKit.Input(card, "至少 6 位", 820, 100, 38); pwd.name = "Input_Password";
            pwd.Rt().Pin(top, C, 0, -580, 820, 100);
            pwd.contentType = TMP_InputField.ContentType.Password;
            var pwdErr = Kit.T(card, "", 28, Skin.danger, TextAlignmentOptions.MidlineLeft, false, "PasswordError");
            pwdErr.rectTransform.Pin(top, new Vector2(0, 0.5f), -410, -652, 500, 40);
            var show = UIKit.Toggle(card, "显示密码", false, 240, 56);
            show.Rt().Pin(top, Rt, 410, -652, 240, 56);

            var login = UIKit.Button(card, "登 录", Skin.primary, 820, 104, null, 40);
            login.Rt().Pin(top, C, 0, -770, 820, 104);
            var status = Kit.T(card, "", 30, Skin.success, TextAlignmentOptions.Center, false, "Status");
            status.rectTransform.Pin(top, C, 0, -855, 820, 46);
            Kit.T(card, "忘记密码？    ·    注册新账号", 28, Skin.muted, TextAlignmentOptions.Center).rectTransform.Pin(new Vector2(0.5f, 0), C, 0, 50, 820, 46);

            var form = card.gameObject.AddComponent<LoginForm>();
            form.Setup(user, pwd, userErr, pwdErr, show, login, status);
            if (s.Panel == 0)
                s.BeforeCapture(() =>
                {
                    user.text = "maidong_01";
                    show.isOn = true;
                    pwd.contentType = TMP_InputField.ContentType.Standard;
                    pwd.text = "mypass2024";
                    pwd.transform.Find("Outline").GetComponent<Image>().color = Skin.primary;
                });
            else
                s.BeforeCapture(() =>
                {
                    user.text = "ab";
                    pwd.text = "123";
                    form.ShowError(user, userErr, "用户名需 4～16 位字母、数字或下划线");
                    form.ShowError(pwd, pwdErr, "密码至少 6 位");
                });
        }

        // ---------------------------------------------------------------- 案例 8.2 聊天输入
        [BookShot("ch08-chat", W = 960, H = 540, Scene = "Ch08_Chat")]
        static void Chat(Shot s)
        {
            var win = Kit.Card(s.Root, 0, 0, 1240, 1010, "ChatWindow");
            var head = UIKit.Image(win, "round_16", Skin.primary, "Header");
            head.rectTransform.anchorMin = new Vector2(0, 1); head.rectTransform.anchorMax = new Vector2(1, 1); head.rectTransform.pivot = new Vector2(0.5f, 1);
            head.rectTransform.sizeDelta = new Vector2(0, 110); head.rectTransform.anchoredPosition = Vector2.zero;
            Kit.T(head.transform, "队伍频道 · 3 人在线", 38, Color.white, TextAlignmentOptions.Center, true).rectTransform.Stretch();

            var sr = UIKit.ScrollView(win, true, false, 1200, 760);
            sr.Rt().anchorMin = Vector2.zero; sr.Rt().anchorMax = Vector2.one; sr.Rt().offsetMin = new Vector2(20, 140); sr.Rt().offsetMax = new Vector2(-20, -115);
            var content = sr.content;
            content.sizeDelta = new Vector2(0, 0);
            var vl = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vl.padding = new RectOffset(24, 24, 24, 24); vl.spacing = 22;
            vl.childControlWidth = true; vl.childForceExpandWidth = true; vl.childControlHeight = true; vl.childForceExpandHeight = false;
            var fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform Template(bool mine)
            {
                var item = UIKit.Node(sr.transform, mine ? "Template_Mine" : "Template_Other");
                item.gameObject.AddComponent<LayoutElement>().preferredHeight = 90;
                var side = mine ? new Vector2(1, 1) : new Vector2(0, 1);
                var av = UIKit.Image(item, mine ? "avatar_1" : "avatar_3", Color.white, "Avatar");
                av.rectTransform.Pin(side, side, 0, 0, 80, 80);
                var bubble = UIKit.Image(item, "round_24", mine ? Skin.primary : Skin.page, "Bubble");
                bubble.rectTransform.Pin(side, side, mine ? -100 : 100, 0, 200, 80);
                var t = UIKit.Text(bubble.transform, "", 36, mine ? Color.white : Skin.ink, TextAlignmentOptions.TopLeft, false, "Text");
                t.enableWordWrapping = true; t.rectTransform.Stretch(28, 18, 28, 18);
                item.gameObject.SetActive(false);
                return item;
            }
            var mineT = Template(true); var otherT = Template(false);

            var bar = UIKit.Node(win, "InputBar"); bar.anchorMin = Vector2.zero; bar.anchorMax = new Vector2(1, 0); bar.pivot = new Vector2(0.5f, 0);
            bar.sizeDelta = new Vector2(0, 130); bar.anchoredPosition = Vector2.zero;
            var input = UIKit.Input(bar, "说点什么…（回车发送）", 920, 96, 36);
            input.Rt().Pin(L, L, 30, 0, 930, 96);
            var send = UIKit.Button(bar, "发送", Skin.primary, 220, 96, null, 36);
            send.Rt().Pin(Rt, Rt, -30, 0, 220, 96);

            var chat = win.gameObject.AddComponent<ChatInput>();
            chat.Setup(input, send, sr, mineT, otherT);
            s.BeforeCapture(() =>
            {
                chat.AddMessage("有人在吗？", false);
                chat.AddMessage("在的！", true);
                chat.AddMessage("今晚八点打团本，大家记得带药水。", false);
                chat.AddMessage("收到！我这边还差一个火抗戒指，打完副本顺路去主城买。", true);
                chat.AddMessage("我带了 20 瓶红药，够用。", false);
                chat.AddMessage("那我开组了，邀请你们～", true);
                input.text = "顺便把宝石也带";
            });
        }

        // ---------------------------------------------------------------- 案例 8.3 搜索过滤
        static readonly SearchFilter.Entry[] kItems =
        {
            new SearchFilter.Entry { name = "红色药水", sprite = "item_potion_red" },
            new SearchFilter.Entry { name = "蓝色药水", sprite = "item_potion_blue" },
            new SearchFilter.Entry { name = "绿色药水", sprite = "item_potion_green" },
            new SearchFilter.Entry { name = "精钢长剑", sprite = "item_sword" },
            new SearchFilter.Entry { name = "骑士盾牌", sprite = "item_shield" },
            new SearchFilter.Entry { name = "骑士头盔", sprite = "item_helmet" },
            new SearchFilter.Entry { name = "猎人长弓", sprite = "item_bow" },
            new SearchFilter.Entry { name = "火抗戒指", sprite = "item_ring" },
            new SearchFilter.Entry { name = "蓝宝石", sprite = "item_gem" },
            new SearchFilter.Entry { name = "地牢钥匙", sprite = "item_key" },
            new SearchFilter.Entry { name = "回城卷轴", sprite = "item_scroll" },
            new SearchFilter.Entry { name = "金币袋", sprite = "item_coin" },
        };

        [BookShot("ch08-search", W = 640, H = 540, Panels = 2, Scene = "Ch08_Search")]
        static void Search(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1160, 1000, "SearchPanel");
            var top = new Vector2(0.5f, 1);
            var box = UIKit.Image(card, "pill", Skin.page, "SearchBox"); box.rectTransform.Pin(top, C, 0, -90, 1040, 100);
            box.pixelsPerUnitMultiplier = 0.64f;
            UIKit.Icon(box.transform, "search", 48, Skin.muted).rectTransform.Pin(L, L, 36, 0, 48, 48);
            var input = UIKit.Input(box.transform, "搜索物品名称", 820, 100, 38);
            input.Rt().Pin(L, L, 100, 0, 820, 100);
            input.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            Object.DestroyImmediate(input.transform.Find("Outline").gameObject);
            var clear = UIKit.IconButton(box.transform, "close", new Color(0, 0, 0, 0.12f), 56, Color.white);
            clear.Rt().Pin(Rt, Rt, -28, 0, 56, 56);
            var count = Kit.T(card, "", 30, Skin.muted, TextAlignmentOptions.MidlineLeft, false, "Count");
            count.rectTransform.Pin(top, C, 0, -185, 1040, 50);

            var list = UIKit.Node(card, "List");
            list.anchorMin = new Vector2(0, 0); list.anchorMax = new Vector2(1, 1); list.offsetMin = new Vector2(60, 40); list.offsetMax = new Vector2(-60, -225);
            var grid = list.Grid(new Vector2(335, 150), new Vector2(17, 18));
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 3;

            var tpl = UIKit.Image(card, "round_16", Skin.page, "ItemTemplate");
            UIKit.Icon(tpl.transform, "item_coin", 96, Color.white, "Icon").rectTransform.Pin(L, L, 20, 0, 96, 96);
            var lb = Kit.T(tpl.transform, "物品", 34, Skin.ink, TextAlignmentOptions.MidlineLeft, false, "Label");
            lb.rectTransform.Stretch(130, 0, 10, 0);
            tpl.gameObject.SetActive(false);

            var filter = card.gameObject.AddComponent<SearchFilter>();
            filter.Setup(input, clear, list, tpl.rectTransform, count, kItems);
            string q = s.Panel == 0 ? "" : "药水";
            s.BeforeCapture(() => { filter.Build(); input.text = q; filter.Apply(q); });
        }

        // ---------------------------------------------------------------- 8.6 Dropdown 结构与展开
        static (TMP_Dropdown dd, RectTransform left) BuildDropdownDemo(Shot s)
        {
            var left = Kit.Card(s.Root, -420, 0, 900, 880, "ServerCard");
            Kit.T(left, "选择服务器", 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 60, -70, 500, 60);
            var dd = UIKit.Dropdown(left, new List<string> { "华东一区", "华东二区", "华南一区", "华北一区", "海外区" }, 760, 100, 38);
            dd.Rt().Pin(new Vector2(0.5f, 1), new Vector2(0.5f, 1), 0, -190, 760, 100);
            Kit.DressDropdown(dd, 100, false);

            var right = Kit.Card(s.Root, 500, 0, 760, 880, "Controls");
            Kit.T(right, "运行时修改选项", 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 60, -80, 600, 60);
            Button Btn(string t, float y)
            {
                var b = UIKit.Button(right, t, Skin.page, 620, 100, null, 34);
                b.Rt().Pin(new Vector2(0.5f, 1), C, 0, y, 620, 100);
                return b;
            }
            var add = Btn("添加一个选项", -200); var rm = Btn("删除最后一个选项", -330); var reset = Btn("重置", -460);
            var log = UIKit.Text(right, "点开下拉框选一项", 30, Skin.accent, TextAlignmentOptions.TopLeft, false, "Log");
            log.rectTransform.Pin(new Vector2(0.5f, 1), new Vector2(0.5f, 1), 0, -560, 620, 200);
            right.gameObject.AddComponent<DropdownOptionsDemo>().Setup(dd, add, rm, reset, log);
            return (dd, left);
        }

        [BookScene("Ch08_Dropdown")]
        static void DropdownScene(Shot s) => BuildDropdownDemo(s);

        [BookShot("ch08-dropdown-open", W = 960, H = 500)]
        static void DropdownOpen(Shot s)
        {
            var (dd, left) = BuildDropdownDemo(s);
            dd.value = 1;
            // 标注层只能框“搭景时就存在”的节点：先放一个空的代理节点，展开后把它对齐到 Dropdown List
            var proxy = UIKit.Node(dd.transform, "_Proxy");
            s.BeforeCapture(() =>
            {
                var list = Kit.Expand(dd);
                proxy.anchorMin = list.anchorMin; proxy.anchorMax = list.anchorMax; proxy.pivot = list.pivot;
                proxy.anchoredPosition = list.anchoredPosition; proxy.sizeDelta = list.sizeDelta;
            });
            s.Box(dd.Rt(), Skin.accent, "TMP_Dropdown 本体（Caption + Arrow）", 8);
            s.Box(proxy, Skin.danger, "Dropdown List：Show() 时克隆 Template 得到", 10, true);
        }

        // ---------------------------------------------------------------- 案例 8.4 语言选择
        [BookShot("ch08-language", W = 960, H = 540, Scene = "Ch08_Language")]
        static void Language(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1100, 1000, "LanguagePanel");
            var gear = UIKit.Image(card, "circle", Skin.page, "IconBg"); gear.rectTransform.Pin(new Vector2(0, 1), C, 110, -100, 100, 100);
            UIKit.Icon(gear.transform, "gear", 56, Skin.primary).rectTransform.Place(0, 0, 56, 56);
            Kit.T(card, "语言 / Language", 44, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(new Vector2(0, 1), L, 190, -100, 700, 70);
            var dd = UIKit.Dropdown(card, new List<string> { "简体中文" }, 900, 104, 38);
            dd.Rt().Pin(new Vector2(0.5f, 1), new Vector2(0.5f, 1), 0, -200, 900, 104);
            Kit.DressDropdown(dd, 104, true);
            dd.template.sizeDelta = new Vector2(0, 104 * 0.8f * 6 + 16);
            var status = Kit.T(card, "", 30, Skin.muted, TextAlignmentOptions.Center, false, "Status");
            status.rectTransform.Pin(new Vector2(0.5f, 0), C, 0, 70, 900, 50);
            var lang = dd.gameObject.AddComponent<LanguageDropdown>();
            lang.Setup(status);
            s.BeforeCapture(() => { lang.Init(); Kit.Expand(dd); });
        }

        // ---------------------------------------------------------------- 案例 8.5 数字步进器
        [BookShot("ch08-stepper", W = 960, H = 420, Scene = "Ch08_Stepper")]
        static void Stepper(Shot s)
        {
            var card = Kit.Card(s.Root, 0, 0, 1600, 700, "ShopPanel");
            NumberStepper Row(float y, string sprite, string name, string desc, int price, int max, int value, out StepperDemo demo)
            {
                var slot = UIKit.Image(card, "slot", Color.white, "Slot"); slot.rectTransform.Pin(L, L, 60, y, 160, 160);
                UIKit.Icon(slot.transform, sprite, 110, Color.white).rectTransform.Place(0, 0, 110, 110);
                Kit.T(card, name, 40, Skin.ink, TextAlignmentOptions.MidlineLeft, true).rectTransform.Pin(L, L, 250, y + 40, 500, 60);
                Kit.T(card, desc, 28, Skin.muted).rectTransform.Pin(L, L, 250, y - 20, 520, 46);
                Kit.T(card, $"单价 ¥{price}", 28, Skin.muted).rectTransform.Pin(L, L, 250, y - 62, 400, 40);

                var minus = UIKit.IconButton(card, "minus", Skin.page, 84, Skin.ink); minus.Rt().Pin(L, C, 800, y, 84, 84);
                var input = UIKit.Input(card, "", 170, 84, 40); input.Rt().Pin(L, C, 930, y, 170, 84);
                input.textComponent.alignment = TextAlignmentOptions.Center;
                var plus = UIKit.IconButton(card, "plus", Skin.primary, 84, Color.white); plus.Rt().Pin(L, C, 1060, y, 84, 84);
                minus.gameObject.AddComponent<PressRepeat>(); plus.gameObject.AddComponent<PressRepeat>();
                var st = card.gameObject.AddComponent<NumberStepper>();
                st.Setup(input, minus, plus, 1, max, value);
                var total = Kit.T(card, "", 38, Skin.ink, TextAlignmentOptions.MidlineRight, false, "Total");
                total.rectTransform.Pin(Rt, Rt, -60, y, 300, 60);
                demo = card.gameObject.AddComponent<StepperDemo>();
                demo.Setup(st, total, price);
                return st;
            }
            var a = Row(150, "item_potion_red", "红色药水", "恢复 200 点生命值", 12, 99, 5, out var da);
            var b = Row(-150, "item_scroll", "回城卷轴", "每日限购 5 个", 30, 5, 5, out var db);
            var line = UIKit.Image(card, null, new Color(0, 0, 0, 0.06f), "Line"); line.rectTransform.Place(0, 0, 1480, 2);
            s.BeforeCapture(() =>
            {
                a.SetValue(5, false); da.Refresh(5);
                b.SetValue(5, false); db.Refresh(5);
            });
            s.Label(new Vector2(260, -255), "到达上限：+ 按钮被禁用", Skin.muted, 26);
        }
    }
}
