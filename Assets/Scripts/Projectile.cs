using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float speed = 15f;
    
    private Vector3 direction;
    private bool hasHit = false;
    
    public void Initialize(Vector3 shootDirection, int damageAmount)
    {
        direction = shootDirection.normalized;
        damage = damageAmount;
        Destroy(gameObject, lifetime);
    }
    
    private void Update()
    {
        if (!hasHit)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void Start()
    {
        int floorLayer = LayerMask.NameToLayer("Floor");
        if (floorLayer != -1)
        {
            Physics.IgnoreLayerCollision(floorLayer, LayerMask.NameToLayer("Projectile"), true);
        }
        }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // Ignore enemies and other projectiles
        if (other.CompareTag("Enemy")) return;
        if (other.CompareTag("Projectile")) return;
        
        Debug.Log($"Projectile hit: {other.gameObject.name}, Tag: '{other.tag}'");
        
        // Check for player (both "Player" tag and PlayerHealth component)
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null || other.CompareTag("Player"))
        {
            if (playerHealth == null)
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Projectile dealt {damage} damage to player!");
            }
            hasHit = true;
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Hit wall or obstacle
            hasHit = true;
            Destroy(gameObject);
        }
    }
}