using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonController : MonoBehaviour
{
    public BattleManager battleManager;
    public PokemonIndividual currentPokemon;

    public bool throwingChargedMove;
    public ChargedMove queuedChargedMove;

    public PokemonIndividual[] pokemonInParty;

    public virtual void PostChargedMoves() { }

    public virtual void PostSwitch() { }
}
