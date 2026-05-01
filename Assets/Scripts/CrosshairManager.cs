using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager Instance { get; private set; }
    
    [Header("Crosshair Settings")]
    [SerializeField] private Sprite crosshairSprite;
    [SerializeField] private float crosshairSize = 30f;
    [SerializeField] private Color crosshairColor = Color.white;
    
    [Header("Dynamic Crosshair")]
    [SerializeField] private bool enableDynamicSpread = true;
    [SerializeField] private float maxSpreadSize = 50f;
    [SerializeField] private float spreadRecoverySpeed = 10f;
    
    [Header("Hit Effect")]
    [SerializeField] private bool enableHitEffect = true;
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.1f;
    
    private Image crosshairImage;
    private GameObject crosshairObject;
    private float currentSpread = 0f;
    private Color originalColor;
    private float hitFlashTimer = 0f;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        CreateCrosshair();
    }
    
    private void CreateCrosshair()
    {
        // Create a Canvas for crosshair
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // Create crosshair object
        crosshairObject = new GameObject("Crosshair");
        crosshairObject.transform.SetParent(transform);
        
        // Add RectTransform
        RectTransform rect = crosshairObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(crosshairSize, crosshairSize);
        
        // Add Image component
        crosshairImage = crosshairObject.AddComponent<Image>();
        
        if (crosshairSprite != null)
        {
            crosshairImage.sprite = crosshairSprite;
            crosshairImage.preserveAspect = true;
        }
        else
        {
            CreateDefaultCrosshair();
        }
        
        crosshairImage.color = crosshairColor;
        originalColor = crosshairColor;
    }
    
    private void CreateDefaultCrosshair()
    {
        // Create a simple crosshair using 4 lines
        // Top line
        CreateLine("Top", new Vector2(0, 15), new Vector2(4, 15));
        // Bottom line
        CreateLine("Bottom", new Vector2(0, -15), new Vector2(4, 15));
        // Left line
        CreateLine("Left", new Vector2(-15, 0), new Vector2(15, 4));
        // Right line
        CreateLine("Right", new Vector2(15, 0), new Vector2(15, 4));
        // Center dot
        CreateDot();
        
        // Hide the main image
        crosshairImage.enabled = false;
    }
    
    private void CreateLine(string name, Vector2 position, Vector2 size)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(crosshairObject.transform);
        RectTransform rect = line.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image img = line.AddComponent<Image>();
        img.color = crosshairColor;
    }
    
    private void CreateDot()
    {
        GameObject dot = new GameObject("Dot");
        dot.transform.SetParent(crosshairObject.transform);
        RectTransform rect = dot.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(4, 4);
        Image img = dot.AddComponent<Image>();
        img.color = crosshairColor;
    }
    
    private void Update()
    {
        if (!enableDynamicSpread) return;
        
        // Recover spread over time
        currentSpread = Mathf.Lerp(currentSpread, 0f, Time.deltaTime * spreadRecoverySpeed);
        
        // Apply spread to crosshair
        float spreadAmount = Mathf.Lerp(crosshairSize, maxSpreadSize, currentSpread);
        RectTransform rect = crosshairObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(spreadAmount, spreadAmount);
        
        // Handle hit flash
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            if (hitFlashTimer <= 0f)
            {
                crosshairImage.color = originalColor;
                // Reset line colors if using default crosshair
                ResetLineColors();
            }
        }
    }
    
    public void AddSpread(float amount)
    {
        if (!enableDynamicSpread) return;
        currentSpread = Mathf.Min(currentSpread + amount, 1f);
    }
    
    public void OnHit()
    {
        if (!enableHitEffect) return;
        
        crosshairImage.color = hitColor;
        // Change line colors if using default crosshair
        SetLineColors(hitColor);
        hitFlashTimer = hitFlashDuration;
    }
    
    private void SetLineColors(Color color)
    {
        foreach (Transform child in crosshairObject.transform)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                img.color = color;
            }
        }
    }
    
    private void ResetLineColors()
    {
        foreach (Transform child in crosshairObject.transform)
        {
            Image img = child.GetComponent<Image>();
            if (img != null)
            {
                img.color = crosshairColor;
            }
        }
    }
    
    public void SetCrosshairSprite(Sprite sprite)
    {
        crosshairSprite = sprite;
        if (crosshairImage != null)
        {
            crosshairImage.sprite = sprite;
            crosshairImage.enabled = true;
            
            // Destroy default lines if they exist
            foreach (Transform child in crosshairObject.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    public void SetCrosshairColor(Color color)
    {
        crosshairColor = color;
        originalColor = color;
        crosshairImage.color = color;
        ResetLineColors();
    }
}