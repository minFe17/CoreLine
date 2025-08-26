using System.Collections.Generic;
using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    [SerializeField] private BuildUI buildUIPrefab;  // 프리팹 참조
    private BuildUI _buildUIInstance;
    void Start()
    {
        // Canvas 밑에 인스턴스 생성
        var canvas = FindAnyObjectByType<Canvas>();
        _buildUIInstance = Instantiate(buildUIPrefab, canvas.transform);
    }

    public void ShowBuildUI(Vector3 worldPos, List<TowerOption> options, System.Action<TowerOption> onPick)
    {
        _buildUIInstance.OpenAtWorld(worldPos, options, onPick);
    }
}
