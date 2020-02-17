using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

public class PlayerEntity : Entity
{
    public float experiencePoints;
    public float expDividend;
    public Battler battler;
        
    new void Awake() {
        base.Awake();

        entityType = "player";
        healthPoints = BASE_STATS["HEALTH"].CalculateValue();
    }

    // Start is called before the first frame update
    void Start()
    {
        moveSpeed = 6f;
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
