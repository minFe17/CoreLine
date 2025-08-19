using System;
using System.Threading.Tasks;
using UnityEngine;
using Utils;

public abstract class PrefabLoadBase
{
    protected AddressableManager _addressableManager;

    public virtual void Init()
    {
        if (_addressableManager == null)
            _addressableManager = SimpleSingleton<AddressableManager>.Instance;
    }

    public abstract Task LoadPrefab();
    public virtual GameObject GetPrefab() => null;
    public virtual T GetPrefab<T>() => default(T);
    public virtual GameObject GetPrefab<TEnum>(TEnum type) where TEnum : Enum => null;
    public virtual TextAsset GetPrefabTextAsset<TEnum>(TEnum type) where TEnum : Enum => null;
}