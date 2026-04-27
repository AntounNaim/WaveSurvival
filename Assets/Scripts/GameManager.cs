using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private GameObject gameOverPanel;
    
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
    
    public void GameOver()
    {
        Debug.Log("GAME OVER!");
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Freeze time
        Time.timeScale = 0f;
    }
    
    public void RestartGame()
{
    // Reset upgrades before reloading
    if (UpgradeManager.Instance != null)
    {
        UpgradeManager.Instance.ResetUpgrades();
    }
    
    // Reset score
    if (ScoreManager.Instance != null)
    {
        ScoreManager.Instance.ResetScore();
    }
    
    Time.timeScale = 1f;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
}