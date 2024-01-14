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
        foodLayerMask = GetFoodLayerMask();
        
    }
    
    LayerMask GetFoodLayerMask(){
        if(age >= mateAge){
            return LayerMask.GetMask("Eggs", "Chick", "ChickenF", "ChickenM");
        } else {
            return LayerMask.GetMask("Eggs", "Chick");
        }
    }
    override protected bool IsHungry(){
        return hunger > maxHunger / 3;

    }
    override protected void TryToMate(){
        AccelerateToSpeed(maxSpeed * urgencyLevel);
        GoToTarget();
    }
    private void InitializeFox(Vector3 position){
        Gender randomGender = Random.Range(0, 1) > 0.75f ? Gender.Female : Gender.Male;
        GameObject modelToLoad;
        if(randomGender == Gender.Female){
            modelToLoad = Resources.Load("FoxFemaleChild") as GameObject;
        } else {
            modelToLoad = Resources.Load("FoxMaleChild") as GameObject;
        }
        GameObject newFox = Instantiate(modelToLoad, position, Quaternion.identity);
        newFox.transform.parent = transform.parent;
        newFox.GetComponent<FoxController>().age = 0;
        newFox.GetComponent<FoxController>().gender = randomGender;

    }
    
    override protected void HaveChildren(){
        InitializeFox(transform.position + Vector3.right);
        InitializeFox(transform.position - Vector3.right);

    }
    override protected void GrowUp(){
        GameObject modelToLoad;
        LayerMask newMateLayerMask;
        if(gender == Gender.Female){
            modelToLoad = Resources.Load("FoxFemale") as GameObject;
            newMateLayerMask = LayerMask.GetMask("FoxM");
        } else {
            modelToLoad = Resources.Load("FoxMale") as GameObject;
            newMateLayerMask = LayerMask.GetMask("FoxF");
        }

        Destroy(gameObject);
        GameObject newFox = Instantiate(modelToLoad, transform.position, Quaternion.identity);
        newFox.transform.parent = transform.parent;
        newFox.GetComponent<FoxController>().hunger = hunger;
        newFox.GetComponent<FoxController>().thirst = thirst;
        newFox.GetComponent<FoxController>().energy = energy;
        newFox.GetComponent<FoxController>().age = age;
        newFox.GetComponent<FoxController>().mateLayerMask = newMateLayerMask;
        newFox.GetComponent<FoxController>().foodLayerMask = GetFoodLayerMask();

    }
}
