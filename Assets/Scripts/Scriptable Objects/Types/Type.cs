using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum WeatherType{
    None,
    Rain,
    Sun,
    Snow,
    Sandstorm
}
public enum TerrainType{
    None,
    Electric,
    Grassy,
    Psychic,
    Misty
}
public enum StatusEffect{
    None,
    Poison,
    Sleep,
    Paralysis,
    Burn,
    Confusion,
    Attraction
}
public enum BuffLevel { 
    None,
    AttackSlight,
    Attack1,
    Attack2,
    Attack3,
    Attack4,
    DefenseSlight,
    Defense1,
    Defense2,
    Defense3,
    Defense4,
    SpAttackSlight,
    SpAttack1,
    SpAttack2,
    SpAttack3,
    SpAttack4,
    SpDefenseSlight,
    SpDefense1,
    SpDefense2,
    SpDefense3,
    SpDefense4,
    SpeedSlight,
    Speed1,
    Speed2,
    Speed3,
    Speed4
}
public enum TypeName{
    Normal,
    Fire, 
    Water,
    Grass,
    Flying,
    Bug,
    Ground,
    Rock,
    Fighting,
    Psychic,
    Poison,
    Electric,
    Ice,
    Steel,
    Dark,
    Ghost,
    Dragon,
    Fairy
}
[CreateAssetMenu(fileName = "New Pokemon Type", menuName = "Pokemon/Pokemon Type", order = 103)]
public class Type : ScriptableObject {

    public TypeName typeName;

    public List<TypeName> weaknesses;
    public List<TypeName> resistances;
    public List<TypeName> immunities;

    public Color typeColor;
    public Sprite typeIcon;

}
