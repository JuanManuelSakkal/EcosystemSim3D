using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggsController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      StartCoroutine(Hatch());
    }

    void InitializeChick(GameObject newChick){
        newChick.transform.parent = transform.parent;
        newChick.GetComponent<ChickenController>().gender = Random.Range(0, 1) > 0.75f ? Gender.Female : Gender.Male;
        newChick.GetComponent<ChickenController>().age = 0;
    }
    IEnumerator Hatch()
    {
        yield return new WaitForSeconds(20f);
        Debug.Log("Hatching...");
        GameObject newChick1 = Instantiate(Resources.Load("Chick") as GameObject, transform.position + Vector3.forward * -1, Quaternion.identity);
        GameObject newChick2 = Instantiate(Resources.Load("Chick") as GameObject, transform.position + Vector3.right * 1, Quaternion.identity);
        GameObject newChick3 = Instantiate(Resources.Load("Chick") as GameObject, transform.position + Vector3.right * -1, Quaternion.identity);
        InitializeChick(newChick1);
        InitializeChick(newChick2);
        InitializeChick(newChick3);
        Destroy(gameObject, 1f);
    }
}
