using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
public enum Ability {
    None,
    Intimidate,
    FlameBody,
    EffectSpore,
    Justified,
    SapSipper,
    StormDrain,
    LightningRod,
    EarthEater,
    Scrappy,
    Levitate
}
public static class BattleCalculations {
    public static Vector2 CalculateAttackDamage(PokemonIndividual attackingPokemon, PokemonIndividual defendingPokemon, PokemonMove pokemonMove) {
        float mult = 1;
        bool attackerHasScrappy = false;
        bool gravityInEffect = false;
        bool defendingPokemonHasLevitate = false;
        bool scrappyAppliesPrimary = attackerHasScrappy && defendingPokemon.battleType[0].typeName == TypeName.Ghost && (pokemonMove.moveType.typeName == TypeName.Normal || pokemonMove.moveType.typeName == TypeName.Fighting);
        bool gravityAppliesPrimary = gravityInEffect && defendingPokemon.battleType[0].typeName == TypeName.Flying && pokemonMove.moveType.typeName == TypeName.Ground;

        if (scrappyAppliesPrimary) {
            mult = 1;

        } else if (gravityAppliesPrimary) {
            mult = 1;

        } else if (scrappyAppliesPrimary && gravityAppliesPrimary) {
            mult = 1;

        } else {
            mult = CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.battleType[0]);
        }

        if (defendingPokemon.battleType.Length > 1) {
            bool scrappyAppliesSecondary = attackerHasScrappy && defendingPokemon.battleType[1].typeName == TypeName.Ghost && (pokemonMove.moveType.typeName == TypeName.Normal || pokemonMove.moveType.typeName == TypeName.Fighting);
            bool gravityAppliesSecondary = gravityInEffect && defendingPokemon.battleType[1].typeName == TypeName.Flying && pokemonMove.moveType.typeName == TypeName.Ground;

            if (scrappyAppliesSecondary) {
                mult *= 1;

            } else if (gravityAppliesSecondary) {
                mult *= 1;

            } else if (gravityAppliesSecondary && scrappyAppliesSecondary) {
                mult *= 1;

            } else {
                mult *= CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.battleType[1]);
            }
        }

        if (defendingPokemonHasLevitate && pokemonMove.moveType.typeName == TypeName.Ground) {
            mult *= 0.5f;
        }

        float typeMult = mult;

        float stab = 1.2f;
        if (attackingPokemon.battleType[0].typeName == pokemonMove.moveType.typeName) {
            mult *= stab;
        }
        if (attackingPokemon.battleType.Length > 1) {
            if (attackingPokemon.battleType[1].typeName == pokemonMove.moveType.typeName) mult *= stab;
        }

        float finalDamage = 0;
        float shadowBoost = 1f;
        float shadowPenalty = 1f;
        if (attackingPokemon.shadow) shadowBoost = 1.2f;
        if (defendingPokemon.shadow) shadowPenalty = 0.8f; 

        if (pokemonMove.attackType == AttackType.Physical) {
            finalDamage = (0.64f * (pokemonMove.baseDamage * shadowBoost) * (attackingPokemon.Attack * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[0])) / ((defendingPokemon.Defense * ConvertBuffLevelToMultiplier(defendingPokemon.currentBuffs[1])) * shadowPenalty) * mult) + 1;

        } else if (pokemonMove.attackType == AttackType.Special) {
            finalDamage = (0.64f * (pokemonMove.baseDamage * shadowBoost) * (attackingPokemon.SpecialAttack * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[2])) / ((defendingPokemon.SpecialDefense * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[3])) * shadowPenalty) * mult) + 1;

        } else { // STATUS MOVES
            finalDamage = 0;
        }

        Vector2 final = new Vector2(finalDamage, typeMult);

        return final;
     
    }
    public static float CalculateWeatherMultiplier(PokemonMove pokemonMove, WeatherType currentWeather) {
        float mult = 1;
        if (currentWeather == WeatherType.Rain) {
            if (pokemonMove.moveType.typeName == TypeName.Water) {
                mult = 1.1f;
            } else if (pokemonMove.moveType.typeName == TypeName.Fire) {
                mult = 0.9f;
            }
        } else if (currentWeather == WeatherType.Sun) {
            if (pokemonMove.moveType.typeName == TypeName.Water) {
                mult = 0.9f;
            } else if (pokemonMove.moveType.typeName == TypeName.Fire) {
                mult = 1.1f;
            }
        } else if (currentWeather == WeatherType.Sandstorm) {

        } else if (currentWeather == WeatherType.Snow) {

        }
        return mult;
    }
    public static float CalculateTerrainMultiplier(PokemonMove pokemonMove, TerrainType currentTerrain) {
        float mult = 1;
        if (currentTerrain == TerrainType.Electric) {
            if (pokemonMove.moveType.typeName == TypeName.Electric) {
                mult = 1.1f;
            }
        } else if (currentTerrain == TerrainType.Grassy) {
            if (pokemonMove.moveType.typeName == TypeName.Grass) {
                mult = 1.1f;
            }
        } else if (currentTerrain == TerrainType.Misty) {
            if (pokemonMove.moveType.typeName == TypeName.Fairy) {
                mult = 1.1f;
            } else if (pokemonMove.moveType.typeName == TypeName.Fairy) {
                mult = 0.9f;
            }
        } else if (currentTerrain == TerrainType.Psychic) {
            if (pokemonMove.moveType.typeName == TypeName.Psychic) {
                mult = 1.1f;
            }
        }
        return mult;
    }
    public static float ConvertBuffLevelToMultiplier(float buffLevel) {
        switch (buffLevel) {
            case 4:
                return 1.8f;
            case 3.5f:
                return 1.7f;
            case 3:
                return 1.6f;
            case 2.5f:
                return 1.5f;
            case 2:
                return 1.4f;
            case 1.5f:
                return 1.3f;
            case 1:
                return 1.2f;
            case 0.5f:
                return 1.1f;
            case 0:
                return 1;
            case -0.5f:
                return 0.95f;
            case -1:
                return 0.9f;
            case -1.5f:
                return 0.85f;
            case -2:
                return 0.8f;
            case -2.5f:
                return 0.75f;
            case -3:
                return 0.7f;
            case -3.5f:
                return 0.65f;
            case -4f:
                return 0.6f;
            default:
                return 1;
        }
    }
    public static string ConvertMultiplierToEffectivenessString(float effectivenessMultiplier) {
        if (effectivenessMultiplier > 1) {
            return "Super Effective!";
        } else if (effectivenessMultiplier < 1) {
            return "Not very effective...";
        } else {
            return "";
        }
    }
    public static float CheckForAttackBuffsAndDebuffs(AttackType attackType, PokemonIndividual pokemonToCheck) {
        if (attackType == AttackType.Physical) {
            return 1f;

        } else if (attackType == AttackType.Special) {
            return 1f;

        } else {
            return 1f;
        }
    }
    public static float CheckForDefenseBuffsAndDebuffs(AttackType attackType, PokemonIndividual pokemonToCheck) {
        if (attackType == AttackType.Physical) {
            return 1f;

        } else if (attackType == AttackType.Special) {
            return 1f;

        } else {
            return 1f;
        }
    }
    public static bool CalculateChargedMovePriority(PokemonIndividual playerPokemon, PokemonIndividual aiTrainerPokemon) {
        bool doesPlayerPokemonGoFirst = false;

        if (playerPokemon.Speed > aiTrainerPokemon.Speed) {
            doesPlayerPokemonGoFirst = true;

        } else if (playerPokemon.Speed == aiTrainerPokemon.Speed) {

            float coinFlip = Random.Range(0, 1f);

            if (coinFlip < 0.5f) doesPlayerPokemonGoFirst = true;
        }

        return doesPlayerPokemonGoFirst;
    }
    public static float CalculateTypeEffectiveness(TypeName attackType, Type defensiveType) {
        float eff = 1;
        if (defensiveType.weaknesses.Contains(attackType)) {
            eff *= 1.6f;
        } else if (defensiveType.resistances.Contains(attackType)) {
            eff *= 0.625f;
        } else if (defensiveType.immunities.Contains(attackType)) {
            eff *= 0.391f;
        }
        return eff;
    }
    public static float CalculateAbilityMultipliers(PokemonIndividual attackingPokemon, PokemonIndividual defendingPokemon, PokemonMove pokemonMove) {
        float eff = 1;
        return eff;
    }
}