// 伤害飘字：对象池 + 每帧把世界坐标换算到屏幕。
// 一场团战每秒可能冒出几十个数字，Instantiate/Destroy 会带来 GC 和卡顿，必须复用。
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UGUIBook.Ch17
{
    public enum DamageKind { Normal, Crit, Heal, Miss }

    //#region item
    /// 单个飘字：只管自己的动画曲线，位置由 Spawner 统一算
    public class DamageText : MonoBehaviour
    {
        [SerializeField] TMP_Text m_Text;

        public Vector3 WorldPos { get; private set; }
        public float Age { get; set; }
        public float Life { get; private set; }
        Vector2 m_Drift;
        float m_BaseScale;

        public void Setup(Vector3 worldPos, int amount, DamageKind kind, Vector2 drift)
        {
            WorldPos = worldPos;
            Age = 0;
            m_Drift = drift;
            switch (kind)
            {
                case DamageKind.Crit:
                    m_Text.text = $"<size=60%>暴击</size>{amount}";
                    m_Text.color = new Color32(0xFF, 0x9F, 0x1C, 0xFF);
                    m_BaseScale = 1.5f; Life = 1.1f;
                    break;
                case DamageKind.Heal:
                    m_Text.text = "+" + amount;
                    m_Text.color = new Color32(0x5B, 0xE5, 0x84, 0xFF);
                    m_BaseScale = 1.1f; Life = 1f;
                    break;
                case DamageKind.Miss:
                    m_Text.text = "闪避";
                    m_Text.color = new Color32(0xC9, 0xCF, 0xDD, 0xFF);
                    m_BaseScale = 0.9f; Life = 0.8f;
                    break;
                default:
                    m_Text.text = amount.ToString();
                    m_Text.color = Color.white;
                    m_BaseScale = 1f; Life = 0.9f;
                    break;
            }
        }

        /// 根据年龄算出相对锚点的偏移、缩放和透明度
        public void Evaluate(Vector2 anchorLocal)
        {
            float t = Mathf.Clamp01(Age / Life);
            // 0~0.12：从 1.8 倍“砸”下来；之后保持
            float pop = t < 0.12f ? Mathf.Lerp(1.8f, 1f, t / 0.12f) : 1f;
            // 上升：先快后慢（EaseOutCubic）
            float rise = 1 - Mathf.Pow(1 - t, 3);
            var rt = (RectTransform)transform;
            rt.localPosition = anchorLocal + new Vector2(m_Drift.x * rise, 40 + m_Drift.y * rise);
            rt.localScale = Vector3.one * (m_BaseScale * pop);
            // 最后 30% 的时间淡出
            m_Text.alpha = t < 0.7f ? 1f : Mathf.InverseLerp(1f, 0.7f, t);
        }
    }
    //#endregion
}
