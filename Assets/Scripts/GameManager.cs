using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI waveReachedText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Pause UI")]
    [SerializeField] private GameObject pausePanel;
    
    private bool isGameOver = false;
    private bool isPaused = false;
    
    public bool IsGameOver => isGameOver;
    
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
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void Update()
    {
        if (!isGameOver && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    
    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        isPaused = false;
        Time.timeScale = 0f;
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (finalScoreText != null && ScoreManager.Instance != null)
                finalScoreText.text = $"SCORE: {ScoreManager.Instance.CurrentScore}";
            
            if (waveReachedText != null && WaveManager.Instance != null)
                waveReachedText.text = $"WAVE: {WaveManager.Instance.CurrentWave - 1}";
            
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (highScoreText != null)
                highScoreText.text = $"HIGH SCORE: {highScore}";
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void RestartGame()
    {
        // Reset time scale first
        Time.timeScale = 1f;
        
        // Reset all managers
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ResetUpgrades();
        
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
        
        // Reset wave manager state
        if (WaveManager.Instance != null)
        {
            // You may need to add a Reset method to WaveManager
            WaveManager.Instance.ResetWave();
        }
        
        // Clear any active enemies or minions
        ClearAllEnemies();
        
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ClearAllEnemies()
    {
        // Find and destroy all enemies
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void TogglePause()
    {
        if (isGameOver) return;
        
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);
        
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isPaused;
    }
    
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}