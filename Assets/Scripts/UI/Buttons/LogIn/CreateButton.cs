using TMPro;
using UnityEngine;

public class CreateButton : BaseButton
{
    [SerializeField]
    private TMP_InputField _email;
    [SerializeField]
    private TMP_InputField _password;
    protected override void OnClick()
    {
        base.OnClick();
        FireBaseManager.Instance.CreateToEmail(_email.text, _password.text);
    }
}
