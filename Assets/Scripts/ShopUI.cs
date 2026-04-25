using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private TextMeshProUGUI playerScoreText;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private WaveManager waveManager;
    
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
    
    private Weapon currentWeapon;
    private int currentDamageBonus = 0;
    
    private void Start()
    {
        shopPanel.SetActive(false);
        
        healButton.onClick.AddListener(BuyHeal);
        ammoButton.onClick.AddListener(BuyAmmo);
        damageButton.onClick.AddListener(BuyDamage);
    }
    
    public void OpenShop(int waveNumber)
    {
        shopPanel.SetActive(true);
        UpdateUI(waveNumber);
    }
    
    public void CloseShop()
    {
        shopPanel.SetActive(false);
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
        
        // Update button interactability
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
            
            if (waveManager != null)
                UpdateUI(waveManager.CurrentWave - 1);
            else
                UpdateUI(1);
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
            
            if (waveManager != null)
                UpdateUI(waveManager.CurrentWave - 1);
            else
                UpdateUI(1);
        }
    }
    
    private void BuyDamage()
    {
        if (ScoreManager.Instance != null && ScoreManager.Instance.CurrentScore >= damageCost)
        {
            ScoreManager.Instance.AddScore(-damageCost);
            currentDamageBonus += damageIncrease;
            
            // Apply damage buff to weapon
            ActiveWeapon activeWeapon = FindFirstObjectByType<ActiveWeapon>();
            if (activeWeapon != null && activeWeapon.CurrentWeapon != null)
            {
                // You'll need to add a damage buff system to Weapon
                Debug.Log($"Damage increased by {damageIncrease}! Total bonus: {currentDamageBonus}");
            }
            
            if (waveManager != null)
                UpdateUI(waveManager.CurrentWave - 1);
            else
                UpdateUI(1);
        }
    }
}