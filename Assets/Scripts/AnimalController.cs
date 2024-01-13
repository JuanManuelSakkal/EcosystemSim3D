using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public enum AnimalState
{
    Wandering,
    Resting,
    Eating,
    LookingForFood,
    LookingForWater,
    LookingForMate,
    GoingToTarget,
    Mating,
    Hunting,
    Drinking,
    Fleeing,
    Dead
}

public enum Needs
{
    Food,
    Water,
    Rest,
    Mate,
    Escape,
    None
}

public enum Gender
{
    Male,
    Female
}

public abstract class AnimalController : MonoBehaviour
{

    //life attributes
    public float life = 10f;
    protected float maxLife = 10f;
    public float hunger = 0f;
    protected float maxHunger = 10f;
    public float thirst = 0f;
    protected float maxThirst = 10f;
    public float energy = 10f;
    protected float maxEnergy = 10f;
    public float desireToMate = 0f;
    protected float maxDesireToMate = 5f;

    //individual characteristics
    protected float hungerRate = 0.4f; //0 to 1
    protected float thirstRate = 0.1f; //0 to 1
    protected float energyRate = 0.1f; //0 to 1
    protected float matingRate = 0.1f; // 1 to 10
    public Gender gender;
    public float age = 5.5f;
    public float lifeExpectancy = 15;
    public float mateAge = 5f;
    public bool pregnant = false;
    public float pregnancyPeriod = 20f;

    public LayerMask foodLayerMask;
    public LayerMask mateLayerMask;
    public AnimalState state = AnimalState.Wandering;
    public Needs priorityNeed = Needs.None;

    //movement
    public float maxSpeed = 5f;
    public float wanderSpeed = 1.5f;
    public float speed = 0f;
    public Vector3 velocity = Vector3.zero;
    public float wanderStrength = 0.05f;
    public float steerStrength = 0.1f;
    public float deltaSpeed = 1f;
    private Vector3 desiredDirection;
    public float accelerationStrength = 1f;

    public float urgencyLevel = 0.5f;
    public float fearLevel = 0f;
    
    [HideInInspector]
    public Vector3 escapeRoute = Vector3.zero;

    public GameObject target;
   
    public float energyLog;
    [HideInInspector]
    public Animator animator;

    private float nonDivideByZeroConst = 0.01f;
    public void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("Speed_f", 0f);
    }

    public void Die(){
        state = AnimalState.Dead;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, Vector3.right), 1); 
        speed = 0;
        Destroy(animator, 1f);

    }

    protected abstract bool IsHungry();
    
    bool IsThirsty(){
        return thirst > maxThirst / 3;
    }

    protected abstract void Hunt();

    void UpdateLifeStatsOverTime(){
        hunger += hungerRate / (energy + nonDivideByZeroConst) * Time.deltaTime;
        thirst += thirstRate / (energy + nonDivideByZeroConst) * Time.deltaTime;
        energy -= energyRate * (hunger/2 + thirst/2) * speed/maxSpeed * Time.deltaTime - 0.01f;
        desireToMate += mateAge >= age || pregnant ? 0 : lifeExpectancy/(lifeExpectancy + age - mateAge) * matingRate * Time.deltaTime;

        energy = Mathf.Clamp(energy, 0, maxEnergy);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
        desireToMate = Mathf.Clamp(desireToMate, 0, maxDesireToMate);

        if(hunger == maxHunger) life -= 0.1f * Time.deltaTime;
        if(thirst == maxThirst) life -= 0.1f * Time.deltaTime;
        if(energy == 0) life -= 0.1f * Time.deltaTime;
        if(life <= 0) Die();
        age += 0.01f * Time.deltaTime;
    }
    void SetUrgencyLevelFromStats(){
        urgencyLevel = priorityNeed switch
        {
            Needs.Food => hunger / maxHunger,
            Needs.Water => thirst / maxThirst,
            Needs.Rest => energy / maxEnergy,
            _ => 0,
        };
        urgencyLevel = Mathf.Clamp(urgencyLevel, 0.5f, 1f);
    }

    void SetFearLevelAndEscapeRoute(){
        FieldOfView fov = GetComponent<FieldOfView>();
        Vector3 resultant = Vector3.zero;
        foreach (GameObject scaryTarget in fov.scaryTargets) {
            resultant += scaryTarget.transform.position - transform.position;
            fearLevel += Vector3.Distance(transform.position, scaryTarget.transform.position) * scaryTarget.GetComponent<AnimalController>().speed  * Time.deltaTime;
        }
        if(fov.scaryTargets.Count == 0) fearLevel -= 1f * Time.deltaTime;

        fearLevel = Mathf.Clamp(fearLevel, 0f, 10f);
        escapeRoute = resultant.normalized * -1;
    }

    void SetPriorityNeed(){
        float maxNeed = Mathf.Max(hunger, thirst, desireToMate, maxEnergy/Mathf.Max(energy, 1f), fearLevel, 1f);
        Needs previousPriorityNeed = priorityNeed;

        if(maxNeed == desireToMate) priorityNeed = Needs.Mate;
        if(maxNeed == hunger) priorityNeed = Needs.Food;
        if(maxNeed == thirst) priorityNeed = Needs.Water;
        if(maxNeed == maxEnergy/Mathf.Max(energy, 1f)) priorityNeed = Needs.Rest;
        if(maxNeed == fearLevel) priorityNeed = Needs.Escape;
        if(maxNeed <= 1) priorityNeed = Needs.None;

        if(previousPriorityNeed != priorityNeed) target = null;

    }

    AnimalState HandleHungerState(){
        if(!IsHungry()) return AnimalState.Wandering;
        if(target == null) 
            return AnimalState.LookingForFood;
        else
            return AnimalState.Hunting;
    }    
    AnimalState HandleThirstState(){
        if(!IsThirsty()) return AnimalState.Wandering;
        if(target == null) 
            return AnimalState.LookingForWater;
        else
            return AnimalState.GoingToTarget;
    }    
    AnimalState HandleRestingState(){
        if(energy >= maxEnergy * 0.6f) return AnimalState.Wandering;
        else return AnimalState.Resting;
    }

    AnimalState HandleEscapeState() {
        return AnimalState.Fleeing;
    }

    AnimalState HandleMateState() {
        if(target == null) 
            return AnimalState.LookingForMate;
        else 
            return AnimalState.Mating;
    }

    void HandleStateChange(AnimalState previousState){
        Wander wanderController = GetComponent<Wander>();
        if(previousState == AnimalState.Wandering && state != AnimalState.Wandering){
            wanderController.StopWandering();
        }
        if(state == AnimalState.Wandering && previousState != AnimalState.Wandering){
            wanderController.StartWandering();
        }
    }

    void UpdateState(){
        if(state == AnimalState.Eating && target != null || state == AnimalState.Drinking) return;
        else animator.SetBool("Eat_b", false);

        AnimalState previousState = state;
        SetPriorityNeed();
        SetUrgencyLevelFromStats();
        state = priorityNeed switch
        {
            Needs.Food => HandleHungerState(),
            Needs.Water => HandleThirstState(),
            Needs.Rest => HandleRestingState(),
            Needs.Escape => HandleEscapeState(),
            Needs.Mate => HandleMateState(),
            _ => AnimalState.Wandering,
        };

        //HandleStateChange(previousState);
    }

    void SetFoVMask(){
        LayerMask desiredMask = state switch
        {
            AnimalState.Wandering => LayerMask.GetMask("Nothing"),
            AnimalState.LookingForFood => foodLayerMask,
            AnimalState.LookingForWater => LayerMask.GetMask("Water"),
            AnimalState.LookingForMate => mateLayerMask,
            AnimalState.Hunting => LayerMask.GetMask("Nothing"),
            _ => LayerMask.GetMask("Nothing"),

        };
        gameObject.GetComponent<FieldOfView>().targetMask = desiredMask;
    }

    protected void AccelerateToSpeed(float desiredSpeed){
        if(desiredSpeed > speed){
            speed += accelerationStrength * Time.deltaTime * energy/10f;
        }
        if(energy < maxEnergy/2 && speed > 0){
            speed -= accelerationStrength/(energy + nonDivideByZeroConst) * Time.deltaTime;
        }
        speed = Mathf.Clamp(speed, 0f, maxSpeed);
    } 

    protected void GoToTarget(){
        animator.SetBool("Eat_b", false);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), steerStrength);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void Wander(float desiredSpeed){
        animator.SetBool("Eat_b", false);
        speed += Random.Range(-deltaSpeed, deltaSpeed * (1f + urgencyLevel)) * Time.deltaTime;
        speed = Mathf.Clamp(speed, 0f, desiredSpeed);
        if(speed < 0.15) return;

   /*      Vector3 desiredDirection = WanderDirection();
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredDirection), steerStrength);
        transform.Translate(Vector3.forward * Time.deltaTime * speed); */
    }

    public void Stop(){
            speed = 0;
            animator.SetFloat("Speed_f", 0f);
    }

    protected void Eat(GameObject food){
        Stop();
        animator.SetBool("Eat_b", true);
        state = AnimalState.Eating;
        hunger = 0;
        Destroy(food, 8f);
            
    }

    protected abstract void HaveChildren();

    IEnumerator Pregnancy(){
        yield return new WaitForSeconds(pregnancyPeriod);
        pregnant = false;
        HaveChildren();
    }

    void GetPregnant(){
        desireToMate = 0;
        pregnant = true;
        StartCoroutine(Pregnancy());
    }

    protected void Mate(GameObject mate){
        Stop();
        if (mate.GetComponent<AnimalController>().gender == Gender.Female && !mate.GetComponent<AnimalController>().pregnant) {
            mate.GetComponent<AnimalController>().GetPregnant();
            desireToMate = 0;

        } else if(gender == Gender.Female && !pregnant){
            GetPregnant();
            mate.GetComponent<AnimalController>().desireToMate = 0;
        }
    }

    void Rest(){
        speed = 0;
    }

    public void Drink(){
        Stop();
        Debug.Log("Drinking");
        animator.SetBool("Eat_b", true);
        state = AnimalState.Drinking;
        thirst -= thirstRate * 10f * Time.deltaTime;
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
        
        if(thirst == 0){
            state = AnimalState.Wandering;
            animator.SetBool("Eat_b", false);
        } 
        
    }

    protected abstract void TryToMate();
    protected abstract void GrowUp();

    void Flee() {
        AccelerateToSpeed(maxSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(escapeRoute), steerStrength);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    void HandleAnimalState(){
        switch(state){
            case AnimalState.LookingForMate:
            case AnimalState.Wandering:
                Wander(wanderSpeed);
                break;
            case AnimalState.LookingForFood:
            case AnimalState.LookingForWater:
                Wander(maxSpeed * urgencyLevel);
                break;
            case AnimalState.GoingToTarget:
                AccelerateToSpeed(maxSpeed * urgencyLevel);
                GoToTarget();
                break;
            case AnimalState.Hunting:
                Hunt();
                break;
            case AnimalState.Mating:
                TryToMate();
                break;
            case AnimalState.Eating:
                break;
            case AnimalState.Drinking:
                Drink();
                break;
            case AnimalState.Resting:
                Rest();
                break;
            case AnimalState.Fleeing:
                Flee();
                break;
            case AnimalState.Dead:
                break;

        }
    }

    void FixedUpdate()
    {   
        if(state == AnimalState.Dead) return;

        //keep animal from falling sideways
        var rot = Quaternion.FromToRotation(transform.up, Vector3.up);
        GetComponent<Rigidbody>().AddTorque(new Vector3(rot.x, rot.y, rot.z)*5f);

        if(age >= mateAge && LayerMask.GetMask("Nothing") == mateLayerMask) GrowUp(); 
        SetFearLevelAndEscapeRoute();
        UpdateState();
        UpdateLifeStatsOverTime();
        SetFoVMask();
        HandleAnimalState();
        animator.SetFloat("Speed_f", Mathf.Sqrt(speed / maxSpeed));
    }
}
