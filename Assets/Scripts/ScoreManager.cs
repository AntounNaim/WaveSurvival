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
        int multiplier = 1;
        if (currentCombo >= 20)
            multiplier = 3;
        else if (currentCombo >= 10)
            multiplier = 2;
        else if (currentCombo >= 5)
            multiplier = 1;
        
        int finalAmount = amount * multiplier;
        currentScore += finalAmount;
        
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }
        
        UpdateScoreUI();
        
        if (FloatingTextManager.Instance != null)
        {
            string multiplierText = multiplier > 1 ? $" x{multiplier}!" : "";
            FloatingTextManager.Instance.ShowFloatingText($"+{finalAmount}{multiplierText}", position, Color.yellow);
        }
        
        currentCombo++;
        lastKillTime = Time.time;
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