using System.Runtime.CompilerServices;
using UnityEngine;

public class UpgradePanelManager : MonoBehaviour
{
    private PoolingManager _buttons;

    private void Start()
    {
        GameObject parent = GameObject.Find("UpgradeButtons");
        _buttons = new PoolingManager("UI/Prefabs/Button/Upgrade/UpgradeButton", parent, 4);
        CreateButtons();
    }
    private void CreateButtons()
    {
        for(int i=0;i<=(int)UpgradeType.AttackSpeed; i++)
        {
            UpgradeButton btn = _buttons.Pop().GetComponent<UpgradeButton>();
            btn.Status = (UpgradeType)i;
        }
    }
}
