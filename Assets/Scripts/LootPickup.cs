using UnityEngine;

public class LootPickup : MonoBehaviour
{
    private LootData lootData;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatHeight = 0.2f;
    
    private Vector3 startPosition;
    private float floatTimer;
    private bool isCollected = false;
    
    public void Initialize(LootData loot)
    {
        lootData = loot;
        startPosition = transform.position;
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            
            floatTimer += Time.deltaTime * floatSpeed;
            float yOffset = Mathf.Sin(floatTimer) * floatHeight;
            transform.position = startPosition + new Vector3(0, yOffset, 0);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;
        if (!other.CompareTag("Player")) return;
        
        // Check if we can pick up based on loot type
        bool canPickup = true;
        
        switch (lootData.lootType)
        {
            case LootData.LootType.Health:
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null && playerHealth.CurrentHealth >= playerHealth.MaxHealth)
                {
                    canPickup = false;
                    Debug.Log("Health full - cannot pick up health");
                }
                break;
        }
        
        if (!canPickup) return;
        
        isCollected = true;
        
        string message = "";
        Color color = Color.white;
        
        // Apply effect and set notification message
        switch (lootData.lootType)
        {
            case LootData.LootType.Health:
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.Heal(lootData.healAmount);
                    message = $"+{lootData.healAmount} HP";
                    color = Color.green;
                    Debug.Log($"Picked up health: +{lootData.healAmount}");
                }
                break;
                
            case LootData.LootType.Ammo:
    ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
    if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
    {
        activeWeapon.CurrentWeapon.AddAmmo(lootData.ammoAmount);
        message = $"+{lootData.ammoAmount} AMMO";
        color = Color.cyan;
        Debug.Log($"Picked up ammo: +{lootData.ammoAmount}");
    }
    break;
                
            case LootData.LootType.Score:
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(lootData.scoreAmount);
                    message = $"+{lootData.scoreAmount} SCORE";
                    color = Color.yellow;
                    Debug.Log($"Picked up score: +{lootData.scoreAmount}");
                }
                break;
                
            case LootData.LootType.DamageUpgrade:
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.AddDamageUpgrade(lootData.damageUpgradeAmount);
                    message = $"+{lootData.damageUpgradeAmount} DAMAGE";
                    color = new Color(1f, 0.5f, 0f);
                    Debug.Log($"Picked up damage upgrade: +{lootData.damageUpgradeAmount}");
                }
                break;
                
            case LootData.LootType.AmmoCapacityUpgrade:
                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.AddAmmoCapacityUpgrade(lootData.ammoCapacityUpgradeAmount);
                    message = $"+{lootData.ammoCapacityUpgradeAmount} AMMO CAP";
                    color = Color.magenta;
                    Debug.Log($"Picked up ammo capacity upgrade: +{lootData.ammoCapacityUpgradeAmount}");
                }
                break;
        }
        
        // Show notification
        if (FloatingTextManager.Instance != null && !string.IsNullOrEmpty(message))
        {
            FloatingTextManager.Instance.ShowFloatingText(message, color);
        }
        
        // Effects
        if (lootData.pickupVFX != null)
            Instantiate(lootData.pickupVFX, transform.position, Quaternion.identity);
        
        if (lootData.pickupSound != null)
            AudioSource.PlayClipAtPoint(lootData.pickupSound, transform.position);
        
        Destroy(gameObject);
    }
}