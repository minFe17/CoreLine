using UnityEngine;

public class StageButton : MonoBehaviour
{
    private NormalStageData _data;

    public NormalStageData Data
    {
        get { return _data; }
        set { _data = value; }
    }
}
