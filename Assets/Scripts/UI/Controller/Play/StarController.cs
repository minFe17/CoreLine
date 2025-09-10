using UnityEngine;
using UnityEngine.UI;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public class StarController : MonoBehaviour
{
    private List<Pair<int,Image>> _stars = new List<Pair<int, Image>>();

    private void Awake()
    {
        FindImage();
       
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<NormalStageData>("SelectStage", UpdateStar);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("SelectStage", (Action<NormalStageData>)UpdateStar);
    }
    private void UpdateStar(NormalStageData data)
    {
        List<ClearStage> clearStage = DataManager.Instance.GameData.ClearStage;
        foreach (ClearStage stage in clearStage)
        {
            if (stage.StageId == data.Id)
            {
                ChangeStar(stage);
                return;
            }
        }
        foreach (var star in _stars)
        {
            star.Second.sprite = Resources.Load<Sprite>("UI/Image/Icon/ItemIcon_Star_Disable");
        }
    }
    private void FindImage()
    {
        Image[] images = GetComponentsInChildren<Image>();
        int count = 1;
        foreach (Image image in images)
        {
            _stars.Add(new Pair<int, Image>(count++, image));
        }
    }
    private void ChangeStar(ClearStage stage)
    {
        foreach(var star in _stars)
        {
            switch (star.First)
            {
                case 1:
                    {
                        if (stage.Star.FirstStar)
                            star.Second.sprite = Resources.Load<Sprite>("UI/Image/Icon/ItemIcon_Star");
                        continue ;
                    }
                case 2:
                    {
                        if (stage.Star.SecondStar)
                            star.Second.sprite = Resources.Load<Sprite>("UI/Image/Icon/ItemIcon_Star");
                        continue;
                    }
                case 3:
                    {
                        if (stage.Star.ThirdStar)
                            star.Second.sprite = Resources.Load<Sprite>("UI/Image/Icon/ItemIcon_Star");
                        continue;
                    }
            }
            star.Second.sprite = Resources.Load<Sprite>("UI/Image/Icon/ItemIcon_Star_Disable");
        }
        

    }
}
