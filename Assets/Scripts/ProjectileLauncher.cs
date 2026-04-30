using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    
    private void Start()
    {
        if (firePoint == null)
        {
            firePoint = transform;
            Debug.Log($"FirePoint set to transform for {gameObject.name}");
        }
        
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile prefab not assigned on {gameObject.name}!");
        }
    }
    
    public void Shoot(int damage, Vector3 targetPosition)
{
    if (projectilePrefab == null)
    {
        Debug.LogError("Projectile prefab not assigned!");
        return;
    }
    
    Vector3 direction = (targetPosition - firePoint.position).normalized;
    
    // Spawn projectile slightly in front to avoid self-collision
    Vector3 spawnPosition = firePoint.position + direction * 0.5f;
    
    // Add slight spread
    float spread = 0.05f;
    direction.x += Random.Range(-spread, spread);
    direction.y += Random.Range(-spread, spread);
    direction.z += Random.Range(-spread, spread);
    direction.Normalize();
    
    GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
    Projectile proj = projectile.GetComponent<Projectile>();
    
    if (proj != null)
    {
        proj.Initialize(direction, damage);
        Debug.Log($"Projectile launched from {spawnPosition} toward {targetPosition}");
    }
}
}