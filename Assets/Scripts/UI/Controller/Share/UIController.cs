using UnityEngine;

public class UIController : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.ClearPanelStack();
        UIManager.Instance.AddPanelStack(PanelStatus.LobbyPanel);
    }

}
