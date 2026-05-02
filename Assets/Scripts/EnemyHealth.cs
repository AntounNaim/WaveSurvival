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
    private Animator animator;

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
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.updatePosition = true;
            agent.updateRotation = true;
        }
        
        if (animator != null)
        {
            animator.applyRootMotion = false;
            Debug.Log($"Animator found on {animator.gameObject.name}");
        }
        else
        {
            Debug.LogError($"No Animator found in children of {gameObject.name}");
        }
    }

    private void Start()
    {
        FindPlayer();

        if (animator != null)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] clips = ac.animationClips;
            Debug.Log($"Found {clips.Length} animations:");
            foreach (AnimationClip clip in clips)
            {
                Debug.Log($"- {clip.name}");
            }
        }
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
        
        // Update animator speed
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
        
        // EXPLODER BEHAVIOR
        if (isExploder)
        {
            if (distanceToPlayer <= detonationDistance && !isDetonating)
            {
                Detonate();
                return;
            }
            
            if (agent.isOnNavMesh && agent.enabled)
            {
                agent.SetDestination(player.position);
            }
            return;
        }
        
        // RANGED BEHAVIOR
        if (isRanged)
        {
            bool canAttack = distanceToPlayer <= enemyData.rangedAttackRange;
            
            if (canAttack && Time.time >= lastRangedAttackTime + enemyData.attackCooldown)
            {
                lastRangedAttackTime = Time.time;
                PerformRangedAttack();
                agent.ResetPath();
                return;
            }
            
            if (agent.isOnNavMesh && agent.enabled && distanceToPlayer > enemyData.rangedAttackRange * 0.7f)
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
            // DISTANCE-BASED DAMAGE (most reliable)
            if (distanceToPlayer <= enemyData.attackRange)
            {
                // Stop moving when in attack range
                if (agent.isOnNavMesh && agent.enabled)
                {
                    agent.ResetPath();
                }
                
                // Deal damage on cooldown
                if (Time.time >= lastAttackTime + enemyData.attackCooldown)
                {
                    lastAttackTime = Time.time;
                    DealDamageToPlayer();
                }
            }
            else
            {
                // Move toward player when outside attack range
                if (agent.isOnNavMesh && agent.enabled)
                {
                    agent.SetDestination(player.position);
                }
            }
        }
    }

    // COLLISION-BASED DAMAGE
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + enemyData.attackCooldown)
            {
                lastAttackTime = Time.time;
                DealDamageToPlayer();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= lastAttackTime + enemyData.attackCooldown)
            {
                lastAttackTime = Time.time;
                DealDamageToPlayer();
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        RaycastHit hit;
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
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        Debug.Log($"{enemyData.enemyName} DETONATED!");
        
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
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hitColliders)
        {
            EnemyHealth nearbyEnemy = hit.GetComponent<EnemyHealth>();
            if (nearbyEnemy != null && nearbyEnemy != this)
            {
                nearbyEnemy.TakeDamage(50);
                Debug.Log($"Explosion damaged nearby enemy!");
            }
        }
        
        if (explosionVFX != null)
        {
            GameObject explosion = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }
        
        OnDied?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void PerformRangedAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }
        
        if (projectileLauncher != null)
        {
            projectileLauncher.Shoot(enemyData.damage, player.position);
            Debug.Log($"{enemyData.enemyName} shot at player!");
        }
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void DealDamageToPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(enemyData.damage);
            Debug.Log($"{enemyData.enemyName} attacked! Distance: {Vector3.Distance(transform.position, player.position)}");
        }
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
    }

    private void ApplyStats()
    {
        if (enemyData == null)
        {
            Debug.LogError($"EnemyData missing on {gameObject.name}");
            return;
        }
        
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
        
        isRanged = enemyData.isRanged;
        if (isRanged && projectileLauncher == null)
        {
            projectileLauncher = gameObject.AddComponent<ProjectileLauncher>();
        }
        
        isExploder = enemyData.isExploder;
        if (isExploder)
        {
            explosionRadius = enemyData.explosionRadius;
            explosionDamage = enemyData.explosionDamage;
            explosionVFX = enemyData.explosionVFX;
            detonationDistance = enemyData.detonationDistance;
        }
        
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
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
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
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.OnEnemyKilled(transform.position);
        }
        
        if (isSummoner && !isDetonating)
        {
            SummonMinions();
        }
        
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
        
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
        
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
                
                minionHealth.OnDied += (diedMinion) => {
                    activeMinionsCount--;
                    Debug.Log($"Minion died. Remaining minions: {activeMinionsCount}");
                };
            }
        }
    }

    private int GetMinionTypeIndex()
    {
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
        Debug.Log($"Minion died, remaining: {FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None).Length}");
    }
}