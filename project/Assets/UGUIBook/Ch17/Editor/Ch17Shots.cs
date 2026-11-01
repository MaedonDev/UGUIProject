// 第 17 章搭景：3D 小场景（基本几何体）+ HUD。批处理不进 Play 模式，状态都通过组件的公开方法摆好
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UGUIBook.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UGUIBook.Ch17
{
    class World
    {
        public Transform Player;
        public List<HUDUnit> Units = new List<HUDUnit>();
        public Transform Chest, Portal, Npc;
    }

    class Hud
    {
        public HUDFollowRoot Follow;
        public DamageTextSpawner Damage;
        public EdgeIndicator Edge;
        public HitDirection Hit;
        public HealthBar PlayerBar;
        public BuffBar Buffs;
        public MinimapIcons Map;
        public QuestTracker Quest;
        public SkillButton[] Skills;
        public HUDDemo Demo;
    }

    static class Ch17Shots
    {
        static readonly Color Panel = new Color(0.06f, 0.07f, 0.12f, 0.6f);

        // ---------------- 工具 ----------------
        static void Set(Object o, string field, object v)
        {
            var f = o.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (f == null) throw new Exception($"{o.GetType().Name}.{field} 不存在");
            f.SetValue(o, v);
            EditorUtility.SetDirty(o);
        }

        static Material Mat(Shot s, string name, string hex)
        {
            var m = s.MatAsset("Ch17", name, "Standard");
            m.color = UIKit.Hex(hex);
            m.SetFloat("_Glossiness", 0.15f);
            EditorUtility.SetDirty(m);
            return m;
        }

        /// 带描边的 TMP 材质（共享资源）。直接改 TMP 的 outlineWidth 会给每个文字生成一份材质实例，打断合批
        static Material OutlineMat()
        {
            const string dir = "Assets/UGUIBook/Ch17/Materials", path = dir + "/HUD_TextOutline.mat";
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder("Assets/UGUIBook/Ch17", "Materials");
            var src = UISkin.Instance.fontBold.material;
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(src); AssetDatabase.CreateAsset(m, path); }
            m.shader = src.shader;
            m.SetTexture("_MainTex", src.GetTexture("_MainTex"));
            m.SetFloat("_TextureWidth", src.GetFloat("_TextureWidth"));
            m.SetFloat("_TextureHeight", src.GetFloat("_TextureHeight"));
            m.SetFloat("_GradientScale", src.GetFloat("_GradientScale"));
            m.EnableKeyword("OUTLINE_ON");
            m.SetFloat("_OutlineWidth", 0.24f);
            m.SetColor("_OutlineColor", new Color(0.06f, 0.06f, 0.12f, 1));
            m.SetFloat("_FaceDilate", 0.12f);
            ShaderUtilities.UpdateShaderRatios(m);
            EditorUtility.SetDirty(m);
            return m;
        }

        static TextMeshProUGUI OText(Transform p, string text, float size, string name = "Text")
        {
            var t = UIKit.Text(p, text, size, Color.white, TextAlignmentOptions.Center, true, name);
            t.fontSharedMaterial = OutlineMat();
            t.enableWordWrapping = false;
            return t;
        }

        static Image Filled(Transform p, string sprite, Color c, Image.FillMethod method, int origin, string name, bool clockwise = true)
        {
            var img = UIKit.Image(p, sprite, c, name);
            img.type = Image.Type.Filled;
            img.fillMethod = method;
            img.fillOrigin = origin;
            img.fillClockwise = clockwise;
            img.raycastTarget = false;
            return img;
        }

        static GameObject Prim(PrimitiveType t, string name, Vector3 pos, Vector3 scale, Material m, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(t);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = m;
            return go;
        }

        // ---------------- 3D 场景 ----------------
        static World BuildWorld(Shot s)
        {
            var w = new World();
            var sky = UIKit.Hex("#A8CBE8");
            s.Background(sky);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.58f, 0.6f, 0.68f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = sky;
            RenderSettings.fogStartDistance = 35;
            RenderSettings.fogEndDistance = 110;
            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.05f;
            light.shadows = LightShadows.Soft;
            light.transform.rotation = Quaternion.Euler(50, -35, 0);

            var env = new GameObject("Environment").transform;
            Prim(PrimitiveType.Plane, "Ground", Vector3.zero, new Vector3(12, 1, 12), Mat(s, "Ground", "#86A86F"), env);
            var trunk = Mat(s, "Trunk", "#8B5A3C");
            var leaf = Mat(s, "Leaf", "#3F8E52");
            var rock = Mat(s, "Rock", "#8C93A3");
            Vector2[] trees = { new Vector2(-7, 5), new Vector2(9, 12), new Vector2(-14, 16), new Vector2(15, 22), new Vector2(-3, 30),
                new Vector2(4, 34), new Vector2(-18, 4), new Vector2(18, 2), new Vector2(-10, -6), new Vector2(12, -8), new Vector2(-22, 26), new Vector2(24, 34) };
            foreach (var t in trees)
            {
                Prim(PrimitiveType.Cylinder, "Trunk", new Vector3(t.x, 1, t.y), new Vector3(0.5f, 1, 0.5f), trunk, env);
                Prim(PrimitiveType.Sphere, "Leaves", new Vector3(t.x, 2.8f, t.y), new Vector3(2.6f, 2.6f, 2.6f), leaf, env);
            }
            Vector3[] rocks = { new Vector3(-5, 0.4f, 18), new Vector3(8, 0.3f, 4), new Vector3(2, 0.5f, 26), new Vector3(-12, 0.4f, 10) };
            foreach (var r in rocks)
            {
                var go = Prim(PrimitiveType.Cube, "Rock", r, new Vector3(1.6f, 0.9f, 1.3f), rock, env);
                go.transform.rotation = Quaternion.Euler(0, r.x * 17, 8);
            }

            var player = Prim(PrimitiveType.Capsule, "Player", new Vector3(0, 1, 0), Vector3.one, Mat(s, "Player", "#3D7BEA"));
            Prim(PrimitiveType.Cube, "Facing", new Vector3(0, 0.45f, 0.45f), new Vector3(0.35f, 0.18f, 0.3f), Mat(s, "Visor", "#1F2436"), player.transform);
            w.Player = player.transform;

            var enemy = Mat(s, "Enemy", "#D9534F");
            var enemy2 = Mat(s, "Enemy2", "#9B59D0");
            (string name, Vector3 pos, bool alt)[] units =
            {
                ("史莱姆", new Vector3(2.5f, 1, 7), false), ("哥布林", new Vector3(-4.5f, 1, 12), true), ("史莱姆", new Vector3(6, 1, 19), false),
                ("骷髅兵", new Vector3(-8, 1, 25), true), ("哥布林", new Vector3(9.5f, 1, 11), true),
            };
            foreach (var u in units)
            {
                var go = Prim(PrimitiveType.Capsule, u.name, u.pos, Vector3.one, u.alt ? enemy2 : enemy);
                var unit = go.AddComponent<HUDUnit>();
                unit.unitName = u.name;
                w.Units.Add(unit);
            }
            w.Chest = Prim(PrimitiveType.Cube, "宝箱", new Vector3(-24, 0.5f, -14), new Vector3(1.4f, 1, 1), Mat(s, "Chest", "#E0A63A")).transform;
            w.Portal = Prim(PrimitiveType.Cylinder, "传送门", new Vector3(-34, 1.5f, 30), new Vector3(2.5f, 1.5f, 2.5f), Mat(s, "Portal", "#8A5CF6")).transform;

            s.Cam.orthographic = false;
            s.Cam.fieldOfView = 50;
            s.Cam.farClipPlane = 300;
            PlaceCamera(s.Cam, w.Player, new Vector3(0, 6, -9));
            return w;
        }

        static void PlaceCamera(Camera cam, Transform player, Vector3 offset)
        {
            cam.transform.position = player.position + player.rotation * offset;
            cam.transform.LookAt(player.position + player.forward * 6 + Vector3.up * 1.5f);
        }

        // ---------------- HUD ----------------
        static RectTransform Layer(Transform root, string name)
        {
            var rt = UIKit.Node(root, name);
            rt.Stretch();
            return rt;
        }

        static HealthBar Bar(Transform p, Vector2 size, bool numbers, string name, Color back)
        {
            var bg = UIKit.Image(p, "round_4", back, "HealthBar");
            bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = size;
            var delay = Filled(bg.transform, "round_4", new Color(1, 1, 1, 0.9f), Image.FillMethod.Horizontal, 0, "Delay");
            delay.rectTransform.Stretch(3, 3, 3, 3);
            var fill = Filled(bg.transform, "round_4", Color.green, Image.FillMethod.Horizontal, 0, "Fill");
            fill.rectTransform.Stretch(3, 3, 3, 3);
            TextMeshProUGUI label = null;
            if (numbers)
            {
                label = OText(bg.transform, "", size.y * 0.72f, "Label");
                label.rectTransform.Stretch();
            }
            var hb = bg.gameObject.AddComponent<HealthBar>();
            Set(hb, "m_Fill", fill);
            Set(hb, "m_Delay", delay);
            Set(hb, "m_Label", label);
            Set(hb, "m_ShowNumbers", numbers);
            Set(hb, "m_DisplayName", name);
            return hb;
        }

        static Hud BuildHud(Shot s, World w, bool full)
        {
            var h = new Hud();
            s.Canvas.gameObject.name = "HUD";
            var root = s.Root;

            // 头顶血条
            var heads = Layer(root, "HeadBars");
            h.Follow = heads.gameObject.AddComponent<HUDFollowRoot>();
            Set(h.Follow, "m_WorldCamera", s.Cam);
            Set(h.Follow, "m_Canvas", s.Canvas);
            foreach (var u in w.Units)
            {
                var f = UIKit.Node(heads, "Follow_" + u.unitName);
                f.sizeDelta = new Vector2(220, 70);
                var content = UIKit.Node(f, "Content");
                content.Stretch();
                var name = OText(content, u.unitName, 28, "Name");
                name.rectTransform.Place(0, 24, 260, 40);
                var bar = Bar(content, new Vector2(180, 20), false, u.unitName, new Color(0, 0, 0, 0.65f));
                bar.Rt().anchoredPosition = new Vector2(0, -4);
                var follower = f.gameObject.AddComponent<HUDFollower>();
                follower.target = u.transform;
                follower.worldOffset = new Vector3(0, 1.45f, 0);
                follower.content = content.gameObject;
                Set(u, "m_Bar", bar);
            }

            // 屏幕边缘指示
            var edgeLayer = Layer(root, "EdgeIndicators");
            h.Edge = edgeLayer.gameObject.AddComponent<EdgeIndicator>();
            Set(h.Edge, "m_WorldCamera", s.Cam);
            Set(h.Edge, "m_Player", w.Player);
            AddEdge(h.Edge, edgeLayer, w.Chest, "item_key", UIKit.Hex("#E0A63A"));
            AddEdge(h.Edge, edgeLayer, w.Portal, "star", UIKit.Hex("#8A5CF6"));

            // 受击方向
            var hitLayer = Layer(root, "HitDirection");
            var tpl = UIKit.Node(hitLayer, "Template");
            tpl.sizeDelta = Vector2.zero;
            var arc = Filled(tpl, "ring", new Color(0.95f, 0.18f, 0.18f, 0.8f), Image.FillMethod.Radial360, (int)Image.Origin360.Top, "Arc");
            arc.rectTransform.sizeDelta = new Vector2(620, 620);
            tpl.gameObject.SetActive(false);
            h.Hit = hitLayer.gameObject.AddComponent<HitDirection>();
            Set(h.Hit, "m_Template", tpl);
            Set(h.Hit, "m_Player", w.Player);
            Set(h.Hit, "m_Camera", s.Cam);

            // 伤害飘字
            var dmgLayer = Layer(root, "DamageTexts");
            var dt = OText(dmgLayer, "999", 54, "Template");
            dt.rectTransform.sizeDelta = new Vector2(400, 90);
            dt.gameObject.SetActive(false);
            var dtc = dt.gameObject.AddComponent<DamageText>();
            Set(dtc, "m_Text", dt);
            h.Damage = dmgLayer.gameObject.AddComponent<DamageTextSpawner>();
            Set(h.Damage, "m_Template", dtc);
            Set(h.Damage, "m_WorldCamera", s.Cam);
            Set(h.Damage, "m_Canvas", s.Canvas);

            if (!full) return h;

            // 左上：玩家状态 + Buff
            var status = UIKit.Image(root, "round_24", Panel, "PlayerStatus").rectTransform;
            status.Place(new Vector2(0, 1), new Vector2(30, -30), new Vector2(560, 150));
            var avf = UIKit.Image(status, "circle", Color.white, "AvatarFrame");
            avf.rectTransform.Place(new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(118, 118));
            UIKit.Icon(avf.transform, "avatar_1", 106, Color.white, "Avatar");
            var nm = OText(status, "麦冬  <size=75%>Lv.12</size>", 32, "Name");
            nm.alignment = TextAlignmentOptions.MidlineLeft;
            nm.rectTransform.Place(new Vector2(0, 1), new Vector2(160, -16), new Vector2(380, 44));
            h.PlayerBar = Bar(status, new Vector2(370, 32), true, "麦冬", new Color(0, 0, 0, 0.6f));
            h.PlayerBar.Rt().Place(new Vector2(0, 1), new Vector2(160, -68), new Vector2(370, 32));
            var mp = UIKit.Image(status, "round_4", new Color(0, 0, 0, 0.6f), "ManaBar");
            mp.rectTransform.Place(new Vector2(0, 1), new Vector2(160, -108), new Vector2(370, 18));
            var mpf = Filled(mp.transform, "round_4", UIKit.Hex("#4C8DFF"), Image.FillMethod.Horizontal, 0, "Fill");
            mpf.rectTransform.Stretch(3, 3, 3, 3); mpf.fillAmount = 0.72f;

            var buffs = UIKit.Node(root, "BuffBar");
            buffs.Place(new Vector2(0, 1), new Vector2(34, -196), new Vector2(560, 110));
            var hl = buffs.HLayout(12, 0, TextAnchor.UpperLeft);
            h.Buffs = buffs.gameObject.AddComponent<BuffBar>();
            Set(h.Buffs, "m_Template", BuildBuffIcon(buffs));

            // 右上：小地图 + 任务
            h.Map = BuildIconMinimap(root, w, new Vector2(-30, -30), 300, new Vector2(1, 1));
            h.Quest = BuildQuest(root, w, s.Cam, new Vector2(-30, -360));
            h.Quest.Quests.Add(new Quest
            {
                title = "清理森林", main = true,
                objectives = new[] { new QuestObjective { desc = "击败任意怪物", current = 2, required = 10 } },
            });
            h.Quest.Quests.Add(new Quest
            {
                title = "神秘宝箱", main = false, target = w.Chest,
                objectives = new[] { new QuestObjective { desc = "找到森林里的宝箱", required = 1 }, new QuestObjective { desc = "和村长对话", current = 1, required = 1 } },
            });

            // 右下：技能
            h.Skills = new[]
            {
                BuildSkill(root, "Skill_快斩", "item_sword", 150, new Vector2(-520, 60), 4f, 3, false, KeyCode.Alpha1),
                BuildSkill(root, "Skill_旋风", "star", 150, new Vector2(-340, 60), 6f, 1, false, KeyCode.Alpha2),
                BuildSkill(root, "Skill_蓄力射击", "item_bow", 190, new Vector2(-110, 60), 3f, 1, true, KeyCode.Alpha3),
            };

            var hint = UIKit.Image(root, "pill", new Color(0, 0, 0, 0.45f), "Hint").rectTransform;
            hint.Place(new Vector2(0, 0), new Vector2(30, 30), new Vector2(1000, 60));
            var ht = UIKit.Text(hint, "W/S 前进后退 · A/D 转向 · 点击敌人攻击 · 1/2/3 放技能（3 可长按蓄力）", 26, Color.white);
            ht.rectTransform.Stretch(20, 0, 20, 0);

            h.Demo = new GameObject("HUDDemo").AddComponent<HUDDemo>();
            Set(h.Demo, "m_Camera", s.Cam);
            Set(h.Demo, "m_Player", w.Player);
            Set(h.Demo, "m_Units", w.Units);
            Set(h.Demo, "m_Damage", h.Damage);
            Set(h.Demo, "m_Hit", h.Hit);
            Set(h.Demo, "m_Buffs", h.Buffs);
            Set(h.Demo, "m_PlayerBar", h.PlayerBar);
            Set(h.Demo, "m_Skills", h.Skills);
            Set(h.Demo, "m_Quest", h.Quest);
            return h;
        }

        static void AddEdge(EdgeIndicator edge, Transform layer, Transform target, string icon, Color color)
        {
            var view = UIKit.Node(layer, "Edge_" + target.name);
            view.sizeDelta = new Vector2(120, 120);
            var pivot = UIKit.Node(view, "ArrowPivot");
            pivot.sizeDelta = Vector2.zero;
            var arrow = UIKit.Icon(pivot, "play", 40, color, "Arrow");
            arrow.rectTransform.anchoredPosition = new Vector2(62, 0);
            var bg = UIKit.Image(view, "circle", new Color(0.06f, 0.07f, 0.12f, 0.8f), "Bg");
            bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = new Vector2(86, 86);
            var ring = UIKit.Image(view, "ring_thin", color, "Ring");
            ring.raycastTarget = false;
            ring.rectTransform.sizeDelta = new Vector2(86, 86);
            UIKit.Icon(view, icon, 50, icon == "star" ? color : Color.white);
            var dist = OText(view, "0m", 24, "Distance");
            dist.rectTransform.Place(0, -64, 120, 34);
            view.gameObject.SetActive(false);   // 由 EdgeIndicator 决定何时显示
            edge.Entries.Add(new EdgeIndicator.Entry { target = target, view = view, arrow = pivot, distance = dist });
        }

        static BuffIcon BuildBuffIcon(Transform parent)
        {
            var root = UIKit.Image(parent, "round_8", Color.green, "BuffTemplate");
            root.raycastTarget = false;
            root.rectTransform.sizeDelta = new Vector2(72, 72);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var bg = UIKit.Image(root.transform, "round_8", UIKit.Hex("#1F2436"), "Bg");
            bg.raycastTarget = false; bg.rectTransform.Stretch(4, 4, 4, 4);
            var icon = UIKit.Icon(root.transform, "item_sword", 54, Color.white, "Icon");
            var mask = Filled(root.transform, "round_8", new Color(0, 0, 0, 0.6f), Image.FillMethod.Radial360, (int)Image.Origin360.Top, "Mask");
            mask.rectTransform.Stretch(4, 4, 4, 4);
            var stacks = OText(root.transform, "", 24, "Stacks");
            stacks.alignment = TextAlignmentOptions.BottomRight;
            stacks.rectTransform.Stretch(0, 0, 6, 2);
            var time = OText(root.transform, "", 22, "Time");
            time.rectTransform.Place(new Vector2(0.5f, 0), new Vector2(0, -30), new Vector2(80, 28));
            var bi = root.gameObject.AddComponent<BuffIcon>();
            Set(bi, "m_Frame", root);
            Set(bi, "m_Icon", icon);
            Set(bi, "m_Mask", mask);
            Set(bi, "m_Stacks", stacks);
            Set(bi, "m_Time", time);
            Set(bi, "m_Group", group);
            root.gameObject.SetActive(false);
            return bi;
        }

        static MinimapIcons BuildIconMinimap(Transform root, World w, Vector2 pos, float size, Vector2 anchor, string name = "Minimap")
        {
            var mm = UIKit.Node(root, name);
            mm.Place(anchor, pos, new Vector2(size, size));
            var frame = UIKit.Image(mm, "circle", new Color(1, 1, 1, 0.85f), "Frame");
            frame.raycastTarget = false; frame.rectTransform.Stretch(-8, -8, -8, -8);
            var maskImg = UIKit.Image(mm, "circle", UIKit.Hex("#2E4A36"), "MapMask");
            maskImg.raycastTarget = false; maskImg.rectTransform.Stretch();
            maskImg.gameObject.AddComponent<Mask>();
            var bg = UIKit.Image(maskImg.transform, "checker", new Color(1, 1, 1, 0.1f), "Background");
            bg.type = Image.Type.Tiled; bg.pixelsPerUnitMultiplier = 0.45f; bg.raycastTarget = false;
            bg.rectTransform.sizeDelta = new Vector2(size * 1.5f, size * 1.5f);
            var rings = UIKit.Image(maskImg.transform, "ring_thin", new Color(1, 1, 1, 0.2f), "Range");
            rings.raycastTarget = false; rings.rectTransform.sizeDelta = new Vector2(size * 0.5f, size * 0.5f);
            var markers = UIKit.Node(maskImg.transform, "Markers");
            markers.Stretch();
            var map = mm.gameObject.AddComponent<MinimapIcons>();
            foreach (var u in w.Units)
            {
                var ic = UIKit.Icon(markers, "circle_small", 18, UIKit.Hex("#FF5A5F"), "Enemy_" + u.unitName);
                map.Markers.Add(new MinimapIcons.Marker { target = u.transform, icon = ic.rectTransform });
            }
            var chest = UIKit.Icon(markers, "item_key", 34, Color.white, "Quest_宝箱");
            map.Markers.Add(new MinimapIcons.Marker { target = w.Chest, icon = chest.rectTransform, clampToEdge = true });
            var portal = UIKit.Icon(markers, "star", 30, UIKit.Hex("#C9A8FF"), "Portal");
            map.Markers.Add(new MinimapIcons.Marker { target = w.Portal, icon = portal.rectTransform, clampToEdge = true });
            var player = UIKit.Node(maskImg.transform, "PlayerIcon");
            PlayerArrow(player);
            Set(map, "m_Map", mm);
            Set(map, "m_Background", bg.rectTransform);
            Set(map, "m_PlayerIcon", player);
            Set(map, "m_Player", w.Player);
            Set(map, "m_WorldRadius", 30f);
            var north = OText(mm, "N", 26, "North");
            north.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, 4), new Vector2(40, 34));
            return map;
        }

        /// 玩家箭头（pointer 图标默认朝上），节点本身由代码旋转
        static void PlayerArrow(RectTransform node)
        {
            node.sizeDelta = new Vector2(44, 44);
            UIKit.Icon(node, "pointer", 44, UIKit.Hex("#5BC0FF"), "Arrow");
        }

        static QuestTracker BuildQuest(Transform root, World w, Camera cam, Vector2 pos)
        {
            var list = UIKit.Node(root, "QuestTracker");
            list.Place(new Vector2(1, 1), pos, new Vector2(440, 420));
            var vl = list.VLayout(12, 0, TextAnchor.UpperRight, true);
            vl.childControlHeight = true;
            var e = UIKit.Image(list, "round_16", Panel, "QuestTemplate");
            e.raycastTarget = false;
            var evl = e.rectTransform.VLayout(6, 18, TextAnchor.UpperLeft, true);
            evl.childControlHeight = true;
            var flash = UIKit.Image(e.transform, "round_16", new Color(1, 0.8f, 0.3f, 0), "Flash");
            flash.raycastTarget = false;
            flash.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            flash.rectTransform.Stretch();
            var title = UIKit.Text(e.transform, "任务", 28, Color.white, TextAlignmentOptions.TopLeft, true, "Title");
            title.margin = new Vector4(0, 0, 110, 0);
            var obj = UIKit.Text(e.transform, "目标", 25, UIKit.Hex("#DDE2EE"), TextAlignmentOptions.TopLeft, false, "Objectives");
            obj.lineSpacing = 8;
            var dir = UIKit.Node(e.transform, "Direction");
            dir.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            dir.Place(new Vector2(1, 1), new Vector2(-14, -16), new Vector2(110, 36));
            var arrow = UIKit.Icon(dir, "pointer", 26, UIKit.Hex("#FFB547"), "Arrow");
            arrow.rectTransform.anchoredPosition = new Vector2(-34, 0);
            var dist = UIKit.Text(dir, "0m", 24, UIKit.Hex("#FFB547"), TextAlignmentOptions.MidlineRight, true, "Distance");
            dist.rectTransform.Stretch(40, 0, 0, 0);
            var view = e.gameObject.AddComponent<QuestEntryView>();
            Set(view, "m_Title", title);
            Set(view, "m_Objectives", obj);
            Set(view, "m_Flash", flash);
            Set(view, "m_Direction", dir.gameObject);
            Set(view, "m_Arrow", arrow.rectTransform);
            Set(view, "m_Distance", dist);
            var qt = list.gameObject.AddComponent<QuestTracker>();
            Set(qt, "m_Template", view);
            Set(qt, "m_Player", w.Player);
            Set(qt, "m_Camera", cam);
            e.gameObject.SetActive(false);
            return qt;
        }

        static SkillButton BuildSkill(Transform root, string name, string icon, float size, Vector2 pos, float cd, int charges, bool hold, KeyCode key)
        {
            var bg = UIKit.Image(root, "circle", new Color(0.08f, 0.09f, 0.14f, 0.78f), name);
            bg.rectTransform.Place(new Vector2(1, 0), pos, new Vector2(size, size));
            UIKit.Image(bg.transform, "ring_thin", new Color(1, 1, 1, 0.55f), "Frame").rectTransform.Stretch();
            var ic = UIKit.Icon(bg.transform, icon, size * 0.6f, icon == "star" ? UIKit.Hex("#FFD166") : Color.white, "Icon");
            var cdMask = Filled(bg.transform, "circle", new Color(0, 0, 0, 0.65f), Image.FillMethod.Radial360, (int)Image.Origin360.Top, "CooldownMask", false);
            cdMask.rectTransform.Stretch();
            var cdText = OText(bg.transform, "", size * 0.4f, "CooldownText");
            cdText.rectTransform.Stretch();
            var recharge = Filled(bg.transform, "ring_thin", UIKit.Hex("#FFD166"), Image.FillMethod.Radial360, (int)Image.Origin360.Top, "RechargeRing");
            recharge.rectTransform.Stretch(-5, -5, -5, -5);
            var holdRing = Filled(bg.transform, "ring", UIKit.Hex("#FF8A3D"), Image.FillMethod.Radial360, (int)Image.Origin360.Top, "HoldRing");
            holdRing.rectTransform.Stretch(-16, -16, -16, -16);
            var badge = UIKit.Image(bg.transform, "circle", UIKit.Hex("#FF8A3D"), "ChargeBadge");
            badge.raycastTarget = false;
            badge.rectTransform.Place(new Vector2(0.85f, 0.15f), Vector2.zero, new Vector2(50, 50));
            badge.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var badgeText = UIKit.Text(badge.transform, "3", 30, Color.white, TextAlignmentOptions.Center, true, "ChargeText");
            badgeText.rectTransform.Stretch();
            var keyBg = UIKit.Image(bg.transform, "round_8", new Color(0, 0, 0, 0.7f), "Key");
            keyBg.raycastTarget = false;
            keyBg.rectTransform.Place(new Vector2(0.15f, 0.85f), Vector2.zero, new Vector2(40, 40));
            keyBg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var keyText = UIKit.Text(keyBg.transform, "1", 26, Color.white, TextAlignmentOptions.Center, true, "KeyText");
            keyText.rectTransform.Stretch();
            var sb = bg.gameObject.AddComponent<SkillButton>();
            Set(sb, "m_Icon", ic);
            Set(sb, "m_CooldownMask", cdMask);
            Set(sb, "m_CooldownText", cdText);
            Set(sb, "m_ChargeBadge", badge.gameObject);
            Set(sb, "m_ChargeText", badgeText);
            Set(sb, "m_RechargeRing", recharge);
            Set(sb, "m_HoldRing", holdRing);
            Set(sb, "m_KeyText", keyText);
            Set(sb, "m_Cooldown", cd);
            Set(sb, "m_MaxCharges", charges);
            Set(sb, "m_HoldToCharge", hold);
            Set(sb, "m_Hotkey", key);
            return sb;
        }

        // 截图前：画布贴近相机（避免被 3D 物体挡住），然后把 HUD 各部分刷新到“运行中”的样子
        static void Prepare(Shot s)
        {
            s.Canvas.planeDistance = 1f;
        }

        // 标注层画布默认 planeDistance = 50，透视相机下会被 50 米以内的 3D 物体挡住（UI Shader 做深度测试）。
        // 在所有标注之前插一步：把标注画布拉到相机跟前
        static void AnnotInFront(Shot s)
        {
            s.Annot.Insert(0, () =>
            {
                s.AnnotRoot.GetComponent<Canvas>().planeDistance = 0.6f;
                Canvas.ForceUpdateCanvases();
            });
        }

        static void InitUnits(World w)
        {
            foreach (var u in w.Units) u.ResetHp();
        }

        // ---------------- 配图 ----------------
        [BookShot("ch17-overview", Scene = "Ch17_HUD")]
        static void Overview(Shot s)
        {
            var w = BuildWorld(s);
            var h = BuildHud(s, w, true);
            s.BeforeCapture(() =>
            {
                Prepare(s);
                UnityEngine.Random.InitState(7);
                InitUnits(w);
                w.Units[0].TakeDamage(380);
                w.Units[0].Bar.Tick(0.2f);
                w.Units[1].TakeDamage(720);
                w.Units[1].Bar.Tick(2f);
                w.Units[2].TakeDamage(150);
                h.Follow.Collect();
                h.Follow.Refresh();
                h.Damage.Init();
                h.Damage.Spawn(w.Units[0].transform.position + Vector3.up * 1.4f, 186, DamageKind.Normal).Age = 0.35f;
                h.Damage.Spawn(w.Units[0].transform.position + Vector3.up * 1.4f, 512, DamageKind.Crit).Age = 0.15f;
                h.Damage.Spawn(w.Player.position + Vector3.up * 2f, 60, DamageKind.Heal).Age = 0.3f;
                h.Damage.Tick(0);
                h.Edge.Refresh();
                h.Hit.Show(new Vector3(-6, 1, -5));
                h.Hit.Tick(0.1f);
                h.PlayerBar.SetHealth(1460, 2000);
                h.Buffs.Add("atk", "item_sword", 8, false, 3);
                h.Buffs.Add("atk", "item_sword", 8, false, 3);
                h.Buffs.Add("shield", "item_shield", 30, false);
                h.Buffs.Add("poison", "item_potion_green", 5, true);
                h.Buffs.Tick(2.6f);
                h.Map.Refresh();
                h.Quest.Rebuild();
                foreach (var sk in h.Skills) sk.Init();
                h.Skills[0].SetState(2, 1.6f);
                h.Skills[1].SetState(0, 3.4f);
                h.Skills[2].SetState(1, 0, 0.6f);
            });
        }

        // 左：不按距离缩放；右：按距离缩放 + 按深度排序
        [BookShot("ch17-follow", W = 640, H = 360, Panels = 2)]
        static void Follow(Shot s)
        {
            s.Ref(1920, 1080);
            var w = BuildWorld(s);
            var h = BuildHud(s, w, false);
            bool scale = s.Panel == 1;
            foreach (var f in h.Follow.GetComponentsInChildren<HUDFollower>(true)) f.scaleByDistance = scale;
            s.BeforeCapture(() =>
            {
                Prepare(s);
                InitUnits(w);
                w.Units[1].TakeDamage(450);
                w.Units[3].TakeDamage(300);
                h.Follow.Collect();
                h.Follow.Refresh();
            });
            AnnotInFront(s);
            s.Label(new Vector2(0, 470), scale ? "按距离缩放：远处的血条小、近处的大" : "固定大小：远处的血条和近处一样大", scale ? UIKit.Hex("#2FB36A") : UIKit.Hex("#E5484D"), 44);
        }

        [BookShot("ch17-damage")]
        static void Damage(Shot s)
        {
            var w = BuildWorld(s);
            w.Units[0].transform.position = new Vector3(-1.8f, 1, 7);
            w.Units[1].transform.position = new Vector3(2.2f, 1, 8);
            w.Player.position = new Vector3(0, 1, 2);
            PlaceCamera(s.Cam, w.Player, new Vector3(0, 2.6f, -4.5f));
            var h = BuildHud(s, w, false);
            var stats = UIKit.Image(s.Root, "round_16", new Color(0.06f, 0.07f, 0.12f, 0.75f), "PoolStats").rectTransform;
            stats.Place(new Vector2(0, 1), new Vector2(30, -30), new Vector2(640, 70));
            var st = UIKit.Text(stats, "", 30, Color.white, TextAlignmentOptions.Center, false, "Text");
            st.rectTransform.Stretch();
            s.BeforeCapture(() =>
            {
                Prepare(s);
                InitUnits(w);
                h.Follow.Collect();
                var a = w.Units[0].transform.position + Vector3.up * 1.4f;
                var b = w.Units[1].transform.position + Vector3.up * 1.4f;
                h.Damage.Init();
                h.Damage.Spawn(a, 128, DamageKind.Normal, new Vector2(-150, 150)).Age = 0.8f;
                h.Damage.Spawn(a, 143, DamageKind.Normal, new Vector2(130, 140)).Age = 0.5f;
                h.Damage.Spawn(a, 1280, DamageKind.Crit, new Vector2(0, 120)).Age = 0.05f;
                h.Damage.Spawn(b, 0, DamageKind.Miss, new Vector2(110, 110)).Age = 0.3f;
                h.Damage.Spawn(b, 96, DamageKind.Normal, new Vector2(-40, 160)).Age = 0.62f;
                h.Damage.Spawn(w.Player.position + Vector3.up * 1.2f, 240, DamageKind.Heal, new Vector2(0, 100)).Age = 0.4f;
                w.Units[0].TakeDamage(1551);
                w.Units[1].TakeDamage(96);
                h.Follow.Refresh();
                h.Damage.Tick(0);
                st.text = $"飘字对象池：活跃 {h.Damage.ActiveCount} · 池中 {h.Damage.PooledCount} · 累计创建 {h.Damage.CreatedCount}";
            });
        }

        [BookShot("ch17-edge")]
        static void Edge(Shot s)
        {
            var w = BuildWorld(s);
            var h = BuildHud(s, w, false);
            s.BeforeCapture(() =>
            {
                Prepare(s);
                InitUnits(w);
                h.Follow.Collect();
                h.Follow.Refresh();
                h.Edge.Refresh();
                h.Hit.Show(new Vector3(9, 1, -4));
                h.Hit.Show(new Vector3(-10, 1, 3));
                h.Hit.SetAge(1, 0.9f);
                h.Hit.Tick(0);
            });
            var chest = (RectTransform)h.Edge.transform.Find("Edge_宝箱");
            var portal = (RectTransform)h.Edge.transform.Find("Edge_传送门");
            AnnotInFront(s);
            s.Box(chest, UIKit.Hex("#E5484D"));
            s.LabelAt(chest, new Vector2(1, 0.5f), new Vector2(330, 0), "宝箱在左后方：先翻转方向再贴边", UIKit.Hex("#E5484D"));
            s.Box(portal, UIKit.Hex("#5B4BD6"));
            s.LabelAt(portal, new Vector2(1, 0.5f), new Vector2(260, 0), "传送门在左前方屏幕外", UIKit.Hex("#5B4BD6"));
            s.Label(new Vector2(0, -160), "受击方向：弧指向攻击来源（右后、左侧），逐渐淡出", UIKit.Hex("#E5484D"), 30);
        }

        [BookShot("ch17-minimap", Scene = "Ch17_Minimap")]
        static void Minimap(Shot s)
        {
            var w = BuildWorld(s);
            w.Player.rotation = Quaternion.Euler(0, 35, 0);
            PlaceCamera(s.Cam, w.Player, new Vector3(0, 6, -9));
            var h = BuildHud(s, w, false);
            var dim = UIKit.Image(s.Root, null, new Color(0.05f, 0.06f, 0.1f, 0.55f), "Dim");
            dim.raycastTarget = false; dim.rectTransform.Stretch();
            // 左：图标方案
            var icons = BuildIconMinimap(s.Root, w, new Vector2(-420, 40), 520, new Vector2(0.5f, 0.5f), "IconMinimap");
            // 右：RenderTexture 方案
            var rtPath = "Assets/UGUIBook/Ch17/Materials/Minimap.renderTexture";
            var rtex = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
            if (rtex == null)
            {
                OutlineMat();   // 顺便确保 Materials 目录存在
                rtex = new RenderTexture(512, 512, 16) { name = "Minimap" };
                AssetDatabase.CreateAsset(rtex, rtPath);
            }
            var camGo = new GameObject("MinimapCamera");
            var mcam = camGo.AddComponent<Camera>();
            mcam.orthographic = true;
            mcam.orthographicSize = 30;
            mcam.clearFlags = CameraClearFlags.SolidColor;
            mcam.backgroundColor = UIKit.Hex("#2E4A36");
            mcam.cullingMask = ~(1 << LayerMask.NameToLayer("UI"));
            mcam.targetTexture = rtex;
            mcam.farClipPlane = 100;
            var mc = camGo.AddComponent<MinimapCamera>();
            Set(mc, "m_Player", w.Player);
            var rtMap = UIKit.Node(s.Root, "RTMinimap");
            rtMap.Place(420, 40, 520, 520);
            var frame = UIKit.Image(rtMap, "circle", new Color(1, 1, 1, 0.85f), "Frame");
            frame.raycastTarget = false; frame.rectTransform.Stretch(-8, -8, -8, -8);
            var mask = UIKit.Image(rtMap, "circle", Color.white, "MapMask");
            mask.raycastTarget = false; mask.rectTransform.Stretch();
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            var rawImg = UIKit.Node(mask.transform, "RawImage").gameObject.AddComponent<RawImage>();
            rawImg.texture = rtex;
            rawImg.raycastTarget = false;
            rawImg.rectTransform.Stretch();
            var pi = UIKit.Node(mask.transform, "PlayerIcon");
            PlayerArrow(pi);   // 俯视相机不随玩家旋转，所以箭头要转 -yaw（这里玩家朝向 35°）
            pi.localRotation = Quaternion.Euler(0, 0, -35);
            var l1 = UIKit.Text(s.Root, "图标方案：坐标映射 + 图标", 36, Color.white, TextAlignmentOptions.Center, true, "Caption1");
            l1.rectTransform.Place(-420, -280, 700, 60);
            var l2 = UIKit.Text(s.Root, "RenderTexture 方案：俯视相机", 36, Color.white, TextAlignmentOptions.Center, true, "Caption2");
            l2.rectTransform.Place(420, -280, 700, 60);
            var hint = UIKit.Text(s.Root, "W/S 前进后退 · A/D 转向：观察两种小地图的变化", 28, UIKit.Hex("#DDE2EE"), TextAlignmentOptions.Center, false, "Hint");
            hint.rectTransform.Place(0, -440, 1200, 50);
            var demo = new GameObject("HUDDemo").AddComponent<HUDDemo>();
            Set(demo, "m_Camera", s.Cam);
            Set(demo, "m_Player", w.Player);
            Set(demo, "m_Units", w.Units);
            Set(demo, "m_Damage", h.Damage);
            s.BeforeCapture(() =>
            {
                Prepare(s);
                InitUnits(w);
                h.Follow.Collect();
                h.Follow.Refresh();
                icons.Refresh();
                mc.RenderNow();
            });
        }

        [BookShot("ch17-skills", W = 960, H = 420, Scene = "Ch17_Skills")]
        static void Skills(Shot s)
        {
            s.Background(UIKit.Hex("#2A3047"));
            s.Canvas.gameObject.name = "HUD";
            string[] captions = { "就绪", "冷却中", "充能 2/3", "长按蓄力 60%", "蓝量不足" };
            var skills = new List<SkillButton>();
            for (int i = 0; i < 5; i++)
            {
                float x = -720 + i * 360;
                bool hold = i == 3;
                int charges = i == 2 ? 3 : 1;
                string icon = new[] { "item_sword", "star", "item_sword", "item_bow", "item_potion_blue" }[i];
                var sk = BuildSkill(s.Root, "Skill_" + captions[i], icon, 180, Vector2.zero, i == 1 ? 8f : 5f, charges, hold, KeyCode.Alpha1 + i);
                sk.Rt().Place(new Vector2(0.5f, 0.5f), new Vector2(x, 130), new Vector2(180, 180));
                var cap = UIKit.Text(s.Root, captions[i], 34, Color.white, TextAlignmentOptions.Center, true, "Caption");
                cap.rectTransform.Place(x, -20, 320, 50);
                skills.Add(sk);
            }
            // 第二排：Buff 栏
            var buffs = UIKit.Node(s.Root, "BuffBar");
            buffs.Place(new Vector2(0.5f, 0.5f), new Vector2(-200, -200), new Vector2(560, 110));
            buffs.HLayout(16, 0, TextAnchor.UpperLeft);
            var bar = buffs.gameObject.AddComponent<BuffBar>();
            Set(bar, "m_Template", BuildBuffIcon(buffs));
            var bl = UIKit.Text(s.Root, "Buff 栏：剩余时间扇形 · 叠层数字 · 最后 3 秒闪烁", 30, UIKit.Hex("#C9CFDD"), TextAlignmentOptions.MidlineLeft, false, "BuffCaption");
            bl.rectTransform.Place(new Vector2(0.5f, 0.5f), new Vector2(330, -170), new Vector2(700, 50));
            var hint = UIKit.Text(s.Root, "点击或按 1~5 试试：第 3 个可连放三次，第 4 个要按住蓄力", 26, UIKit.Hex("#8A93AD"), TextAlignmentOptions.Center, false, "Hint");
            hint.rectTransform.Place(new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(1400, 40));
            var demo = new GameObject("SkillDemo").AddComponent<SkillDemo>();
            Set(demo, "m_Buffs", bar);
            Set(demo, "m_Skills", skills.ToArray());
            s.BeforeCapture(() =>
            {
                foreach (var sk in skills) sk.Init();
                skills[1].SetState(0, 4.8f);
                skills[2].SetState(2, 2.1f);
                skills[3].SetState(1, 0, 0.6f);
                skills[4].SetUsable(false);
                bar.Add("atk", "item_sword", 12, false, 5);
                bar.Add("atk", "item_sword", 12, false, 5);
                bar.Add("atk", "item_sword", 12, false, 5);
                bar.Add("regen", "heart", 20, false);
                bar.Add("poison", "item_potion_green", 6, true);
                bar.Add("slow", "arrow_down", 90, true);
                bar.Tick(3.4f);
            });
        }

        [BookShot("ch17-dialogue", W = 640, H = 360, Panels = 2, Scene = "Ch17_Dialogue")]
        static void Dialogue(Shot s)
        {
            s.Ref(1920, 1080);
            var w = BuildWorld(s);
            var npc = Prim(PrimitiveType.Capsule, "村长", new Vector3(0, 1, 4), Vector3.one, Mat(s, "Npc", "#E8B04B"));
            w.Player.position = new Vector3(-2.6f, 1, 1.2f);
            w.Player.rotation = Quaternion.Euler(0, 60, 0);
            s.Cam.transform.position = new Vector3(1.6f, 2.4f, -3.2f);
            s.Cam.transform.LookAt(new Vector3(0, 1.3f, 4));
            s.Canvas.gameObject.name = "HUD";
            var lines = new[]
            {
                new DialogueLine { speaker = "村长", avatar = "avatar_4", text = "年轻人，你总算来了。最近东边的森林里，<color=#FFB547>史莱姆</color>越来越多，连猎人都不敢进山了。" },
                new DialogueLine { speaker = "村长", avatar = "avatar_4", text = "你愿意帮我们清理掉它们吗？报酬嘛，村里一定不会亏待你。",
                    options = new[] { new DialogueOption { text = "交给我吧！", next = 2 }, new DialogueOption { text = "报酬是多少？", next = 3 }, new DialogueOption { text = "我再想想。", next = 4 } } },
                new DialogueLine { speaker = "村长", avatar = "avatar_4", text = "太好了！击败 5 只史莱姆后回来找我。路上小心！", end = true },
                new DialogueLine { speaker = "村长", avatar = "avatar_4", text = "500 金币，外加一瓶祖传的生命药水。",
                    options = new[] { new DialogueOption { text = "成交！", next = 2 }, new DialogueOption { text = "我再想想。", next = 4 } } },
                new DialogueLine { speaker = "村长", avatar = "avatar_4", text = "唉……那你考虑好了再来找我吧。（按 T 重新对话）", end = true },
            };
            var box = BuildDialogue(s.Root, lines);
            var quest = BuildQuest(s.Root, w, s.Cam, new Vector2(-30, -30));
            quest.Quests.Add(new Quest { title = "初入村庄", main = true, objectives = new[] { new QuestObjective { desc = "与村长交谈", current = 1, required = 1 } } });
            quest.Quests.Add(new Quest
            {
                title = "清理史莱姆", main = false, target = npc.transform,
                objectives = new[] { new QuestObjective { desc = "接受村长的委托", required = 1 }, new QuestObjective { desc = "击败史莱姆", required = 5 } },
            });
            var demo = new GameObject("DialogueDemo").AddComponent<DialogueDemo>();
            Set(demo, "m_Box", box);
            Set(demo, "m_Quest", quest);
            Set(demo, "m_Lines", lines);
            int panel = s.Panel;
            s.BeforeCapture(() =>
            {
                Prepare(s);
                quest.Rebuild();
                box.Play(lines, panel == 0 ? 0 : 1);
                if (panel == 0) box.SetProgress(0.55f);
                else box.CompleteTyping();
            });
            AnnotInFront(s);
            s.Label(new Vector2(-560, 470), panel == 0 ? "① 打字中（maxVisibleCharacters）" : "② 一句说完，出现选项", UIKit.Hex("#5B4BD6"), 40, new Vector2(0, 0.5f));
        }

        static DialogueBox BuildDialogue(Transform root, DialogueLine[] lines)
        {
            var panel = UIKit.Image(root, "round_24", new Color(0.06f, 0.07f, 0.12f, 0.9f), "DialogueBox");
            var rt = panel.rectTransform;
            rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0); rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(140, 40); rt.offsetMax = new Vector2(-140, 330);
            var avf = UIKit.Image(rt, "circle", Color.white, "AvatarFrame");
            avf.raycastTarget = false;
            avf.rectTransform.Place(new Vector2(0, 1), new Vector2(40, 60), new Vector2(200, 200));
            var av = UIKit.Icon(avf.transform, "avatar_4", 184, Color.white, "Avatar");
            var plate = UIKit.Image(rt, "pill", UIKit.Hex("#FF8A3D"), "NamePlate");
            plate.raycastTarget = false;
            plate.rectTransform.Place(new Vector2(0, 1), new Vector2(270, 30), new Vector2(220, 60));
            var speaker = UIKit.Text(plate.transform, "村长", 34, Color.white, TextAlignmentOptions.Center, true, "Speaker");
            speaker.rectTransform.Stretch();
            var body = UIKit.Text(rt, "", 38, Color.white, TextAlignmentOptions.TopLeft, false, "Body");
            body.rectTransform.Stretch(270, 70, 80, 40);
            body.lineSpacing = 14;
            var cont = UIKit.Icon(rt, "arrow_down", 40, UIKit.Hex("#FFB547"), "ContinueHint");
            cont.rectTransform.Place(new Vector2(1, 0), new Vector2(-50, 40), new Vector2(40, 40));
            var opts = UIKit.Node(root, "Options");
            opts.Place(new Vector2(1, 0), new Vector2(-160, 360), new Vector2(560, 320));
            var vl = opts.VLayout(14, 0, TextAnchor.LowerRight, false);
            var tpl = UIKit.Button(opts, "选项", new Color(1, 1, 1, 0.94f), 560, 84, null, 32, "OptionTemplate");
            var tt = tpl.GetComponentInChildren<TextMeshProUGUI>();
            tt.alignment = TextAlignmentOptions.MidlineLeft;
            tt.rectTransform.offsetMin = new Vector2(36, 0);
            tpl.gameObject.SetActive(false);
            var box = panel.gameObject.AddComponent<DialogueBox>();
            Set(box, "m_Speaker", speaker);
            Set(box, "m_Avatar", av);
            Set(box, "m_Body", body);
            Set(box, "m_ContinueHint", cont.gameObject);
            Set(box, "m_OptionTemplate", tpl);
            Set(box, "m_Lines", lines);
            return box;
        }
    }
}
