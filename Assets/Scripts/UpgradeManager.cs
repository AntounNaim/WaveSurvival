using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
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
    Debug.Log($"GetUpgradeCost: {upgrade.upgradeName} - Purchases: {purchases}, BaseCost: {upgrade.baseCost}, Increase: {upgrade.costIncreasePerPurchase}, Total: {cost}");
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
    Debug.Log($"Current Score before check: {ScoreManager.Instance.CurrentScore}");
    
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
    }
    
    // Track purchase
    if (purchasedUpgrades.ContainsKey(upgrade.upgradeType))
        purchasedUpgrades[upgrade.upgradeType]++;
    else
        purchasedUpgrades[upgrade.upgradeType] = 1;
    
    Debug.Log($"Score before deduction: {ScoreManager.Instance.CurrentScore}");
    
    // USE DEDUCTSCORE INSTEAD OF ADDSCORE
    ScoreManager.Instance.DeductScore(cost);
    
    Debug.Log($"Score after deduction: {ScoreManager.Instance.CurrentScore}");
    Debug.Log($"=====================");
    
    return true;
}
    
    private void ApplyAmmoCapacityUpgrade()
    {
        ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
        if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
        {
            activeWeapon.CurrentWeapon.IncreaseMaxAmmo(currentAmmoCapacityBonus);
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
    
    // ADD THESE METHODS:
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
        
        ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
        if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
        {
            activeWeapon.CurrentWeapon.ResetMaxAmmo();
        }
    }
}