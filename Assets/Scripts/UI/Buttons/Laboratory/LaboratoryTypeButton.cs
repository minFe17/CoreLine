using UnityEngine;

public class LaboratoryTypeButton : BaseButton
{
    [SerializeField]
    private LaboratoryType _type;


    protected override void OnClick()
    {
        EventManager.Instance.Invoke<LaboratoryType>("ChoiceContent", _type);
       
    }

   
}
