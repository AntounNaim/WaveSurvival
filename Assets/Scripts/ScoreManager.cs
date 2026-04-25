using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    [Header("Score Settings")]
    [SerializeField] private int currentScore = 0;
    
    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Combo Settings (Optional)")]
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
        UpdateScoreUI();
    }
    
    public void AddScore(int amount)
    {
        // Apply combo multiplier (combo 5 = x1.5, combo 10 = x2, etc.)
        int multiplier = 1;
        if (currentCombo >= 10)
            multiplier = 2;
        else if (currentCombo >= 5)
            multiplier = 1;
        
        int finalAmount = amount * multiplier;
        currentScore += finalAmount;
        
        Debug.Log($"Score +{finalAmount} (Base: {amount}, Combo: x{multiplier})");
        
        UpdateScoreUI();
        
        // Update combo
        currentCombo++;
        lastKillTime = Time.time;
    }
    
    public void ResetCombo()
    {
        currentCombo = 0;
    }
    
    private void Update()
    {
        // Reset combo if no kills for comboDuration seconds
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
            
            // Optional: Show combo
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