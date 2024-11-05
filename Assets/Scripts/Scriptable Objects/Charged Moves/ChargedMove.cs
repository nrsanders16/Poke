using UnityEngine;
[CreateAssetMenu(fileName = "New Charged Move", menuName = "Pokemon/Charged Move", order = 102)]
public class ChargedMove : PokemonMove {

    public int baseEnergyReq;

    public float[] ownBuffs;
    public float ownBuffChance;
    public float[] oppBuffs;
    public float oppBuffChance;
    public StatusEffect moveEffect;
    public float moveEffectChance;
    public WeatherType weatherEffect;
    public TerrainType terrainEffect;

    public float MoveInteractionsCheck(WeatherType currentWeather, TerrainType currentTerrain, StatusEffect ownStatus, StatusEffect oppStatus) {
        float mult = 1;

        return mult;
    }
}