using UnityEngine;
using UnityEngine.U2D;
using Utils;

public class ChefBomb : MonoBehaviour
{
    SpriteRenderer _spriteRenderer;
    SpriteAtlas _atlas;
    int _damage;

    public void Init(int damage, int level)
    {
        if(_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        _damage = damage;
        _atlas = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.SpriteAtlas).GetPrefabAtlas(EAtlasPrefabType.ChefBombAtlas);
        _spriteRenderer.sprite = _atlas.GetSprite(((EChefBombType)level).ToString());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<Monster>(out Monster monster))
        {
            monster.TakeDamage(_damage);
            MonoSingleton<ObjectPoolManager>.Instance.Push(EBulletType.ChefBomb, gameObject);
        }
    }
}