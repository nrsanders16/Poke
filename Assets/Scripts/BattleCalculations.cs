using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            mult = BattleCalculations.CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.pokemonBaseInfo.PrimaryType);
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
                mult *= BattleCalculations.CalculateTypeEffectiveness(pokemonMove.moveType.typeName, defendingPokemon.pokemonBaseInfo.SecondaryType);
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

        return (0.65f * pokemonMove.baseDamage * attackingPokemon.Attack / defendingPokemon.Defense * mult) + 1;
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