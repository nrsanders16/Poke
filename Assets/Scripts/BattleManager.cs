using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class BattleManager : MonoBehaviour {

    public PokemonIndividual playerPokemonIndividual;
    public PokemonIndividual aiTrainerPokemonIndividual;

    public PlayerPokemonController playerPokemonController;
    public AIPokemonController aiTrainerPokemonController;

    public Image[] playerPartyPokemonSprites;

    public TMP_Text playerPokemonText;
    public Image playerPokemonSprite;
    public TMP_Text playerPokemonHPText;
    public TMP_Text playerPokemonEffectivenessText;
    public Animation playerPokemonAnimation;
    public TMP_Text playerChargedMove1NameText;
    public TMP_Text playerChargedMove2NameText;
    public Image playerEnergyOutline1;
    public Image playerEnergyBackground1;
    public Image playerEnergyImage1layer1;
    public Image playerEnergyImage1layer2;
    public Image playerEnergyBackground2;
    public Image playerEnergyOutline2;
    public Image playerEnergyImage1layer3;
    public Image playerEnergyImage2layer1;
    public Image playerEnergyImage2layer2;
    public Image playerEnergyImage2layer3;

    public GameObject playerHPBar;
    public Image playerHPFill;
    public GameObject aiTrainerHPBar;
    public Image aiTrainerHPFill;

    public TMP_Text aiTrainerPokemonText;
    public Image aiTrainerPokemonSprite;
    public TMP_Text aiTrainerPokemonHPText;
    public TMP_Text aiTrainerPokemonEffectivenessText;
    public Animation aiTrainerPokemonAnimation;
    //public Image aiTrainerEnergyImage1Layer1;
    //public Image aiTrainerEnergyImage1layer2;
    //public Image aiTrainerEnergyImage1layer3;
    //public Image aiTrainerEnergyImage2layer1;
    //public Image aiTrainerEnergyImage2layer2;
    //public Image aiTrainerEnergyImage2layer3;

    public TMP_Text chargedMoveText;

    public bool playerPokemonUsingFastMove;
    public bool aiTrainerPokemonUsingFastMove;

    public bool playerPokemonUsingChargedMove;
    public bool aiTrainerPokemonUsingChargedMove;

    public bool playerSelectingPokemon;
    public bool aiTrainerSelectingPokemon;

    //[SerializeField] bool chargedMoveTie;

    private void Start() {
        playerPokemonController.currentPokemon.currentEnergy = 0;
        aiTrainerPokemonController.currentPokemon.currentEnergy = 0;
        SetHUD();
        UpdateHUD();
    }
    #region Attacking
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

        UpdateHUD();

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
        chargedMoveText.enabled = true;
        chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " is unleashing energy!";

        yield return new WaitForSeconds(5);

        chargedMoveText.text = attackingPokemonController.currentPokemon.pokemonBaseInfo.PokemonName + " used " + attackingPokemonController.queuedChargedMove.moveName + "!";

        yield return new WaitForSeconds(2.5f);
        chargedMoveText.enabled = false;

        //play attack animations and apply damage
        attackingPokemonController.currentPokemon.currentEnergy -= attackingPokemonController.queuedChargedMove.baseEnergyReq;

        ApplyAttackDamage(attackingPokemonController.currentPokemon, defendingPokemonController.currentPokemon, attackingPokemonController.queuedChargedMove);
        //apply attack effects

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
        //print(damage);
        defendingPokemon.currentHP -= damage;
        UpdateHUD();
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
            playerPokemonSprite.sprite = null;
            playerPokemonSprite.enabled = false;

        } else {
            aiTrainerPokemonController.queuedChargedMove = null;
            aiTrainerPokemonSprite.sprite = null;
            aiTrainerPokemonSprite.enabled = false;
        }

        yield return new WaitForSeconds(0.5f);

        if (playerPokemon) {
            playerPokemonController.currentPokemon = playerPokemonController.pokemonInParty[newPokemonIndex];
            playerPokemonIndividual = playerPokemonController.currentPokemon;
            playerPokemonSprite.sprite = playerPokemonIndividual.pokemonBattleSprite;
            playerPokemonSprite.enabled = true;
        } else {
            aiTrainerPokemonController.currentPokemon = aiTrainerPokemonController.pokemonInParty[newPokemonIndex];
            aiTrainerPokemonIndividual = aiTrainerPokemonController.currentPokemon;
            aiTrainerPokemonSprite.sprite = aiTrainerPokemonIndividual.pokemonBattleSprite;
            aiTrainerPokemonSprite.enabled = true;
        }

        yield return new WaitForEndOfFrame();

        playerPokemonController.PostSwitch();
        aiTrainerPokemonController.PostSwitch();
        SetHUD();
        UpdateHUD();
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
    #region HUD
    void SetHUD() {
        playerPokemonText.text = playerPokemonIndividual.pokemonBaseInfo.PokemonName;
        aiTrainerPokemonText.text = aiTrainerPokemonIndividual.pokemonBaseInfo.PokemonName;

        playerPokemonSprite.sprite = playerPokemonIndividual.pokemonBaseInfo.PokemonBattleSprite;
        aiTrainerPokemonSprite.sprite = aiTrainerPokemonIndividual.pokemonBaseInfo.PokemonBattleSprite;

        playerChargedMove1NameText.text = playerPokemonIndividual.chargedMove1.moveName;
        playerEnergyBackground1.sprite = playerPokemonIndividual.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer1.color = playerPokemonIndividual.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer1.sprite = playerPokemonIndividual.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer2.color = playerPokemonIndividual.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer2.sprite = playerPokemonIndividual.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer3.color = playerPokemonIndividual.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer3.sprite = playerPokemonIndividual.chargedMove1.moveType.typeIcon;

        if (playerPokemonIndividual.chargedMove2 != null) {
            playerChargedMove2NameText.text = playerPokemonIndividual.chargedMove2.moveName;
            playerEnergyBackground2.sprite = playerPokemonIndividual.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer1.color = playerPokemonIndividual.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer1.sprite = playerPokemonIndividual.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer2.color = playerPokemonIndividual.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer2.sprite = playerPokemonIndividual.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer3.color = playerPokemonIndividual.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer3.sprite = playerPokemonIndividual.chargedMove2.moveType.typeIcon;
        }

        for (int i = 0; i < playerPartyPokemonSprites.Length; i++) {
            if (playerPartyPokemonSprites[i] != null) {
                playerPartyPokemonSprites[i].sprite = playerPokemonController.pokemonInParty[i].pokemonBattleSprite;
            }
        }
    }
    void UpdateHUD() {
        //playerPokemonHPText.text = playerPokemonIndividual.currentHP.ToString();
        float playerFill = (float)playerPokemonController.currentPokemon.currentHP / (float)playerPokemonController.currentPokemon.pokemonBaseInfo.BaseHP;
        playerHPFill.fillAmount = playerFill;

        if (playerFill >= 0.5f) {
            playerHPFill.color = Color.green;
        } else if (playerFill < 0.5f && playerFill >= 0.2f) {
            playerHPFill.color = Color.yellow;
        } else if (playerFill < 0.2f) {
            playerHPFill.color = Color.red;
        }
        print(playerFill);

        //aiTrainerPokemonHPText.text = aiTrainerPokemonIndividual.currentHP.ToString();
        float aiFill = (float)aiTrainerPokemonController.currentPokemon.currentHP / (float)aiTrainerPokemonController.currentPokemon.MaxHP;
        aiTrainerHPFill.fillAmount = aiFill;

        if (aiFill >= 0.5f) {
            aiTrainerHPFill.color = Color.green;
        } else if (aiFill < 0.5f && aiFill >= 0.2f) {
            aiTrainerHPFill.color = Color.yellow;
        } else if (aiFill < 0.2f) {
            aiTrainerHPFill.color = Color.red;
        }

        ManageChargedMove1Fill();

        if (playerPokemonIndividual.chargedMove2 != null) {
            ManageChargedMove2Fill();
        }
    }
    public void ManageChargedMove1Fill() {
        float _1fill1Amount = (float)playerPokemonIndividual.currentEnergy / (float)playerPokemonIndividual.chargedMove1.baseEnergyReq;
        playerEnergyImage1layer1.fillAmount = _1fill1Amount;

        float _1fill2Amount = ((float)playerPokemonIndividual.currentEnergy - (float)playerPokemonIndividual.chargedMove1.baseEnergyReq) / (float)playerPokemonIndividual.chargedMove1.baseEnergyReq;
        playerEnergyImage1layer2.fillAmount = _1fill2Amount;

        float _1fill3Amount = ((float)playerPokemonIndividual.currentEnergy - (float)(playerPokemonIndividual.chargedMove1.baseEnergyReq * 2)) / (float)playerPokemonIndividual.chargedMove1.baseEnergyReq;
        playerEnergyImage1layer3.fillAmount = _1fill3Amount;

        if (playerPokemonIndividual.currentEnergy >= playerPokemonIndividual.chargedMove1.baseEnergyReq) {
            playerChargedMove1NameText.color = Color.white;
            playerEnergyOutline1.color = Color.white;
        } else {
            playerChargedMove1NameText.color = Color.black;
            playerEnergyOutline1.color = Color.black;
        }
    }
    public void ManageChargedMove2Fill() {
        float _2fill1Amount = (float)playerPokemonIndividual.currentEnergy / (float)playerPokemonIndividual.chargedMove2.baseEnergyReq;
        playerEnergyImage2layer1.fillAmount = _2fill1Amount;

        float _2fill2Amount = ((float)playerPokemonIndividual.currentEnergy - (float)playerPokemonIndividual.chargedMove2.baseEnergyReq) / (float)playerPokemonIndividual.chargedMove2.baseEnergyReq;
        playerEnergyImage2layer2.fillAmount = _2fill2Amount;

        float _2fill3Amount = ((float)playerPokemonIndividual.currentEnergy - (float)(playerPokemonIndividual.chargedMove2.baseEnergyReq * 2)) / (float)playerPokemonIndividual.chargedMove2.baseEnergyReq;
        playerEnergyImage2layer3.fillAmount = _2fill3Amount;

        if (playerPokemonIndividual.currentEnergy >= playerPokemonIndividual.chargedMove2.baseEnergyReq) {
            playerChargedMove2NameText.color = Color.white;
            playerEnergyOutline2.color = Color.white;

        } else {
            playerChargedMove2NameText.color = Color.black;
            playerEnergyOutline2.color = Color.black;
        }
    }
    public IEnumerator EffectivenessTextTimer(TMP_Text text) {
        text.enabled = true;
        yield return new WaitForSeconds(2.5f);
        text.enabled = false;
    }
    #endregion
}