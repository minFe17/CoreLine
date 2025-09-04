public abstract class LabData
{
    public abstract void ApplyEffect(LaboratoryData data);
}

[System.Serializable]
public class UnitLabData : LabData
{
    public Pair<ValueType, float> AttackDamage = new();
    public Pair<ValueType, float> AttackSpeed = new();
    public Pair<ValueType, float> Shield = new();
    public Pair<ValueType, float> HealthPoint = new();

    public override void ApplyEffect(LaboratoryData data)
    {
        Effect effect = data.Effect;
        switch (effect.TargetStatus)
        {
            case TargetStatus.AttackDamage:
                AttackDamage = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
            case TargetStatus.AttackSpeed:
                AttackSpeed = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
            case TargetStatus.Shield:
                Shield = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
            case TargetStatus.HealthPoint:
                HealthPoint = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
        }
    }
}

[System.Serializable]
public class KingLabData : LabData
{
    public Pair<ValueType, float> Heal = new();
    public Pair<ValueType, float> Shield = new();
    public Pair<ValueType, float> HealthPoint = new();

    public override void ApplyEffect(LaboratoryData data)
    {
        Effect effect = data.Effect;
        switch (effect.TargetStatus)
        {
            case TargetStatus.Heal:
                Heal = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
            case TargetStatus.Shield:
                Shield = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
            case TargetStatus.HealthPoint:
                HealthPoint = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                break;
        }
    }
}

[System.Serializable]
public class UtilityLabData : LabData
{
    public Pair<ValueType, float> GetMoney = new();
    public Pair<ValueType, float> GetSpawnUnitCost = new();
    public Pair<ValueType, float> SubPlayTime = new();
    public Pair<ValueType, float> SubTimeUnitCost = new();

    public override void ApplyEffect(LaboratoryData data)
    {
        Effect effect = data.Effect;
        switch (effect.TargetStatus)
        {
            case TargetStatus.SubTime:
                {
                    if (effect.TargetType == TargetType.PlayTime)
                        SubPlayTime = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                    else
                        SubTimeUnitCost = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                    break;
                }
            case TargetStatus.GetMoney:
                {
                    if (effect.TargetType == TargetType.Money)
                        GetMoney = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                    else
                        GetSpawnUnitCost = new Pair<ValueType, float>(effect.ValueType, effect.Value);
                    break;
                }
        }
    }
}