using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonMove : ScriptableObject {
    public string moveName;
    public int baseDamage;
    public Type moveType;
    public AnimationClip[] animationClips;
}
