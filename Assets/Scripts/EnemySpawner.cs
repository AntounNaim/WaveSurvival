using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private EnemyHealth[] enemyPrefabs;
    [SerializeField] private int prewarmCount = 5;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxActiveEnemies = 10;

    [Header("Spawn Weights")]
    [SerializeField] private int[] spawnWeights;

    private ObjectPool<EnemyHealth>[] pools;

    // PUBLIC PROPERTIES - ADD THESE
    public int ActiveEnemyCount => GetTotalActiveEnemies();
    public int MaxActiveEnemies => maxActiveEnemies;

    private void Start()
    {
        pools = new ObjectPool<EnemyHealth>[enemyPrefabs.Length];
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            pools[i] = new ObjectPool<EnemyHealth>(enemyPrefabs[i], transform, prewarmCount);
        }
        
        // REMOVE the old SpawnLoop - WaveManager now handles spawning
        // StartCoroutine(SpawnLoop());
    }

    private int GetWeightedRandomIndex()
    {
        int totalWeight = 0;
        foreach (int weight in spawnWeights)
        {
            totalWeight += weight;
        }
    
        int randomValue = Random.Range(0, totalWeight);
    
        int cumulativeWeight = 0;
        for (int i = 0; i < spawnWeights.Length; i++)
        {
            cumulativeWeight += spawnWeights[i];
            if (randomValue < cumulativeWeight)
            {
                return i;
            }
        }   
    
        return 0;
    }

    // REMOVE the old SpawnLoop
    /*
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (GetTotalActiveEnemies() < maxActiveEnemies && spawnPoints.Length > 0)
                SpawnEnemy();
        }
    }
    */

    private int GetTotalActiveEnemies()
    {
        int total = 0;
        foreach (var pool in pools)
        {
            if (pool != null)
                total += pool.CountActive;
        }
        return total;
    }

    // MAKE THIS PUBLIC
    public void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;
        
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        int enemyIndex = GetWeightedRandomIndex();
        EnemyHealth enemy = pools[enemyIndex].Get(point.position, point.rotation);
        enemy.OnDied += HandleEnemyDied;
    }

    private void HandleEnemyDied(EnemyHealth enemy)
    {
        enemy.OnDied -= HandleEnemyDied;
        
        for (int i = 0; i < pools.Length; i++)
        {
            if (pools[i].Contains(enemy))
            {
                pools[i].Return(enemy);
                break;
            }
        }
    }
}