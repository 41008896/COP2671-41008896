using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject powerupPrefab;
    private float spawnRange = 9.0f;
    private int enemyCount;
    private int waveNumber = 1;

    // Start is called before the first frame update
    void Start()
    {
        //SpawnEnemyWave(waveNumber);
        //Instantiate(powerupPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        enemyCount = FindObjectsOfType<Enemy>().Length;

        if (enemyCount == 0 )
        {
            SpawnEnemyWave(waveNumber);
            Instantiate(powerupPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
            waveNumber++;
        }
    }

    private Vector3 GenerateSpawnPosition()
    {
        float spawnPosX = Random.Range(-spawnRange, spawnRange);
        float spawnPosZ = Random.Range(-spawnRange, spawnRange);
        Vector3 randomPos = new Vector3(spawnPosX, 0, spawnPosZ);

        return randomPos;
    }

    void SpawnEnemyWave(int enemies)
    {
        for (int i = 0; i < enemies; i++)
        {
            Instantiate(enemyPrefab, GenerateSpawnPosition(), enemyPrefab.transform.rotation);
        }
    }
}
