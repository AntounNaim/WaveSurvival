using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button closeButton;
    
    [Header("Upgrade Buttons - Dynamic")]
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeNameTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeCostTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescTexts;
    
    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    
    private List<ShopUpgrade> currentUpgrades = new List<ShopUpgrade>();
    private FirstPersonController playerController;
    private ActiveWeapon activeWeapon;
    private WeaponSwitcher weaponSwitcher;
    private bool isOpen = false;
    
    public bool IsOpen => isOpen;
    
    private void Start()
    {
        shopPanel.SetActive(false);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            activeWeapon = player.GetComponentInChildren<ActiveWeapon>();
            weaponSwitcher = player.GetComponentInChildren<WeaponSwitcher>();
        }
        
        closeButton.onClick.AddListener(CloseShop);
        
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i;
            upgradeButtons[i].onClick.AddListener(() => PurchaseUpgrade(index));
        }
    }
    
    public void OpenShop(int waveNumber)
    {

        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ClearAllNotifications();
        }

        // Get random upgrades - ONLY ONCE when shop opens
        currentUpgrades = upgradeManager.GetRandomUpgrades();
        
        isOpen = true;
        shopPanel.SetActive(true);
        UpdateUI(waveNumber);
        
        Time.timeScale = 0f;
        
        if (playerController != null)
            playerController.enabled = false;
        if (activeWeapon != null)
            activeWeapon.enabled = false;
        if (weaponSwitcher != null)
            weaponSwitcher.enabled = false;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void CloseShop()
    {
        isOpen = false;
        shopPanel.SetActive(false);
        
        Time.timeScale = 1f;
        
        if (playerController != null)
            playerController.enabled = true;
        if (activeWeapon != null)
            activeWeapon.enabled = true;
        if (weaponSwitcher != null)
            weaponSwitcher.enabled = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void UpdateUI(int waveNumber)
    {
        if (waveText != null)
            waveText.text = $"Wave {waveNumber} Complete!";
        
        if (playerScoreText != null && ScoreManager.Instance != null)
            playerScoreText.text = $"Score: {ScoreManager.Instance.CurrentScore}";
        
        for (int i = 0; i < upgradeButtons.Length && i < currentUpgrades.Count; i++)
        {
            ShopUpgrade upgrade = currentUpgrades[i];
            if (upgrade == null) continue;
            
            int purchases = upgradeManager.GetPurchaseCount(upgrade);
            bool isMaxed = purchases >= upgrade.maxPurchases;
            int cost = upgradeManager.GetUpgradeCost(upgrade);
            
            // Debug log to see button state
            Debug.Log($"Button {i}: {upgrade.upgradeName} - Cost: {cost}, Score: {ScoreManager.Instance.CurrentScore}, CanAfford: {ScoreManager.Instance.CurrentScore >= cost}, IsMaxed: {isMaxed}");
            
            if (upgradeNameTexts != null && i < upgradeNameTexts.Length && upgradeNameTexts[i] != null)
                upgradeNameTexts[i].text = upgrade.upgradeName;
            
            if (upgradeCostTexts != null && i < upgradeCostTexts.Length && upgradeCostTexts[i] != null)
                upgradeCostTexts[i].text = isMaxed ? "MAX" : cost.ToString();
            
            if (upgradeDescTexts != null && i < upgradeDescTexts.Length && upgradeDescTexts[i] != null)
                upgradeDescTexts[i].text = GetUpgradeDescription(upgrade);
            
            // This should disable the button if can't afford
            upgradeButtons[i].interactable = !isMaxed && ScoreManager.Instance.CurrentScore >= cost;
        }
    }
    
    private string GetUpgradeDescription(ShopUpgrade upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case ShopUpgrade.UpgradeType.Heal:
                return "Restore full health";
            case ShopUpgrade.UpgradeType.IncreaseAmmoCapacity:
                return $"+{upgrade.ammoCapacityIncrease} ammo capacity";
            case ShopUpgrade.UpgradeType.IncreaseDamage:
                int nextDamage = upgradeManager.GetNextDamageIncrease();
                return $"+{nextDamage} damage";
            case ShopUpgrade.UpgradeType.IncreaseCriticalChance:
                return $"+{upgrade.criticalChanceIncrease}% critical chance";
            case ShopUpgrade.UpgradeType.IncreaseHealthRegen:
                return $"+{upgrade.healthRegenAmount} HP per 5 sec";
            case ShopUpgrade.UpgradeType.IncreaseMoveSpeed:
                return $"+{upgrade.moveSpeedIncrease}% move speed";
            case ShopUpgrade.UpgradeType.IncreaseMaxHealth:
                return $"+{upgrade.maxHealthIncrease} max health";
            case ShopUpgrade.UpgradeType.LeechRounds:
                return $"+{upgrade.leechAmount} HP on kill";
            case ShopUpgrade.UpgradeType.ExplosiveRounds:
                return "Bullets deal AoE damage";
            default:
                return upgrade.description;
        }
    }
    
    private void PurchaseUpgrade(int index)
    {
        if (index >= currentUpgrades.Count) return;
        
        ShopUpgrade upgrade = currentUpgrades[index];
        if (upgrade == null) return;
        
        // Get the actual cost
        int cost = upgradeManager.GetUpgradeCost(upgrade);
        
        // Double-check if player can afford
        if (ScoreManager.Instance.CurrentScore < cost)
        {
            Debug.Log($"Cannot afford {upgrade.upgradeName}! Need {cost}, have {ScoreManager.Instance.CurrentScore}");
            return;
        }
        
        // Check if already maxed
        int purchases = upgradeManager.GetPurchaseCount(upgrade);
        if (purchases >= upgrade.maxPurchases)
        {
            Debug.Log($"{upgrade.upgradeName} already at max level!");
            return;
        }
        
        if (upgradeManager.PurchaseUpgrade(upgrade))
        {
            UpdateUI(WaveManager.Instance.CurrentWave - 1);
            Debug.Log($"Purchased {upgrade.upgradeName} for {cost}!");
        }
    }
}