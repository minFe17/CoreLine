using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum EUnitType
{
    King,
    Archer,
    Warrior,
    Wizard,
    Chef,
    Pirate,
    ShieldSoldier,
    Assassin,
    Dwarf,
    Hammer,
    Gunner,
    Hunter,
    FireWizard,
    ThunderWizard,
    Priest,
    Max,
}