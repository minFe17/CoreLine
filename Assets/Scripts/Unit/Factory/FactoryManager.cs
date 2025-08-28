using System.Collections.Generic;
using UnityEngine;
using Utils;

public class FactoryManager : MonoBehaviour
{
    // ╫л╠шео
    List<IFactory> _factories = new List<IFactory>();

    public void Init()
    {
        SetFactories();

        for (int i = 0; i < _factories.Count; i++)
            _factories[i].Register();
    }

    void SetFactories()
    {
        _factories.Add(new AttackRangeFactory());
        // юс╫ц
        for (int i = 0; i < (int)EUnitType.Max; i++)
            _factories.Add(new UnitFactory((EUnitType)i));
        for (int i = 0; i < (int)EFusionUnitType.Max; i++)
            _factories.Add(new FusionUnitFactory((EFusionUnitType)i));
    }
}
