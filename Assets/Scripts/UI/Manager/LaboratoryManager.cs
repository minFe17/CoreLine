using UnityEngine;
using System.Collections.Generic;
using Utils;

public class LaboratoryManager : MonoSingleton<LaboratoryManager>
{
    //이거 모노비헤이비어 빼주고 써야될듯.
    private Dictionary<LaboratoryType, PoolingManager> _nodes = new Dictionary<LaboratoryType, PoolingManager>();

    private void Start()
    {
        CreateNodes();
    }
    private void CreateNodes()
    {
        //_nodes[LaboratoryType.Attack] = new PoolingManager()
    }
}
