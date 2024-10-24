using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerPokemonController : PokemonController {
    public PlayerInputController playerInput;
    public bool autoFastAttack;
    private void Awake() {
        currentPokemon = pokemonInParty[0];
        battleManager.playerPokemonIndividual = currentPokemon;
        playerInput = new PlayerInputController();

        playerInput.Enable();
        playerInput.Battle.FastAttack.performed += FastAttack;
        playerInput.Battle.ChargedMove1.performed += ChargedAttack1;
        playerInput.Battle.PressFastAttack.performed += PressFastAttack;
        playerInput.Battle.ReleaseFastAttack.performed += ReleaseFastAttack;
        playerInput.Battle.Switch1.performed += Switch1;
        playerInput.Battle.Switch2.performed += Switch2;
        playerInput.Battle.Switch3.performed += Switch3;
        playerInput.Battle.Switch4.performed += Switch4;
        playerInput.Battle.Switch5.performed += Switch5;
        playerInput.Battle.Switch6.performed += Switch6;
        if (currentPokemon.chargedMove2 != null) playerInput.Battle.ChargedMove2.performed += ChargedAttack2;
    }

    private void Switch1(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[0].currentHP > 0) battleManager.SwitchPokemon(true, 0);
    }
    private void Switch2(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[1].currentHP > 0) battleManager.SwitchPokemon(true, 1);
    }
    private void Switch3(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[2].currentHP > 0) battleManager.SwitchPokemon(true, 2);
    }
    private void Switch4(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[3].currentHP > 0) battleManager.SwitchPokemon(true, 3);
    }
    private void Switch5(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[4].currentHP > 0) battleManager.SwitchPokemon(true, 4);
    }
    private void Switch6(InputAction.CallbackContext context) {
        if (switchTimer > 0 && currentPokemon.currentHP > 0) return;
        if (!throwingChargedMove && pokemonInParty[5].currentHP > 0) battleManager.SwitchPokemon(true, 5);
    }
    private void PressFastAttack(InputAction.CallbackContext context) {
        autoFastAttack = true;
    }
    private void ReleaseFastAttack(InputAction.CallbackContext context) {
        autoFastAttack = false;
    }
    private void FastAttack(InputAction.CallbackContext context) {
        if (battleManager.playerSelectingPokemon || battleManager.aiTrainerSelectingPokemon) return;
        if (throwingChargedMove) { return; }
        battleManager.StartFastAttack(this, currentPokemon.fastMove, true);
    }
    private void ChargedAttack1(InputAction.CallbackContext context) {
        ChargedAttack(currentPokemon.chargedMove1);
    }
    private void ChargedAttack2(InputAction.CallbackContext context) {
        if (currentPokemon.chargedMove2) ChargedAttack(currentPokemon.chargedMove2);
    }
    private void ChargedAttack(ChargedMove chargedMove) {
        if (battleManager.playerSelectingPokemon || battleManager.aiTrainerSelectingPokemon) return;
        if (throwingChargedMove) return;
        if (currentPokemon.currentEnergy < chargedMove.baseEnergyReq) return;
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

    }
}