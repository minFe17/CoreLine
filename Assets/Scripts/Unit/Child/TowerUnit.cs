using UnityEngine;

public class TowerUnit : Unit, ILevelUp, IFusion
{
    void OnMouseDown()
    {
        Debug.Log(1);
        // UI ��ȯ
    }

    void ILevelUp.Upgrade()
    {
        // ������Ʈ Ǯ���� ã��?
    }

    void IFusion.Fusion()
    {

    }
}