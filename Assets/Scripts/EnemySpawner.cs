using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private EnemyHealth[] enemyPrefabs;
    [SerializeField] private int prewarmCount = 5;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxActiveEnemies = 10;
    [SerializeField] private int numberOfClosestSpawnPoints = 2; // How many closest spawners to use

    [Header("Spawn Weights")]
    [SerializeField] private int[] spawnWeights;

    private ObjectPool<EnemyHealth>[] pools;
    private Transform player;
    private List<Transform> activeSpawnPoints = new List<Transform>();

    public int ActiveEnemyCount => GetTotalActiveEnemies();
    public int MaxActiveEnemies => maxActiveEnemies;

    private void Start()
    {
        pools = new ObjectPool<EnemyHealth>[enemyPrefabs.Length];
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            pools[i] = new ObjectPool<EnemyHealth>(enemyPrefabs[i], transform, prewarmCount);
        }
        
        FindPlayer();
    }

    private void Update()
    {
        // Update closest spawn points every second (performance optimization)
        if (Time.frameCount % 60 == 0)
        {
            UpdateClosestSpawnPoints();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void UpdateClosestSpawnPoints()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }
        
        if (spawnPoints.Length == 0) return;
        
        // Sort spawn points by distance to player
        var sortedSpawnPoints = spawnPoints.OrderBy(sp => Vector3.Distance(sp.position, player.position)).ToList();
        
        // Take the closest N spawn points
        activeSpawnPoints.Clear();
        for (int i = 0; i < Mathf.Min(numberOfClosestSpawnPoints, sortedSpawnPoints.Count); i++)
        {
            activeSpawnPoints.Add(sortedSpawnPoints[i]);
        }
        
        // Debug: Log which spawn points are active
        // Debug.Log($"Active spawn points: {activeSpawnPoints.Count}");
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
        if (activeSpawnPoints.Count == 0)
        {
            // Fallback to all spawn points if none are active
            if (spawnPoints.Length == 0) return;
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemyAtPoint(point);
            return;
        }
        
        // Pick a random spawn point from the closest ones
        Transform spawnPoint = activeSpawnPoints[Random.Range(0, activeSpawnPoints.Count)];
        SpawnEnemyAtPoint(spawnPoint);
    }
    
    private void SpawnEnemyAtPoint(Transform point)
    {
        int enemyIndex = GetWeightedRandomIndex();
        EnemyHealth enemy = pools[enemyIndex].Get(point.position, point.rotation);
        enemy.OnDied += HandleEnemyDied;
    }

    public void SpawnEnemyAtPosition(Vector3 position, int enemyTypeIndex)
    {
        if (enemyTypeIndex >= pools.Length) return;
        
        EnemyHealth enemy = pools[enemyTypeIndex].Get(position, Quaternion.identity);
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
    
    public int GetEnemyTypeIndex(GameObject enemyPrefab)
    {
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i].gameObject.name == enemyPrefab.name)
            {
                return i;
            }
        }
        return 0;
    }
}