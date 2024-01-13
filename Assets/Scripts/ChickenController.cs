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
        energyRate = 1f;
        wanderSpeed = 1.5f;
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
        if(collisionInfo.gameObject == target && mateLayerMask.Includes(collisionInfo.gameObject.layer)){
            Mate(collisionInfo.gameObject);
            target = null;
        }
        if(collisionInfo.gameObject == target && LayerMask.NameToLayer("Water") == collisionInfo.gameObject.layer){
            Drink();
            target = null;
        }
        
    } 

    protected override void TryToMate(){
        AccelerateToSpeed(maxSpeed * urgencyLevel);
        GoToTarget();
    }

    protected override void HaveChildren()
    {
        LayEggs();
    }

    void LayEggs(){
        Instantiate(Resources.Load("Eggs") as GameObject, new Vector3(transform.position.x, transform.position.y, transform.position.z -1), Quaternion.identity);
    }

    protected override void GrowUp()
    {
        GameObject modelToLoad;
        if(gender == Gender.Female){
            modelToLoad = Random.Range(0, 2) > 1 ? Resources.Load("ChickenBrown") as GameObject : Resources.Load("ChickenWhite") as GameObject;
        } else {
            modelToLoad = Resources.Load("RoosterBrown") as GameObject;
        }

        Destroy(gameObject);
        GameObject newChicken = Instantiate(modelToLoad, transform.position, Quaternion.identity);
        newChicken.transform.parent = transform.parent;
        newChicken.GetComponent<ChickenController>().hunger = hunger;
        newChicken.GetComponent<ChickenController>().thirst = thirst;
        newChicken.GetComponent<ChickenController>().energy = energy;
        newChicken.GetComponent<ChickenController>().age = age;
    }
}
