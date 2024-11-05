using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
public class HUDManager : MonoBehaviour {

    public Image weatherIcon;
    public Image weatherTimerIcon;
    public Image terrainIcon;
    public Image terrainTimerIcon;
    public TMP_Text chargedMoveText;

    [Header("Player HUD")]
    public GameObject playerHUD;
    public RectTransform chargedMove1RectTransform;
    public RectTransform chargedMove2RectTransform;
    public GameObject playerHPBar;
    public Image playerHPFill;
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
    public Image playerSwitchTimerImage;

    public Image[] playerPartyPokemonSprites;
    public Image[] playerPartyPokemonTimerImages;
    public Image[] playerPartyPokemonShadowSprites;

    [Header("AI Trainer HUD")]
    public GameObject aiTrainerHUD;
    public TMP_Text aiTrainerPokemonEffectivenessText;
    public Image aiTrainerSwitchTimerImage;
    public GameObject aiTrainerHPBar;
    public Image aiTrainerHPFill;

    public Sprite[] techIcons;
    public Sprite[] weatherIcons;
    public Sprite[] terrainIcons;
    public Sprite[] statusIcons;

    public void SetHUD(PokemonController playerPokemonController, PokemonController aiTrainerPokemonController) {

        SetPokemonBattleSprite(playerPokemonController, aiTrainerPokemonController);
        SetPokemonBattleSprite(aiTrainerPokemonController, playerPokemonController);

        SetBattleHUD(playerPokemonController, aiTrainerPokemonController);
        SetBattleHUD(aiTrainerPokemonController, playerPokemonController);

        SetDittoTransformInfo(playerPokemonController, aiTrainerPokemonController);
        SetDittoTransformInfo(aiTrainerPokemonController, playerPokemonController);

        SetPlayerChargedMoveHUD(playerPokemonController);

        SetFormChangedBool(playerPokemonController);
        SetFormChangedBool(aiTrainerPokemonController);

        for (int i = 0; i < playerPartyPokemonSprites.Length; i++) {
            if (playerPartyPokemonSprites[i] != null) {
                playerPartyPokemonSprites[i].sprite = playerPokemonController.pokemonInParty[i].currentPokemonBattleSprite;
                if (playerPokemonController.pokemonInParty[i].shadow) {
                    playerPartyPokemonShadowSprites[i].enabled = true;
                } else {
                    playerPartyPokemonShadowSprites[i].enabled = false;
                }
            }
        }
    }
    public void SetBattleHUD(PokemonController pokemonController, PokemonController opposingPokemonController) {
        
        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zorua" || pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zoroark") {
            if (pokemonController.currentPokemon.formChanged) return;

            pokemonController.pokemonNameText.text = pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].pokemonBaseInfo.PokemonName;
            pokemonController.pokemonBPText.text = "BP " + pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].BattlePower.ToString();

            pokemonController.type1Icon.sprite = pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].battleType[0].typeIcon;
            if (pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].battleType.Length > 1) {
                pokemonController.type2Icon.enabled = true;
                pokemonController.type2IconBackground.enabled = true;
                pokemonController.type2Icon.sprite = pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].battleType[1].typeIcon;
            } else {
                pokemonController.type2Icon.enabled = false;
                pokemonController.type2IconBackground.enabled = false;
            }

        } else if(pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto") {
            if (pokemonController.currentPokemon.formChanged) return;

            pokemonController.pokemonNameText.text = pokemonController.currentPokemon.pokemonBaseInfo.PokemonName;
            pokemonController.pokemonBPText.text = "BP " + pokemonController.currentPokemon.BattlePower.ToString();

            pokemonController.type1Icon.sprite = opposingPokemonController.currentPokemon.battleType[0].typeIcon;
            if (opposingPokemonController.currentPokemon.battleType.Length > 1) {
                pokemonController.type2Icon.enabled = true;
                pokemonController.type2IconBackground.enabled = true;
                pokemonController.type2Icon.sprite = opposingPokemonController.currentPokemon.battleType[1].typeIcon;
            } else {
                pokemonController.type2Icon.enabled = false;
                pokemonController.type2IconBackground.enabled = false;
            }
        } else {

            pokemonController.pokemonNameText.text = pokemonController.currentPokemon.pokemonBaseInfo.PokemonName;
            pokemonController.pokemonBPText.text = "BP " + pokemonController.currentPokemon.BattlePower.ToString();

            pokemonController.type1Icon.sprite = pokemonController.currentPokemon.battleType[0].typeIcon;
            if (pokemonController.currentPokemon.battleType.Length > 1) {
                pokemonController.type2Icon.enabled = true;
                pokemonController.type2IconBackground.enabled = true;
                pokemonController.type2Icon.sprite = pokemonController.currentPokemon.battleType[1].typeIcon;

            } else {
                pokemonController.type2Icon.enabled = false;
                pokemonController.type2IconBackground.enabled = false;
            }
        }

        if(pokemonController.currentPokemon.currentStatus == StatusEffect.None) {
            pokemonController.statusIcon.enabled = false;

        } else if(pokemonController.currentPokemon.currentStatus == StatusEffect.Attraction) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[0];

        } else if (pokemonController.currentPokemon.currentStatus == StatusEffect.Burn) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[1];

        } else if (pokemonController.currentPokemon.currentStatus == StatusEffect.Confusion) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[2];

        } else if (pokemonController.currentPokemon.currentStatus == StatusEffect.Paralysis) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[3];

        } else if (pokemonController.currentPokemon.currentStatus == StatusEffect.Poison) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[4];

        } else if (pokemonController.currentPokemon.currentStatus == StatusEffect.Sleep) {
            pokemonController.statusIcon.enabled = true;
            pokemonController.statusIcon.sprite = statusIcons[5];
        }

        if (pokemonController.currentPokemon.shadow) {
            pokemonController.pokemonShadowSprite.enabled = true;
        } else {
            pokemonController.pokemonShadowSprite.enabled = false;
        }

        if (pokemonController.currentPokemon.mega) {
            pokemonController.techIcon.enabled = true;
            pokemonController.techIcon.sprite = techIcons[0];

        } else if (pokemonController.currentPokemon.dynamax) {
            pokemonController.techIcon.enabled = true;
            pokemonController.techIcon.sprite = techIcons[1];

        } else if (pokemonController.currentPokemon.tera) {
            pokemonController.techIcon.enabled = true;
            pokemonController.techIcon.sprite = techIcons[2];

        } else {
            pokemonController.techIcon.enabled = false;
            pokemonController.techIcon.sprite = null;
        }
    }
    public void SetPokemonBattleSprite(PokemonController pokemonController, PokemonController opposingPokemonController) {
        var p = new Vector2();

        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto") {
            if (pokemonController.currentPokemon.formChanged) {
                return;
            }
        }
        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zorua") {
            if (pokemonController.currentPokemon.formChanged) {
                return;
            }
        }
        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zoroark") {
            if (pokemonController.currentPokemon.formChanged) {
                return;
            }
        }

        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto" && !pokemonController.currentPokemon.formChanged) {
            p = new Vector2(opposingPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, opposingPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
            pokemonController.currentPokemon.currentPokemonBattleSprite = opposingPokemonController.currentPokemon.currentPokemonBattleSprite;
            pokemonController.pokemonBattleImage.sprite = pokemonController.currentPokemon.currentPokemonBattleSprite;

        } else if ((pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zorua" && !pokemonController.currentPokemon.formChanged) || (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zoroark" && !pokemonController.currentPokemon.formChanged)) {
            p = new Vector2(pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].pokemonBaseInfo.SpriteSize, pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].pokemonBaseInfo.SpriteSize);
            pokemonController.currentPokemon.currentPokemonBattleSprite = pokemonController.pokemonInParty[pokemonController.pokemonInParty.Length - 1].currentPokemonBattleSprite;
            pokemonController.pokemonBattleImage.sprite = pokemonController.currentPokemon.currentPokemonBattleSprite;

        } else if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName != "Ditto" && pokemonController.currentPokemon.pokemonBaseInfo.PokemonName != "Zorua" && pokemonController.currentPokemon.pokemonBaseInfo.PokemonName != "Zoroark")  {
            p = new Vector2(pokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, pokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
            pokemonController.pokemonBattleImage.sprite = pokemonController.currentPokemon.currentPokemonBattleSprite;
        }
        pokemonController.pokemonImageRt.sizeDelta = p;
        pokemonController.shadowImageRt.sizeDelta = new Vector2(p.x * 0.875f, p.y * 0.875f);
    }
    public void SetPlayerChargedMoveHUD(PokemonController playerPokemonController) {

        if (playerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto" && playerPokemonController.currentPokemon.formChanged) return;

        playerChargedMove1NameText.text = playerPokemonController.currentPokemon.chargedMove1.moveName;
        playerEnergyBackground1.sprite = playerPokemonController.currentPokemon.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer1.color = playerPokemonController.currentPokemon.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer1.sprite = playerPokemonController.currentPokemon.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer2.color = playerPokemonController.currentPokemon.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer2.sprite = playerPokemonController.currentPokemon.chargedMove1.moveType.typeIcon;
        playerEnergyImage1layer3.color = playerPokemonController.currentPokemon.chargedMove1.moveType.typeColor;
        playerEnergyImage1layer3.sprite = playerPokemonController.currentPokemon.chargedMove1.moveType.typeIcon;

        if (playerPokemonController.currentPokemon.chargedMove2 != null) {
            chargedMove2RectTransform.gameObject.SetActive(true);
            chargedMove1RectTransform.transform.localPosition = new Vector3(-620, chargedMove1RectTransform.transform.localPosition.y, chargedMove1RectTransform.transform.localPosition.z);
            playerChargedMove2NameText.text = playerPokemonController.currentPokemon.chargedMove2.moveName;
            playerEnergyBackground2.sprite = playerPokemonController.currentPokemon.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer1.color = playerPokemonController.currentPokemon.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer1.sprite = playerPokemonController.currentPokemon.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer2.color = playerPokemonController.currentPokemon.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer2.sprite = playerPokemonController.currentPokemon.chargedMove2.moveType.typeIcon;
            playerEnergyImage2layer3.color = playerPokemonController.currentPokemon.chargedMove2.moveType.typeColor;
            playerEnergyImage2layer3.sprite = playerPokemonController.currentPokemon.chargedMove2.moveType.typeIcon;
        } else {
            chargedMove2RectTransform.gameObject.SetActive(false);
            chargedMove1RectTransform.transform.localPosition = new Vector3(-500, chargedMove1RectTransform.transform.localPosition.y, chargedMove1RectTransform.transform.localPosition.z);
        }
    }
    public void SetDittoTransformInfo(PokemonController pokemonController, PokemonController opposingPokemonController) {

        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto") {
            if (pokemonController.currentPokemon.formChanged) return;
            pokemonController.currentPokemon.currentBuffs = opposingPokemonController.currentPokemon.currentBuffs;
            pokemonController.currentPokemon.shadow = opposingPokemonController.currentPokemon.shadow; // questionable
            pokemonController.currentPokemon.mega = opposingPokemonController.currentPokemon.mega; // questionable
            pokemonController.currentPokemon.fastMove = opposingPokemonController.currentPokemon.fastMove;
            print(opposingPokemonController.currentPokemon.fastMove.moveName);
            pokemonController.currentPokemon.chargedMove1 = opposingPokemonController.currentPokemon.chargedMove1;
            if (opposingPokemonController.currentPokemon.chargedMove2 != null) pokemonController.currentPokemon.chargedMove2 = opposingPokemonController.currentPokemon.chargedMove2;
        }

    }
    public void SetFormChangedBool(PokemonController pokemonController) {
        if (pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Ditto" || pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zorua" || pokemonController.currentPokemon.pokemonBaseInfo.PokemonName == "Zoroark") {

            if (pokemonController.currentPokemon.formChanged) return;
            pokemonController.currentPokemon.formChanged = true;
        }
    }
    public void DisablePokemonHUD(bool playerPokemon) {
        if(playerPokemon) {
            playerHUD.SetActive(false);
        } else {
            aiTrainerHUD.SetActive(false);
        }
    }
    public void UpdateWeatherIcon(WeatherType currentWeather) {
        if (currentWeather == WeatherType.None) {
            weatherIcon.enabled = false;
            weatherTimerIcon.enabled = false;
            weatherIcon.sprite = null;
        } else if (currentWeather == WeatherType.Rain) {
            weatherIcon.enabled = true;
            weatherTimerIcon.enabled = true;
            weatherIcon.sprite = weatherIcons[0];
        } else if (currentWeather == WeatherType.Sun) {
            weatherIcon.enabled = true;
            weatherTimerIcon.enabled = true;
            weatherIcon.sprite = weatherIcons[1];
        } else if (currentWeather == WeatherType.Sandstorm) {
            weatherIcon.enabled = true;
            weatherTimerIcon.enabled = true;
            weatherIcon.sprite = weatherIcons[2];
        } else if (currentWeather == WeatherType.Snow) {
            weatherIcon.enabled = true;
            weatherTimerIcon.enabled = true;
            weatherIcon.sprite = weatherIcons[3];
        }
    }
    public void UpdateTerrainIcon(TerrainType currentTerrain) {
        if (currentTerrain == TerrainType.None) {
            terrainIcon.enabled = false;
            terrainTimerIcon.enabled = false;
            terrainIcon.sprite = null;
        } else if (currentTerrain == TerrainType.Grassy) {
            terrainIcon.enabled = true;
            terrainTimerIcon.enabled = true;
            terrainIcon.sprite = terrainIcons[0];
        } else if (currentTerrain == TerrainType.Electric) {
            terrainIcon.enabled = true;
            terrainTimerIcon.enabled = true;
            terrainIcon.sprite = terrainIcons[1];
        } else if (currentTerrain == TerrainType.Misty) {
            terrainIcon.enabled = true;
            terrainTimerIcon.enabled = true;
            terrainIcon.sprite = terrainIcons[2];
        } else if (currentTerrain == TerrainType.Psychic) {
            terrainIcon.enabled = true;
            terrainTimerIcon.enabled = true;
            terrainIcon.sprite = terrainIcons[3];
        }
    }
    public void UpdateHPBar(PokemonController pokemonController, Image barFillImage) {
        float playerFill = (float)pokemonController.currentPokemon.currentHP / (float)pokemonController.currentPokemon.MaxHP;
        barFillImage.fillAmount = playerFill;

        if (playerFill >= 0.5f) {
            barFillImage.color = Color.green;
        } else if (playerFill < 0.5f && playerFill >= 0.2f) {
            barFillImage.color = Color.yellow;
        } else if (playerFill < 0.2f) {
            barFillImage.color = Color.red;
        }
    }
    public void UpdateHUD(PokemonController playerPokemonController, PokemonController aiTrainerPokemonController) {
        //playerPokemonHPText.text = playerPokemonIndividual.currentHP.ToString();
        //aiTrainerPokemonHPText.text = aiTrainerPokemonIndividual.currentHP.ToString();
        UpdateHPBar(playerPokemonController, playerHPFill);
        UpdateHPBar(aiTrainerPokemonController, aiTrainerHPFill);

        ManageChargedMove1Fill(playerPokemonController.currentPokemon);

        if (playerPokemonController.currentPokemon.chargedMove2 != null) {
            ManageChargedMove2Fill(playerPokemonController.currentPokemon);
        }
    }
    public void ManageChargedMove1Fill(PokemonIndividual playerPokemonIndividual) {
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
    public void ManageChargedMove2Fill(PokemonIndividual playerPokemonIndividual) {
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
    public IEnumerator TextTimer(TMP_Text text, string effectiveness, float timer) {
        text.text = effectiveness;
        text.enabled = true;
        yield return new WaitForSeconds(timer);
        text.text = null;
        text.enabled = false;
    }
    public IEnumerator BuffTextTimer(TMP_Text text, string buff) {
        text.text = buff;
        text.enabled = true;
        yield return new WaitForSeconds(3f);
        text.text = null;
        text.enabled = false;
    }
    public IEnumerator SwitchCountdownTimer(Image timerImage, PokemonController pokemonController) {
        yield return new WaitForSeconds(0.1f);
        timerImage.fillAmount = pokemonController.switchTimer / 10f;
        pokemonController.switchTimer -= 0.1f;
        if (pokemonController.switchTimer > 0) {
            StartCoroutine(SwitchCountdownTimer(timerImage, pokemonController));
        }
    }
}