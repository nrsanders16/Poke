using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AttackType {
    Physical,
    Special,
    Status
}
public class PokemonMove : ScriptableObject {
    public string moveName;
    public int baseDamage;
    public Type moveType;
    public AttackType attackType;
    public AnimationClip[] animationClips;
}
