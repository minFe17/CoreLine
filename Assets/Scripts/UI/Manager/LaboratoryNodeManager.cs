using UnityEngine;
using System.Collections.Generic;
using Utils;

public class LaboratoryNodeManager : MonoBehaviour 
{
    private Dictionary<LaboratoryType, PoolingManager> _nodes = new Dictionary<LaboratoryType, PoolingManager>();

    private void Start()
    {
        CreateNodes();
        SpawnNodes();
    }
    private void CreateNodes()
    {
        string prefabsPath = "UI/Prefabs/Button/Laboratory/LaboratoryNode";
        string parentPath = "UI/LaboratoryPanel/NodePanel/Viewport/";
        _nodes[LaboratoryType.Attack] = new PoolingManager(prefabsPath, parentPath + "Attack", 20);
        _nodes[LaboratoryType.Defense] = new PoolingManager(prefabsPath, parentPath + "Defense", 20);
        _nodes[LaboratoryType.Utility] = new PoolingManager(prefabsPath, parentPath + "Utility", 20);
    }
    private void SpawnNodes()
    {
        //연구실 데이터 들고온거기준으로 빼자.
    }
}
