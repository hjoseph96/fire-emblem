using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TGS;

public class OtherEntity : Entity
{   
    

    // Start is called before the first frame update
    void Start()
    {
        entityType = "Other";
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