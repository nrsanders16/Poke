using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PokemonController : MonoBehaviour {
    public BattleManager battleManager;
    public PokemonIndividual currentPokemon;

    public bool throwingChargedMove;
    public ChargedMove queuedChargedMove;
    public bool switching;

    public PokemonIndividual[] pokemonInParty;
    public float switchTimer;
    [Header("Battle HUD")]
    public Image pokemonBattleImage;
    public Image type1Icon;
    public Image type2Icon;
    public Image statusIcon;
    [HideInInspector] public RectTransform pokemonImageRt;
    [HideInInspector] public RectTransform shadowImageRt;
    public TMP_Text effectivenessText;
    public TMP_Text[] buffTexts;
    public Animation pokeballSwitchAnimation;

    public virtual void PostChargedMoves() { }
    public virtual void PostSwitch() { }
    public virtual IEnumerator PokemonSelectTimer(float timer) { 
        yield return new WaitForEndOfFrame();
    }
}
