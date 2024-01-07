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
    Hunting,
    Fleeing,
    Dead
}

public enum Needs
{
    Food,
    Water,
    Rest,
    Mate,
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
    protected float hungerRate = 0.1f;
    protected float thirstRate = 0.1f;
    protected float energyRate = 0.1f;

    public LayerMask foodLayerMask;
    public AnimalState state = AnimalState.Wandering;
    public Needs priorityNeed = Needs.None;

    //movement
    public float maxSpeed = 3f;
    public float speed = 0f;
    public Vector3 velocity = Vector3.zero;
    public float wanderStrength = 0.1f;
    public float steerStrength = 0.05f;
    public float deltaSpeed = 1f;
    private Vector3 desiredDirection;
    public float accelerationStrength = 1f;

    public float urgencyLevel = 0f;

    public GameObject target;
   
    Animator animator;

    private float nonDivideByZeroConst = 0.01f;
    public void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("Speed_f", 0f);
    }

    Vector3 WanderDirection()
    {
        desiredDirection = (desiredDirection + new Vector3(Random.Range(-wanderStrength, wanderStrength), 0, Random.Range(-wanderStrength, wanderStrength))).normalized;
        return desiredDirection;
    }

    public void Die(){
        state = AnimalState.Dead;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.forward, Vector3.right), 1); 
        speed = 0;
        Destroy(animator, 1f);

    }

    protected abstract bool IsHungry();
    protected abstract void Hunt();

    void UpdateLifeStatsOverTime(){
        hunger += hungerRate / (energy + nonDivideByZeroConst) * Time.deltaTime;
        //thirst += thirstRate / (energy + nonDivideByZeroConst) * Time.deltaTime;
        energy -= energyRate * (hunger/2 + thirst/2) * speed/10f * Time.deltaTime - 0.01f;
        energy = Mathf.Clamp(energy, 0, maxEnergy);
        hunger = Mathf.Clamp(hunger, 0, maxHunger);
        thirst = Mathf.Clamp(thirst, 0, maxThirst);
        if(hunger == maxHunger) life -= 0.5f * Time.deltaTime;
        if(thirst == maxThirst) life -= 0.5f * Time.deltaTime;
        if(energy == 0) life -= 0.5f * Time.deltaTime;
        if(life <= 0) Die();
    }
    void SetUrgencyLevelFromStats(){
        urgencyLevel = priorityNeed switch
        {
            Needs.Food => hunger / maxHunger,
            Needs.Water => thirst / maxThirst,
            Needs.Rest => energy / maxEnergy,
            _ => 0,
        };
        urgencyLevel = Mathf.Clamp(urgencyLevel, 0f, 1f);
    }

    void SetPriorityNeed(){
        float maxNeed = Mathf.Max(hunger, thirst, maxEnergy/Mathf.Max(energy, 1f), 1f);

        if(maxNeed == hunger) priorityNeed = Needs.Food;
        if(maxNeed == thirst) priorityNeed = Needs.Water;
        if(maxNeed == maxEnergy/Mathf.Max(energy, 1f)) priorityNeed = Needs.Rest;
        if(maxNeed <= 1) priorityNeed = Needs.None;

    }

    AnimalState HandleHungerState(){
        if(!IsHungry()) return AnimalState.Wandering;
        if(target == null) 
            return AnimalState.LookingForFood;
        else
            return AnimalState.Hunting;

    }    
    AnimalState HandleRestingState(){
        if(energy >= maxEnergy * 0.8) return AnimalState.Wandering;
        else return AnimalState.Resting;

    }

    void UpdateState(){
        if(state == AnimalState.Eating && target != null) return;
        else animator.SetBool("Eat_b", false);

        SetPriorityNeed();
        SetUrgencyLevelFromStats();
        state = priorityNeed switch
        {
            Needs.Food => HandleHungerState(),
            Needs.Water => AnimalState.Wandering,
            Needs.Rest => HandleRestingState(),
            _ => AnimalState.Wandering,
        };
    }

    void SetFoVMask(){
        LayerMask desiredMask = state switch
        {
            AnimalState.Wandering => LayerMask.GetMask("Nothing"),
            AnimalState.LookingForFood => foodLayerMask,
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
    } 

    protected void GoToTarget(){
        animator.SetBool("Eat_b", false);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), steerStrength);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
        animator.SetFloat("Speed_f", Mathf.Sqrt(speed / maxSpeed));
    }

    void Wander(){
        animator.SetBool("Eat_b", false);
        speed += Random.Range(-deltaSpeed, deltaSpeed * (1f + urgencyLevel)) * Time.deltaTime;
        speed = Mathf.Clamp(speed, 0f, maxSpeed);
        if(speed < 0.15) return;
        animator.SetFloat("Speed_f", Mathf.Sqrt(speed / maxSpeed));

        Vector3 desiredDirection = WanderDirection();
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredDirection), steerStrength);

        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    protected void Stop(){
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

    void Rest(){
        speed = 0;
    }

    void HandleAnimalState(){
        switch(state){
            case AnimalState.Wandering:
                Wander();
                break;
            case AnimalState.LookingForFood:
                Wander();
                break;
            case AnimalState.Hunting:
                Hunt();
                break;
            case AnimalState.Eating:
                break;
            case AnimalState.Resting:
                Rest();
                break;
            case AnimalState.Dead:
                break;

        }
    }

    void FixedUpdate()
    {   
        if(state == AnimalState.Dead) return;
        UpdateState();
        UpdateLifeStatsOverTime();
        SetFoVMask();
        HandleAnimalState();
        animator.SetFloat("Speed_f", Mathf.Sqrt(speed / maxSpeed));
    }
}
