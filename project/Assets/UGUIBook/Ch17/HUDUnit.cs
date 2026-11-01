using UnityEngine;

namespace UGUIBook.Ch17
{
    /// 示例里的敌人：有血量、会原地巡逻；血条是一个 HUDFollower 下的 HealthBar
    public class HUDUnit : MonoBehaviour
    {
        public string unitName = "史莱姆";
        public int maxHp = 1000;
        [SerializeField] HealthBar m_Bar;
        [SerializeField] float m_PatrolRadius = 1.5f;
        [SerializeField] float m_PatrolSpeed = 0.5f;

        public int Hp { get; private set; }
        public bool Alive => Hp > 0;
        public HealthBar Bar => m_Bar;

        Vector3 m_Home;
        float m_Phase;

        void Awake()
        {
            m_Home = transform.position;
            m_Phase = Random.value * Mathf.PI * 2;
            ResetHp();
        }

        public void ResetHp()
        {
            Hp = maxHp;
            if (m_Bar != null) m_Bar.SetHealth(Hp, maxHp, true);
        }

        /// 返回 true 表示这一下打死了
        public bool TakeDamage(int amount)
        {
            if (!Alive) return false;
            Hp = Mathf.Max(0, Hp - amount);
            if (m_Bar != null) m_Bar.SetHealth(Hp, maxHp);
            return Hp == 0;
        }

        void Update()
        {
            m_Phase += Time.deltaTime * m_PatrolSpeed;
            transform.position = m_Home + new Vector3(Mathf.Cos(m_Phase), 0, Mathf.Sin(m_Phase)) * m_PatrolRadius;
        }
    }
}
