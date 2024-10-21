using UnityEngine;
[CreateAssetMenu(fileName = "New Charged Move", menuName = "Pokemon/Charged Move", order = 102)]
public class ChargedMove : PokemonMove {

    public int baseEnergyReq;
    public BuffLevel OwnBuff1;
    public float ownBuff1Chances;
    public BuffLevel OwnBuff2;
    public float ownBuff2Chances;
    public BuffLevel OppBuff1;
    public float oppBuff1Chances;
    public BuffLevel OppBuff2;
    public float oppBuff2Chances;

    public BuffLevel OwnDebuff1;
    public float ownDebuff1Chances;
    public BuffLevel OwnDebuff2;
    public float ownDebuff2Chances;
    public BuffLevel OppDebuff1;
    public float oppDebuff1Chances;
    public BuffLevel OppDebuff2;
    public float oppDebuff2Chances;


    //public MoveEffect[] moveEffects;
    //public float[] moveEffectChances;

    public float MoveInteractionsCheck(WeatherType currentWeather, TerrainType currentTerrain, StatusEffect ownStatus, StatusEffect oppStatus)
    {
        float mult = 1;

        return mult;
    }
}