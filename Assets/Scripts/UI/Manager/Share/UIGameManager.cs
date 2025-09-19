using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using static SkillManager;

public class UIGameManager : MonoBehaviour
{
    private GameObject _uiPanel;
    private GameObject _unitAnimation;

    private void Awake()
    {
        _uiPanel = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UIPanel);
        _unitAnimation = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UnitAnimations);
        Instantiate(_uiPanel);
        Instantiate(_unitAnimation);
    }
    private void Start()
    {

        MonoSingleton<AudioClipManager>.Instance.StopBGM();
        MonoSingleton<AudioClipManager>.Instance.PlayBGM(EBGMType.UI_BGM2);
        //SkillManager.Instance.ClearLoadout();
    }
}
