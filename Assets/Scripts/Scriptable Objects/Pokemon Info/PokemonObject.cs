using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Pokemon Info", menuName = "Pokemon/Pokemon Info", order = 100)]
public class PokemonObject : ScriptableObject{
    [SerializeField] string pokemonName;
    [SerializeField] Sprite[] pokemonBattleSprites;
    [SerializeField] Sprite shinyPokemonBattleSprite;

    [SerializeField] Type primaryType;
    [SerializeField] Type secondaryType;

    [SerializeField] int baseHP;
    [SerializeField] int baseAttack;
    [SerializeField] int baseDefense;
    [SerializeField] int baseSpAttack;
    [SerializeField] int baseSpDefense;
    [SerializeField] int baseSpeed;

    [SerializeField] FastMove[] learnableFastMoves;

    [SerializeField] ChargedMove[] learnableChargedMoves;
    [SerializeField] Ability[] possibleAbilities;

    public string PokemonName {
        get { return pokemonName; }
    }
    public Sprite PokemonBattleSpriteMain {
        get { return pokemonBattleSprites[0]; }
    }
    public Sprite[] PokemonBattleSprites {
        get { return pokemonBattleSprites; }
    }
    public Sprite ShinyPokemonBattleSprite {  
        get { return shinyPokemonBattleSprite; }
    }
    public Type PrimaryType {
        get { return primaryType; }
    }
    public Type SecondaryType {
        get { return secondaryType; }
    }
    public int BaseHP {
        get { return baseHP; }
    }
    public int BaseAttack {
        get { return baseAttack; }
    }
    public int BaseSpAttack {
        get { return baseSpAttack; }
    }
    public int BaseDefense {
        get { return baseDefense; }
    }
    public int BaseSpDefense {
        get { return baseSpDefense; }
    }
    public int BaseSpeed {
        get { return baseSpeed; }
    }
    public FastMove[] LearnableFastMoves {
        get { return learnableFastMoves; }
    }
    public ChargedMove[] LearnableChargedMoves {
        get { return learnableChargedMoves; }
    }
    public Ability[] PossibleAbilities {
        get { return possibleAbilities; }
    }
}
