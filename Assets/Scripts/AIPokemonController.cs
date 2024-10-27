using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class AIPokemonController : PokemonController {
    public bool shouldThrowChargedMoves;

    [SerializeField] float currentMatchupQuality;
    [SerializeField] float[] partyMatchupQuality;

    int currentPokemonIndex = 0;

    private void Awake() {
        currentPokemon = pokemonInParty[0];
        currentPokemonIndex = 0;
        battleManager.aiTrainerPokemonIndividual = currentPokemon;
    }
    void Start() {
        StartCoroutine(BattleAI());
    }
    IEnumerator BattleAI() {

        //Analyze effectiveness of each move against opponent
        if (currentPokemon && battleManager.playerPokemonIndividual) AssessPartyMatchupQuality();

        yield return new WaitForEndOfFrame();

        if (!battleManager.playerSelectingPokemon && !battleManager.aiTrainerSelectingPokemon) {

            if (currentMatchupQuality > 0) {

                if (currentPokemon.currentEnergy >= currentPokemon.chargedMove1.baseEnergyReq && shouldThrowChargedMoves && !battleManager.aiTrainerPokemonUsingChargedMove) {
                    throwingChargedMove = true;
                    queuedChargedMove = currentPokemon.chargedMove1;
                    battleManager.ThrowChargedMove(false);
                    //print("Battle AI Throw charged move");

                } else {
                    if (!battleManager.playerSelectingPokemon && !battleManager.aiTrainerPokemonUsingFastMove && !(currentPokemon.currentHP <= 0 || battleManager.playerPokemonController.currentPokemon.currentHP <= 0)) {
                        battleManager.StartFastAttack(this, currentPokemon.fastMove, false);
                    }
                }

            } else {

                var l = new List<float>();
                for (int i = 0; i < pokemonInParty.Length; i++) {
                    if (pokemonInParty[i] != currentPokemon) l.Add(pokemonInParty[i].currentHP);
                }
                var t = Mathf.Max(l.ToArray());

                if (t <= 0) { //if all other pokemon are fainted

                    if (currentPokemon.currentEnergy >= currentPokemon.chargedMove1.baseEnergyReq && shouldThrowChargedMoves && !battleManager.aiTrainerPokemonUsingChargedMove) {
                        throwingChargedMove = true;
                        queuedChargedMove = currentPokemon.chargedMove1;
                        battleManager.ThrowChargedMove(false);
                        //print("Battle AI Throw charged move");

                    } else {
                        if (!battleManager.playerSelectingPokemon && !battleManager.aiTrainerPokemonUsingFastMove && !(currentPokemon.currentHP <= 0 || battleManager.playerPokemonController.currentPokemon.currentHP <= 0)) {
                            battleManager.StartFastAttack(this, currentPokemon.fastMove, false);
                        }
                    }

                } else {

                    if (switchTimer <= 0) {
                        SwitchToBestMatchup();
                    } else {
                        if (currentPokemon.currentEnergy >= currentPokemon.chargedMove1.baseEnergyReq && shouldThrowChargedMoves && !battleManager.aiTrainerPokemonUsingChargedMove) {
                            throwingChargedMove = true;
                            queuedChargedMove = currentPokemon.chargedMove1;
                            battleManager.ThrowChargedMove(false);
                            //print("Battle AI Throw charged move");

                        } else {
                            if (!battleManager.playerSelectingPokemon && !battleManager.aiTrainerPokemonUsingFastMove && !(currentPokemon.currentHP <= 0 || battleManager.playerPokemonController.currentPokemon.currentHP <= 0)) {
                                battleManager.StartFastAttack(this, currentPokemon.fastMove, false);
                            }
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.25f);

        StartCoroutine(BattleAI());

    }
    public override void PostChargedMoves() {
        //evaluate AI options
        battleManager.StartFastAttack(this, currentPokemon.fastMove, false);
    }
    public override void PostSwitch() {
        //evaluate AI options
        if (currentPokemon && battleManager.playerPokemonIndividual) AssessPartyMatchupQuality();
        battleManager.StartFastAttack(this, currentPokemon.fastMove, false);
    }
    void AssessPartyMatchupQuality() {
        partyMatchupQuality = new float[pokemonInParty.Length];
        for (int i = 0; i < pokemonInParty.Length; i++) {
            if(pokemonInParty[i] != null) partyMatchupQuality[i] = AssessMatchupQuality(pokemonInParty[i]);
        }
        currentMatchupQuality = partyMatchupQuality[currentPokemonIndex];
    }
    float AssessMatchupQuality(PokemonIndividual pokemonToAssess){
        float matchupQuality = 0;
        //Check to see effectiveness of moves against opponent
        if(pokemonToAssess.currentHP <= 0) {
            matchupQuality -= 1000;
        }
        //Opponent Weaknesses
        if (battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.weaknesses.Contains(pokemonToAssess.fastMove.moveType.typeName)) {
            matchupQuality += 0.5f;
        }
        if (battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.weaknesses.Contains(pokemonToAssess.chargedMove1.moveType.typeName)) {
            matchupQuality += 0.5f;
        }
        if (pokemonToAssess.chargedMove2 != null && battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.weaknesses.Contains(pokemonToAssess.chargedMove2.moveType.typeName)) {
            matchupQuality  += 0.5f;
        }
        //Opponent Resistances
        if (battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.resistances.Contains(pokemonToAssess.fastMove.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }
        if (battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.resistances.Contains(pokemonToAssess.chargedMove1.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }
        if (pokemonToAssess.chargedMove2 != null && battleManager.playerPokemonIndividual.pokemonBaseInfo.PrimaryType.resistances.Contains(pokemonToAssess.chargedMove2.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }

        if (battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType != null) {
            //Opponent Weaknesses
            if (battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.weaknesses.Contains(pokemonToAssess.fastMove.moveType.typeName))  {
                matchupQuality += 0.5f;
            }
            if (battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.weaknesses.Contains(pokemonToAssess.chargedMove1.moveType.typeName)) {
                matchupQuality += 0.5f;
            }
            if (pokemonToAssess.chargedMove2 != null && battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.weaknesses.Contains(pokemonToAssess.chargedMove2.moveType.typeName)) {
                matchupQuality += 0.5f;
            }
            //Opponent Resistances
            if (battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.resistances.Contains(pokemonToAssess.fastMove.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
            if (battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.resistances.Contains(pokemonToAssess.chargedMove1.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
            if (pokemonToAssess.chargedMove2 != null && battleManager.playerPokemonIndividual.pokemonBaseInfo.SecondaryType.resistances.Contains(pokemonToAssess.chargedMove2.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
        }

        //Check to see effectiveness of opponent's moves
        //Resistances
        if (pokemonToAssess.pokemonBaseInfo.PrimaryType.resistances.Contains(battleManager.playerPokemonIndividual.fastMove.moveType.typeName)){
            matchupQuality += 0.5f;
        }
        if (pokemonToAssess.pokemonBaseInfo.PrimaryType.resistances.Contains(battleManager.playerPokemonIndividual.chargedMove1.moveType.typeName)) {
            matchupQuality += 0.5f;
        }
        if (battleManager.playerPokemonIndividual.chargedMove2 != null && pokemonToAssess.pokemonBaseInfo.PrimaryType.resistances.Contains(battleManager.playerPokemonIndividual.chargedMove2.moveType.typeName)) {
            matchupQuality += 0.5f;
        }
        //Weaknesses
        if (pokemonToAssess.pokemonBaseInfo.PrimaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.fastMove.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }
        if (pokemonToAssess.pokemonBaseInfo.PrimaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.chargedMove1.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }
        if (battleManager.playerPokemonIndividual.chargedMove2 != null && pokemonToAssess.pokemonBaseInfo.PrimaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.chargedMove2.moveType.typeName)) {
            matchupQuality -= 0.5f;
        }

        if (pokemonToAssess.pokemonBaseInfo.SecondaryType != null) {
            //Resistances
            if (pokemonToAssess.pokemonBaseInfo.SecondaryType.resistances.Contains(battleManager.playerPokemonIndividual.fastMove.moveType.typeName)) {
                matchupQuality += 0.5f;
            }
            if (pokemonToAssess.pokemonBaseInfo.SecondaryType.resistances.Contains(battleManager.playerPokemonIndividual.chargedMove1.moveType.typeName)) {
                matchupQuality += 0.5f;
            }
            if (battleManager.playerPokemonIndividual.chargedMove2 != null && pokemonToAssess.pokemonBaseInfo.SecondaryType.resistances.Contains(battleManager.playerPokemonIndividual.chargedMove2.moveType.typeName)) {
                matchupQuality += 0.5f;
            }
            //Weaknesses
            if (pokemonToAssess.pokemonBaseInfo.SecondaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.fastMove.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
            if (pokemonToAssess.pokemonBaseInfo.SecondaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.chargedMove1.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
            if (battleManager.playerPokemonIndividual.chargedMove2 != null && pokemonToAssess.pokemonBaseInfo.SecondaryType.weaknesses.Contains(battleManager.playerPokemonIndividual.chargedMove2.moveType.typeName)) {
                matchupQuality -= 0.5f;
            }
        }

        return matchupQuality;
    }
    public void SwitchToBestMatchup() {
        var l = new List<float>();
        for (int i = 0; i < pokemonInParty.Length; i++) {
            l.Add(pokemonInParty[i].currentHP);
        }
        var t = Mathf.Max(l.ToArray());

        if (t <= 0) {

            print("You win!");

        } else {
            var list = partyMatchupQuality.ToList();
            var ind = Mathf.Max(list.ToArray());
            int newPokemonIndex = list.IndexOf(ind);
            //int newPokemonIndex = List<float>.IndexOf(ind); //Wrong, sort list and choose index 0 of sorted list
            //check if pokemon is fainted
            //if so, newPokemonIndex++ and check again
            battleManager.SwitchPokemon(false, currentPokemon.currentHP <= 0, newPokemonIndex);
            //battleManager.SwitchPokemon(false, 1);
        }

    }
    public override IEnumerator PokemonSelectTimer(float timer) {

        yield return new WaitForSeconds(0.1f);
        battleManager.HUDManager.aiTrainerPokemonSprite.enabled = false;
        battleManager.HUDManager.aiTrainerSwitchTimerImage.fillAmount = timer / 10f;
        timer -= 0.1f;
        if(timer < 5) {
            SwitchToBestMatchup();
            //print("Switch to best matchup");
            StopCoroutine(PokemonSelectTimer(timer));
        } else {
            StartCoroutine(PokemonSelectTimer(timer));
        }
        /*
        if (currentPokemon.currentHP <= 0) {

            int nextHealthyPokemon = 0;

            for (int i = 0; i < pokemonInParty.Length; i++) {
                if (pokemonInParty[i].currentHP > 0) {
                    battleManager.SwitchPokemon(false, true, nextHealthyPokemon);
                    break;
                } else {
                    nextHealthyPokemon++;
                }
            }
        }
        */
    }
}