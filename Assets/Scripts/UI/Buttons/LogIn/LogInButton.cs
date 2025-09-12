using TMPro;
using UnityEngine;

public class LogInButton : BaseButton
{
    [SerializeField]
    private TMP_InputField _email;
    [SerializeField]
    private TMP_InputField _password;
    protected override void OnClick()
    {
        FireBaseManager.Instance.LogInToEmail(_email.text, _password.text);
    }
}
