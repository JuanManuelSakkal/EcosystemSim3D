using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoxController : PredatorController
{  
    new void Start()
    {
        base.Start();
        maxSpeed = 10f;
        FieldOfView fov = gameObject.GetComponent<FieldOfView>();
        fov.viewRadius = 45f;
        fov.viewAngle = 100f;
        
    }
    override protected bool IsHungry(){
        return hunger > maxHunger / 3;

    }

    override protected void TryToMate(){

    }
    
    override protected void HaveChildren(){

    }
}
