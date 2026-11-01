// 封面与宣传图用的画面（不进正文）
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Core
{
    static class CoverShots
    {
        [BookShot("cover-hero", W = 960, H = 585, Scale = 2f)]
        static void Hero(Shot s)
        {
            var skin = s.Skin;
            s.Background(UIKit.Hex("#141228"));
            var bg = UIKit.Image(s.Root, "bg_night", Color.white, "Bg");
            bg.rectTransform.Stretch();
            bg.color = new Color(1, 1, 1, 0.9f);
            // 背景按原比例铺满（不拉伸），否则月亮会变成椭圆
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = bg.sprite.rect.width / bg.sprite.rect.height;

            // 顶部 HUD：头像 + 血条 + 货币
            var hud = UIKit.Node(s.Root, "HUD").Place(new Vector2(0, 1), new Vector2(60, -50), new Vector2(760, 150));
            var av = UIKit.Image(hud, "circle", Color.white, "AvatarFrame");
            av.rectTransform.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(140, 140));
            var avImg = UIKit.Image(av.transform, "avatar_3", Color.white, "Avatar");
            avImg.rectTransform.Stretch(8, 8, 8, 8);
            var m = avImg.gameObject.AddComponent<Mask>(); m.showMaskGraphic = true;
            avImg.sprite = UIKit.Sp("circle"); avImg.color = UIKit.Hex("#64b5f6");
            var face = UIKit.Image(avImg.transform, "avatar_3", Color.white, "Face"); face.rectTransform.Stretch();
            var name = UIKit.Text(hud, "麦冬 Lv.32", 40, Color.white, TextAlignmentOptions.MidlineLeft, true);
            name.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(170, 36), new Vector2(500, 56));
            Bar(hud, new Vector2(170, -16), 520, 0.78f, skin.danger, "HP 7820 / 10000");
            Bar(hud, new Vector2(170, -60), 520, 0.45f, UIKit.Hex("#3d7be0"), "MP 450 / 1000");

            var money = UIKit.Node(s.Root, "Money").Place(new Vector2(1, 1), new Vector2(-60, -60), new Vector2(620, 80));
            money.HLayout(24, 0, TextAnchor.MiddleRight);
            Coin(money, "item_coin", "128,450");
            Coin(money, "item_gem", "3,260");

            // 左：背包
            var bag = UIKit.Card(s.Root, "Bag", 24, true, UIKit.Hex("#232742")).Place(new Vector2(0, 0), new Vector2(60, 60), new Vector2(900, 700));
            var bt = UIKit.Text(bag, "背包", 44, Color.white, TextAlignmentOptions.MidlineLeft, true);
            bt.rectTransform.Place(new Vector2(0, 1), new Vector2(40, -30), new Vector2(400, 70));
            var tabs = UIKit.Node(bag, "Tabs").Place(new Vector2(1, 1), new Vector2(-36, -34), new Vector2(420, 64));
            tabs.HLayout(12, 0, TextAnchor.MiddleRight);
            string[] tabNames = { "全部", "装备", "道具" };
            for (int i = 0; i < 3; i++)
                UIKit.Button(tabs, tabNames[i], i == 0 ? skin.primary : UIKit.Hex("#353a5c"), 120, 60, null, 28);
            var grid = UIKit.Node(bag, "Grid").Place(new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(828, 540));
            grid.Grid(new Vector2(150, 150), new Vector2(19, 19));
            string[] items = { "item_sword", "item_shield", "item_bow", "item_helmet", "item_ring", "item_potion_red", "item_potion_blue",
                "item_potion_green", "item_gem", "item_key", "item_scroll", "item_apple", "item_coin", "", "" };
            for (int i = 0; i < items.Length; i++)
            {
                var slot = UIKit.Image(grid, "slot", Color.white, "Slot");
                if (items[i] != "")
                {
                    var ic = UIKit.Icon(slot.transform, items[i], 110, Color.white);
                    if (i < 8)
                    {
                        var n = UIKit.Text(slot.transform, (i * 7 % 20 + 1).ToString(), 26, Color.white, TextAlignmentOptions.BottomRight, true);
                        n.rectTransform.Stretch(0, 0, 14, 8);
                    }
                }
                if (i == 4)
                {
                    var sel = UIKit.Image(slot.transform, "frame_select", skin.accent, "Select");
                    sel.rectTransform.Stretch(-6, -6, -6, -6);
                }
            }

            // 右：设置卡片
            var set = UIKit.Card(s.Root, "Settings", 24).Place(new Vector2(1, 0), new Vector2(-60, 60), new Vector2(780, 700));
            var st = UIKit.Text(set, "设置", 44, skin.ink, TextAlignmentOptions.MidlineLeft, true);
            st.rectTransform.Place(new Vector2(0, 1), new Vector2(44, -30), new Vector2(400, 70));
            var col = UIKit.Node(set, "Column").Place(new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(690, 520));
            col.VLayout(30, 0, TextAnchor.UpperLeft);
            Row(col, "音乐", 0.7f, skin);
            Row(col, "音效", 0.45f, skin);
            UIKit.Toggle(col, "显示伤害数字", true, 690, 70);
            UIKit.Toggle(col, "省电模式", false, 690, 70);
            var ok = UIKit.Button(set, "保存设置", skin.accent, 300, 96, "check");
            ok.Rt().Place(new Vector2(1, 0), new Vector2(-44, 40), new Vector2(300, 96));
        }

        static void Bar(Transform parent, Vector2 pos, float w, float v, Color c, string label)
        {
            var bg = UIKit.Image(parent, "pill", new Color(0, 0, 0, 0.45f), "Bar");
            bg.rectTransform.Place(new Vector2(0, 0.5f), pos, new Vector2(w, 34));
            var fill = UIKit.Image(bg.transform, "pill", c, "Fill");
            fill.rectTransform.anchorMin = Vector2.zero; fill.rectTransform.anchorMax = new Vector2(v, 1);
            fill.rectTransform.offsetMin = new Vector2(3, 3); fill.rectTransform.offsetMax = new Vector2(-3, -3);
            var t = UIKit.Text(bg.transform, label, 22, Color.white, TextAlignmentOptions.Center, true);
            t.rectTransform.Stretch();
        }

        static void Coin(Transform parent, string icon, string text)
        {
            var pill = UIKit.Image(parent, "pill", new Color(0, 0, 0, 0.45f), "Currency");
            pill.rectTransform.sizeDelta = new Vector2(280, 72);
            UIKit.Icon(pill.transform, icon, 64, Color.white).rectTransform.Place(new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(64, 64));
            var t = UIKit.Text(pill.transform, text, 32, Color.white, TextAlignmentOptions.MidlineRight, true);
            t.rectTransform.Stretch(80, 0, 28, 0);
        }

        static void Row(Transform parent, string label, float v, UISkin skin)
        {
            var row = UIKit.Node(parent, "Row_" + label).Size(690, 70);
            var t = UIKit.Text(row, label, 34, skin.ink, TextAlignmentOptions.MidlineLeft);
            t.rectTransform.Place(new Vector2(0, 0.5f), Vector2.zero, new Vector2(140, 60));
            UIKit.Slider(row, v, 520, 44).Rt().Place(new Vector2(1, 0.5f), Vector2.zero, new Vector2(520, 44));
        }
    }
}
