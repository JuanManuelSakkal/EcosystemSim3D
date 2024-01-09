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
            if (prey.state == AnimalState.Dead){
                Eat(collisionInfo.gameObject);
                target = null;
            } else {
                Stop();
                collisionInfo.gameObject.GetComponent<AnimalController>()?.Die();
                target = null;
            }
        }
        if(collisionInfo.gameObject == target && mateLayerMask.Includes(collisionInfo.gameObject.layer)){
            Mate(collisionInfo.gameObject);
        }
    }
}
