using UnityEngine;

[CreateAssetMenu(fileName = "New Shop Upgrade", menuName = "Shop/Upgrade")]
public class ShopUpgrade : ScriptableObject
{
    [Header("Identity")]
    public string upgradeName;
    public string description;
    public Sprite icon;
    
    [Header("Cost")]
    public int baseCost = 100;
    public int costIncreasePerPurchase = 50;
    
    [Header("Upgrade Type")]
    public UpgradeType upgradeType;
    
    [Header("Effect Values")]
    public int healthAmount = 100;
    public int ammoCapacityIncrease = 10;
    public int baseDamageIncrease = 1;
    public int maxPurchases = 5;
    
    // New upgrade values
    public int criticalChanceIncrease = 10;
    public int healthRegenAmount = 1;
    public int moveSpeedIncrease = 10;
    public int maxHealthIncrease = 25;
    public int leechAmount = 5;
    
    public enum UpgradeType
    {
        Heal,
        IncreaseAmmoCapacity,
        IncreaseDamage,
        IncreaseCriticalChance,
        IncreaseHealthRegen,
        IncreaseMoveSpeed,
        IncreaseMaxHealth,
        LeechRounds,
        ExplosiveRounds
    }
}