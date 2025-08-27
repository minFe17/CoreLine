using UnityEngine;
using Utils;

public class BuildUIFactory : IFactory
{
    GameObject _buildUIPrefab;

    #region Interface
    GameObject IFactory.Create()
    {
        if (_buildUIPrefab == null)
            _buildUIPrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.BuildUI);
        return Object.Instantiate(_buildUIPrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(EPrefabType.UI, this);
    }
    #endregion
}
