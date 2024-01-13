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
        AccelerateToSpeed(maxSpeed * urgencyLevel);
        GoToTarget();

    }
    private void InitializeDeer(GameObject newDeer){
        newDeer.transform.parent = transform.parent;
        newDeer.GetComponent<DeerController>().gender = Random.Range(0, 1) > 0.75f ? Gender.Female : Gender.Male;
        newDeer.GetComponent<DeerController>().age = 0;

    }
    override protected void HaveChildren(){
        GameObject newDeer = Instantiate(Resources.Load("DeerChild") as GameObject, new Vector3(transform.position.x, transform.position.y, transform.position.z -1), Quaternion.identity);
        InitializeDeer(newDeer);
    }
    override protected void GrowUp(){
        GameObject modelToLoad;
        if(gender == Gender.Female){
            modelToLoad = Resources.Load("DeerFemale") as GameObject;
        } else {
            modelToLoad = Resources.Load("DeerfMale") as GameObject;
        }

        Destroy(gameObject);
        GameObject newChicken = Instantiate(modelToLoad, transform.position, Quaternion.identity);
        newChicken.transform.parent = transform.parent;
        newChicken.GetComponent<DeerController>().hunger = hunger;
        newChicken.GetComponent<DeerController>().thirst = thirst;
        newChicken.GetComponent<DeerController>().energy = energy;
        newChicken.GetComponent<DeerController>().age = age;

    }
    void OnCollisionEnter(Collision collisionInfo)
    {
        if(collisionInfo.gameObject == target && foodLayerMask.Includes(collisionInfo.gameObject.layer)){
            Eat(collisionInfo.gameObject);
            target = null;
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
}
