using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Staff : Weapon
{
    new void Awake() {
        base.Awake();

        _weaponType = "STAFF";
        _damageType = "MAGIC";
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
