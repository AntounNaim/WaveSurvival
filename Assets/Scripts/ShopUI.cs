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
    
    [Header("Upgrade Settings")]
    [SerializeField] private int healCost = 200;
    [SerializeField] private int ammoCost = 150;
    [SerializeField] private int damageCost = 300;
    [SerializeField] private int damageIncrease = 5;
    
    private int currentDamageBonus = 0;
    private bool isOpen = false;
    
    // References to player components to disable
    private FirstPersonController playerController;
    private ActiveWeapon activeWeapon;
    private WeaponSwitcher weaponSwitcher;
    
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
        
        healButton.onClick.AddListener(BuyHeal);
        ammoButton.onClick.AddListener(BuyAmmo);
        damageButton.onClick.AddListener(BuyDamage);
        closeButton.onClick.AddListener(CloseShop);
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
        
        // Show and unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void CloseShop()
    {
        isOpen = false;
        shopPanel.SetActive(false);
        
        // Resume time
        Time.timeScale = 1f;
        
        // Re-enable player input
        if (playerController != null)
            playerController.enabled = true;
        
        if (activeWeapon != null)
            activeWeapon.enabled = true;
        
        if (weaponSwitcher != null)
            weaponSwitcher.enabled = true;
        
        // Lock cursor back to game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void UpdateUI(int waveNumber)
    {
        if (waveText != null)
            waveText.text = $"Wave {waveNumber} Complete!";
        
        if (playerScoreText != null && ScoreManager.Instance != null)
            playerScoreText.text = $"Score: {ScoreManager.Instance.CurrentScore}";
        
        if (healCostText != null)
            healCostText.text = healCost.ToString();
        
        if (ammoCostText != null)
            ammoCostText.text = ammoCost.ToString();
        
        if (damageCostText != null)
            damageCostText.text = damageCost.ToString();
        
        if (ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.CurrentScore;
            healButton.interactable = currentScore >= healCost;
            ammoButton.interactable = currentScore >= ammoCost;
            damageButton.interactable = currentScore >= damageCost;
        }
    }
    
    private void BuyHeal()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= healCost)
        {
            ScoreManager.Instance.AddScore(-healCost);
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
            playerHealth?.HealFull();
            UpdateUI(WaveManager.Instance.CurrentWave - 1);
        }
    }
    
    private void BuyAmmo()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= ammoCost)
        {
            ScoreManager.Instance.AddScore(-ammoCost);
            ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
            if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
            {
                activeWeapon.CurrentWeapon.RefillAmmo();
            }
            UpdateUI(WaveManager.Instance.CurrentWave - 1);
        }
    }
    
    private void BuyDamage()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= damageCost)
        {
            ScoreManager.Instance.AddScore(-damageCost);
            currentDamageBonus += damageIncrease;
            Debug.Log($"Damage increased by {damageIncrease}! Total bonus: {currentDamageBonus}");
            UpdateUI(WaveManager.Instance.CurrentWave - 1);
        }
    }
}