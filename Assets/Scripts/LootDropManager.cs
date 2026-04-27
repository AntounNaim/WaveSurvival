using System.Collections.Generic;
using UnityEngine;

public class LootDropManager : MonoBehaviour
{
    public static LootDropManager Instance { get; private set; }
    
    [Header("Drop Settings")]
    [SerializeField] private float baseDropChance = 0.3f;
    
    [Header("Loot Pools")]
    [SerializeField] private LootPool[] lootPools;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    public void TryDropLoot(Vector3 position)
    {
        if (Random.value > baseDropChance)
            return;
        
        if (lootPools.Length == 0) return;
        
        LootPool selectedPool = lootPools[Random.Range(0, lootPools.Length)];
        LootData droppedLoot = selectedPool.GetRandomLoot();
        
        if (droppedLoot != null)
        {
            SpawnLoot(droppedLoot, position);
        }
    }
    
    private void SpawnLoot(LootData loot, Vector3 position)
    {
        if (loot.lootPrefab != null)
        {
            GameObject lootObj = Instantiate(loot.lootPrefab, position, Quaternion.identity);
            LootPickup pickup = lootObj.GetComponent<LootPickup>();
            if (pickup != null)
            {
                pickup.Initialize(loot);
            }
        }
    }
    
    // This method is called from LootPickup when picked up
    public void ApplyLootEffect(LootData loot, Vector3 pickupPosition)
    {
        string message = "";
        Color color = Color.white;
        
        switch (loot.lootType)
        {
            case LootData.LootType.Ammo:
                ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
                if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
                {
                    // Add ammo logic here
                    message = $"+{loot.ammoAmount} AMMO";
                    color = Color.cyan;
                }
                break;
                
            case LootData.LootType.Health:
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
                playerHealth?.Heal(loot.healAmount);
                message = $"+{loot.healAmount} HP";
                color = Color.green;
                break;
                
            case LootData.LootType.Score:
                ScoreManager.Instance?.AddScore(loot.scoreAmount);
                message = $"+{loot.scoreAmount} SCORE";
                color = Color.yellow;
                break;
                
            case LootData.LootType.DamageUpgrade:
                UpgradeManager.Instance?.AddDamageUpgrade(loot.damageUpgradeAmount);
                message = $"+{loot.damageUpgradeAmount} DAMAGE";
                color = new Color(1f, 0.5f, 0f);
                break;
                
            case LootData.LootType.AmmoCapacityUpgrade:
                UpgradeManager.Instance?.AddAmmoCapacityUpgrade(loot.ammoCapacityUpgradeAmount);
                message = $"+{loot.ammoCapacityUpgradeAmount} AMMO CAP";
                color = Color.magenta;
                break;
        }
        
        // Show notification - FIXED: removed position parameter
        if (FloatingTextManager.Instance != null && !string.IsNullOrEmpty(message))
        {
            FloatingTextManager.Instance.ShowFloatingText(message, color);
        }
        
        if (loot.pickupVFX != null)
            Instantiate(loot.pickupVFX, pickupPosition, Quaternion.identity);
        
        if (loot.pickupSound != null)
            AudioSource.PlayClipAtPoint(loot.pickupSound, pickupPosition);
    }
}

[System.Serializable]
public class LootPool
{
    public string poolName;
    public WeightedLoot[] lootItems;
    
    public LootData GetRandomLoot()
    {
        int totalWeight = 0;
        foreach (WeightedLoot item in lootItems)
        {
            totalWeight += item.weight;
        }
        
        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;
        
        foreach (WeightedLoot item in lootItems)
        {
            cumulativeWeight += item.weight;
            if (randomValue < cumulativeWeight)
            {
                return item.loot;
            }
        }
        
        return null;
    }
}

[System.Serializable]
public class WeightedLoot
{
    public LootData loot;
    [Range(0, 100)]
    public int weight = 10;
}