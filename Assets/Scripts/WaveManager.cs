using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    private float waveNumber = 0; 
  
    public float levelIncreaseInterval = 3; //number of waves between each enemy level increase
    public float timeBetweenWaves;
    public float timeBetweenEnemySpawn;
    private bool betweenWaves;

    public static int enemiesLeft;
    public int enemiesToSpawn = 2;
    public static int enemyLevel = 1;

    public Transform[] spawnPositions;
    public Camera cam;
    public LayerMask whatIsPlayer;
    
    public float closestPlayerDistance;
 

    // Update is called once per frame
    void Update()
    {
        
        if (enemiesLeft == 0 && !betweenWaves)
        {
            betweenWaves = true;
            StartCoroutine(NewWave());
        }
    }

    
    
    IEnumerator NewWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);   
        betweenWaves = false;
        waveNumber++;

        if (waveNumber % levelIncreaseInterval == 0 )
        {
            enemyLevel++;
            enemiesToSpawn--;
        }
        else 
        {
            enemiesToSpawn++;
        }

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            float y = Random.Range(0, 360); //random rotation on spawn
            GameObject enemy = Instantiate(enemyPrefab, RandomSpawnPos(), Quaternion.Euler(0, y, 0));
            enemy.GetComponentInChildren<EnemyInfo>().level = enemyLevel;
            enemiesLeft++;
            //yield return new WaitForSeconds(timeBetweenEnemySpawn);
        }
    }

    Vector3 RandomSpawnPos()
    {
        List<Vector3> allowedPositions = new List<Vector3>();   

        foreach (Transform sp in spawnPositions)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            foreach (Plane plane in planes)
            {
                if (plane.GetDistanceToPoint(sp.position) < 0 && !Physics.CheckSphere(sp.position, closestPlayerDistance, whatIsPlayer))
                {
                    allowedPositions.Add(sp.position);
                }
            }

        }

        if (allowedPositions.Count > 0)
        {
            int index = Random.Range(0, allowedPositions.Count);
            return allowedPositions[index];
        }
        else
        {
            print("no positions found");
            return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
        }

        
    }
}
