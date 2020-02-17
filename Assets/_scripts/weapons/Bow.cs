using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : Weapon 
{
    public Arrow arrow;

    public Arrow SpawnArrow(Transform spawnPoint) {
        return arrow.SpawnArrow(this, spawnPoint);
    }

    new void Awake() {
        base.Awake();

        _weaponType = "BOW";
        _damageType = "PHYSICAL";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
