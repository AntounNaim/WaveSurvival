using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitMarker : MonoBehaviour
{
    public static HitMarker Instance { get; private set; }
    
    [Header("Hit Marker Settings")]
    [SerializeField] private Sprite hitMarkerSprite;
    [SerializeField] private float displayDuration = 0.15f;
    [SerializeField] private float fadeDuration = 0.1f;
    [SerializeField] private float normalSize = 50f;
    [SerializeField] private float critSize = 70f;
    
    [Header("Colors")]
    [SerializeField] private Color normalHitColor = Color.white;
    [SerializeField] private Color killHitColor = Color.red;
    [SerializeField] private Color critHitColor = new Color(1f, 0.7f, 0f); // Orange/Gold
    
    private GameObject hitMarkerObject;
    private CanvasGroup canvasGroup;
    private Image hitMarkerImage;
    private RectTransform rectTransform;
    private Coroutine currentCoroutine;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        CreateHitMarker();
    }
    
    private void CreateHitMarker()
    {
        // Create a Canvas
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        
        // Create the hit marker container
        hitMarkerObject = new GameObject("HitMarker");
        hitMarkerObject.transform.SetParent(transform);
        
        // Add RectTransform
        rectTransform = hitMarkerObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(normalSize, normalSize);
        
        // Add Image component
        hitMarkerImage = hitMarkerObject.AddComponent<Image>();
        
        // Use custom sprite if provided
        if (hitMarkerSprite != null)
        {
            hitMarkerImage.sprite = hitMarkerSprite;
            hitMarkerImage.preserveAspect = true;
        }
        else
        {
            // Create a simple X if no sprite provided (fallback)
            CreateDefaultX();
            hitMarkerImage = null;
        }
        
        if (hitMarkerImage != null)
            hitMarkerImage.color = normalHitColor;
        
        // Add canvas group for fading
        canvasGroup = hitMarkerObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }
    
    private void CreateDefaultX()
    {
        // Create a simple X using two UI images (fallback if no sprite)
        GameObject line1 = new GameObject("Line1");
        line1.transform.SetParent(hitMarkerObject.transform);
        RectTransform rect1 = line1.AddComponent<RectTransform>();
        rect1.sizeDelta = new Vector2(40, 4);
        rect1.anchoredPosition = Vector2.zero;
        rect1.localRotation = Quaternion.Euler(0, 0, 45);
        Image img1 = line1.AddComponent<Image>();
        img1.color = Color.white;
        
        GameObject line2 = new GameObject("Line2");
        line2.transform.SetParent(hitMarkerObject.transform);
        RectTransform rect2 = line2.AddComponent<RectTransform>();
        rect2.sizeDelta = new Vector2(40, 4);
        rect2.anchoredPosition = Vector2.zero;
        rect2.localRotation = Quaternion.Euler(0, 0, -45);
        Image img2 = line2.AddComponent<Image>();
        img2.color = Color.white;
    }
    
    public void ShowHitMarker(bool isKill = false, bool isCritical = false)
    {
        // Trigger crosshair flash
        if (CrosshairManager.Instance != null)
        {
            CrosshairManager.Instance.OnHit();
        }
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        
        // Determine color and size based on hit type
        Color targetColor = normalHitColor;
        float targetSize = normalSize;
        
        if (isCritical)
        {
            targetColor = critHitColor;
            targetSize = critSize;
            Debug.Log("CRITICAL HIT MARKER!");
        }
        else if (isKill)
        {
            targetColor = killHitColor;
            targetSize = normalSize;
        }
        
        // Apply color
        if (hitMarkerImage != null)
        {
            hitMarkerImage.color = targetColor;
        }
        else
        {
            // For default X, change line colors
            Image[] lines = hitMarkerObject.GetComponentsInChildren<Image>();
            foreach (Image img in lines)
            {
                img.color = targetColor;
            }
        }
        
        // Apply size
        rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
        
        // Add a quick scale pop animation
        rectTransform.localScale = Vector3.one * 1.3f;
        StartCoroutine(ScalePop());
        
        hitMarkerObject.SetActive(true);
        canvasGroup.alpha = 1f;
        
        currentCoroutine = StartCoroutine(FadeHitMarker());
    }
    
    private IEnumerator ScalePop()
    {
        yield return new WaitForSeconds(0.05f);
        rectTransform.localScale = Vector3.one;
    }
    
    private IEnumerator FadeHitMarker()
    {
        yield return new WaitForSeconds(displayDuration);
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        hitMarkerObject.SetActive(false);
        currentCoroutine = null;
    }
}