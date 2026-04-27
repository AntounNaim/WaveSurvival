using UnityEngine;

[CreateAssetMenu(fileName = "New Loot", menuName = "Loot/Loot Item")]
public class LootData : ScriptableObject
{
    [Header("Identity")]
    public string lootName;
    public Sprite lootIcon;
    public GameObject lootPrefab;  // This should accept any GameObject prefab
    
    [Header("Loot Type")]
    public LootType lootType;
    
    [Header("Effect Values")]
    public int ammoAmount = 30;
    public int healAmount = 25;
    public int scoreAmount = 100;
    public int damageUpgradeAmount = 1;
    public int ammoCapacityUpgradeAmount = 5;
    
    [Header("VFX")]
    public GameObject pickupVFX;
    public AudioClip pickupSound;
    
    public enum LootType
    {
        Ammo,
        Health,
        Score,
        DamageUpgrade,
        AmmoCapacityUpgrade
    }
}