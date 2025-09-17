using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class ShowMonsterController : MonoBehaviour
{
    private Dictionary<string, GameObject> _monsters = new Dictionary<string, GameObject>();
    private string _monster;

    private void Awake()
    {
        FindAndAddController();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<StageType>("ChangeStage", TurnOnTheMonster);
        TurnOnTheMonster(StageType.Stage1);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeStage", (Action<StageType>)TurnOnTheMonster);
    }
    private void FindAndAddController()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            GameObject monster = child.gameObject;
            _monsters.Add(monster.name, monster);
            monster.gameObject.SetActive(false);
            monster.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        }
    }

    private void TurnOnTheMonster(StageType type)
    {
        if(_monster!=null)
            _monsters[_monster].gameObject.SetActive(false);
        string name;
        switch(type)
        {
            case StageType.Infinity:
                name = "EBlock";
                break;
            case StageType.Stage1:
                name = "Block";
                break;
            case StageType.Stage2:
                name = "EPlanet";
                break;

            case StageType.Stage3:
                name = "Planet";
                break;
            default:
                name = "Planet";
                break;
        }
        _monsters[name].gameObject.SetActive(true);
        Animator anima = _monsters[name].GetComponent<Animator>();
        anima.SetBool("isMoving", true);
        _monster = name;
    }
}
