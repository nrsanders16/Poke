using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerPokemonController : PokemonController {
    public PlayerInputController playerInput;
    public bool autoFastAttack;
    private void Awake() {
        pokemonImageRt = pokemonBattleImage.GetComponent<RectTransform>();
        shadowImageRt = pokemonBattleImage.gameObject.transform.GetChild(0).GetComponent<RectTransform>();
        currentPokemon = pokemonInParty[0];
        battleManager.playerPokemonIndividual = currentPokemon;
        playerInput = new PlayerInputController();

        playerInput.Enable();
        playerInput.Battle.FastAttack.performed += FastAttack;
        playerInput.Battle.ChargedMove1.performed += ChargedAttack1;
        playerInput.Battle.ChargedMove2.performed += ChargedAttack2;
        playerInput.Battle.PressFastAttack.performed += PressFastAttack;
        playerInput.Battle.ReleaseFastAttack.performed += ReleaseFastAttack;
        playerInput.Battle.Switch1.performed += Switch1;
        playerInput.Battle.Switch2.performed += Switch2;
        playerInput.Battle.Switch3.performed += Switch3;
        playerInput.Battle.Switch4.performed += Switch4;
        playerInput.Battle.Switch5.performed += Switch5;
        playerInput.Battle.Switch6.performed += Switch6;
    }
    private void Switch1(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[0].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[0].currentHP > 0) battleManager.SwitchPokemon(this, false, 0); switching = true;
    }
    private void Switch2(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[1].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[1].currentHP > 0) battleManager.SwitchPokemon(this, false, 1); switching = true;
    }
    private void Switch3(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[2].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[2].currentHP > 0) battleManager.SwitchPokemon(this, false, 2); switching = true;
    }
    private void Switch4(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[3].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[3].currentHP > 0) battleManager.SwitchPokemon(this, false, 3); switching = true;
    }
    private void Switch5(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[4].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[4].currentHP > 0) battleManager.SwitchPokemon(this, false, 4); switching = true;
    }
    private void Switch6(InputAction.CallbackContext context) {
        if (switching) return;
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (pokemonInParty[5].currentHP <= 0) return;
        if (!throwingChargedMove && pokemonInParty[5].currentHP > 0) battleManager.SwitchPokemon(this, false, 5); switching = true;
    }
    private void PressFastAttack(InputAction.CallbackContext context) {
        autoFastAttack = true;
        FastAttack(context);
    }
    private void ReleaseFastAttack(InputAction.CallbackContext context) {
        autoFastAttack = false;
    }
    private void FastAttack(InputAction.CallbackContext context) {
        if (battleManager.playerSelectingPokemon || battleManager.aiTrainerSelectingPokemon) return;
        if (throwingChargedMove) { return; }
        if (currentPokemon.currentHP > 0) battleManager.StartFastAttack(this, currentPokemon.fastMove, true);
    }
    private void ChargedAttack1(InputAction.CallbackContext context) {
        if(currentPokemon.currentHP > 0) ChargedAttack(currentPokemon.chargedMove1);
    }
    private void ChargedAttack2(InputAction.CallbackContext context) {
        if (currentPokemon.currentHP > 0 && currentPokemon.chargedMove2 != null) ChargedAttack(currentPokemon.chargedMove2);
    }
    private void ChargedAttack(ChargedMove chargedMove) {
        if (battleManager.playerSelectingPokemon || battleManager.aiTrainerSelectingPokemon) return;
        if (throwingChargedMove) return;
        if (currentPokemon.currentEnergy < chargedMove.baseEnergyReq) return;
        print("pkmn ctrl Charged Attack");
        throwingChargedMove = true;
        queuedChargedMove = chargedMove;
        if (battleManager.aiTrainerPokemonUsingChargedMove) return;
        battleManager.ThrowChargedMove(true);
    }
    public override void PostChargedMoves() {
        //evaluate AI options
        throwingChargedMove = false;
        queuedChargedMove = null;
    }
    public override void PostSwitch() {
        pokemonBattleImage.enabled = true;
        switching = false;
    }
    public override IEnumerator PokemonSelectTimer(float timer) {

        yield return new WaitForSeconds(0.1f);
        timer -= 0.1f;

        if (battleManager.playerSelectingPokemon) {
            if (timer <= 0) {
                int nextHealthyPokemon = 0;

                for (int i = 0; i < pokemonInParty.Length; i++) {
                    if (pokemonInParty[i].currentHP > 0) {
                        battleManager.SwitchPokemon(this, true, nextHealthyPokemon);
                        break;
                    } else {
                        nextHealthyPokemon++;
                    }
                }
            } else {
                battleManager.HUDManager.playerSwitchTimerImage.fillAmount = timer / 10f;
                StartCoroutine(PokemonSelectTimer(timer));
            }
        }
    }
}