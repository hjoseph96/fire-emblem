using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : Weapon
{
    new void Awake() {
        base.Awake();
        
        _weaponType = "SWORD";
        _damageType = "PHYSICAL";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
