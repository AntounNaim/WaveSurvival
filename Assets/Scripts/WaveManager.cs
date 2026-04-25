using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI waveCountdownText;
    
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private float timeBetweenWaves = 10f;
    
    [Header("Shop")]
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private GameObject overlayPanel;
    
    private bool isWaveActive = false;
    
    public static WaveManager Instance { get; private set; }
    
    public int CurrentWave => currentWave;
    public bool IsWaveActive => isWaveActive;
    
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
        StartCoroutine(WaveLoop());
    }
    
    private IEnumerator WaveLoop()
    {
        while (true)
        {
            // Open shop between waves (skip before first wave)
            if (shopUI != null && currentWave > 1)
            {
                // Show countdown BEFORE disabling overlay
                yield return StartCoroutine(ShowPreShopCountdown());
                
                // Now disable overlay
                if (overlayPanel != null)
                    overlayPanel.SetActive(false);
                
                // Open shop
                shopUI.OpenShop(currentWave);
                yield return new WaitWhile(() => shopUI.IsOpen);
                
                // Re-enable overlay after shop closes
                if (overlayPanel != null)
                    overlayPanel.SetActive(true);
            }
            
            // Show countdown before wave
            yield return StartCoroutine(ShowPreWaveCountdown());
            
            // Start wave
            yield return StartCoroutine(StartWave());
            
            // Wait for wave to complete
            yield return new WaitUntil(() => !isWaveActive);
            
            currentWave++;
        }
    }
    
    private IEnumerator ShowPreShopCountdown()
    {
        float countdown = 3f;
        
        while (countdown > 0)
        {
            if (waveCountdownText != null)
            {
                waveCountdownText.text = $"Shop in: {Mathf.CeilToInt(countdown)}";
            }
            countdown -= Time.deltaTime;
            yield return null;
        }
        
        if (waveCountdownText != null)
        {
            waveCountdownText.text = "";
        }
    }
    
    private IEnumerator ShowPreWaveCountdown()
    {
        float countdown = timeBetweenWaves;
        
        while (countdown > 0)
        {
            if (waveCountdownText != null)
            {
                waveCountdownText.text = $"Next Wave: {Mathf.CeilToInt(countdown)}";
            }
            countdown -= Time.deltaTime;
            yield return null;
        }
        
        if (waveCountdownText != null)
        {
            waveCountdownText.text = "";
        }
    }
    
    private IEnumerator StartWave()
    {
        isWaveActive = true;
        
        // Show wave number
        if (waveText != null)
        {
            waveText.text = $"WAVE {currentWave}";
            waveText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            waveText.gameObject.SetActive(false);
        }
        
        // Calculate enemies for this wave
        int enemiesToSpawn = CalculateEnemiesForWave();
        
        Debug.Log($"Wave {currentWave} starting! Enemies: {enemiesToSpawn}");
        
        // Spawn enemies gradually
        while (enemiesToSpawn > 0)
        {
            if (enemySpawner.ActiveEnemyCount < enemySpawner.MaxActiveEnemies)
            {
                enemySpawner.SpawnEnemy();
                enemiesToSpawn--;
                yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Wait for all enemies to die
        yield return new WaitUntil(() => enemySpawner.ActiveEnemyCount == 0);
        
        isWaveActive = false;
        Debug.Log($"Wave {currentWave} complete!");
    }
    
    private int CalculateEnemiesForWave()
    {
        // Base enemies: 5 + (wave * 2), capped at 30
        int baseEnemies = 5 + (currentWave - 1) * 2;
        return Mathf.Min(baseEnemies, 30);
    }
}