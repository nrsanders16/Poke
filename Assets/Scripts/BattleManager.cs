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
    public float weatherTimer;
    public TerrainType currentTerrain;
    public float terrainTimer;

    public Type[] types;
    public PokemonObject[] aegislashForms;
    public PokemonObject[] meloettaForms;
    public PokemonObject[] darmanitanForms;
    public PokemonObject[] wishiwashiForms;

    private void Start() {
        playerPokemonController.currentPokemon.currentEnergy = 0;
        aiTrainerPokemonController.currentPokemon.currentEnergy = 0;

        playerPokemonController.currentPokemonBattleSprite = playerPokemonController.currentPokemon.pokemonBattleSprite;
        aiTrainerPokemonController.currentPokemonBattleSprite = aiTrainerPokemonController.currentPokemon.pokemonBattleSprite;

        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);

        for (int i = 0; i < playerPokemonController.pokemonInParty.Length; i++) {
            if (playerPokemonController.pokemonInParty[i].currentHP > 0) {
                HUDManager.playerPartyPokemonTimerImages[i].fillAmount = playerPokemonController.switchTimer / switchTimerLength;
                playerPokemonController.pokemonInParty[i].currentHP = playerPokemonController.pokemonInParty[i].MaxHP;
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

            OnPokemonFaint(playerPokemon);

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

        PostChargedMoveFormChanges(attackingPokemonController);
        PostChargedMoveFormChanges(defendingPokemonController);

        if (defendingPokemonController.currentPokemon.currentHP <= 0) {
            // start coroutine for switch screen
            playerPokemonUsingFastMove = false;
            aiTrainerPokemonUsingFastMove = false;
            OnPokemonFaint(playerPokemon);


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

        if(!defendingPokemonController.effectivenessText.enabled && damage > 0) StartCoroutine(HUDManager.TextTimer(defendingPokemonController.effectivenessText, BattleCalculations.ConvertMultiplierToEffectivenessString(calc.y), 3f));
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
            weatherTimer = switchTimerLength;
            StartCoroutine(HUDManager.TextTimer(HUDManager.chargedMoveText, "It started to " + currentWeather.ToString() + "!", 3f));
            StopCoroutine(WeatherTimer());
            StartCoroutine(WeatherTimer());
        }

        if (chargedMove.terrainEffect != TerrainType.None) {
            currentTerrain = chargedMove.terrainEffect;
            HUDManager.UpdateTerrainIcon(currentTerrain);
            terrainTimer = switchTimerLength;
            StopCoroutine(TerrainTimer(switchTimerLength));
            StartCoroutine(TerrainTimer(switchTimerLength));
        }
    }
    #endregion
    #region Effects
    IEnumerator WeatherTimer() {
        yield return new WaitForSeconds(0.1f);
        HUDManager.weatherTimerIcon.fillAmount = weatherTimer / switchTimerLength;
        weatherTimer -= 0.1f;
        if (weatherTimer <= 0) {
            currentWeather = WeatherType.None;
            weatherTimer = switchTimerLength;
            HUDManager.UpdateWeatherIcon(currentWeather);
        } else {
            StartCoroutine(WeatherTimer());
        }
    }
    IEnumerator TerrainTimer(float timerLength) {
        yield return new WaitForSeconds(0.1f);
        HUDManager.terrainTimerIcon.fillAmount = terrainTimer / switchTimerLength;
        terrainTimer -= 0.1f;
        if (terrainTimer <= 0) {
            currentTerrain = TerrainType.None;
            terrainTimer = switchTimerLength;
            HUDManager.UpdateTerrainIcon(currentTerrain);
        } else {
            StartCoroutine(TerrainTimer(terrainTimer));
        }
    }
    #endregion
    #region Switching Pokemon
    public void SwitchPokemon(PokemonController pokemonController, bool pokemonFainted, int newPokemonIndex) {
        if (pokemonController == playerPokemonController) { playerSelectingPokemon = false; } else { aiTrainerSelectingPokemon = false; }

        pokemonController.pokeballSwitchAnimation.Play(); 

        StartCoroutine(PokemonSwitch(pokemonController, pokemonFainted, newPokemonIndex));
    }
    IEnumerator PokemonSwitch(PokemonController pokemonController, bool pokemonFainted, int newPokemonIndex) {
        if (pokemonController == playerPokemonController) {
            HUDManager.playerSwitchTimerImage.enabled = false;
        } else {
            HUDManager.aiTrainerSwitchTimerImage.enabled = false;
        }
        pokemonController.pokemonBattleImage.sprite = null;
        pokemonController.pokemonBattleImage.enabled = false;
        pokemonController.queuedChargedMove = null;

        if (!pokemonFainted) {
            if (pokemonController.currentPokemon.currentHP >= 0) pokemonController.switchTimer = switchTimerLength;
            StartCoroutine(SwitchCountdownTimer(pokemonController == playerPokemonController, pokemonController));
        }

        yield return new WaitForSeconds(0.5f);

        pokemonController.currentPokemon = pokemonController.pokemonInParty[newPokemonIndex];

        playerPokemonController.currentPokemonBattleSprite = playerPokemonController.currentPokemon.pokemonBattleSprite;
        aiTrainerPokemonController.currentPokemonBattleSprite = aiTrainerPokemonController.currentPokemon.pokemonBattleSprite;

        //pokemonController.pokemonBattleImage.sprite = pokemonController.currentPokemon.pokemonBattleSprite;

        playerPokemonIndividual = playerPokemonController.currentPokemon;
        aiTrainerPokemonIndividual = aiTrainerPokemonController.currentPokemon;

        SendOutPokemon(pokemonController);

        yield return new WaitForEndOfFrame();

        pokemonController.PostSwitch();

        // check for switch-in abilities
        //PostChargedMoveFormChanges(pokemonController);
        PostSwitchFormChanges(pokemonController);

        pokemonController.pokemonBattleImage.enabled = true;

        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public void PostChargedMoveFormChanges(PokemonController pokemonController) {
        var currentPokemon = pokemonController.currentPokemon;
        if (currentPokemon.pokemonBaseInfo.PokemonName == "Cherrim") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                }
            } else {
                if (currentPokemon.formChanged) {
                    currentPokemon.formChanged = false;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
                }
            }
        } else if (currentPokemon.pokemonBaseInfo.PokemonName == "Castform") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (pokemonController.currentPokemon.battleType[0] != types[2]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                    pokemonController.currentPokemon.battleType[0] = types[2];
                }
            } else if (currentWeather == WeatherType.Rain) {
                if (pokemonController.currentPokemon.battleType[0] != types[3]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[1];
                    pokemonController.currentPokemon.battleType[0] = types[3];
                }
            } else if (currentWeather == WeatherType.Snow) {
                if (pokemonController.currentPokemon.battleType[0] != types[4]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[2];
                    pokemonController.currentPokemon.battleType[0] = types[4];
                }
            } else {
                if (pokemonController.currentPokemon.battleType[0] != types[5]) {
                    currentPokemon.formChanged = false;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
                    pokemonController.currentPokemon.battleType[0] = types[5];
                }
            }
        }

        if (pokemonController.queuedChargedMove == null) return;

        if (pokemonController.queuedChargedMove.moveName == "Aura Wheel") {
            if (!pokemonController.currentPokemon.formChanged) {
                pokemonController.currentPokemon.formChanged = true;
                //pokemonController.queuedChargedMove.moveType = types[1]; Calculate this in calculate damage function to avoid changing move scriptable object
                pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                currentPokemon.pokemonBattleSprite = currentPokemon.pokemonBaseInfo.AlternateFormSprites[0];
            } else {
                pokemonController.currentPokemon.formChanged = false;
                //pokemonController.queuedChargedMove.moveType = types[0]; Calculate this in calculate damage function to avoid changing move scriptable object
                currentPokemon.pokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
                pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
            }
        } else if (pokemonController.queuedChargedMove.moveName == "King's Shield") {

            if (!pokemonController.currentPokemon.formChanged) {
                pokemonController.currentPokemon.formChanged = true;
                pokemonController.currentPokemon.pokemonBaseInfo = aegislashForms[1];
            } else {
                pokemonController.currentPokemon.formChanged = false;
                pokemonController.currentPokemon.pokemonBaseInfo = aegislashForms[0];
            }
            currentPokemon.pokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
            pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
        } else if (pokemonController.queuedChargedMove.moveName == "Relic Song") {

            if (!pokemonController.currentPokemon.formChanged) {
                pokemonController.currentPokemon.formChanged = true;
                pokemonController.currentPokemon.pokemonBaseInfo = meloettaForms[1];
            } else {
                pokemonController.currentPokemon.formChanged = false;
                pokemonController.currentPokemon.pokemonBaseInfo = meloettaForms[0];
            }
            currentPokemon.pokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
            pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
        }
        List<ChargedMove> weatherBallMoves = new List<ChargedMove>();
        if(playerPokemonController.currentPokemon.chargedMove1.moveName == "Weather Ball") {
            weatherBallMoves.Add(playerPokemonController.currentPokemon.chargedMove1);
        }
        if(playerPokemonController.currentPokemon.chargedMove2 != null) {
            if (playerPokemonController.currentPokemon.chargedMove2.moveName == "Weather Ball") {
                weatherBallMoves.Add(playerPokemonController.currentPokemon.chargedMove2);
            }
        }
        if (aiTrainerPokemonController.currentPokemon.chargedMove1.moveName == "Weather Ball") {
            weatherBallMoves.Add(aiTrainerPokemonController.currentPokemon.chargedMove1);
        }
        if (aiTrainerPokemonController.currentPokemon.chargedMove2 != null) {
            if (aiTrainerPokemonController.currentPokemon.chargedMove2.moveName == "Weather Ball") {
                weatherBallMoves.Add(aiTrainerPokemonController.currentPokemon.chargedMove2);
            }
        }
        foreach (ChargedMove cm in weatherBallMoves) {
            if (currentWeather == WeatherType.Sun) {
                cm.moveType = types[2];
            } else if (currentWeather == WeatherType.Rain) {
                cm.moveType = types[3];
            } else if (currentWeather == WeatherType.Snow) {
                cm.moveType = types[4];
            } else {
                cm.moveType = types[5];
            }
        }

        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public void PostSwitchFormChanges(PokemonController pokemonController) {
        var currentPokemon = pokemonController.currentPokemon;
        if (currentPokemon.pokemonBaseInfo.PokemonName == "Cherrim") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                }
            } else {
                if (currentPokemon.formChanged) {
                    currentPokemon.formChanged = false;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
                }
            }
        } else if (currentPokemon.pokemonBaseInfo.PokemonName == "Castform") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (pokemonController.currentPokemon.battleType[0] != types[2]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                    pokemonController.currentPokemon.battleType[0] = types[2];
                }
            } else if (currentWeather == WeatherType.Rain) {
                if (pokemonController.currentPokemon.battleType[0] != types[3]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[1];
                    pokemonController.currentPokemon.battleType[0] = types[3];
                }
            } else if (currentWeather == WeatherType.Snow) {
                if (pokemonController.currentPokemon.battleType[0] != types[4]) {
                    currentPokemon.formChanged = true;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[2];
                    pokemonController.currentPokemon.battleType[0] = types[4];
                }
            } else {
                if (pokemonController.currentPokemon.battleType[0] != types[5]) {
                    currentPokemon.formChanged = false;
                    pokemonController.currentPokemonBattleSprite = currentPokemon.pokemonBattleSprite;
                    pokemonController.currentPokemon.battleType[0] = types[5];
                }
            }
        }
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public void PostDamageFormChanges(PokemonController defendingPokemonController) {
        var currentPokemon = defendingPokemonController.currentPokemon;
        if(currentPokemon.pokemonBaseInfo.PokemonName == "Mimikyu") {
            if(!currentPokemon.formChanged) {
                currentPokemon.formChanged = true;
                defendingPokemonController.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
            }
        }
        if (currentPokemon.currentHP <= Mathf.Floor(currentPokemon.MaxHP * 0.25f)) {

        }
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
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
            //enable charged move hud , name text and bp text
        } else {
            StopCoroutine(aiTrainerPokemonController.PokemonSelectTimer(0));
            //enable charged move hud , name text and bp text
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
            //playerPokemonController.pokemonBattleImage.enabled = false;
            HUDManager.playerPokemonShadowSprite.enabled = false;
            HUDManager.playerSwitchTimerImage.enabled = true;
            StartCoroutine(playerPokemonController.PokemonSelectTimer(timer));
        } else {
            //aiTrainerPokemonController.pokemonBattleImage.enabled = false;
            HUDManager.aiTrainerPokemonShadowSprite.enabled = false;
            HUDManager.aiTrainerSwitchTimerImage.enabled = true;
            StartCoroutine(aiTrainerPokemonController.PokemonSelectTimer(timer));
        }      
    }
    #endregion
    #region Fainting
    void OnPokemonFaint(bool playerPokemon) {
        if (playerPokemon) {
            print(aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " fainted!");
            StartCoroutine(FaintAnimation(aiTrainerPokemonController));
            //disable charged move hud , name text and bp text
            //bring up switch countdown timer
        } else {
            print(playerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " fainted!");
            StartCoroutine(FaintAnimation(playerPokemonController));
            //disable charged move hud , name text and bp text
            //give exp to winning pokemon based on defeated pokemon
        }
        StartPokemonSelectTimer(true, playerPokemon);
    }
    IEnumerator FaintAnimation(PokemonController pokemonController) {
        //print("Faint Animation");
        yield return new WaitForSeconds(0.05f);
        var t = new Vector2(pokemonController.pokemonImageRt.sizeDelta.x - pokemonController.currentPokemon.pokemonBaseInfo.SpriteSize / 10, pokemonController.pokemonImageRt.sizeDelta.y - pokemonController.currentPokemon.pokemonBaseInfo.SpriteSize / 10);
        pokemonController.pokemonImageRt.sizeDelta = t;
        //print(t);
        if (pokemonController.pokemonImageRt.sizeDelta.x >= 0) {
            StartCoroutine(FaintAnimation(pokemonController));
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