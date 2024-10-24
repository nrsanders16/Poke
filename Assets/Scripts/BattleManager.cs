using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class BattleManager : MonoBehaviour {
    public HUDManager HUDManager;

    public PokemonIndividual playerPokemonIndividual;
    public PlayerPokemonController playerPokemonController;
    public Animation playerPokemonAnimation;

    public PokemonIndividual aiTrainerPokemonIndividual;
    public AIPokemonController aiTrainerPokemonController;
    public Animation aiTrainerPokemonAnimation;

    public bool playerPokemonUsingFastMove;
    public bool aiTrainerPokemonUsingFastMove;

    public bool playerPokemonUsingChargedMove;
    public bool aiTrainerPokemonUsingChargedMove;

    public bool playerSelectingPokemon;
    public bool aiTrainerSelectingPokemon;

    //[SerializeField] bool chargedMoveTie;
    public float switchTimerLength;

    private void Start() {
        playerPokemonController.currentPokemon.currentEnergy = 0;
        aiTrainerPokemonController.currentPokemon.currentEnergy = 0;
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);

        for (int i = 0; i < playerPokemonController.pokemonInParty.Length; i++) {
            if (playerPokemonController.pokemonInParty[i].currentHP > 0) {
                HUDManager.playerPartyPokemonTimerImages[i].fillAmount = playerPokemonController.switchTimer / switchTimerLength;
                playerPokemonController.pokemonInParty[i].currentHP = playerPokemonController.pokemonInParty[i].MaxHP;
                print(playerPokemonController.pokemonInParty[i].BattlePower);
            }
        }

        for (int i = 0; i < aiTrainerPokemonController.pokemonInParty.Length; i++) {
            if (aiTrainerPokemonController.pokemonInParty[i].currentHP > 0) {
                aiTrainerPokemonController.pokemonInParty[i].currentHP = aiTrainerPokemonController.pokemonInParty[i].MaxHP;
            }
        }
    }
    #region Fast Attack
    public void StartFastAttack(PokemonController attackingPokemonController, FastMove fastMove, bool playerPokemon) {
        if (playerPokemonUsingChargedMove || aiTrainerPokemonUsingChargedMove || (playerPokemonUsingChargedMove && aiTrainerPokemonUsingChargedMove)) return;

        if (playerPokemon) {
            if (playerPokemonUsingFastMove) return;
            playerPokemonUsingFastMove = true;
            StartCoroutine(FastAttack(playerPokemonController, aiTrainerPokemonController, fastMove, playerPokemon));

        } else {

            if (aiTrainerPokemonUsingFastMove) return;
            aiTrainerPokemonUsingFastMove = true;
            StartCoroutine(FastAttack(aiTrainerPokemonController, playerPokemonController, fastMove, playerPokemon));
        }

    }
    IEnumerator FastAttack(PokemonController attackingPokemonController, PokemonController defendingPokemonController, FastMove fastMove, bool playerPokemon) {
        yield return new WaitForEndOfFrame();

        if (playerPokemon) {
            playerPokemonAnimation.clip = fastMove.animationClips[0];
            playerPokemonAnimation.Play();
            //print("PlayClip");

        } else {
            aiTrainerPokemonAnimation.clip = fastMove.animationClips[1];
            aiTrainerPokemonAnimation.Play();
        }

        yield return new WaitForSeconds((0.5f * fastMove.cycles) * 0.2f);

        if (attackingPokemonController.currentPokemon.currentEnergy < 100) attackingPokemonController.currentPokemon.currentEnergy += fastMove.baseEnergy;
        if (attackingPokemonController.currentPokemon.currentEnergy > 100) attackingPokemonController.currentPokemon.currentEnergy = 100;

        ApplyAttackDamage(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, fastMove);

        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);

        //if (playerPokemon) playerPokemonUsingFastMove = false;

        yield return new WaitForSeconds((0.5f * fastMove.cycles) * 0.8f);

        if (defendingPokemonController.currentPokemon.currentHP <= 0) {

            StartPokemonSelectTimer(true, playerPokemon);

        } else {
            if (playerPokemon) {
                playerPokemonUsingFastMove = false;
                if (attackingPokemonController.throwingChargedMove) {
                    playerPokemonUsingChargedMove = true;
                    ThrowChargedMove(playerPokemon);

                } else {

                    if (attackingPokemonController.GetComponent<PlayerPokemonController>().autoFastAttack) {
                        StartFastAttack(attackingPokemonController, fastMove, true);
                    }
                }

            } else {

                aiTrainerPokemonUsingFastMove = false;
                if (attackingPokemonController.throwingChargedMove) {
                    aiTrainerPokemonUsingChargedMove = true;
                    ThrowChargedMove(playerPokemon);
                } else {
                    //Switch Check
                    //StartFastAttack(attackingPokemonController, fastMove, false);
                    //print("Battle Manager start fast move");
                }

            }
        }

    }
    #endregion
    #region Charged Attack
    public void ThrowChargedMove(bool playerPokemon) {
        //if (playerPokemonUsingChargedMove || aiTrainerPokemonUsingChargedMove) return;
        if (playerPokemon) {
            playerPokemonUsingChargedMove = true;
            if (!playerPokemonUsingFastMove) ChargedAttackSetup();

        } else {

            //if (!aiTrainerPokemonController.shouldThrowChargedMoves) return;
            //print("Battle Manager Throw charged move");
            aiTrainerPokemonUsingChargedMove = true;
            if (!aiTrainerPokemonUsingFastMove) ChargedAttackSetup();

        }
    }
    public void ChargedAttackSetup() {
        //disable attack inputs
        //change camera
        playerPokemonUsingChargedMove = playerPokemonController.throwingChargedMove;
        aiTrainerPokemonUsingChargedMove = aiTrainerPokemonController.throwingChargedMove;
        //print("Battle Manager charged move setup");
        if (playerPokemonUsingChargedMove && aiTrainerPokemonUsingChargedMove) {
            print("Charged Move Tie");
            //chargedMoveTie = true;
            bool playerWinsCmp = BattleCalculations.CalculateChargedMovePriority(playerPokemonIndividual, aiTrainerPokemonIndividual);
            if (playerWinsCmp) {
                StartCoroutine(ChargedAttack(playerPokemonController, aiTrainerPokemonController, true));
            } else {
                StartCoroutine(ChargedAttack(aiTrainerPokemonController, playerPokemonController, false));
            }
        } else if (playerPokemonUsingChargedMove) {
            StartCoroutine(ChargedAttack(playerPokemonController, aiTrainerPokemonController, true));

        } else if (aiTrainerPokemonUsingChargedMove) {
            StartCoroutine(ChargedAttack(aiTrainerPokemonController, playerPokemonController, false));

        }
    }
    IEnumerator ChargedAttack(PokemonController attackingPokemonController, PokemonController defendingPokemonController, bool playerPokemon) {
        //print("Battle Manager charged move Coroutine");
        //This is where trainer would decide to use shield 
        HUDManager.chargedMoveText.enabled = true;
        HUDManager.chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " is unleashing energy!";

        yield return new WaitForSeconds(5);

        HUDManager.chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " used " + attackingPokemonController.queuedChargedMove.moveName + "!";

        yield return new WaitForSeconds(2.5f);
        HUDManager.chargedMoveText.enabled = false;

        //play attack animations and apply damage
        attackingPokemonController.currentPokemon.currentEnergy -= attackingPokemonController.queuedChargedMove.baseEnergyReq;

        ApplyAttackDamage(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, attackingPokemonController.queuedChargedMove);
        ApplyAttackEffects(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, attackingPokemonController.queuedChargedMove);


        attackingPokemonController.throwingChargedMove = false;

        if (playerPokemon) {
            playerPokemonUsingChargedMove = false;
        } else {
            aiTrainerPokemonUsingChargedMove = false;
        }

        if (defendingPokemonController.currentPokemon.currentHP <= 0) {
            // start coroutine for switch screen
            playerPokemonUsingFastMove = false;
            aiTrainerPokemonUsingFastMove = false;
            StartPokemonSelectTimer(true, playerPokemon);
            print(defendingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " fainted!");

        } else {
            if (defendingPokemonController.throwingChargedMove) {
                // queue charged move for defending pokemon
                ChargedAttackSetup();

            } else {
                // resume fast attacking
                defendingPokemonController.PostChargedMoves();
            }
        }
    }
    void ApplyAttackDamage(PokemonIndividual attackingPokemon, PokemonIndividual defendingPokemon, PokemonMove pokemonMove) {
        int damage = Mathf.RoundToInt(BattleCalculations.CalculateAttackDamage(attackingPokemon, defendingPokemon, pokemonMove));
        defendingPokemon.currentHP -= damage;
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    void ApplyAttackEffects(PokemonIndividual attackingPokemon, PokemonIndividual defendingPokemon, ChargedMove chargedMove) {
        float rdmOwn = Random.Range(0, 100);
        if (rdmOwn <= chargedMove.ownBuffChance) {
            string buffString = "";
            for (int i = 0; i < chargedMove.ownBuffs.Length; i++) {
                attackingPokemon.currentBuffs[i] += chargedMove.ownBuffs[i];

                if (i == 0) {
                    buffString += "Attack ";
                } else if (i == 1) {
                    buffString += "Defense ";
                } else if (i == 2) {
                    buffString += "Special Attack ";
                } else if (i == 3) {
                    buffString += "Special Defense ";
                }

                float buffAmount = chargedMove.ownBuffs[i];

                if (buffAmount > 0) {
                    buffString += "rose";

                } else if (buffAmount < 0) {
                    buffString += "fell";
                }

                if (Mathf.Abs(buffAmount) == 0.1f) {
                    buffString += " slightly!";
                } else if (Mathf.Abs(buffAmount) == 0.2f) {
                    buffString += "!";
                } else if (Mathf.Abs(buffAmount) == 0.4f) {
                    buffString += " sharply!";
                } else if (Mathf.Abs(buffAmount) == 0.6f) {
                    buffString += " drastically!";
                }

                if (attackingPokemon.currentBuffs[i] > 4) {
                    attackingPokemon.currentBuffs[i] = 4;

                } else if (attackingPokemon.currentBuffs[i] < -4) {
                    attackingPokemon.currentBuffs[i] = -4;
                } else {
                    print(buffString);
                }
            }

        }

        float rdmOpp = Random.Range(0, 100);
        if (rdmOpp <= chargedMove.oppBuffChance) {
            for (int i = 0; i < chargedMove.ownBuffs.Length; i++) {

                defendingPokemon.currentBuffs[i] += chargedMove.oppBuffs[i];

                if (defendingPokemon.currentBuffs[i] > 4) {
                    defendingPokemon.currentBuffs[i] = 4;

                } else if (defendingPokemon.currentBuffs[i] < -4) {
                    defendingPokemon.currentBuffs[i] = -4;
                }
            }
        }
    }

    #endregion
    #region Switching Pokemon
    public void SwitchPokemon(bool playerPokemon, int newPokemonIndex) {
        StopCoroutine(PokemonSelectTimer(0, playerPokemon));
        if(playerPokemon) { playerSelectingPokemon = false; } else { aiTrainerSelectingPokemon = false; }
        StartCoroutine(PokemonSwitch(playerPokemon, newPokemonIndex));
    }
    IEnumerator PokemonSwitch(bool playerPokemon, int newPokemonIndex) {
        if (playerPokemon) {
            playerPokemonController.queuedChargedMove = null;
            HUDManager.playerPokemonSprite.sprite = null;
            HUDManager.playerPokemonSprite.enabled = false;

        } else {
            aiTrainerPokemonController.queuedChargedMove = null;
            HUDManager.aiTrainerPokemonSprite.sprite = null;
            HUDManager.aiTrainerPokemonSprite.enabled = false;
        }

        if (playerPokemon) {
            if(playerPokemonController.currentPokemon.currentHP >= 0) playerPokemonController.switchTimer = switchTimerLength;
            StartCoroutine(SwitchCountdownTimer(playerPokemon, playerPokemonController));
        } else {
            if (aiTrainerPokemonController.currentPokemon.currentHP >= 0) aiTrainerPokemonController.switchTimer = switchTimerLength;
            StartCoroutine(SwitchCountdownTimer(playerPokemon, aiTrainerPokemonController));
        }

        yield return new WaitForSeconds(0.5f);

        if (playerPokemon) {
            playerPokemonController.currentPokemon = playerPokemonController.pokemonInParty[newPokemonIndex];
            playerPokemonIndividual = playerPokemonController.currentPokemon;
            HUDManager.playerPokemonSprite.sprite = playerPokemonIndividual.pokemonBattleSprite;
            HUDManager.playerPokemonSprite.enabled = true;
        } else {
            aiTrainerPokemonController.currentPokemon = aiTrainerPokemonController.pokemonInParty[newPokemonIndex];
            aiTrainerPokemonIndividual = aiTrainerPokemonController.currentPokemon;
            HUDManager.aiTrainerPokemonSprite.sprite = aiTrainerPokemonIndividual.pokemonBattleSprite;
            HUDManager.aiTrainerPokemonSprite.enabled = true;
        }

        yield return new WaitForEndOfFrame();

        playerPokemonController.PostSwitch();
        aiTrainerPokemonController.PostSwitch();
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public IEnumerator SwitchCountdownTimer(bool playerPokemon, PokemonController pokemonController) {
        yield return new WaitForSeconds(0.1f);
        pokemonController.switchTimer -= 0.1f;

        if (playerPokemon) {
            for (int i = 0; i < pokemonController.pokemonInParty.Length; i++) {
                if (pokemonController.pokemonInParty[i].currentHP > 0) {
                    HUDManager.playerPartyPokemonTimerImages[i].fillAmount = pokemonController.switchTimer / switchTimerLength;
                } else {
                    HUDManager.playerPartyPokemonTimerImages[i].fillAmount = 1;
                }
            }
        }

        if (pokemonController.switchTimer >= 0) StartCoroutine(SwitchCountdownTimer(playerPokemon, pokemonController));
    }
    public void SendOutPokemon() {

    }
    void StartPokemonSelectTimer(bool pokemonFainted, bool playerPokemon) {  // timer for selecting pokemon after pokemon faints
        int timer = 5;
        if (pokemonFainted) timer = 10;
        playerSelectingPokemon = !playerPokemon;
        aiTrainerSelectingPokemon = playerPokemon;
        aiTrainerPokemonUsingFastMove = false;
        playerPokemonUsingFastMove = false;
        StartCoroutine(PokemonSelectTimer(timer, !playerPokemon));
    }
    IEnumerator PokemonSelectTimer(int timerLength, bool playerPokemon) {

        yield return new WaitForSeconds(timerLength / 2);

        if(!playerPokemon) {
            aiTrainerPokemonController.SwitchToBestMatchup();
            print("Switch to best matchup");
        }

        StopCoroutine(PokemonSelectTimer(timerLength, playerPokemon));

        yield return new WaitForSeconds(timerLength / 2);

        if (playerPokemon) {

            if (playerPokemonController.currentPokemon == null) {

                int nextHealthyPokemon = 0;

                for(int i = 0; i < playerPokemonController.pokemonInParty.Length; i++) {
                    if (playerPokemonController.pokemonInParty[i].currentHP > 0) {
                        SwitchPokemon(playerPokemon, nextHealthyPokemon);
                        break;
                    } else {
                        nextHealthyPokemon++;
                    }
                }
            }
        } 
    }
    #endregion
    #region Fainting
    void OnPokemonFaint(bool playerPokemon) {
        if (playerPokemon) {
            //bring up switch countdown timer
        } else {
            //give exp to winning pokemon based on defeated pokemon
        }
    }
    #endregion
    #region Battle Beginning

    #endregion
    #region Battle End
    void OnFinalPokemonFaint(PokemonController winningPokemonController) {
        //register win
        //comments from ai trainer
        //exp for party pokemon
        //give rewards for winning
        //reset pokemon buffs/debuffs/status (can be done onDisable)
        //reset battle manager variables (can be done onDisable)
        //switch back to overworld scene
    }
    #endregion
}