using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class WolfController : PredatorController
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
        return hunger > maxHunger / 2;
    }
    override protected void TryToMate(){

    }
    
    override protected void HaveChildren(){

    }
}
