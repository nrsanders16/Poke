using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PokemonIndividual : MonoBehaviour {
    public PokemonObject pokemonBaseInfo;
    public Sprite currentPokemonBattleSprite;
    public Sprite basePokemonBattleSprite;
    public Sprite[] alternateFormSprites;
    public bool shiny;
    public bool shadow;
    public bool purified;
    public bool mega;
    public bool dynamax;
    public bool tera;

    public int level;
    int battlePower;
    int currentExp;

    public int currentHP;
    public int currentEnergy;
    public StatusEffect currentStatus;
    public float[] currentBuffs;
    public bool formChanged;
    public Type[] battleType;

    public TypeName teraType;

    public int IV_HP;
    public int IV_Attack;
    public int IV_Defense;
    public int IV_SpAttack;
    public int IV_SpDefense;
    public int IV_Speed;

    public FastMove fastMove;
    public ChargedMove chargedMove1;
    public ChargedMove chargedMove2;

    private void Awake() {
        currentBuffs = new float[4];
        SetBattleSprite();

        if (pokemonBaseInfo.SecondaryType != null) {
            battleType = new Type[2];
            battleType[1] = pokemonBaseInfo.SecondaryType;
        } else {
            battleType = new Type[1];
        }
        battleType[0] = pokemonBaseInfo.PrimaryType;
    }

    public void SetBattleSprite() {
        if (shiny) {
            basePokemonBattleSprite = pokemonBaseInfo.ShinyPokemonBattleSprite;
            alternateFormSprites = pokemonBaseInfo.ShinyAlternateFormSprites;
        } else {
            basePokemonBattleSprite = pokemonBaseInfo.PokemonBattleSprite;
            alternateFormSprites = pokemonBaseInfo.AlternateFormSprites;
        }
    }

    public int BattlePower {
        get {
            float baseStamina = 2 * pokemonBaseInfo.BaseHP;
            float baseAttack = 2 * Mathf.Round(Mathf.Sqrt(pokemonBaseInfo.BaseAttack) * Mathf.Sqrt(pokemonBaseInfo.BaseSpAttack) + Mathf.Sqrt(pokemonBaseInfo.BaseSpeed));
            float baseDefense = 2 * Mathf.Round(Mathf.Sqrt(pokemonBaseInfo.BaseDefense) * Mathf.Sqrt(pokemonBaseInfo.BaseSpDefense) + Mathf.Sqrt(pokemonBaseInfo.BaseSpeed));
            float totalCPMult = 0.084975f * Mathf.Sqrt(level);
            float stamina = (baseStamina + ((IV_HP + IV_Speed) * 0.5f)) * totalCPMult;
            float attack = (baseAttack + ((IV_Attack + IV_SpAttack) * 0.5f)) * totalCPMult;
            float defense = (baseDefense + ((IV_Defense + IV_SpDefense) * 0.5f)) * totalCPMult;
            return (int)Mathf.Max(10, (Mathf.Floor(Mathf.Sqrt(stamina) * attack * Mathf.Sqrt(defense))) / 10); }
    }
    public int MaxHP {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseHP * level) / 100f) + 64; }
    }
    public int Attack {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseAttack * level) / 100f) + 5; }
    }
    public int Defense {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseDefense * level) / 100f) + 5; }
    }
    public int SpecialAttack {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpAttack * level) / 100f) + 5; }
    }
    public int SpecialDefense {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpDefense * level) / 100f) + 5; }
    }
    public int Speed {
        get { return Mathf.FloorToInt((pokemonBaseInfo.BaseSpeed * level) / 100f) + 5; }
    }
}
