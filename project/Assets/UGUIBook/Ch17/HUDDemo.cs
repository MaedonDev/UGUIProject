// 第 17 章示例场景的“玩法”：移动、点怪、放技能、挨打。HUD 组件都是独立的，这里只负责把事件接上去。
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UGUIBook.Ch17
{
    // 先于 HUDFollowRoot 等组件执行 LateUpdate：相机先就位，HUD 再按新相机算位置，否则血条会落后一帧“抖动”
    [DefaultExecutionOrder(-100)]
    public class HUDDemo : MonoBehaviour
    {
        [SerializeField] Camera m_Camera;
        [SerializeField] Transform m_Player;
        [SerializeField] Vector3 m_CameraOffset = new Vector3(0, 6, -9);
        [SerializeField] float m_MoveSpeed = 6f, m_TurnSpeed = 120f;
        [SerializeField] List<HUDUnit> m_Units = new List<HUDUnit>();
        [SerializeField] DamageTextSpawner m_Damage;
        [SerializeField] HitDirection m_Hit;
        [SerializeField] BuffBar m_Buffs;
        [SerializeField] HealthBar m_PlayerBar;
        [SerializeField] SkillButton[] m_Skills;
        [SerializeField] QuestTracker m_Quest;

        int m_PlayerHp = 2000;
        const int PlayerMaxHp = 2000;
        float m_EnemyTimer = 2f, m_RegenTimer = 3f;
        readonly List<(HUDUnit unit, float time)> m_Respawn = new List<(HUDUnit, float)>();

        void Start()
        {
            if (m_Skills != null) foreach (var s in m_Skills) if (s != null) s.Cast += OnCast;
            if (m_PlayerBar != null) m_PlayerBar.SetHealth(m_PlayerHp, PlayerMaxHp, true);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            m_Player.Rotate(0, Input.GetAxis("Horizontal") * m_TurnSpeed * dt, 0);
            m_Player.position += m_Player.forward * (Input.GetAxis("Vertical") * m_MoveSpeed * dt);

            // 点到 UI 上时不要穿透去打怪
            if (Input.GetMouseButtonDown(0) && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()))
            {
                if (Physics.Raycast(m_Camera.ScreenPointToRay(Input.mousePosition), out var hit, 100f))
                {
                    var u = hit.collider.GetComponentInParent<HUDUnit>();
                    if (u != null) Hit(u, Random.Range(80, 160), Random.value < 0.2f);
                }
            }
            EnemyAttacks(dt);
            for (int i = m_Respawn.Count - 1; i >= 0; i--)
            {
                if (Time.time < m_Respawn[i].time) continue;
                m_Respawn[i].unit.gameObject.SetActive(true);
                m_Respawn[i].unit.ResetHp();
                m_Respawn.RemoveAt(i);
            }
        }

        void LateUpdate() => PlaceCamera();

        public void PlaceCamera()
        {
            m_Camera.transform.position = m_Player.position + m_Player.rotation * m_CameraOffset;
            m_Camera.transform.LookAt(m_Player.position + m_Player.forward * 6 + Vector3.up * 1.5f);
        }

        public void Hit(HUDUnit u, int amount, bool crit)
        {
            if (crit) amount *= 2;
            if (m_Damage != null) m_Damage.Spawn(u.transform.position + Vector3.up * 1.4f, amount, crit ? DamageKind.Crit : DamageKind.Normal);
            if (!u.TakeDamage(amount)) return;
            u.gameObject.SetActive(false);
            m_Respawn.Add((u, Time.time + 3f));
            if (m_Quest != null && m_Quest.Quests.Count > 0)
            {
                var o = m_Quest.Quests[0].objectives[0];
                m_Quest.SetProgress(0, 0, o.current + 1);
            }
        }

        void OnCast(SkillButton skill, float charge)
        {
            int index = System.Array.IndexOf(m_Skills, skill);
            var nearest = Nearest(14f);
            switch (index)
            {
                case 0:   // 快速斩：三层充能
                    if (nearest != null) Hit(nearest, Random.Range(150, 220), Random.value < 0.3f);
                    break;
                case 1:   // 范围技：附近全部受伤 + 加攻击 Buff
                    foreach (var u in m_Units)
                        if (u.Alive && u.gameObject.activeSelf && Vector3.Distance(u.transform.position, m_Player.position) < 10f)
                            Hit(u, Random.Range(200, 260), false);
                    if (m_Buffs != null) m_Buffs.Add("atk", "item_sword", 8f, false, 3);
                    break;
                case 2:   // 蓄力射击：伤害随蓄力增加，蓄满必暴击
                    if (nearest != null) Hit(nearest, Mathf.RoundToInt(150 + 450 * charge), charge >= 0.99f);
                    break;
            }
        }

        HUDUnit Nearest(float range)
        {
            HUDUnit best = null;
            float bestD = range;
            foreach (var u in m_Units)
            {
                if (!u.Alive || !u.gameObject.activeSelf) continue;
                float d = Vector3.Distance(u.transform.position, m_Player.position);
                if (d < bestD) { bestD = d; best = u; }
            }
            return best;
        }

        void EnemyAttacks(float dt)
        {
            m_EnemyTimer -= dt;
            m_RegenTimer -= dt;
            if (m_RegenTimer <= 0 && m_PlayerHp < PlayerMaxHp)
            {
                m_RegenTimer = 3f;
                m_PlayerHp = Mathf.Min(PlayerMaxHp, m_PlayerHp + 60);
                if (m_Damage != null) m_Damage.Spawn(m_Player.position + Vector3.up * 2f, 60, DamageKind.Heal);
                if (m_PlayerBar != null) m_PlayerBar.SetHealth(m_PlayerHp, PlayerMaxHp);
            }
            if (m_EnemyTimer > 0) return;
            m_EnemyTimer = 2.5f;
            var attacker = Nearest(7f);
            if (attacker == null) return;
            if (Random.value < 0.25f)
            {
                if (m_Damage != null) m_Damage.Spawn(m_Player.position + Vector3.up * 2f, 0, DamageKind.Miss);
                return;
            }
            m_PlayerHp = Mathf.Max(1, m_PlayerHp - 90);
            if (m_PlayerBar != null) m_PlayerBar.SetHealth(m_PlayerHp, PlayerMaxHp);
            if (m_Hit != null) m_Hit.Show(attacker.transform.position);
            if (m_Buffs != null && Random.value < 0.4f) m_Buffs.Add("poison", "item_potion_green", 5f, true);
        }
    }
}
