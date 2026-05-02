using System;
using System.Collections;
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


        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.isKinematic)
        {
            // Reset velocity to stop sliding
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        // If dead, skip everything
        if (currentHealth <= 0) return;
        
        if (player == null)
        {
            FindPlayer();
            return;
        }


        Collider col= GetComponent<Collider>();

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"{gameObject.name} - Collider enabled: {col?.enabled}, isTrigger: {col?.isTrigger}, Layer: {gameObject.layer}");
        }

        
        if(col != null && !col.enabled)
        {
            Debug.LogWarning($"Collider on {gameObject.name} was disabled! Re-enabling.");
            col.enabled = true;
        }

        // FIX SPINNING - Force agent to update rotation
        if (agent != null && agent.isOnNavMesh && agent.enabled)
        {
            if (agent.velocity.magnitude > 0.1f)
            {
                // Update rotation to face movement direction
                Vector3 direction = agent.velocity.normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }
            else if (player != null)
            {
                // When stopped, face the player
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                }
            }
        }
        
        // Unstuck logic - only for melee enemies
        if (agent != null && agent.isOnNavMesh && agent.enabled && !isRanged && !isExploder)
        {
            if (agent.velocity.magnitude < 0.1f && agent.remainingDistance > 0.5f)
            {
                agent.SetDestination(player.position);
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Update animator speed
        if (animator != null && agent != null)
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
            
            if (agent != null && agent.isOnNavMesh && agent.enabled)
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
                
                if (agent != null && agent.isOnNavMesh && agent.enabled)
                {
                    agent.ResetPath();
                }
                return;
            }
            
            if (agent != null && agent.isOnNavMesh && agent.enabled && distanceToPlayer > enemyData.rangedAttackRange * 0.7f)
            {
                agent.SetDestination(player.position);
            }
            else if (agent != null && agent.isOnNavMesh && agent.enabled)
            {
                agent.ResetPath();
            }
        }
        else // MELEE BEHAVIOR
        {
            if (distanceToPlayer <= enemyData.attackRange)
            {
                if (agent != null && agent.isOnNavMesh && agent.enabled)
                {
                    agent.ResetPath();
                }
                
                if (Time.time >= lastAttackTime + enemyData.attackCooldown)
                {
                    lastAttackTime = Time.time;
                    DealDamageToPlayer();
                }
            }
            else
            {
                if (agent != null && agent.isOnNavMesh && agent.enabled)
                {
                    agent.SetDestination(player.position);
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (currentHealth <= 0) return;
        
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
        if (currentHealth <= 0) return;
        
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
        
        Debug.Log($"{enemyData.enemyName} DETONATED!");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyExploderSound, 0.8f);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDied();
        }
        
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
        
        if (agent != null && agent.isOnNavMesh && agent.enabled)
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
        
        if (agent != null && agent.isOnNavMesh && agent.enabled)
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
        if (currentHealth <= 0)
        {
        Debug.LogWarning($"Tried to damage {gameObject.name} but already dead!");
        return;
        }
        
        // Play hit sound
        if (AudioManager.Instance != null && AudioManager.Instance.enemyHitSound != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyHitSound, 0.5f);
        }

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
        
        if (currentHealth > 0) return;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.enemyDeathSound, 0.6f);
        }

        // Disable everything immediately
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        
        // Disable the script to prevent further updates
        enabled = false;
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Die");
            
            // Get death animation length and multiply by speed
            float deathAnimLength = GetAnimationLength("Die");
            
            // If the animator has a speed parameter, use it
            float animSpeed = animator.GetFloat("DeathSpeed");
            if (animSpeed <= 0) animSpeed = 1f;
            
            float actualDelay = deathAnimLength / animSpeed;
            
            // Small buffer to ensure animation completes
            actualDelay += 0.05f;
            
            StartCoroutine(CompleteDeathAfterDelay(actualDelay));
        }
        else
        {
            CompleteDeath();
        }
}

    private IEnumerator CompleteDeathAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CompleteDeath();
    }

    private void CompleteDeath()
    {
        // Notify WaveManager
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.OnEnemyDied();
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

    private float GetAnimationLength(string animationName)
    {
        if (animator != null)
        {
            RuntimeAnimatorController ac = animator.runtimeAnimatorController;
            AnimationClip[] clips = ac.animationClips;
            foreach (AnimationClip clip in clips)
            {
                if (clip.name.ToLower().Contains(animationName.ToLower()))
                {
                    return clip.length;
                }
            }
        }
        return 0.3f;
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
                
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.OnMinionSpawned();
                }
                
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
        lastRangedAttackTime = 0;
        isDetonating = false;
        enabled = true;
        
        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        
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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision on {gameObject.name} with {collision.gameObject.name}");
    }
}