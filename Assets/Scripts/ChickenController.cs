using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : AnimalController
{
    new void Start()
    {
        base.Start();
        maxSpeed = 5f;
        FieldOfView fov = gameObject.GetComponent<FieldOfView>();
        fov.viewRadius = 15f;
        fov.viewAngle = 100f;
    }
    override protected bool IsHungry(){
        return hunger > maxHunger / 3;
    }

    protected override void Hunt()
    {
        AccelerateToSpeed(maxSpeed * urgencyLevel);
        GoToTarget();
    }
    
    void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.gameObject == target && foodLayerMask.Includes(collisionInfo.gameObject.layer)){
            Eat(collisionInfo.gameObject);
        }
    } 
}
