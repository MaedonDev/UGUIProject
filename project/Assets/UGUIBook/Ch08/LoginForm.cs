// 登录表单：逐项校验 + 错误提示、密码可见切换、Tab 切换焦点、回车提交。
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGUIBook.Ch08
{
    public class LoginForm : MonoBehaviour
    {
        [SerializeField] TMP_InputField m_User;
        [SerializeField] TMP_InputField m_Password;
        [SerializeField] TMP_Text m_UserError;
        [SerializeField] TMP_Text m_PasswordError;
        [SerializeField] Toggle m_ShowPassword;        // 眼睛图标
        [SerializeField] Button m_Login;
        [SerializeField] TMP_Text m_Status;
        [SerializeField] Color m_ErrorColor = new Color32(0xE5, 0x48, 0x4D, 0xFF);
        [SerializeField] Color m_NormalColor = new Color32(0xC7, 0xCC, 0xE0, 0xFF);

        static readonly Regex kUserRule = new Regex("^[A-Za-z0-9_]{4,16}$");

        //#region bind
        void Awake()
        {
            m_User.characterLimit = 16;
            m_Password.contentType = TMP_InputField.ContentType.Password;
            // 离开输入框（onEndEdit）时才校验；正在输入时只清掉旧的错误提示，不要边打字边骂人
            m_User.onEndEdit.AddListener(_ => ValidateUser());
            m_Password.onEndEdit.AddListener(_ => ValidatePassword());
            m_User.onValueChanged.AddListener(_ => ShowError(m_User, m_UserError, null));
            m_Password.onValueChanged.AddListener(_ => ShowError(m_Password, m_PasswordError, null));
            // 用户名回车 → 跳到密码框；密码框回车 → 直接登录
            m_User.onSubmit.AddListener(_ => Focus(m_Password));
            m_Password.onSubmit.AddListener(_ => TryLogin());
            m_ShowPassword.onValueChanged.AddListener(SetPasswordVisible);
            m_Login.onClick.AddListener(TryLogin);
        }

        void Update()
        {
            // Tab 在两个输入框之间切换（UGUI 默认不处理 Tab 键；字段多时可按 Selectable 的导航关系找下一个）
            if (Input.GetKeyDown(KeyCode.Tab))
                Focus(m_User.isFocused ? m_Password : m_User);
        }
        //#endregion

        //#region validate
        bool ValidateUser()
        {
            string s = m_User.text;
            string err = s.Length == 0 ? "请输入用户名"
                       : !kUserRule.IsMatch(s) ? "用户名需 4～16 位字母、数字或下划线" : null;
            ShowError(m_User, m_UserError, err);
            return err == null;
        }

        bool ValidatePassword()
        {
            string s = m_Password.text;
            string err = s.Length == 0 ? "请输入密码" : s.Length < 6 ? "密码至少 6 位" : null;
            ShowError(m_Password, m_PasswordError, err);
            return err == null;
        }

        /// err 为 null 表示清除错误。边框用的是输入框下名为 Outline 的描边图
        public void ShowError(TMP_InputField field, TMP_Text label, string err)
        {
            label.text = err ?? "";
            var outline = field.transform.Find("Outline");
            if (outline != null) outline.GetComponent<Graphic>().color = err == null ? m_NormalColor : m_ErrorColor;
        }
        //#endregion

        //#region visible
        void SetPasswordVisible(bool visible)
        {
            // 改 contentType 只改了“规则”，已经显示出来的 * 不会自动刷新，要 ForceLabelUpdate
            m_Password.contentType = visible ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.Password;
            m_Password.ForceLabelUpdate();
        }
        //#endregion

        //#region login
        void TryLogin()
        {
            // 两项都要校验（不要用 &&，否则第一项失败时第二项的错误不显示）
            bool ok = ValidateUser() & ValidatePassword();
            if (!ok) { Focus(m_UserError.text.Length > 0 ? m_User : m_Password); return; }
            StartCoroutine(FakeRequest());
        }

        IEnumerator FakeRequest()
        {
            // 请求期间禁用按钮和输入框，防止重复提交
            m_Login.interactable = m_User.interactable = m_Password.interactable = false;
            m_Status.text = "登录中…";
            yield return new WaitForSecondsRealtime(1f);
            m_Status.text = $"欢迎回来，{m_User.text}！";
            m_Login.interactable = m_User.interactable = m_Password.interactable = true;
        }

        static void Focus(TMP_InputField f)
        {
            // Select 让 EventSystem 选中它；ActivateInputField 让它真正进入编辑状态（下一帧生效）
            f.Select();
            f.ActivateInputField();
        }
        //#endregion

        public void Setup(TMP_InputField user, TMP_InputField pwd, TMP_Text userErr, TMP_Text pwdErr, Toggle show, Button login, TMP_Text status)
        {
            m_User = user; m_Password = pwd; m_UserError = userErr; m_PasswordError = pwdErr; m_ShowPassword = show; m_Login = login; m_Status = status;
        }
    }
}
