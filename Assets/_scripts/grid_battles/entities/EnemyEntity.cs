using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

public class EnemyEntity : Entity
{   
    public Battler battler;
    public bool rangeDisplayed;

    new void Awake() {
        base.Awake();

        entityType = "enemy";
        healthPoints = BASE_STATS["HEALTH"].CalculateValue();
    }

    // Start is called before the first frame update
    void Start()
    {
        tgs = TerrainGridSystem.instance;
        animator = this.gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {   
        if (movePath.Count > 0)
            TraverseCells();
    }    
}