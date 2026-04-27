using UnityEngine;
using TMPro;
using UnityEngine.UI;
using StarterAssets;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Button closeButton;
    
    [Header("Upgrade Buttons")]
    [SerializeField] private Button healButton;
    [SerializeField] private TextMeshProUGUI healCostText;
    [SerializeField] private Button ammoButton;
    [SerializeField] private TextMeshProUGUI ammoCostText;
    [SerializeField] private Button damageButton;
    [SerializeField] private TextMeshProUGUI damageCostText;
    
    [Header("References")]
    [SerializeField] private UpgradeManager upgradeManager;
    
    // Store the actual upgrade data for each button
    private ShopUpgrade healUpgrade;
    private ShopUpgrade ammoUpgrade;
    private ShopUpgrade damageUpgrade;
    
    private FirstPersonController playerController;
    private ActiveWeapon activeWeapon;
    private WeaponSwitcher weaponSwitcher;
    private bool isOpen = false;
    
    public bool IsOpen => isOpen;
    
    private void Start()
    {
        shopPanel.SetActive(false);
        
        // Find player components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<FirstPersonController>();
            activeWeapon = player.GetComponentInChildren<ActiveWeapon>();
            weaponSwitcher = player.GetComponentInChildren<WeaponSwitcher>();
        }
        
        // Find upgrades from UpgradeManager
        FindUpgrades();
        
        healButton.onClick.AddListener(() => PurchaseUpgrade(healUpgrade, healButton));
        ammoButton.onClick.AddListener(() => PurchaseUpgrade(ammoUpgrade, ammoButton));
        damageButton.onClick.AddListener(() => PurchaseUpgrade(damageUpgrade, damageButton));
        closeButton.onClick.AddListener(CloseShop);
    }
    
    private void FindUpgrades()
    {
        // Find the upgrades by type
        ShopUpgrade[] allUpgrades = upgradeManager.GetAllUpgrades();
        
        foreach (ShopUpgrade upgrade in allUpgrades)
        {
            switch (upgrade.upgradeType)
            {
                case ShopUpgrade.UpgradeType.Heal:
                    healUpgrade = upgrade;
                    break;
                case ShopUpgrade.UpgradeType.IncreaseAmmoCapacity:
                    ammoUpgrade = upgrade;
                    break;
                case ShopUpgrade.UpgradeType.IncreaseDamage:
                    damageUpgrade = upgrade;
                    break;
            }
        }
    }
    
    public void OpenShop(int waveNumber)
    {
        isOpen = true;
        shopPanel.SetActive(true);
        UpdateUI(waveNumber);
        
        // Freeze time
        Time.timeScale = 0f;
        
        // Disable player input
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
        
        // Update heal button
        if (healUpgrade != null)
        {
            int purchases = upgradeManager.GetPurchaseCount(healUpgrade);
            int maxPurchases = healUpgrade.maxPurchases;
            bool isMaxed = purchases >= maxPurchases;
            int cost = upgradeManager.GetUpgradeCost(healUpgrade);
            
            if (healCostText != null)
                healCostText.text = isMaxed ? "MAX" : cost.ToString();
            
            healButton.interactable = !isMaxed && ScoreManager.Instance.CurrentScore >= cost;
        }
        
        // Update ammo button
        if (ammoUpgrade != null)
        {
            int purchases = upgradeManager.GetPurchaseCount(ammoUpgrade);
            int maxPurchases = ammoUpgrade.maxPurchases;
            bool isMaxed = purchases >= maxPurchases;
            int cost = upgradeManager.GetUpgradeCost(ammoUpgrade);
            
            if (ammoCostText != null)
                ammoCostText.text = isMaxed ? "MAX" : cost.ToString();
            
            ammoButton.interactable = !isMaxed && ScoreManager.Instance.CurrentScore >= cost;
        }
        
        // Update damage button
        if (damageUpgrade != null)
        {
            int purchases = upgradeManager.GetPurchaseCount(damageUpgrade);
            int maxPurchases = damageUpgrade.maxPurchases;
            bool isMaxed = purchases >= maxPurchases;
            int cost = upgradeManager.GetUpgradeCost(damageUpgrade);
            int nextIncrease = upgradeManager.GetNextDamageIncrease();
            
            if (damageCostText != null)
                damageCostText.text = isMaxed ? "MAX" : $"{cost} (+{nextIncrease})";
            
            damageButton.interactable = !isMaxed && ScoreManager.Instance.CurrentScore >= cost;
        }
    }
    
    private void PurchaseUpgrade(ShopUpgrade upgrade, Button button)
{
    if (upgrade == null) return;
    
    // Let UpgradeManager handle the cost check AND deduction
    if (upgradeManager.PurchaseUpgrade(upgrade))
    {
        UpdateUI(WaveManager.Instance.CurrentWave - 1);
        Debug.Log($"Purchased {upgrade.upgradeName}!");
    }
    else
    {
        Debug.Log("Cannot afford upgrade or already maxed!");
    }
}   
    
}