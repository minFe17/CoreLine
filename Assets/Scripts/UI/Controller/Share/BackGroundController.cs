using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BackGroundController : MonoBehaviour
{
    private Dictionary<StageType, Image> _images = new();
    private StageType _curType = StageType.Stage1;

    private void Start()
    {
        //SettingImage();
        //ChangeImage(StageType.Stage1);
    }
    private void OnEnable()
    {
        //EventManager.Instance.Subscribe<StageType>("ChangeStage", ChangeImage);
    }
    private void OnDisable()
    {
       // EventManager.Instance.UnSubscribe("ChangeStage", (Action<StageType>)ChangeImage);
    }
    private void SettingImage()
    {
        foreach (var stage in DataManager.Instance.WorldStageDatas)
        {
            _images[stage.StageType] = GameObject.Find("BackGround/" + stage.StageType.ToString()).GetComponent<Image>();
        }

    }
    private void ChangeImage(StageType type)
    {
        _images[_curType].gameObject.SetActive(false);
        _curType = type;
        _images[type].gameObject.SetActive(true);
    }
}
