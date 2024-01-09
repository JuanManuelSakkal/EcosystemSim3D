using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeerController : AnimalController
{
 
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        maxSpeed = 9f;
        FieldOfView fov = gameObject.GetComponent<FieldOfView>();
        fov.viewRadius = 45f;
        fov.viewAngle = 100f;
        wanderSpeed = 3f;
        
    }
    override protected bool IsHungry(){
        return hunger > maxHunger / 3;
    }
    
    protected override void Hunt()
    {
        AccelerateToSpeed(maxSpeed/2);
        GoToTarget();
    }
    override protected void TryToMate(){

    }
    override protected void HaveChildren(){

    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.gameObject == target && foodLayerMask.Includes(collisionInfo.gameObject.layer)){
            Eat(collisionInfo.gameObject);
        }
    } 
}
