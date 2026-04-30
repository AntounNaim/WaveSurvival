using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";
    
    [Header("Combat Stats")]
    public int maxHealth = 3;
    public int damage = 10;
    public float attackCooldown = 1f;
    public float attackRange = 1.5f;
    
    [Header("Shooter Specific")]
    public bool isRanged = false;
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float rangedAttackRange = 15f;
    
    [Header("Movement")]
    public float speed = 3.5f;
    public float stoppingDistance = 1f;
    public float angularSpeed = 120f;
    public float acceleration = 8f;
    
    [Header("Reward")]
    public int scoreValue = 100;
    
    [Header("Visuals")]
    public Color color = Color.white;
    public float scale = 1f;
    [Header("Exploder Specific")]
    public bool isExploder = false;
    public float explosionRadius = 3f;
    public float explosionDamage = 30f;
    public GameObject explosionVFX;
    public float detonationDistance = 2f;
    [Header("Summoner Specific")]
    public bool isSummoner = false;
    public GameObject summonedEnemyPrefab;
    public int numberOfSummons = 2;
    public float summonRadius = 2f;
}