// 加载界面：配套场景里用“假任务”模拟加载过程；LoadScene 演示真实项目里如何接 SceneManager.LoadSceneAsync。
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UGUIBook.Ch07
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] ProgressBar m_Bar;
        [SerializeField] TMP_Text m_Stage;            // 当前阶段说明
        [SerializeField] TMP_Text m_Tip;              // 小贴士
        [SerializeField] Button m_Replay;             // 演示用：重新跑一遍

        static readonly string[] kStages = { "读取配置表…", "加载角色模型…", "加载场景贴图…", "初始化音频…", "进入游戏…" };
        static readonly string[] kTips =
        {
            "小贴士：长按技能按钮可以查看技能详情。",
            "小贴士：设置里可以单独调整音乐和音效音量。",
            "小贴士：背包满了以后，新拾取的物品会进入邮件。",
        };

        void Start()
        {
            m_Replay.onClick.AddListener(() => { StopAllCoroutines(); StartCoroutine(FakeLoad()); });
            StartCoroutine(FakeLoad());
        }

        //#region fake
        IEnumerator FakeLoad()
        {
            m_Replay.gameObject.SetActive(false);
            m_Bar.SetProgress(0, true);
            m_Tip.text = kTips[Random.Range(0, kTips.Length)];
            for (int i = 0; i < kStages.Length; i++)
            {
                m_Stage.text = kStages[i];
                // 每个阶段耗时不同；进度条只设“目标”，由 ProgressBar 平滑追上去
                float cost = Random.Range(0.4f, 1.2f);
                for (float t = 0; t < cost; t += Time.unscaledDeltaTime)
                {
                    m_Bar.SetProgress((i + t / cost) / kStages.Length);
                    yield return null;
                }
            }
            m_Bar.SetProgress(1);
            while (m_Bar.Shown < 1) yield return null;     // 等显示也走到 100% 再结束
            m_Stage.text = "加载完成";
            m_Replay.gameObject.SetActive(true);
        }
        //#endregion

        //#region async
        /// 真实项目的写法：AsyncOperation.progress 在 allowSceneActivation = false 时最多只到 0.9，
        /// 剩下的 0.1 是“激活场景”，所以要除以 0.9 再映射到进度条
        public IEnumerator LoadScene(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;              // 先别切，等进度条走满、玩家看清再切
            while (op.progress < 0.9f)
            {
                m_Bar.SetProgress(op.progress / 0.9f);
                yield return null;
            }
            m_Bar.SetProgress(1);
            while (m_Bar.Shown < 1) yield return null;
            op.allowSceneActivation = true;
        }
        //#endregion

        /// 截图用：直接摆出“加载到一半”的样子
        public void Preview(int stage, float progress)
        {
            m_Stage.text = kStages[stage];
            m_Tip.text = kTips[0];
            m_Bar.SetProgress(progress, true);
            m_Replay.gameObject.SetActive(false);
        }

        public void Setup(ProgressBar bar, TMP_Text stage, TMP_Text tip, Button replay)
        {
            m_Bar = bar; m_Stage = stage; m_Tip = tip; m_Replay = replay;
        }
    }
}
