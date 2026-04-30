using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Combo Settings")]
    [SerializeField] private float comboDuration = 3f;
    private int currentCombo = 0;
    private float lastKillTime;
    
    public int CurrentScore => currentScore;
    public int CurrentCombo => currentCombo;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        currentScore = 0;
        UpdateScoreUI();
    }
    
    // For purchases and deductions (NO combo multiplier)
    public void DeductScore(int amount)
    {
        currentScore -= amount;
        
        if (currentScore < 0)
        {
            Debug.LogWarning($"Score went negative ({currentScore}), clamping to 0");
            currentScore = 0;
        }
        
        UpdateScoreUI();
        Debug.Log($"Score deducted: -{amount}. New score: {currentScore}");
    }
    
    // For kills and score pickups (WITH combo multiplier)
        public void AddScore(int amount, Vector3 position)
    {
        currentScore += amount;
        
        // No combo multiplier
        UpdateScoreUI();
        
        if (FloatingTextManager.Instance != null)
        {
            FloatingTextManager.Instance.ShowFloatingText($"+{amount}", position, Color.yellow);
        }
    }
    
    // Simple version for loot pickups that don't need position
    public void AddScore(int amount)
    {
        AddScore(amount, Vector3.zero);
    }
    
    public void ResetCombo()
    {
        currentCombo = 0;
    }
    
    private void Update()
    {
        if (currentCombo > 0 && Time.time > lastKillTime + comboDuration)
        {
            ResetCombo();
        }
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
            
            if (currentCombo > 1)
            {
                scoreText.text += $"  x{currentCombo}";
            }
        }
    }
    
    public void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        UpdateScoreUI();
    }
}