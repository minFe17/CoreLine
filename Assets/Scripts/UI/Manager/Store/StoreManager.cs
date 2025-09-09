using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StoreManager : MonoBehaviour
{
    private List<GameObject> _rows = new List<GameObject>();

    private Dictionary<StoreType, PoolingManager> _buttons = new();

    private void Awake()
    {
        MatchRows();
        CreateButtons();
        
    }
    private void Start()
    {
        SettingButtons();
    }
    private void MatchRows()
    {
        HorizontalLayoutGroup[] rows = GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup row in rows)
        {
            _rows.Add(row.gameObject);
        }
    }
    private void CreateButtons()
    {
        int count = 0;
        _buttons[StoreType.Money] = new PoolingManager("UI/Prefabs/Button/Store/StoreButton", _rows[count++], 10);
        _buttons[StoreType.Gem] = new PoolingManager("UI/Prefabs/Button/Store/StoreButton", _rows[count++], 10);
        _buttons[StoreType.InfinityKey] = new PoolingManager("UI/Prefabs/Button/Store/StoreButton", _rows[count++], 10);
    }
    private void SettingButtons()
    {
        List<StoreData> datas = DataManager.Instance.StoreDatas;

        foreach (StoreData data in datas)
        {
            StoreButton btn = _buttons[data.StoreType].Pop().GetComponent<StoreButton>();
            btn.Data = data;
        }
    }
}
