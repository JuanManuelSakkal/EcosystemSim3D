using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Preference{
    Proximity,
    Intensity
}
public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 45f;
    [Range(0,360)]
    public float viewAngle = 110;

    public LayerMask dangerMask;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<GameObject> visibleTargets = new List<GameObject>();
    public List<GameObject> scaryTargets = new List<GameObject>();
    public List<GameObject> intenseTargets = new List<GameObject>();

    public Preference preference = Preference.Intensity;
    void Start() {
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay) {
        while (true) {
            yield return new WaitForSeconds(delay);
            FindScaryTargets();
            FindVisibleTargets();
            SetClosestTarget();
        }
    }

    void SetClosestTarget() {
        GameObject closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (GameObject target in visibleTargets) {
            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < closestDistance) {
                closestTarget = target;
                closestDistance = distance;
            }
        }
        if (closestTarget != null) {
            gameObject.GetComponent<AnimalController>().target = closestTarget;
        }
    }
    void FindScaryTargets() {
        scaryTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, dangerMask);    
        foreach (Collider target in targetsInViewRadius) {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) {
                scaryTargets.Add(target.gameObject);
            }
        }
    }

    void FindVisibleTargets() {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);    
        foreach (Collider target in targetsInViewRadius) {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < viewAngle / 2) {
                float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)) {
                    visibleTargets.Add(target.gameObject);
                }
            }
        }
    }

    public Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
