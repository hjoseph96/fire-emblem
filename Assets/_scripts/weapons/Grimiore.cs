using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Grimiore : Weapon
{
    public string magicType;    // BLACK || WHITE
    public string element;      // WIND, EARTH, FIRE, WATER, THUNDER, DARK, LIGHT
    public int healthCost = 0; 
    public GameObject castingCircle;
    public MagicEffect magicEffect;


    public UnityEvent OnCastingCircleSpawn;
    public UnityEvent OnParticleEffectSpawn;
    public UnityEvent OnParticleEffectHalfway;
    public UnityEvent OnParticleEffectLanded;

    protected Transform target;
    protected GameObject castingCircleInstance;
    protected bool castingCircleSpawned = false;

    new void Awake() {
        base.Awake();
     
        STATS["HP_COST"] = new Stat(healthCost);

        _weaponType = "GRIMIORE";
        _damageType = "MAGIC";
    }

    public void SpawnCastingCircle(GameObject spawnPoint) {
        if (!castingCircleSpawned) {
            Vector3 spawnPosition = spawnPoint.transform.position;
            castingCircleInstance = Instantiate(castingCircle, spawnPosition, Quaternion.Euler(0, 0, 0), spawnPoint.transform);

            castingCircleSpawned = true;
            
            OnCastingCircleSpawn.Invoke();
        }
    }

    public void DestroyCastingCircle(float delay = 0.2f) {
        if (castingCircleSpawned) {
            Destroy(castingCircleInstance, delay);
            castingCircleSpawned = false;
        }
    }
}
