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
    public int CurrentHealth => currentHealth;

    private bool isRanged = false;
    private ProjectileLauncher projectileLauncher;
    private float lastRangedAttackTime;
    private bool isExploder = false;
    private float explosionRadius;
    private float explosionDamage;
    private GameObject explosionVFX;
    private float detonationDistance;
    private bool isDetonating = false;
    private bool isSummoner = false;
    private GameObject summonedEnemyPrefab;
    private int numberOfSummons;
    private float summonRadius;
    private static int activeMinionsCount = 0;
    public static bool HasActiveMinions => activeMinionsCount > 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        projectileLauncher = GetComponent<ProjectileLauncher>();
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
    
    float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    
    // EXPLODER BEHAVIOR - always tries to get close and detonate
    if (isExploder)
    {
        // Check if close enough to detonate
        if (distanceToPlayer <= detonationDistance && !isDetonating)
        {
            Detonate();
            return;
        }
        
        // Rush towards player
        if (agent.isOnNavMesh && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
        
        // Speed boost when low health (under 50%)
        if (agent != null && currentHealth <= enemyData.maxHealth / 2)
        {
            agent.speed = enemyData.speed * 1.5f;
        }
        
        return;
    }
    
    // RANGED BEHAVIOR
    if (isRanged)
    {
        if (distanceToPlayer <= enemyData.rangedAttackRange)
        {
            if (Time.time >= lastRangedAttackTime + enemyData.attackCooldown)
            {
                lastRangedAttackTime = Time.time;
                PerformRangedAttack();
            }
        }
        
        if (agent.isOnNavMesh && agent.enabled && distanceToPlayer > enemyData.rangedAttackRange * 0.5f)
        {
            agent.SetDestination(player.position);
        }
        else if (agent.isOnNavMesh && agent.enabled)
        {
            agent.ResetPath();
        }
    }
    else // MELEE BEHAVIOR
    {
        if (distanceToPlayer <= enemyData.attackRange)
        {
            if (Time.time >= lastAttackTime + enemyData.attackCooldown)
            {
                lastAttackTime = Time.time;
                DealDamageToPlayer();
            }
        }
        
        if (agent.isOnNavMesh && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
    }
}

private bool HasLineOfSight()
{
    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    RaycastHit hit;
    
    // Adjust ray start position to enemy's head/chest height
    Vector3 rayStart = transform.position + Vector3.up * 0.5f;
    
    if (Physics.Raycast(rayStart, directionToPlayer, out hit, enemyData.rangedAttackRange))
    {
        return hit.collider.CompareTag("Player");
    }
    
    return false;
}

private void Detonate()
{
    if (isDetonating) return;
    
    isDetonating = true;
    
    Debug.Log($"{enemyData.enemyName} DETONATED!");
    
    // Damage player if in radius
    if (player != null)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= explosionRadius)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(explosionDamage));
                Debug.Log($"Explosion dealt {explosionDamage} damage to player!");
            }
        }
    }
    
    // Damage nearby enemies (optional - makes chain reactions)
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
    foreach (var hit in hitColliders)
    {
        EnemyHealth nearbyEnemy = hit.GetComponent<EnemyHealth>();
        if (nearbyEnemy != null && nearbyEnemy != this)
        {
            nearbyEnemy.TakeDamage(50); // Chain damage
            Debug.Log($"Explosion damaged nearby enemy!");
        }
    }
    
    // Spawn explosion VFX
    if (explosionVFX != null)
{
    GameObject explosion = Instantiate(explosionVFX, transform.position, Quaternion.identity);
    Destroy(explosion, 1.5f); // Force destroy after 1.5 seconds
}
    
    // Die without dropping loot (optional)
    OnDied?.Invoke(this);
    gameObject.SetActive(false);
}

private void PerformRangedAttack()
{
    Debug.Log($"PerformRangedAttack called! ProjectileLauncher exists: {projectileLauncher != null}");
    
    if (projectileLauncher != null)
    {
        projectileLauncher.Shoot(enemyData.damage, player.position);
        Debug.Log($"{enemyData.enemyName} shot at player!");
    }
    else
    {
        Debug.LogError("ProjectileLauncher is NULL!");
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
        
        // Store base health for scaling
        int baseHealth = enemyData.maxHealth;
        if (WaveManager.Instance != null)
        {
            float multiplier = WaveManager.Instance.GetHealthMultiplier();
            currentHealth = Mathf.RoundToInt(baseHealth * multiplier);
        }
        else
        {
            currentHealth = baseHealth;
        }
        
        if (agent != null)
        {
            agent.speed = enemyData.speed;
            agent.stoppingDistance = enemyData.isExploder ? 0.5f : enemyData.stoppingDistance;
        }
        
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = enemyData.color;
        }
        
        transform.localScale = Vector3.one * enemyData.scale;
        
        // Ranged setup
        isRanged = enemyData.isRanged;
        if (isRanged && projectileLauncher == null)
        {
            projectileLauncher = gameObject.AddComponent<ProjectileLauncher>();
        }
        
        // Exploder setup
        isExploder = enemyData.isExploder;
        if (isExploder)
        {
            explosionRadius = enemyData.explosionRadius;
            explosionDamage = enemyData.explosionDamage;
            explosionVFX = enemyData.explosionVFX;
            detonationDistance = enemyData.detonationDistance;
        }
        
        // SUMMONER SETUP
        isSummoner = enemyData.isSummoner;
        if (isSummoner)
        {
            summonedEnemyPrefab = enemyData.summonedEnemyPrefab;
            numberOfSummons = enemyData.numberOfSummons;
            summonRadius = enemyData.summonRadius;
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage! Health: {currentHealth}");
        
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
        // Notify UpgradeManager for leech rounds
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnEnemyKilled(transform.position);
        }
        
        // SUMMON ENEMIES ON DEATH
        if (isSummoner && !isDetonating)
        {
            SummonMinions();
        }
        
        // Don't drop loot or give score if exploder already detonated
        if (!isDetonating)
        {
            if (ScoreManager.Instance != null && enemyData != null)
            {
                ScoreManager.Instance.AddScore(enemyData.scoreValue, transform.position);
            }
            
            if (LootDropManager.Instance != null)
            {
                LootDropManager.Instance.TryDropLoot(transform.position);
            }
        }
        
        OnDied?.Invoke(this);
        gameObject.SetActive(false);
    }

        private void SummonMinions()
    {
        Debug.Log($"Summoner spawning {numberOfSummons} minions!");
        
        if (summonedEnemyPrefab == null)
        {
            Debug.LogError("Summoned enemy prefab is not assigned in EnemyData!");
            return;
        }
        
        for (int i = 0; i < numberOfSummons; i++)
        {
            Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * summonRadius;
            randomOffset.y = 0;
            Vector3 spawnPosition = transform.position + randomOffset;
            
            GameObject minion = Instantiate(summonedEnemyPrefab, spawnPosition, Quaternion.identity);
            EnemyHealth minionHealth = minion.GetComponent<EnemyHealth>();
            
            if (minionHealth != null)
            {
                activeMinionsCount++;
                Debug.Log($"Minion spawned. Total active minions: {activeMinionsCount}");
                
                // When this minion dies, decrement the count
                minionHealth.OnDied += (diedMinion) => {
                    activeMinionsCount--;
                    Debug.Log($"Minion died. Remaining minions: {activeMinionsCount}");
                };
            }
        }
    }

    private int GetMinionTypeIndex()
    {
        // Get the index of the minion enemy type from the spawner
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            return spawner.GetEnemyTypeIndex(summonedEnemyPrefab);
        }
        return 0;
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
        private void HandleMinionDied(EnemyHealth minion)
    {
        minion.OnDied -= HandleMinionDied;
        // The ObjectPool will handle returning, but we just need to track
        Debug.Log($"Minion died, remaining: {FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length}");
    }
}