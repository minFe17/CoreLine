using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorEffect : MonoBehaviour
{
    [SerializeField] Meteor _parent;

    List<Monster> _monsterList = new List<Monster>();
    ParticleSystem _effect;

    private void OnEnable()
    {
        if (_effect == null)
            _effect = GetComponent<ParticleSystem>();
        _effect.Clear();
        _effect.Play();
    }

    public void PlayEffect()
    {
        _monsterList.Clear();
        StartCoroutine(PlayEffectCoroutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
        {
            if (_monsterList.Contains(monster))
                return;
            _parent.SpawnSmallMeteor();
            _monsterList.Add(monster);
            _parent.HitMonster(monster);
        }
    }

    IEnumerator PlayEffectCoroutine()
    {
        _effect.Clear();
        _effect.Play();
        yield return new WaitForSeconds(_effect.main.duration);
        gameObject.SetActive(false);
        _parent.Remove();
    }
}