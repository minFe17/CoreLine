using UnityEngine;

public class TowerUnit : Unit, ILevelUp, IFusion
{
    void OnMouseDown()
    {
        Debug.Log(1);
        // UI 소환
    }

    void ILevelUp.Upgrade()
    {
        // 오브젝트 풀에서 찾기?
    }

    void IFusion.Fusion()
    {

    }
}