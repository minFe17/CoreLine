using UnityEngine;

public class UIController : MonoBehaviour
{
    private void Start()
    {
       UIManager.Instance.AddPanelStack(PanelStatus.LobbyPanel);
    }

    private void OnDestroy()
    {
        UIManager.Instance.ClearPanel();
        UIManager.Instance.ClearPopUp();
    }
}
