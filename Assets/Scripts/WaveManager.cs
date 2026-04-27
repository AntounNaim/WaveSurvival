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
        // Waves 1-4: slight increase (1.0 to 1.2)
        // Wave 5: SWARM - spike to 1.8
        // Waves 6-9: gradual from 1.3 to 1.5
        // Wave 10: BOSS SWARM - spike to 2.5
        // Waves 11-14: gradual from 1.8 to 2.2
        // Wave 15: ELITE SWARM - spike to 3.0
        // Waves 16-19: gradual from 2.5 to 2.8
        // Wave 20: APOCALYPSE - spike to 4.0
        
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
            // Gradual increase between spikes
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
            return 2;  // Double enemies
        else if (currentWave == 10)
            return 3;  // Triple enemies
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
        
        // Show wave number with special name
        if (waveText != null)
        {
            waveText.text = GetWaveDisplayName();
            
            // Change color for special waves
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
        enemiesToSpawn *= multiplier;
        enemiesToSpawn = Mathf.Min(enemiesToSpawn, 40); // Cap at 40 enemies
        
        Debug.Log($"Wave {currentWave} starting! Enemies: {enemiesToSpawn} (Health Multiplier: {GetHealthMultiplier()}x)");
        
        if (multiplier > 1)
        {
            Debug.Log($"!!! SPECIAL WAVE! {multiplier}x enemies !!!");
        }
        
        // Spawn enemies gradually
        while (enemiesToSpawn > 0)
        {
            if (enemySpawner.ActiveEnemyCount < enemySpawner.MaxActiveEnemies)
            {
                enemySpawner.SpawnEnemy();
                enemiesToSpawn--;
                
                // Faster spawn rate for special waves
                float spawnDelay = (multiplier > 1) ? Random.Range(0.3f, 0.8f) : Random.Range(0.5f, 1.5f);
                yield return new WaitForSeconds(spawnDelay);
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
        // Base enemies: 5 + (wave * 2), capped at 20 before multiplier
        int baseEnemies = 5 + (currentWave - 1) * 2;
        return Mathf.Min(baseEnemies, 20);
    }
}