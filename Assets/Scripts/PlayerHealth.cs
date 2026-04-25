using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    
    [Header("HUD")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 1f;
    private bool isInvincible = false;
    
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    public void TakeDamage(int damage)
    {
        if (isInvincible || !IsAlive) return;
        
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        UpdateHealthUI();
        
        isInvincible = true;
        Invoke(nameof(EndInvincibility), invincibilityDuration);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void EndInvincibility()
    {
        isInvincible = false;
    }
    
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
        Debug.Log($"Player healed! Health: {currentHealth}/{maxHealth}");
    }
    
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
    
    private void Die()
    {
        Debug.Log("PLAYER DIED!");
        
        FirstPersonController controller = GetComponent<FirstPersonController>();
        if (controller != null)
            controller.enabled = false;
        
        ActiveWeapon activeWeapon = GetComponentInChildren<ActiveWeapon>();
        if (activeWeapon != null)
            activeWeapon.enabled = false;
    }
    
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        isInvincible = false;
    }

    public void HealFull()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log($"Player fully healed! Health: {currentHealth}/{maxHealth}");
    }

}