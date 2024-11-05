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
    BattleReferences battleReferences;
    [Header("Player")]
    [HideInInspector] public PokemonIndividual playerPokemonIndividual;
    public PlayerPokemonController playerPokemonController;
    public Animation playerPokemonAnimation;
    public bool playerPokemonUsingFastMove;
    public bool playerPokemonUsingChargedMove;
    public bool playerSelectingPokemon;
    [Header("AI Trainer")]
    [HideInInspector] public PokemonIndividual aiTrainerPokemonIndividual;
    public AIPokemonController aiTrainerPokemonController;
    public Animation aiTrainerPokemonAnimation;
    public bool aiTrainerPokemonUsingFastMove;
    public bool aiTrainerPokemonUsingChargedMove;
    public bool aiTrainerSelectingPokemon;
    [Header("Settings")]
    public float switchTimerLength;
    [Header("Battle")]
    public WeatherType currentWeather;
    public float weatherTimer;
    public TerrainType currentTerrain;
    public float terrainTimer;

    private void Awake() {
        battleReferences = GetComponentInChildren<BattleReferences>();
    }

    private void Start() {
        playerPokemonController.currentPokemon.currentEnergy = 0;
        aiTrainerPokemonController.currentPokemon.currentEnergy = 0;

        for (int i = 0; i < playerPokemonController.pokemonInParty.Length; i++) {
            if (playerPokemonController.pokemonInParty[i].currentHP > 0) {
                HUDManager.playerPartyPokemonTimerImages[i].fillAmount = playerPokemonController.switchTimer / switchTimerLength;
                playerPokemonController.pokemonInParty[i].currentHP = playerPokemonController.pokemonInParty[i].MaxHP;
                playerPokemonController.pokemonInParty[i].currentPokemonBattleSprite = playerPokemonController.pokemonInParty[i].basePokemonBattleSprite;
            }
        }

        for (int i = 0; i < aiTrainerPokemonController.pokemonInParty.Length; i++) {
            if (aiTrainerPokemonController.pokemonInParty[i].currentHP > 0) {
                aiTrainerPokemonController.pokemonInParty[i].currentHP = aiTrainerPokemonController.pokemonInParty[i].MaxHP;
                aiTrainerPokemonController.pokemonInParty[i].currentPokemonBattleSprite = aiTrainerPokemonController.pokemonInParty[i].basePokemonBattleSprite;
            }
        }

        battleReferences.weatherBall.moveType = battleReferences.normalType;

        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    private void OnDisable() {
        foreach (PokemonIndividual poke in playerPokemonController.pokemonInParty) {
            if(poke.chargedMove1 != null && poke.chargedMove1.moveName == "Aura Wheel") {
                poke.chargedMove1 = battleReferences.auraWheels[0];
            }
            if (poke.chargedMove2 != null && poke.chargedMove2.moveName == "Aura Wheel") {
                poke.chargedMove2 = battleReferences.auraWheels[0];
            }
        }

        battleReferences.weatherBall.moveType = battleReferences.normalType;
    }
    #region Fast Attack
    public void StartFastAttack(PokemonController attackingPokemonController, FastMove fastMove, bool playerPokemon) {
        if (playerPokemonUsingChargedMove || aiTrainerPokemonUsingChargedMove || (playerPokemonUsingChargedMove && aiTrainerPokemonUsingChargedMove)) return;

        if (attackingPokemonController.currentPokemon.currentHP <= 0) return;
        if (attackingPokemonController.currentPokemon.currentStatus == StatusEffect.Sleep) return;
        if (attackingPokemonController.currentPokemon.currentStatus == StatusEffect.Attraction) {
            float rnd = Random.Range(0, 4);
            if (rnd < 1) {
                print(attackingPokemonController.currentPokemon.pokemonNickname + " is immobilized by love!");
                return;
            } 
        }
        if (attackingPokemonController.currentPokemon.currentStatus == StatusEffect.Paralysis) {
            float rnd = Random.Range(0, 4);
            if (rnd < 1) {
                print(attackingPokemonController.currentPokemon.pokemonNickname + " is paralyzed!");
                return;
            }

        }

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

        yield return new WaitForSeconds((0.5f * fastMove.cycles) * 0.25f);

        if (attackingPokemonController.currentPokemon.currentEnergy < 100) attackingPokemonController.currentPokemon.currentEnergy += fastMove.baseEnergy;
        if (attackingPokemonController.currentPokemon.currentEnergy > 100) attackingPokemonController.currentPokemon.currentEnergy = 100;

        ApplyAttackDamage(attackingPokemonController, defendingPokemonController, fastMove);

        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);

        //if (playerPokemon) playerPokemonUsingFastMove = false;

        yield return new WaitForSeconds((0.5f * fastMove.cycles) * 0.75f);

        if (defendingPokemonController.currentPokemon.currentHP <= 0 && !defendingPokemonController.throwingChargedMove) {

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
            print("Throw Charged Move");
        } else {
            //if (!aiTrainerPokemonController.shouldThrowChargedMoves) return;
            aiTrainerPokemonUsingChargedMove = true;
            if (!aiTrainerPokemonUsingFastMove) ChargedAttackSetup();
        }
    }
    public void ChargedAttackSetup() {
        print("Charged Move Setup");
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

        WeatherChanges(attackingPokemonController);
        WeatherChanges(defendingPokemonController);

        PostChargedMoveFormChanges(attackingPokemonController);
        PostChargedMoveFormChanges(defendingPokemonController);

        if (defendingPokemonController.currentPokemon.currentHP <= 0) {
            // start coroutine for switch screen
            playerPokemonUsingFastMove = false;
            aiTrainerPokemonUsingFastMove = false;
            OnPokemonFaint(playerPokemon);

        } else {
            if(attackingPokemonController.currentPokemon.currentHP <= 0) {
                playerPokemonUsingFastMove = false;
                aiTrainerPokemonUsingFastMove = false;
                OnPokemonFaint(!playerPokemon);

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
    }
    void ApplyAttackDamage(PokemonController attackingPokemonController, PokemonController defendingPokemonController, PokemonMove pokemonMove) {
        Vector2 calc = BattleCalculations.CalculateAttackDamage(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, pokemonMove);
        float damage = calc.x;

        damage *= BattleCalculations.CalculateWeatherMultiplier(pokemonMove, currentWeather);
        damage *= BattleCalculations.CalculateTerrainMultiplier(pokemonMove, currentTerrain);

        if(defendingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Mimikyu") {
            if (!defendingPokemonController.currentPokemon.formChanged) damage = 0;
        }

        if(!defendingPokemonController.effectivenessText.enabled && damage > 0) StartCoroutine(HUDManager.TextTimer(defendingPokemonController.effectivenessText, BattleCalculations.ConvertMultiplierToEffectivenessString(calc.y), 3f));
        defendingPokemonController.currentPokemon.currentHP -= Mathf.RoundToInt(damage);

        PostDamageFormChanges(defendingPokemonController);
        HUDManager.UpdateHUD(playerPokemonController, aiTrainerPokemonController);
    }
    void ApplyAttackEffects(PokemonController attackingPokemonController, PokemonController defendingPokemonController, ChargedMove chargedMove) {
        // OWN BUFFS
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
        // OPPONENT DEBUFFS
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
        // STATUS EFFECTS
        float rdmOppStatus = Random.Range(0, 100);
        if (rdmOppStatus <= chargedMove.moveEffectChance) {
            string effectString = "";

            effectString += defendingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName;

            if(chargedMove.moveName == "Dire Claw") {
                float rdmDC = Random.Range(0, 3);
                if (rdmDC < 1) {
                    chargedMove.moveEffect = StatusEffect.Paralysis;
                } else if (rdmDC >= 1 && rdmDC <= 2) {
                    chargedMove.moveEffect = StatusEffect.Poison;
                } else if (rdmDC > 2 && rdmDC <= 3) {
                    chargedMove.moveEffect = StatusEffect.Sleep;
                }
            }

            int statusDuration = 5;
            if (chargedMove.moveEffect == StatusEffect.Attraction) {
                effectString += " fell in love!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Attraction;
                statusDuration = 20;

            } else if (chargedMove.moveEffect == StatusEffect.Burn) {
                effectString += " was burned!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Burn;
                if (defendingPokemonController.currentPokemon.currentBuffs[0] > -3.5f) defendingPokemonController.currentPokemon.currentBuffs[0] -= 0.5f;
                statusDuration = 12;

            } else if (chargedMove.moveEffect == StatusEffect.Confusion) {
                effectString += " got confused!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Confusion;
                statusDuration = 10;

            } else if (chargedMove.moveEffect == StatusEffect.Paralysis) {
                effectString += " was paralyzed!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Paralysis;
                if (defendingPokemonController.currentPokemon.currentBuffs[4] > -3f) defendingPokemonController.currentPokemon.currentBuffs[4] -= 1f;
                statusDuration = 15;

            } else if (chargedMove.moveEffect == StatusEffect.Poison) {
                effectString += " was poisoned!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Poison;
                statusDuration = 10;

            } else if (chargedMove.moveEffect == StatusEffect.Sleep) {
                effectString += " fell asleep!";
                defendingPokemonController.currentPokemon.currentStatus = StatusEffect.Sleep;
                statusDuration = 6;
            }

            if(defendingPokemonController.currentPokemon.currentHP > 0) StartCoroutine(StatusTimer(defendingPokemonController, statusDuration));
            print(effectString);
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
    IEnumerator StatusTimer(PokemonController pokemonController, int durationInTurns) {
        yield return new WaitForSeconds(0.5f);

        durationInTurns--;
        print("StatusTimer " + durationInTurns);
        if (durationInTurns <= 0) {
            print(pokemonController.currentPokemon.currentStatus);
            pokemonController.currentPokemon.currentStatus = StatusEffect.None;
            HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
        } else {
            ApplyStatusEffects(pokemonController, pokemonController.currentPokemon.currentStatus);
            StartCoroutine(StatusTimer(pokemonController, durationInTurns));
        }
    }
    public void ApplyStatusEffects(PokemonController pokemonController, StatusEffect statusEffect) {
        if (statusEffect == StatusEffect.Attraction) {
            //print(pokemonController.currentPokemon.pokemonNickname + " is immobilized by love!");

        } else if (statusEffect == StatusEffect.Burn) {
            pokemonController.currentPokemon.currentHP -= 1;

        } else if (statusEffect == StatusEffect.Confusion) {
            int rdm = Random.Range(0, 10);
            if (rdm < 3) pokemonController.currentPokemon.currentHP -= 3;
            // Change this to pokemon hitting itself with its own fast attack

        } else if (statusEffect == StatusEffect.Paralysis) {
            //print(pokemonController.currentPokemon.pokemonNickname + " is paralyzed!");

        } else if (statusEffect == StatusEffect.Poison) {
            pokemonController.currentPokemon.currentHP -= 2;

        } else if (statusEffect == StatusEffect.Sleep) {
            //Pokemon can't attack
            print(pokemonController.currentPokemon.pokemonNickname + " is fast asleep!");
        }
    }
    public void WeatherChanges(PokemonController pokemonController) {
        var currentPokemon = pokemonController.currentPokemon;
        if (currentPokemon.pokemonBaseInfo.PokemonName == "Cherrim") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                }
            } else {
                if (currentPokemon.formChanged) {
                    currentPokemon.formChanged = false;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
                }
            }
        } else if (currentPokemon.pokemonBaseInfo.PokemonName == "Castform") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (currentPokemon.battleType[0] != battleReferences.fireType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                    currentPokemon.battleType[0] = battleReferences.fireType;
                }
            } else if (currentWeather == WeatherType.Rain) {
                if (currentPokemon.battleType[0] != battleReferences.waterType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[1];
                    currentPokemon.battleType[0] = battleReferences.waterType;
                }
            } else if (currentWeather == WeatherType.Snow) {
                if (currentPokemon.battleType[0] != battleReferences.iceType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[2];
                    currentPokemon.battleType[0] = battleReferences.iceType;
                }
            } else {
                if (currentPokemon.battleType[0] != battleReferences.normalType) {
                    currentPokemon.formChanged = false;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
                    currentPokemon.battleType[0] = battleReferences.normalType;
                }
            }
        }
        pokemonController.pokemonBattleImage.sprite = currentPokemon.currentPokemonBattleSprite;
    }
    public void PostChargedMoveFormChanges(PokemonController pokemonController) {
        var currentPokemon = pokemonController.currentPokemon;

        if (pokemonController.queuedChargedMove == null) return;

        if (pokemonController.queuedChargedMove.moveName == "Aura Wheel") {
            if (!currentPokemon.formChanged) { // HANGRY MODE
                currentPokemon.formChanged = true;
                if (pokemonController.queuedChargedMove == currentPokemon.chargedMove1) {
                    currentPokemon.chargedMove1 = battleReferences.auraWheels[1];
                } else if (currentPokemon.chargedMove2 && pokemonController.queuedChargedMove == currentPokemon.chargedMove2) {
                    currentPokemon.chargedMove2 = battleReferences.auraWheels[1];
                }
                currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];

            } else {  // FULL BELLY MODE
                currentPokemon.formChanged = false;
                if (pokemonController.queuedChargedMove == currentPokemon.chargedMove1) {
                    currentPokemon.chargedMove1 = battleReferences.auraWheels[0];
                } else if (currentPokemon.chargedMove2 && pokemonController.queuedChargedMove == currentPokemon.chargedMove2) {
                    currentPokemon.chargedMove2 = battleReferences.auraWheels[0];
                }
                currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
            }
        } else if (pokemonController.queuedChargedMove.moveName == "King's Shield") {

            if (!currentPokemon.formChanged) { // SWORD FORM
                currentPokemon.formChanged = true;
                currentPokemon.pokemonBaseInfo = battleReferences.aegislashForms[1];

            } else { // SHIELD FORM
                currentPokemon.formChanged = false;
                currentPokemon.pokemonBaseInfo = battleReferences.aegislashForms[0];
            }
            currentPokemon.SetBattleSprite();
            currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;

        } else if (pokemonController.queuedChargedMove.moveName == "Relic Song") {

            if (!currentPokemon.formChanged) { // PIROUETTE FORM
                currentPokemon.formChanged = true;
                currentPokemon.pokemonBaseInfo = battleReferences.meloettaForms[1];

            } else { // ARIA FORM
                currentPokemon.formChanged = false;
                currentPokemon.pokemonBaseInfo = battleReferences.meloettaForms[0];
            }
            currentPokemon.SetBattleSprite();
            currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
        }

        if (currentWeather == WeatherType.Sun) {
            battleReferences.weatherBall.moveType = battleReferences.fireType;
        } else if (currentWeather == WeatherType.Rain) {
            battleReferences.weatherBall.moveType = battleReferences.waterType;
        } else if (currentWeather == WeatherType.Snow) {
            battleReferences.weatherBall.moveType = battleReferences.iceType;
        } else if (currentWeather == WeatherType.Sandstorm) {
            battleReferences.weatherBall.moveType = battleReferences.rockType;
        } else {
            battleReferences.weatherBall.moveType = battleReferences.normalType;
        }
        //pokemonController.pokemonBattleImage.sprite = currentPokemon.currentPokemonBattleSprite;
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public void PostSwitchFormChanges(PokemonController pokemonController) {
        var currentPokemon = pokemonController.currentPokemon;
        if (currentPokemon.pokemonBaseInfo.PokemonName == "Cherrim") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                }
            } else {
                if (currentPokemon.formChanged) {
                    currentPokemon.formChanged = false;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
                }
            }
        } else if (currentPokemon.pokemonBaseInfo.PokemonName == "Castform") { // CHANGE THIS TO CHECK FOR ABILITY RATHER THAN NAME
            if (currentWeather == WeatherType.Sun) {
                if (currentPokemon.battleType[0] != battleReferences.fireType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
                    currentPokemon.battleType[0] = battleReferences.fireType;
                }
            } else if (currentWeather == WeatherType.Rain) {
                if (currentPokemon.battleType[0] != battleReferences.waterType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[1];
                    currentPokemon.battleType[0] = battleReferences.waterType;
                }
            } else if (currentWeather == WeatherType.Snow) {
                if (currentPokemon.battleType[0] != battleReferences.iceType) {
                    currentPokemon.formChanged = true;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[2];
                    currentPokemon.battleType[0] = battleReferences.iceType;
                }
            } else {
                if (currentPokemon.battleType[0] != battleReferences.normalType) {
                    currentPokemon.formChanged = false;
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.basePokemonBattleSprite;
                    currentPokemon.battleType[0] = battleReferences.normalType;
                }
            }
        } else if (currentPokemon.pokemonBaseInfo.PokemonName == "Palafin") {
            if (currentPokemon.formChanged) {
                currentPokemon.pokemonBaseInfo = battleReferences.palafinForms[1];
                currentPokemon.currentPokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
            }
        }
        //pokemonController.pokemonBattleImage.sprite = currentPokemon.currentPokemonBattleSprite;
        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
    }
    public void PostDamageFormChanges(PokemonController defendingPokemonController) {
        var currentPokemon = defendingPokemonController.currentPokemon;

        if (currentPokemon.pokemonBaseInfo.PokemonName == "Mimikyu") {
            if (!currentPokemon.formChanged) {
                currentPokemon.formChanged = true;
                currentPokemon.currentPokemonBattleSprite = currentPokemon.alternateFormSprites[0];
            }
        }
        if (currentPokemon.currentHP <= Mathf.Floor(currentPokemon.MaxHP * 0.5f) && currentPokemon.currentHP > 0) {
            if (currentPokemon.pokemonBaseInfo.PokemonName == "Minior") {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    currentPokemon.pokemonBaseInfo = battleReferences.miniorForms[1];
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
                }
            } else if(currentPokemon.pokemonBaseInfo.PokemonName == "Darmanitan") {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;

                    if(currentPokemon.pokemonBaseInfo.PrimaryType.typeName == TypeName.Fire) {
                        currentPokemon.pokemonBaseInfo = battleReferences.darmanitanForms[1];
                        currentPokemon.currentPokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
                    } else {
                        currentPokemon.pokemonBaseInfo = battleReferences.darmanitanForms[3];
                        currentPokemon.currentPokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
                    }
                }
            }
        } 
        if (currentPokemon.currentHP <= Mathf.Floor(currentPokemon.MaxHP * 0.25f) && currentPokemon.currentHP > 0) {
            if (currentPokemon.pokemonBaseInfo.PokemonName == "Wishiwashi") {
                if (!currentPokemon.formChanged) {
                    currentPokemon.formChanged = true;
                    print("should change");
                    currentPokemon.pokemonBaseInfo = battleReferences.wishiwashiForms[1];
                    currentPokemon.currentPokemonBattleSprite = currentPokemon.pokemonBaseInfo.PokemonBattleSprite;
                }
            }
        }

        HUDManager.SetHUD(playerPokemonController, aiTrainerPokemonController);
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

        //Reset debuffs and battle types here
        if(pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto" || pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zorua" || pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zoroark") {
            pokemonController.currentPokemon.formChanged = false;
        } else if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Palafin") {
            pokemonController.currentPokemon.formChanged = true;
        }

        pokemonController.currentPokemon.currentBuffs = new float[5];
        pokemonController.currentPokemon.currentStatus = StatusEffect.None;
        StopCoroutine(StatusTimer(pokemonController, 5));

        pokemonController.currentPokemon.battleType[0] = pokemonController.currentPokemon.pokemonBaseInfo.PrimaryType;

        if (pokemonController.currentPokemon.pokemonBaseInfo.SecondaryType != null) {
            pokemonController.currentPokemon.battleType[1] = pokemonController.currentPokemon.pokemonBaseInfo.SecondaryType;
        }

        pokemonController.currentPokemon = pokemonController.pokemonInParty[newPokemonIndex];

        /*
        if(pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Palafin") {
            if(pokemonController.currentPokemon.formChanged) {
                pokemonController.currentPokemon.pokemonBaseInfo = battleReferences.palafinForms[1];
            } else {
                pokemonController.currentPokemon.pokemonBaseInfo = battleReferences.palafinForms[0];
            }
        }
        */
        playerPokemonIndividual = playerPokemonController.currentPokemon;
        aiTrainerPokemonIndividual = aiTrainerPokemonController.currentPokemon;

        yield return new WaitForEndOfFrame();

        pokemonController.PostSwitch();

        // check for switch-in abilities
        PostSwitchFormChanges(pokemonController);

        SendOutPokemon(pokemonController == playerPokemonController);

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
    public void SendOutPokemon(bool playerPokemon) {
        if (playerPokemon) {
            HUDManager.playerHUD.SetActive(true);
            StopCoroutine(playerPokemonController.PokemonSelectTimer(0));

        } else {
            HUDManager.aiTrainerHUD.SetActive(true);
            StopCoroutine(aiTrainerPokemonController.PokemonSelectTimer(0));
            print("Send out pokemon");
        }
    }
    void StartPokemonSelectTimer(bool pokemonFainted, bool playerPokemon) {  // timer for selecting pokemon after pokemon faints
        int timer = 5;
        if (pokemonFainted) {
            timer = 10;
        }
        playerSelectingPokemon = !playerPokemon;
        aiTrainerSelectingPokemon = playerPokemon;
        aiTrainerPokemonUsingFastMove = false;
        playerPokemonUsingFastMove = false;

        if(!playerPokemon) {
            //playerPokemonController.pokemonBattleImage.enabled = false;
            playerPokemonController.pokemonShadowSprite.enabled = false;
            HUDManager.playerSwitchTimerImage.enabled = true;
            StartCoroutine(playerPokemonController.PokemonSelectTimer(timer));
        } else {
            //aiTrainerPokemonController.pokemonBattleImage.enabled = false;
            aiTrainerPokemonController.pokemonShadowSprite.enabled = false;
            HUDManager.aiTrainerSwitchTimerImage.enabled = true;
            StartCoroutine(aiTrainerPokemonController.PokemonSelectTimer(timer));
        }      
    }
    #endregion
    #region Fainting
    void OnPokemonFaint(bool playerPokemon) {
        if (playerPokemon) {
            print(aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " fainted!");
            aiTrainerPokemonController.pokemonBattleImage.sprite = aiTrainerPokemonController.currentPokemon.currentPokemonBattleSprite;
            var t = new Vector2(aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
            aiTrainerPokemonController.pokemonImageRt.sizeDelta = t;
            StopCoroutine(StatusTimer(playerPokemonController, 0));
            StartCoroutine(FaintAnimation(aiTrainerPokemonController));
            //disable charged move hud , name text and bp text
            //bring up switch countdown timer
        } else {
            print(playerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " fainted!");
            playerPokemonController.pokemonBattleImage.sprite = playerPokemonController.currentPokemon.currentPokemonBattleSprite;
            var t = new Vector2(playerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, playerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
            playerPokemonController.pokemonImageRt.sizeDelta = t;
            StopCoroutine(StatusTimer(aiTrainerPokemonController, 0));
            StartCoroutine(FaintAnimation(playerPokemonController));
            //disable charged move hud , name text and bp text
            //give exp to winning pokemon based on defeated pokemon
        }
        HUDManager.DisablePokemonHUD(!playerPokemon);
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