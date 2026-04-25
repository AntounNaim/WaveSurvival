using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IPoolable
{
    [SerializeField] private EnemyData enemyData;
    private int currentHealth;
    private NavMeshAgent agent;
    private Renderer enemyRenderer;
    private float lastAttackTime;
    private Transform player;

    public event Action<EnemyHealth> OnDied;
    public EnemyData Data => enemyData;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        FindPlayer();
    }

    private void OnEnable()
    {
        ApplyStats();
        FindPlayer();
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        
        // If within attack range
        if (distance <= 1.5f)
        {
            if (Time.time >= lastAttackTime + enemyData.attackCooldown)
            {
                lastAttackTime = Time.time;
                DealDamageToPlayer();
            }
        }
    }

    private void DealDamageToPlayer()
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(enemyData.damage);
            Debug.Log($"{enemyData.enemyName} attacked! Distance: {Vector3.Distance(transform.position, player.position)}");
        }
    }

    private void ApplyStats()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData missing on {gameObject.name}");
            return;
        }
        
        currentHealth = enemyData.maxHealth;
        
        if (agent != null)
        {
            agent.speed = enemyData.speed;
            agent.stoppingDistance = enemyData.stoppingDistance;
        }
        
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = enemyData.color;
        }
        
        transform.localScale = Vector3.one * enemyData.scale;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}/{enemyData.maxHealth}");
        
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.red;
            Invoke(nameof(ResetColor), 0.1f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void ResetColor()
    {
        if (enemyRenderer != null && enemyData != null)
        {
            enemyRenderer.material.color = enemyData.color;
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        if(ScoreManager.Instance != null && enemyData != null)
        {
            ScoreManager.Instance.AddScore(enemyData.scoreValue);
        }
        OnDied?.Invoke(this);
        gameObject.SetActive(false);
    }

    public void OnGetFromPool()
    {
        ApplyStats();
        lastAttackTime = 0;
        gameObject.SetActive(true);
    }

    public void OnReturnFromPool()
    {
        OnDied = null;
    }
}