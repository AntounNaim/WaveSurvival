using System.Collections;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI waveCountdownText;
    [SerializeField] private TextMeshProUGUI enemiesLeftText;
    
    [Header("Wave Settings")]
    [SerializeField] private int currentWave = 1;
    [SerializeField] private float timeBetweenWaves = 10f;
    
    [Header("Shop")]
    [SerializeField] private ShopUI shopUI;
    [SerializeField] private GameObject overlayPanel;
    
    private bool isWaveActive = false;
    private int totalEnemiesInWave = 0;
    private int enemiesRemaining = 0;
    
    public static WaveManager Instance { get; private set; }
    
    public int CurrentWave => currentWave;
    public bool IsWaveActive => isWaveActive;
    public int EnemiesRemaining => enemiesRemaining;
    
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
    
    // Get wave display name
    public string GetWaveDisplayName()
    {
        if (currentWave == 5)
            return $"WAVE {currentWave}: SWARM";
        else if (currentWave == 10)
            return $"WAVE {currentWave}: BOSS SWARM";
        else if (currentWave == 15)
            return $"WAVE {currentWave}: ELITE SWARM";
        else if (currentWave == 20)
            return $"WAVE {currentWave}: APOCALYPSE";
        else
            return $"WAVE {currentWave}";
    }
    
    // Get wave-specific multiplier
    public float GetHealthMultiplier()
    {
        if (currentWave == 5)
            return 1.8f;
        else if (currentWave == 10)
            return 2.5f;
        else if (currentWave == 15)
            return 3.0f;
        else if (currentWave == 20)
            return 4.0f;
        else
        {
            if (currentWave < 5)
                return 1.0f + (currentWave - 1) * 0.07f;
            else if (currentWave < 10)
                return 1.3f + (currentWave - 5) * 0.08f;
            else if (currentWave < 15)
                return 1.8f + (currentWave - 10) * 0.1f;
            else
                return 2.5f + (currentWave - 15) * 0.12f;
        }
    }
    
    // Get enemy count multiplier for special waves
    private int GetEnemyCountMultiplier()
    {
        if (currentWave == 5)
            return 2;
        else if (currentWave == 10)
            return 3;
        else if (currentWave == 15)
            return 3;
        else if (currentWave == 20)
            return 4;
        else
            return 1;
    }
    
    private IEnumerator WaveLoop()
    {
        while (true)
        {
            if (shopUI != null && currentWave > 1)
            {
                yield return StartCoroutine(ShowPreShopCountdown());
                
                if (overlayPanel != null)
                    overlayPanel.SetActive(false);
                
                shopUI.OpenShop(currentWave);
                yield return new WaitWhile(() => shopUI.IsOpen);
                
                if (overlayPanel != null)
                    overlayPanel.SetActive(true);
            }
            
            yield return StartCoroutine(ShowPreWaveCountdown());
            yield return StartCoroutine(StartWave());
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
        
        if (waveText != null)
        {
            waveText.text = GetWaveDisplayName();
            
            if (currentWave == 5 || currentWave == 10 || currentWave == 15 || currentWave == 20)
            {
                waveText.color = Color.red;
            }
            else
            {
                waveText.color = Color.white;
            }
            
            waveText.gameObject.SetActive(true);
            yield return new WaitForSeconds(2f);
            waveText.gameObject.SetActive(false);
        }
        
        int enemiesToSpawn = CalculateEnemiesForWave();
        int multiplier = GetEnemyCountMultiplier();
        totalEnemiesInWave = enemiesToSpawn * multiplier;
        enemiesRemaining = totalEnemiesInWave;
        enemiesToSpawn = Mathf.Min(enemiesToSpawn * multiplier, 40);
        
        // Update enemies left UI
        UpdateEnemiesLeftUI();
        
        Debug.Log($"Wave {currentWave} starting! Enemies: {totalEnemiesInWave} (Health Multiplier: {GetHealthMultiplier()}x)");
        
        if (multiplier > 1)
        {
            Debug.Log($"!!! SPECIAL WAVE! {multiplier}x enemies !!!");
        }
        
        while (enemiesToSpawn > 0)
        {
            if (enemySpawner.ActiveEnemyCount < enemySpawner.MaxActiveEnemies)
            {
                enemySpawner.SpawnEnemy();
                enemiesToSpawn--;
                
                float spawnDelay = (multiplier > 1) ? Random.Range(0.3f, 0.8f) : Random.Range(0.5f, 1.5f);
                yield return new WaitForSeconds(spawnDelay);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        yield return new WaitUntil(() => enemySpawner.ActiveEnemyCount == 0 && !EnemyHealth.HasActiveMinions);
        
        isWaveActive = false;
        Debug.Log($"Wave {currentWave} complete!");
    }
    
    private void UpdateEnemiesLeftUI()
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.text = $"Enemies Left: {enemiesRemaining}";
        }
    }
    
    public void OnEnemyDied()
    {
        enemiesRemaining--;
        UpdateEnemiesLeftUI();
    }
    
    public void ResetWave()
    {
        StopAllCoroutines();
        currentWave = 1;
        isWaveActive = false;
        
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        
        StartCoroutine(WaveLoop());
    }
    
    private int CalculateEnemiesForWave()
    {
        int baseEnemies = 5 + (currentWave - 1) * 2;
        return Mathf.Min(baseEnemies, 20);
    }

    public void OnMinionSpawned()
    {
        enemiesRemaining++;
        UpdateEnemiesLeftUI();
    }
}