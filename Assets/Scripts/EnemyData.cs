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
    
    [Header("Movement")]
    public float speed = 3.5f;
    public float stoppingDistance = 1f;
    
    [Header("Reward")]
    public int scoreValue = 100;
    
    [Header("Visuals")]
    public Color color = Color.white;
    public float scale = 1f;
}