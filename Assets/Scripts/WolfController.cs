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
        foodLayerMask = GetFoodLayerMask();
    }
    override protected bool IsHungry(){
        return hunger > maxHunger / 3;
    }
    override protected void TryToMate(){
        AccelerateToSpeed(maxSpeed * urgencyLevel);
        GoToTarget();

    }

    LayerMask GetFoodLayerMask(){
        if(age >= mateAge){
            return LayerMask.GetMask("Eggs", "Chick", "ChickenF", "ChickenM", "DeerChild", "DeerM", "DeerF");
        } else {
            return LayerMask.GetMask("Eggs", "Chick", "ChickenF", "ChickenM");
        }
    }

    private void InitializeWolf(GameObject newWolf){
        newWolf.transform.parent = transform.parent;
        newWolf.GetComponent<WolfController>().gender = Random.Range(0, 1) > 0.75f ? Gender.Female : Gender.Male;
        newWolf.GetComponent<WolfController>().age = 0;

    }
    
    override protected void HaveChildren(){
        GameObject newWolf1 = Instantiate(Resources.Load("WolfChild") as GameObject, new Vector3(transform.position.x - 1, transform.position.y, transform.position.z), Quaternion.identity);
        GameObject newWolf2 = Instantiate(Resources.Load("WolfChild") as GameObject, new Vector3(transform.position.x + 1, transform.position.y, transform.position.z), Quaternion.identity);
        InitializeWolf(newWolf1);
        InitializeWolf(newWolf2);

    }
    override protected void GrowUp(){
        GameObject modelToLoad;
        LayerMask newMateLayerMask;
        if(gender == Gender.Female){
            modelToLoad = Resources.Load("WolfFemale") as GameObject;
            newMateLayerMask = LayerMask.GetMask("WolfM");
        } else {
            modelToLoad = Resources.Load("WolfMale") as GameObject;
            newMateLayerMask = LayerMask.GetMask("WolfF");
        }

        Destroy(gameObject);
        GameObject newWolf = Instantiate(modelToLoad, transform.position, Quaternion.identity);
        newWolf.transform.parent = transform.parent;
        newWolf.GetComponent<WolfController>().hunger = hunger;
        newWolf.GetComponent<WolfController>().thirst = thirst;
        newWolf.GetComponent<WolfController>().energy = energy;
        newWolf.GetComponent<WolfController>().age = age;
        newWolf.GetComponent<WolfController>().mateLayerMask = newMateLayerMask;
        newWolf.GetComponent<WolfController>().foodLayerMask = GetFoodLayerMask();    
    }
}
