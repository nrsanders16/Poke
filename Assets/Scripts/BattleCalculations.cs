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
    public static float CalculateAttackDamage(PokemonIndividual attackingPokemon, PokemonIndividual defendingPokemon, PokemonMove pokemonMove) {
        float mult = 1;
        bool attackerHasScrappy = false;
        bool gravityInEffect = false;
        bool defendingPokemonHasLevitate = false;
        bool scrappyAppliesPrimary = attackerHasScrappy && defendingPokemon.pokemonBaseInfo.PrimaryType.typeName == TypeName.Ghost && (pokemonMove.moveType.typeName == TypeName.Normal || pokemonMove.moveType.typeName == TypeName.Fighting);
        bool gravityAppliesPrimary = gravityInEffect && defendingPokemon.pokemonBaseInfo.PrimaryType.typeName == TypeName.Flying && pokemonMove.moveType.typeName == TypeName.Ground;

        if (scrappyAppliesPrimary) {
            mult = 1;

        } else if (gravityAppliesPrimary) {
            mult = 1;

        } else if (scrappyAppliesPrimary && gravityAppliesPrimary) {
            mult = 1;

        } else {
            mult = CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.pokemonBaseInfo.PrimaryType);
        }

        if (defendingPokemon.pokemonBaseInfo.SecondaryType) {
            bool scrappyAppliesSecondary = attackerHasScrappy && defendingPokemon.pokemonBaseInfo.SecondaryType.typeName == TypeName.Ghost && (pokemonMove.moveType.typeName == TypeName.Normal || pokemonMove.moveType.typeName == TypeName.Fighting);
            bool gravityAppliesSecondary = gravityInEffect && defendingPokemon.pokemonBaseInfo.SecondaryType.typeName == TypeName.Flying && pokemonMove.moveType.typeName == TypeName.Ground;

            if (scrappyAppliesSecondary) {
                mult *= 1;

            } else if (gravityAppliesSecondary) {
                mult *= 1;

            } else if (gravityAppliesSecondary && scrappyAppliesSecondary) {
                mult *= 1;

            } else {
                mult *= CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.pokemonBaseInfo.SecondaryType);
            }
        }

        if (defendingPokemonHasLevitate && pokemonMove.moveType.typeName == TypeName.Ground) {
            mult *= 0.5f;
        }

        float stab = 1.2f;
        if (attackingPokemon.pokemonBaseInfo.PrimaryType.typeName == pokemonMove.moveType.typeName) {
            mult *= stab;
        }
        if (attackingPokemon.pokemonBaseInfo.SecondaryType != null && attackingPokemon.pokemonBaseInfo.SecondaryType.typeName == pokemonMove.moveType.typeName) {
            mult *= stab;
        }

        if (pokemonMove.attackType == AttackType.Physical) {
            return (0.64f * pokemonMove.baseDamage * (attackingPokemon.Attack * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[0])) / (defendingPokemon.Defense * ConvertBuffLevelToMultiplier(defendingPokemon.currentBuffs[1])) * mult) + 1;

        } else if (pokemonMove.attackType == AttackType.Special) {
            return (0.64f * pokemonMove.baseDamage * (attackingPokemon.SpecialAttack * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[2])) / (defendingPokemon.SpecialDefense * ConvertBuffLevelToMultiplier(attackingPokemon.currentBuffs[3])) * mult) + 1;

        } else { // STATUS MOVES
            return 0;
        }
     
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