using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Pokemon Info", menuName = "Pokemon/Pokemon Info", order = 100)]
public class PokemonObject : ScriptableObject {
    [SerializeField] string pokemonName;
    [SerializeField] Sprite pokemonBattleSprite;
    [SerializeField] Sprite shinyPokemonBattleSprite;
    [SerializeField] Sprite[] alternateFormSprites;
    [SerializeField] Sprite[] shinyAlternateFormSprites;

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

    [SerializeField] int spriteSize;

    public string PokemonName {
        get { return pokemonName; }
    }
    public Sprite PokemonBattleSprite {
        get { return pokemonBattleSprite; }
    }
    public Sprite ShinyPokemonBattleSprite {
        get { return shinyPokemonBattleSprite; }
    }
    public Sprite[] AlternateFormSprites {
        get { return alternateFormSprites; }
    }
    public Sprite[] ShinyAlternateFormSprites {  
        get { return shinyAlternateFormSprites; }
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
    public int SpriteSize {
        get { return spriteSize; }
    }
}