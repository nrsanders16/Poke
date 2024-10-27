using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using Unity.Burst.CompilerServices;
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

    public WeatherType currentWeather;
    public TerrainType currentTerrain;

    public Type[] types;
    public PokemonObject[] aegislashForms;

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
    private void OnDisable() {
        foreach (PokemonIndividual poke in playerPokemonController.pokemonInParty) {
            if(poke.chargedMove1.moveName == "Aura Wheel") {
                poke.chargedMove1.moveType = types[0];
            }
            if (poke.chargedMove2 != null && poke.chargedMove2.moveName == "Aura Wheel") {
                poke.chargedMove2.moveType = types[0];
            }
        }
    }
    #region Fast Attack
    public void StartFastAttack(PokemonController attackingPokemonController, FastMove fastMove, bool playerPokemon) {
        if (playerPokemonUsingChargedMove || aiTrainerPokemonUsingChargedMove || (playerPokemonUsingChargedMove && aiTrainerPokemonUsingChargedMove)) return;

        if (attackingPokemonController.currentPokemon.currentHP <= 0) return;

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

        ApplyAttackDamage(attackingPokemonController, defendingPokemonController, fastMove);

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
            aiTrainerPokemonUsingChargedMove = true;
            if (!aiTrainerPokemonUsingFastMove) ChargedAttackSetup();
        }
    }
    public void ChargedAttackSetup() {
        //disable attack inputs
        //change camera
        playerPokemonUsingChargedMove = playerPokemonController.throwingChargedMove;
        aiTrainerPokemonUsingChargedMove = aiTrainerPokemonController.throwingChargedMove;
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
        //This is where trainer would decide to use shield 
        HUDManager.chargedMoveText.enabled = true;
        HUDManager.chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " is unleashing energy!";

        yield return new WaitForSeconds(5);

        HUDManager.chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " used " + attackingPokemonController.queuedChargedMove.moveName + "!";

        yield return new WaitForSeconds(2.5f);
        HUDManager.chargedMoveText.enabled = false;

        //play attack animations and apply damage
        attackingPokemonController.currentPokemon.currentEnergy -= attackingPokemonController.queuedChargedMove.baseEnergyReq;

        ApplyAttackDamage(attackingPokemonController, defendingPokemonController, attackingPokemonController.queuedChargedMove);
        ApplyAttackEffects(attackingPokemonController, defendingPokemonController, attackingPokemonController.queuedChargedMove);

        attackingPokemonController.throwingChargedMove = false;

        if (playerPokemon) {
            playerPokemonUsingChargedMove = false;
        } else {
            aiTrainerPokemonUsingChargedMove = false;
        }

        if (attackingPokemonController.queuedChargedMove.moveName == "Aura Wheel") {
            if (attackingPokemonController.currentPokemon.chargedMove1.moveType == types[0]) {
                attackingPokemonController.currentPokemon.chargedMove1.moveType = types[1];
                attackingPokemonController.currentPokemon.pokemonBattleSprite = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonBattleSprites[1];
            } else {
                attackingPokemonController.currentPokemon.chargedMove1.moveType = types[0];
                attackingPokemonController.currentPokemon.pokemonBattleSprite = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonBattleSprites[0];
            }
            //HUDManager.playerPokemonSprite.sprite = attackingPokemonController.currentPokemon.pokemonBattleSprite; // only works for player
            HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);

        } else if (attackingPokemonController.queuedChargedMove.moveName == "King's Shield") {
            if (attackingPokemonController.currentPokemon.pokemonBaseInfo == aegislashForms[0]) {
                attackingPokemonController.currentPokemon.pokemonBaseInfo = aegislashForms[1];
                attackingPokemonController.currentPokemon.pokemonBattleSprite = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonBattleSprites[0];
            } else {
                attackingPokemonController.currentPokemon.pokemonBaseInfo = aegislashForms[0];
                attackingPokemonController.currentPokemon.pokemonBattleSprite = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonBattleSprites[0];
            }
            //HUDManager.playerPokemonSprite.sprite = attackingPokemonController.currentPokemon.pokemonBattleSprite; // only works for player
            HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
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
    void ApplyAttackDamage(PokemonController attackingPokemonController, PokemonController defendingPokemonController, PokemonMove pokemonMove) {
        Vector2 calc = BattleCalculations.CalculateAttackDamage(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, pokemonMove);
        float damage = calc.x;

        damage *= BattleCalculations.CalculateWeatherMultiplier(pokemonMove, currentWeather);
        damage *= BattleCalculations.CalculateTerrainMultiplier(pokemonMove, currentTerrain);

        if(!defendingPokemonController.effectivenessText.enabled && damage > 0) StartCoroutine(HUDManager.EffectivenessTextTimer(defendingPokemonController.effectivenessText, ConvertMultiplierToEffectivenessString(calc.y)));
        defendingPokemonController.currentPokemon.currentHP -= Mathf.RoundToInt(damage);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    void ApplyAttackEffects(PokemonController attackingPokemonController, PokemonController defendingPokemonController, ChargedMove chargedMove) {
        float rdmOwn = Random.Range(0, 100);
        if (rdmOwn <= chargedMove.ownBuffChance) {
            string buffString = "";
            for (int i = 0; i < chargedMove.ownBuffs.Length; i++) {

                if (chargedMove.ownBuffs[i] != 0) {
                    attackingPokemonController.currentPokemon.currentBuffs[i] += chargedMove.ownBuffs[i];

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

                    if (attackingPokemonController.currentPokemon.currentBuffs[i] > 4) {
                        attackingPokemonController.currentPokemon.currentBuffs[i] = 4;

                    } else if (attackingPokemonController.currentPokemon.currentBuffs[i] < -4) {
                        attackingPokemonController.currentPokemon.currentBuffs[i] = -4;
                    } else {
                        StartCoroutine(HUDManager.BuffTextTimer(attackingPokemonController.buffTexts[0], buffString));
                    }
                }
            }

        }

        float rdmOpp = Random.Range(0, 100);
        if (rdmOpp <= chargedMove.oppBuffChance) {
            string buffString = "";
            for (int i = 0; i < chargedMove.oppBuffs.Length; i++) {

                if (chargedMove.oppBuffs[i] != 0) {

                    defendingPokemonController.currentPokemon.currentBuffs[i] += chargedMove.oppBuffs[i];

                    if (i == 0) {
                        buffString += "Attack ";
                    } else if (i == 1) {
                        buffString += "Defense ";
                    } else if (i == 2) {
                        buffString += "Special Attack ";
                    } else if (i == 3) {
                        buffString += "Special Defense ";
                    }

                    float buffAmount = chargedMove.oppBuffs[i];

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

                    if (defendingPokemonController.currentPokemon.currentBuffs[i] > 4) {
                        defendingPokemonController.currentPokemon.currentBuffs[i] = 4;

                    } else if (defendingPokemonController.currentPokemon.currentBuffs[i] < -4) {
                        defendingPokemonController.currentPokemon.currentBuffs[i] = -4;
                    } else {
                        StartCoroutine(HUDManager.BuffTextTimer(defendingPokemonController.buffTexts[0], buffString));
                    }
                }
            }
        }

        if (chargedMove.weatherEffect != WeatherType.None) {
            currentWeather = chargedMove.weatherEffect;
            HUDManager.UpdateWeatherIcon(currentWeather);
            StartCoroutine(WeatherTimer(switchTimerLength));
        }

        if (chargedMove.terrainEffect != TerrainType.None) {
            currentTerrain = chargedMove.terrainEffect;
            HUDManager.UpdateTerrainIcon(currentTerrain);
            StartCoroutine(TerrainTimer(switchTimerLength));
        }
    }
    IEnumerator WeatherTimer(float timerLength) {
        yield return new WaitForSeconds(0.1f);
        timerLength -= 0.1f;
        if (timerLength <= 0) {
            currentWeather = WeatherType.None;
        } else {
            StartCoroutine(WeatherTimer(timerLength));
        }
    }
    IEnumerator TerrainTimer(float timerLength) {
        yield return new WaitForSeconds(0.1f);
        timerLength -= 0.1f;
        if (timerLength <= 0) {
            currentTerrain = TerrainType.None;
        } else {
            StartCoroutine(TerrainTimer(timerLength));
        }
    }
    string ConvertMultiplierToEffectivenessString(float effectivenessMultiplier) {
        if (effectivenessMultiplier > 1) {
            return "Super Effective!";
        } else if (effectivenessMultiplier < 1) {
            return "Not very effective...";
        } else {
            return "";
        }
    }
    #endregion
    #region Switching Pokemon
    public void SwitchPokemon(bool playerPokemon, bool pokemonFainted, int newPokemonIndex) {
        if (playerPokemon) { playerSelectingPokemon = false; } else { aiTrainerSelectingPokemon = false; }
        if (playerPokemon) { 
            playerPokemonController.pokeballSwitchAnimation.Play(); 
        } else { 
            aiTrainerPokemonController.pokeballSwitchAnimation.Play(); 
        }
        StartCoroutine(PokemonSwitch(playerPokemon, pokemonFainted, newPokemonIndex));
    }
    IEnumerator PokemonSwitch(bool playerPokemon, bool pokemonFainted, int newPokemonIndex) {
        if (playerPokemon) {
            HUDManager.playerSwitchTimerImage.enabled = false;
            playerPokemonController.queuedChargedMove = null;
            HUDManager.playerPokemonSprite.sprite = null;
            HUDManager.playerPokemonSprite.enabled = false;
        } else {
            HUDManager.aiTrainerSwitchTimerImage.enabled = false;
            aiTrainerPokemonController.queuedChargedMove = null;
            HUDManager.aiTrainerPokemonSprite.sprite = null;
            HUDManager.aiTrainerPokemonSprite.enabled = false;
        }

        if (!pokemonFainted) {
            if (playerPokemon) {
                if (playerPokemonController.currentPokemon.currentHP >= 0) playerPokemonController.switchTimer = switchTimerLength;
                StartCoroutine(SwitchCountdownTimer(playerPokemon, playerPokemonController));
            } else {
                if (aiTrainerPokemonController.currentPokemon.currentHP >= 0) aiTrainerPokemonController.switchTimer = switchTimerLength;
                StartCoroutine(SwitchCountdownTimer(playerPokemon, aiTrainerPokemonController));
            }
        }

        yield return new WaitForSeconds(0.5f);

        SendOutPokemon(playerPokemon);

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

        // check for switch-in abilities
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
    public void SendOutPokemon(bool playerPokemon) {
        if (playerPokemon) {
            StopCoroutine(playerPokemonController.PokemonSelectTimer(0));
        } else {
            StopCoroutine(aiTrainerPokemonController.PokemonSelectTimer(0));
        }
    }
    void StartPokemonSelectTimer(bool pokemonFainted, bool playerPokemon) {  // timer for selecting pokemon after pokemon faints
        int timer = 5;
        if (pokemonFainted) timer = 10;
        playerSelectingPokemon = !playerPokemon;
        aiTrainerSelectingPokemon = playerPokemon;
        aiTrainerPokemonUsingFastMove = false;
        playerPokemonUsingFastMove = false;
        if(!playerPokemon) {
            HUDManager.playerPokemonSprite.enabled = false;
            HUDManager.playerPokemonShadowSprite.enabled = false;
            HUDManager.playerSwitchTimerImage.enabled = true;
            StartCoroutine(playerPokemonController.PokemonSelectTimer(timer));
        } else {
            HUDManager.aiTrainerPokemonSprite.enabled = false;
            HUDManager.aiTrainerPokemonShadowSprite.enabled = false;
            HUDManager.aiTrainerSwitchTimerImage.enabled = true;
            StartCoroutine(aiTrainerPokemonController.PokemonSelectTimer(timer));
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