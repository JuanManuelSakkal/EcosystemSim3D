using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(AnimalController))]
public class Wander : MonoBehaviour
{
	public float speed = 5;
	public float directionChangeInterval = 1;
	public float maxHeadingChange = 180;

	AnimalController controller;
	FieldOfView fov;
	public float heading;
	public Vector3 targetRotation;

	public bool stopped = false;
	public bool changingArea = false;

	void Awake ()
	{
		controller = GetComponent<AnimalController>();
		fov = GetComponent<FieldOfView>();

		// Set random initial rotation
		heading = Random.Range(0, 360);
		transform.eulerAngles = new Vector3(0, heading, 0);

		StartCoroutine(NewHeading());
	}

	bool isInAWanderingState(){
		return controller.state == AnimalState.Wandering || 
			controller.state == AnimalState.LookingForFood || 
			controller.state == AnimalState.LookingForMate;
	}

	void Update ()
	{
		if(isInAWanderingState() && !stopped) {
			speed = controller.speed;
        	transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation), Time.deltaTime * directionChangeInterval/2);
			
			transform.Translate(Vector3.forward * speed * Time.deltaTime);
		} else if(isInAWanderingState() && stopped){
			controller.Stop();
		}
	}

	IEnumerator NewHeading ()
	{
		while (true) {
			NewHeadingRoutine();
			yield return new WaitForSeconds(directionChangeInterval);
		}
	}

	public void StopWandering ()
	{
		StopCoroutine(NewHeading());
	}

	public void StartWandering ()
	{
		StartCoroutine(NewHeading());
	}

	void NewHeadingRoutine ()
	{	
		stopped = Random.Range(0f, 1f) <= 0.2f;
		changingArea = Random.Range(0f, 1f) <= 0.1f;

		var floor = transform.eulerAngles.y - maxHeadingChange;
		var ceil  = transform.eulerAngles.y + maxHeadingChange;
		directionChangeInterval = changingArea ? Random.Range(5.0f, 10.0f) : Random.Range(0.5f, 2.0f);
		heading = Random.Range(floor, ceil);
		targetRotation = fov.DirectionFromAngle(heading, true);
	}
}