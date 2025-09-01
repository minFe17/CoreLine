using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System;
using Unity.VisualScripting;

public class ShowUnitController : MonoBehaviour
{
    private bool _isStart = false;
    private Dictionary<EUnitType, GameObject> _units = new Dictionary<EUnitType, GameObject>();
    private EUnitType _turnOntheUnitType;

    private void Awake()
    {
        FindAndAddController();
        SettingUnits();
    }
    private void Start()
    {
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", TurnOnTheUnit);
        EventManager.Instance.Subscribe("Reset", ResetUnit);
    }
    private void FindAndAddController()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GameObject unit = child.gameObject;
            // 기존 컴포넌트 제거
            Component[] components = unit.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp is Transform) continue; // Transform은 제거 X
                
                DestroyImmediate(comp); // 에디터에서 바로 제거
            }
            // 이름 → enum 변환
            string unitName = unit.name;
            EUnitType unitType = (EUnitType)Enum.Parse(typeof(EUnitType), unitName);

            child.AddComponent<UnitAnimationController>().UnitType = unitType;
            //child.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);   
            child.gameObject.layer = LayerMask.NameToLayer("Unit");
            SetLayerRecursively(child.gameObject, LayerMask.NameToLayer("Unit"));
        }
    }
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    private void SettingUnits()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var unit = child.GetComponent<UnitAnimationController>();
            if (unit == null) continue;

            if (!_units.ContainsKey(unit.UnitType))
                _units.Add(unit.UnitType, child.gameObject);
            else
                Debug.LogWarning($"중복된 UnitType 발견: {unit.UnitType}, 무시됨.");

            child.gameObject.SetActive(false);
        }
    }
    private void TurnOnTheUnit(EUnitType type)
    {
        TurnOffTheUnit();
        _units[type].gameObject.SetActive(true);
        _turnOntheUnitType = type;
    }
    private void TurnOffTheUnit()
    {
        if (_turnOntheUnitType == EUnitType.King) return;
        _units[_turnOntheUnitType].gameObject.SetActive(false);
    }
    private void ResetUnit()
    {
        TurnOffTheUnit();
        _turnOntheUnitType = UnitManager.Instance.ChoiceUnit.UnitType;
        TurnOnTheUnit(_turnOntheUnitType);
    }
}
