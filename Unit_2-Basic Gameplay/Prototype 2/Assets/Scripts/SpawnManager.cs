using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Initialize Variables
    public GameObject[] animalPrefabs;
    
    private float spawnStart = 2.0f;
    private float spawnSpeed = 2.0f;
    private int spawnRange = 15;
    private int spawnPositionZ = 20;
    private int animalArrayLength;
    private float timer = 0.0f;
    private int animalIndex = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        // Initialize tickers
        InvokeRepeating("SpawnRandomAnimal", spawnStart, spawnSpeed);
        InvokeRepeating("TimerTick", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {        


    }

    void TimerTick()
    {
        // Timer that tracks the number of seconds the game has been running.
        timer++;
    }

    void SpawnRandomAnimal()
    {
        // Randomize animalIndex
        animalArrayLength = animalPrefabs.Length;
        animalIndex = Random.Range(0, animalArrayLength);

        // Randomize spawn position of animals
        Vector3 spawnPosition = new Vector3(Random.Range(-spawnRange, spawnRange), 0, spawnPositionZ);
        
        // Spawn random animal
        Instantiate(animalPrefabs[animalIndex], spawnPosition, animalPrefabs[animalIndex].transform.rotation);
    }
}