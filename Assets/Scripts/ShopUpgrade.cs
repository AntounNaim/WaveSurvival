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
    public int baseDamageIncrease = 1;  // First purchase gives +1
    public int maxPurchases = 5;
    
    public enum UpgradeType
    {
        Heal,
        IncreaseAmmoCapacity,
        IncreaseDamage
    }
}