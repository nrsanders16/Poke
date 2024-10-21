using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonIndividual : MonoBehaviour
{
    public PokemonObject pokemonBaseInfo;
    public Sprite pokemonBattleSprite;
    public int level;

    public int currentHP;
    public int currentEnergy;
    public StatusEffect currentStatus;

    public FastMove fastMove;

    public ChargedMove chargedMove1;
    public ChargedMove chargedMove2;

    public int MaxHP
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseHP * level) / 100f) + 5; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseAttack * level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseDefense * level) / 100f) + 5; }
    }
    public int SpecialAttack
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpAttack * level) / 100f) + 5; }
    }
    public int SpecialDefense
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpDefense * level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpeed * level) / 100f) + 5; }
    }
}
