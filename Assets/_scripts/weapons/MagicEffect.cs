using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MagicEffect : MonoBehaviour
{
    // TODO: Unity Editor Label: Magic Projectile, Area Effect, Land On Target
    public string particleType;
    public Grimiore grimiore;

    public UnityEvent OnEffectSpawn;
    public UnityEvent OnBeforeEffectHit;
    public UnityEvent OnEffectHit;
    public static List<string> MAGIC_TYPES = new List<string>{ 
        "Magic Projectile", "Area Effect", "Magic On Target" 
    }; 
    
    protected static bool _movingToTarget;
    protected static bool _halfway;
    protected static bool _hitTarget;

    public void Reset() {
        _halfway = false;
        _hitTarget = false;
        _movingToTarget = false;
    }
}
