using UnityEngine;
public abstract class PredatorController : AnimalController
{  
    override protected void Hunt(){
        AccelerateToSpeed(maxSpeed);
        GoToTarget();
    }

    void OnCollisionEnter(Collision collisionInfo)
    {   
        if (collisionInfo.gameObject == target && foodLayerMask.Includes(collisionInfo.gameObject.layer)){
            AnimalController prey = collisionInfo.gameObject.GetComponent<AnimalController>();
            Stop();
            if (prey && prey.state != AnimalState.Dead){
                collisionInfo.gameObject.GetComponent<AnimalController>()?.Die();
            } else {
                Eat(collisionInfo.gameObject);
            }
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
