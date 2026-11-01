// 旗帜飘动：每帧推进材质的 _WavePhase。这里用的是材质“实例”（new Material），
// 所以这面旗会单独占一个批次——和 UIParamEffect 走顶点通道的做法正好对照。
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch14
{
    [RequireComponent(typeof(Graphic))]
    public class FlagWave : MonoBehaviour
    {
        [SerializeField] float m_Speed = 4;
        Material m_Instance;
        static readonly int s_Phase = Shader.PropertyToID("_WavePhase");

        void Start()
        {
            var g = GetComponent<Graphic>();
            m_Instance = new Material(g.material);   // 不改共享的材质资源
            g.material = m_Instance;
        }

        void Update() => m_Instance.SetFloat(s_Phase, -Time.time * m_Speed);

        void OnDestroy() { if (m_Instance != null) Destroy(m_Instance); }
    }
}
