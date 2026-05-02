using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private EnemyHealth[] enemyPrefabs;
    [SerializeField] private int prewarmCount = 5;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxActiveEnemies = 10;

    [Header("Spawn Weights")]
    [SerializeField] private int[] spawnWeights;

    private ObjectPool<EnemyHealth>[] pools;

    public int ActiveEnemyCount => GetTotalActiveEnemies();
    public int MaxActiveEnemies => maxActiveEnemies;

    private void Start()
    {
        pools = new ObjectPool<EnemyHealth>[enemyPrefabs.Length];
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            pools[i] = new ObjectPool<EnemyHealth>(enemyPrefabs[i], transform, prewarmCount);
        }
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

    public void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;
        
        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 spawnPos = point.position;
        
        // Try a large offset to see if it's just not high enough
        spawnPos.y += -0.2f;  // Increased from 0.8 to 3
        
        int enemyIndex = GetWeightedRandomIndex();
        EnemyHealth enemy = pools[enemyIndex].Get(spawnPos, point.rotation);
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

        public void SpawnEnemyAtPosition(Vector3 position, int enemyTypeIndex)
    {
        EnemyHealth enemy = pools[enemyTypeIndex].Get(position, Quaternion.identity);
        enemy.OnDied += HandleEnemyDied;
    }

        public int GetEnemyTypeIndex(GameObject enemyPrefab)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].gameObject.name == enemyPrefab.name)
            {
                return i;
            }
        }
        return 0; // Default to first type
    }
}