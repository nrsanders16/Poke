using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HUDManager : MonoBehaviour {

    public Image weatherIcon;
    public Image weatherTimerIcon;
    public Image terrainIcon;
    public Image terrainTimerIcon;
    public TMP_Text chargedMoveText;

    [Header("Player HUD")]
    //public Sprite currentPlayerPokemonSprite;
    public GameObject playerHUD;
    public RectTransform chargedMove1RectTransform;
    public RectTransform chargedMove2RectTransform;
    public TMP_Text playerPokemonText;
    public Image playerPokemonShadowSprite;
    public GameObject playerHPBar;
    public Image playerHPFill;
    public TMP_Text playerPokemonBPText;
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
    public Image playerTechIcon;
    public Image[] playerPartyPokemonSprites;
    public Image[] playerPartyPokemonTimerImages;
    public Image[] playerPartyPokemonShadowSprites;

    [Header("AI Trainer HUD")]
    public GameObject aiTrainerHUD;
    public TMP_Text aiTrainerPokemonText;
    //public Sprite currentAITrainerPokemonSprite;
    //public TMP_Text aiTrainerPokemonHPText;
    public TMP_Text aiTrainerPokemonBPText;
    public TMP_Text aiTrainerPokemonEffectivenessText;
    public Image aiTrainerSwitchTimerImage;
    public Image aiTrainerPokemonShadowSprite;
    public GameObject aiTrainerHPBar;
    public Image aiTrainerHPFill;
    public Image aiTechIcon;

    public Sprite[] techIcons;
    public Sprite[] weatherIcons;
    public Sprite[] terrainIcons;

    public void SetHUD(PokemonController playerPokemonController, PokemonController aiTrainerPokemonController) {
        playerPokemonText.text = playerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName;
        aiTrainerPokemonText.text = aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.PokemonName;

        playerPokemonBPText.text = "BP " + playerPokemonController.currentPokemon.BattlePower.ToString();
        aiTrainerPokemonBPText.text = "BP " + aiTrainerPokemonController.currentPokemon.BattlePower.ToString();

        var p = new Vector2(playerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, playerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
        playerPokemonController.pokemonImageRt.sizeDelta = p;
        playerPokemonController.shadowImageRt.sizeDelta = new Vector2(p.x * 0.875f, p.y * 0.875f);
        playerPokemonController.pokemonBattleImage.sprite = playerPokemonController.currentPokemon.currentPokemonBattleSprite;

        var a = new Vector2(aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize, aiTrainerPokemonController.currentPokemon.pokemonBaseInfo.SpriteSize);
        aiTrainerPokemonController.pokemonImageRt.sizeDelta = a;
        aiTrainerPokemonController.shadowImageRt.sizeDelta = new Vector2(a.x * 0.875f, a.y * 0.875f);
        aiTrainerPokemonController.pokemonBattleImage.sprite = aiTrainerPokemonController.currentPokemon.currentPokemonBattleSprite;

        if(playerPokemonController.currentPokemon.shadow) {
            playerPokemonShadowSprite.enabled = true;
        } else {
            playerPokemonShadowSprite.enabled = false;
        }
        if (aiTrainerPokemonController.currentPokemon.shadow) {
            aiTrainerPokemonShadowSprite.enabled = true;
        } else {
            aiTrainerPokemonShadowSprite.enabled = false;
        }

        if (playerPokemonController.currentPokemon.mega) {
            playerTechIcon.enabled = true;
            playerTechIcon.sprite = techIcons[0];

        } else if (playerPokemonController.currentPokemon.dynamax) { 
            playerTechIcon.enabled = true;
            playerTechIcon.sprite = techIcons[1];

        } else if (playerPokemonController.currentPokemon.tera) {
            playerTechIcon.enabled = true;
            playerTechIcon.sprite = techIcons[2];

        } else {
            playerTechIcon.enabled = false;
            playerTechIcon.sprite = null;
        }

        if (aiTrainerPokemonController.currentPokemon.mega) {
            aiTechIcon.enabled = true;
            aiTechIcon.sprite = techIcons[0];

        } else if (aiTrainerPokemonController.currentPokemon.dynamax) {
            aiTechIcon.enabled = true;
            aiTechIcon.sprite = techIcons[1];

        } else if (aiTrainerPokemonController.currentPokemon.tera) {
            aiTechIcon.enabled = true;
            aiTechIcon.sprite = techIcons[2];

        } else {
            aiTechIcon.enabled = false;
            aiTechIcon.sprite = null;
        }

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
    public void ClearPokemonHUD(bool playerPokemon) {
        if(playerPokemon) {
            //playerPokemonText.enabled = false;
            //playerPokemonBPText.enabled = false;
            playerHUD.SetActive(false);
        } else {
            //aiTrainerPokemonText.enabled = false;
            //aiTrainerPokemonBPText.enabled = false;
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