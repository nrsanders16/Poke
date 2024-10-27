using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PokemonController : MonoBehaviour
{
    public BattleManager battleManager;
    public PokemonIndividual currentPokemon;

    public bool throwingChargedMove;
    public ChargedMove queuedChargedMove;

    public PokemonIndividual[] pokemonInParty;
    public float switchTimer;
    public float postFaintTimer;
    public TMP_Text effectivenessText;
    public TMP_Text[] buffTexts;
    public Animation pokeballSwitchAnimation;
    public bool switching;

    public virtual void PostChargedMoves() { }
    public virtual void PostSwitch() { }
    public virtual IEnumerator PokemonSelectTimer(float timer) { 
        yield return new WaitForEndOfFrame();
    }
}
