using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    private float amountToSpawn = 1000f;
    public GameObject apple;
    
    Vector3 GetRandomPointOnTerrain(Terrain terrain, Vector2 minXZ, Vector2 maxXZ) 
    {
        var point = new Vector3(Random.Range(minXZ.x, maxXZ.x),
                                0,
                                Random.Range(minXZ.y, maxXZ.y));

        point.y = terrain.SampleHeight(point) + terrain.GetPosition().y;

        return point;
    }

    private void SpawnApples(){

        GameObject appleGroupsContainer = GameObject.Find("AppleGroups");
        var terrain = FindObjectOfType<Terrain>();

        var minXZ = new Vector2(terrain.transform.position.x, terrain.transform.position.z);
        var maxXZ = minXZ + new Vector2(terrain.terrainData.size.x, terrain.terrainData.size.z);

        for (int i = 0; i < amountToSpawn; i++)
        {
            Instantiate(
                apple, 
                GetRandomPointOnTerrain(terrain, minXZ, maxXZ),
                Quaternion.identity).transform.parent = appleGroupsContainer.transform;
        }
    }

    IEnumerator SpawnApplesRoutine(){
        while(true){
            SpawnApples();
            yield return new WaitForSeconds(60f);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnApplesRoutine());
    }

}
