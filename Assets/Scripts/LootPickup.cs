using UnityEngine;

public class LootPickup : MonoBehaviour
{
    private LootData lootData;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float floatSpeed = 0.5f;
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float groundOffset = 0.5f;
    [SerializeField] private LayerMask groundLayer = -1;
    
    private Vector3 startPosition;
    private float floatTimer;
    private bool isCollected = false;
    private bool isGrounded = false;
    
    public void Initialize(LootData loot)
    {
        lootData = loot;
        SnapToGround();
        startPosition = transform.position;
    }
    
    private void SnapToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 3f))
        {
            Vector3 newPos = transform.position;
            newPos.y = hit.point.y + groundOffset;
            transform.position = newPos;
            isGrounded = true;
        }
        else
        {
            Vector3 newPos = transform.position;
            newPos.y += groundOffset;
            transform.position = newPos;
        }
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            if (Time.frameCount % 30 == 0 && !isGrounded)
            {
                SnapToGround();
                startPosition = transform.position;
            }
            
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
        
        bool canPickup = true;
        string message = "";
        Color color = Color.white;
        
        // Apply effect based on loot type
        switch (lootData.lootType)
        {
            case LootData.LootType.Health:
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (playerHealth.CurrentHealth < playerHealth.MaxHealth)
                    {
                        playerHealth.Heal(lootData.healAmount);
                        message = $"+{lootData.healAmount} HP";
                        color = Color.green;
                        Debug.Log($"Picked up health: +{lootData.healAmount}");
                    }
                    else
                    {
                        canPickup = false;
                        Debug.Log("Health full - cannot pick up health");
                    }
                }
                else
                {
                    canPickup = false;
                }
                break;
                
            case LootData.LootType.Ammo:
            // Find all weapons on the player
            WeaponSwitcher switcher = FindFirstObjectByType<WeaponSwitcher>();
            if (switcher != null)
            {
                Weapon[] allWeapons = switcher.GetAllWeapons();
                foreach (Weapon weapon in allWeapons)
                {
                    if (weapon != null)
                    {
                        weapon.AddAmmo(lootData.ammoAmount);
                    }
                }
                message = $"+{lootData.ammoAmount} AMMO (All Weapons)";
                color = Color.cyan;
                Debug.Log($"Picked up ammo: +{lootData.ammoAmount} to all weapons");
            }
            else
            {
                // Fallback to just current weapon
                ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
                if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
                {
                    activeWeapon.CurrentWeapon.AddAmmo(lootData.ammoAmount);
                    message = $"+{lootData.ammoAmount} AMMO";
                    color = Color.cyan;
                }
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
        
        if (!canPickup) return;
        
        isCollected = true;
        
        if (FloatingTextManager.Instance != null && !string.IsNullOrEmpty(message))
        {
            FloatingTextManager.Instance.ShowFloatingText(message, color);
        }
        
        if (lootData.pickupVFX != null)
            Instantiate(lootData.pickupVFX, transform.position, Quaternion.identity);
        
        if (lootData.pickupSound != null)
            AudioSource.PlayClipAtPoint(lootData.pickupSound, transform.position);
        
        Destroy(gameObject);
    }
}