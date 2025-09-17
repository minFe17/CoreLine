using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;

public class BackGroundController : MonoBehaviour
{
    private readonly float MAX_RANGE = 25;
    private readonly float MIN_RANGE = -24.5f;
    private float speed = 3f;
    
    private Dictionary<StageType, Transform> _stages = new();
    private StageType _curType = StageType.Stage1;
    private Vector3 _position = new Vector3();


    private void Start()
    {
        SettingStage();
        ChangeImage(StageType.Stage1);
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<StageType>("ChangeStage", ChangeImage);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeStage", (Action<StageType>)ChangeImage);
    }
    private void Update()
    {
        if (UIManager.Instance.FrontPanel() == PanelStatus.UpgradePanel ||
            UIManager.Instance.FrontPanel() == PanelStatus.SettingPanel)
            return;
        _position.x -= speed * Time.deltaTime;

        if (_position.x < MIN_RANGE)
            _position.x = MAX_RANGE;
        
        _stages[_curType].position = _position;
    }
    private void SettingStage()
    {
        Transform background = GameObject.Find("BackGround")?.transform;

        if (background != null)
        {
            foreach (Transform child in background)
            {
                StageType type;
                if (Enum.TryParse(child.name, out type))
                {
                    _stages[type] = child;
                    _stages[type].gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning("BackGround 하위에 StageType에 없는 이름이 있습니다: " + child.name);
                }
            }
        }
    }
    private void ChangeImage(StageType type)
    {
        _position = new Vector3(0,0,0);
        _stages[_curType].position = new Vector3(0,0,0);
        _stages[_curType].gameObject.SetActive(false);
        _curType = type;
        _stages[type].gameObject.SetActive(true);
    }
}
