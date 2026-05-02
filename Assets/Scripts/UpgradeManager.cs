using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using StarterAssets;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    private int currentCriticalChance = 0;
    private int currentHealthRegen = 0;
    private int currentMoveSpeed = 0;
    private int currentMaxHealth = 0;
    private int currentLeechRounds = 0;
    private bool hasExplosiveRounds = false;
    private float lastRegenTime = 0f;
    
    [Header("Upgrade Pool")]
    [SerializeField] private List<ShopUpgrade> allUpgrades;
    [SerializeField] private int shopUpgradeCount = 3;
    
    [Header("Player Upgrades")]
    private Dictionary<ShopUpgrade.UpgradeType, int> purchasedUpgrades = new Dictionary<ShopUpgrade.UpgradeType, int>();
    private int currentDamageBonus = 0;
    private int currentAmmoCapacityBonus = 0;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Update()
    {
        // Health Regen
        if (currentHealthRegen > 0 && Time.time >= lastRegenTime + 5f)
        {
            lastRegenTime = Time.time;
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null && playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                playerHealth.Heal(currentHealthRegen);
                Debug.Log($"Health Regen: +{currentHealthRegen} HP");
            }
        }
    }
    
    public ShopUpgrade[] GetAllUpgrades()
    {
        return allUpgrades.ToArray();
    }

    public int GetPurchaseCount(ShopUpgrade upgrade)
    {
        return purchasedUpgrades.ContainsKey(upgrade.upgradeType) ? purchasedUpgrades[upgrade.upgradeType] : 0;
    }

    public List<ShopUpgrade> GetRandomUpgrades()
    {
        List<ShopUpgrade> availableUpgrades = allUpgrades.Where(upgrade => 
        {
            int purchases = GetPurchaseCount(upgrade);
            return purchases < upgrade.maxPurchases;
        }).ToList();
        
        List<ShopUpgrade> shuffled = availableUpgrades.OrderBy(x => Random.value).ToList();
        return shuffled.Take(shopUpgradeCount).ToList();
    }
    
    public int GetUpgradeCost(ShopUpgrade upgrade)
    {
        int purchases = GetPurchaseCount(upgrade);
        int cost = upgrade.baseCost + (upgrade.costIncreasePerPurchase * purchases);
        return cost;
    }
    
    public int GetNextDamageIncrease()
    {
        int purchases = GetPurchaseCount(allUpgrades.First(u => u.upgradeType == ShopUpgrade.UpgradeType.IncreaseDamage));
        return purchases + 1;
    }
    
    public bool PurchaseUpgrade(ShopUpgrade upgrade)
    {
        int currentPurchases = GetPurchaseCount(upgrade);
    
        if (currentPurchases >= upgrade.maxPurchases)
        {
            Debug.Log($"{upgrade.upgradeName} already at max level!");
            return false;
        }
        
        int cost = GetUpgradeCost(upgrade);
        
        Debug.Log($"=== PURCHASE DEBUG ===");
        Debug.Log($"Upgrade: {upgrade.upgradeName}");
        Debug.Log($"Cost: {cost}");
        Debug.Log($"Current Score: {ScoreManager.Instance.CurrentScore}");
        Debug.Log($"Can afford: {ScoreManager.Instance.CurrentScore >= cost}");
        
        if (ScoreManager.Instance.CurrentScore < cost)
        {
            Debug.Log($"Cannot afford! Need {cost}, have {ScoreManager.Instance.CurrentScore}");
            return false;
        }
        
        // Apply upgrade effect
        switch (upgrade.upgradeType)
        {
            case ShopUpgrade.UpgradeType.Heal:
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
                playerHealth?.HealFull();
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseAmmoCapacity:
                currentAmmoCapacityBonus += upgrade.ammoCapacityIncrease;
                ApplyAmmoCapacityUpgrade();
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseDamage:
                int actualIncrease = currentPurchases + 1;
                currentDamageBonus += actualIncrease;
                ApplyDamageUpgrade(actualIncrease);
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseCriticalChance:
                currentCriticalChance += upgrade.criticalChanceIncrease;
                Debug.Log($"Critical chance increased! Now: {currentCriticalChance}%");
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseHealthRegen:
                currentHealthRegen += upgrade.healthRegenAmount;
                Debug.Log($"Health regen increased! Now: {currentHealthRegen} HP per 5 sec");
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseMoveSpeed:
                currentMoveSpeed += upgrade.moveSpeedIncrease;
                ApplyMoveSpeedUpgrade();
                break;
                
            case ShopUpgrade.UpgradeType.IncreaseMaxHealth:
                currentMaxHealth += upgrade.maxHealthIncrease;
                PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
                player?.IncreaseMaxHealth(upgrade.maxHealthIncrease);
                break;
                
            case ShopUpgrade.UpgradeType.LeechRounds:
                currentLeechRounds += upgrade.leechAmount;
                Debug.Log($"Leech rounds increased! Now: +{currentLeechRounds} HP on kill");
                break;
                
            case ShopUpgrade.UpgradeType.ExplosiveRounds:
                hasExplosiveRounds = true;
                Debug.Log("Explosive rounds unlocked!");
                break;
        }
        
        // Track purchase
        if (purchasedUpgrades.ContainsKey(upgrade.upgradeType))
            purchasedUpgrades[upgrade.upgradeType]++;
        else
            purchasedUpgrades[upgrade.upgradeType] = 1;
        
        ScoreManager.Instance.DeductScore(cost);
        
        return true;
    }
    
    private void ApplyAmmoCapacityUpgrade()
{
    // Find all weapons on the player
    ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
    if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
    {
        // Get all weapons from the WeaponSwitcher
        WeaponSwitcher switcher = activeWeapon.GetComponentInChildren<WeaponSwitcher>();
        if (switcher != null)
        {
            // Apply to all unlocked weapons
            Weapon[] allWeapons = switcher.GetAllWeapons();
            foreach (Weapon weapon in allWeapons)
            {
                if (weapon != null)
                {
                    weapon.IncreaseMaxAmmo(currentAmmoCapacityBonus);
                }
            }
        }
        else
        {
            // Fallback to just current weapon
            activeWeapon.CurrentWeapon.IncreaseMaxAmmo(currentAmmoCapacityBonus);
        }
    }
}
    
    private void ApplyMoveSpeedUpgrade()
    {
        FirstPersonController controller = FindFirstObjectByType<FirstPersonController>();
        if (controller != null)
        {
            // Apply speed boost (adjust based on your controller)
            float speedMultiplier = 1f + (currentMoveSpeed / 100f);
            Debug.Log($"Movement speed increased! Multiplier: {speedMultiplier}");
        }
    }
    
    private void ApplyDamageUpgrade(int increase)
    {
        Debug.Log($"Damage increased by +{increase}! Total bonus: +{currentDamageBonus}");
    }
    
    public int GetDamageBonus()
    {
        return currentDamageBonus;
    }
    
    public int GetCriticalChance()
    {
        return currentCriticalChance;
    }
    
    public int GetLeechAmount()
    {
        return currentLeechRounds;
    }
    
    public bool HasExplosiveRounds()
    {
        return hasExplosiveRounds;
    }
    
    public void OnEnemyKilled(Vector3 enemyPosition)
    {
        // Leech Rounds
        if (currentLeechRounds > 0)
        {
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(currentLeechRounds);
                Debug.Log($"Leech rounds: +{currentLeechRounds} HP");
            }
        }
        
        // Explosive Rounds on kill (optional effect)
        if (hasExplosiveRounds)
        {
            // Could add small explosion on kill
            Debug.Log("Explosive rounds triggered!");
        }
    }
    
    public void AddDamageUpgrade(int amount)
    {
        currentDamageBonus += amount;
        Debug.Log($"Loot: Damage increased by +{amount}! Total damage bonus: +{currentDamageBonus}");
    }
    
    public void AddAmmoCapacityUpgrade(int amount)
    {
        currentAmmoCapacityBonus += amount;
        ApplyAmmoCapacityUpgrade();
        Debug.Log($"Loot: Ammo capacity increased by +{amount}! Total bonus: +{currentAmmoCapacityBonus}");
    }
    
    public void ResetUpgrades()
    {
        purchasedUpgrades.Clear();
        currentDamageBonus = 0;
        currentAmmoCapacityBonus = 0;
        currentCriticalChance = 0;
        currentHealthRegen = 0;
        currentMoveSpeed = 0;
        currentMaxHealth = 0;
        currentLeechRounds = 0;
        hasExplosiveRounds = false;
        
        ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
        if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
        {
            activeWeapon.CurrentWeapon.ResetMaxAmmo();
        }
    }
}