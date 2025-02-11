using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveManager : MonoBehaviour
{
    [Header("Waves")]    
    private float waveNumber = 0;
    public float levelIncreaseInterval = 3; //number of waves between each enemy level increase
    public float timeBetweenWaves;
    private bool betweenWaves;

    private PlayerCombat playerCombat;
    public float baseHeathRegen = 15f;

    [Header("Spawning")]
    public GameObject enemyPrefab;
    public int enemiesToSpawn = 2;
    public static int enemyLevel = 1;
    public static int enemiesLeft;
    public float closestPlayerDistance;

    public Transform[] spawnPositions;
    public Camera cam;
    public LayerMask whatIsPlayer;

    [Header("UI")]
    public TextMeshProUGUI enemiesLeftText, roundsText;
    public TextMeshProUGUI kills, roundsSurvived;
    public GameObject gameOver;
    public static int totalKills;

    private void Start()
    {
        playerCombat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
    }

    // Update is called once per frame
    void Update()
    {
        if (enemiesLeft == 0 && !betweenWaves)
        {
            betweenWaves = true;
            StartCoroutine(NewWave());
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }



    IEnumerator NewWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        betweenWaves = false;
        waveNumber++;

        if (waveNumber % levelIncreaseInterval == 0) //every x waves, the enemy level increases, but enemy spawns decrease to compensate
        {
            enemyLevel++;
            enemiesToSpawn--;
        }
        else
        {
            enemiesToSpawn++;
        }

        playerCombat.GainHealth(enemyLevel * baseHeathRegen); //player gains an amount of health based on the enemy level that they will face


        for (int i = 0; i < enemiesToSpawn; i++)
        {
            float y = Random.Range(0, 360); //random rotation on spawn
            GameObject enemy = Instantiate(enemyPrefab, RandomSpawnPos(), Quaternion.Euler(0, y, 0));
            enemy.GetComponentInChildren<EnemyInfo>().level = enemyLevel;
            enemiesLeft++;            
        }

        UpdateUI();
    }

    Vector3 RandomSpawnPos()
    {
        List<Vector3> allowedPositions = new List<Vector3>();

        //the position is added to the list of allowed spawn positions if it is not in view of the camera or too close to the player
        //found this on youtube not clever enough to figure this out myself
        foreach (Transform sp in spawnPositions)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

            foreach (Plane plane in planes)
            {
                if (plane.GetDistanceToPoint(sp.position) < 5 && !Physics.CheckSphere(sp.position, closestPlayerDistance, whatIsPlayer))
                {
                    allowedPositions.Add(sp.position);
                }
            }

        }

        //if any positions are allowed, return them
        if (allowedPositions.Count > 0)
        {
            int index = Random.Range(0, allowedPositions.Count);
            return allowedPositions[index];
        }
        //if not, choose random positions
        else
        {
            print("no positions found");
            return spawnPositions[Random.Range(0, spawnPositions.Length)].position;
        }

        //the spawning method could definately be condensed to not do so much foreach, but i cba to change it
    }

    public void DisplayGameOver()
    {
        kills.text = totalKills + " miners killed";
        roundsSurvived.text = (waveNumber - 1) + " rounds survived";
        gameOver.SetActive(true);

        roundsText.text = "";
        enemiesLeftText.text = "";
    }

    public void UpdateUI()
    {
        roundsText.text = "Round " + waveNumber;
        enemiesLeftText.text = enemiesLeft + " enemies left";
    }
}
