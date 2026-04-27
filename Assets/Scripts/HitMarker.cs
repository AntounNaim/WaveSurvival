using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitMarker : MonoBehaviour
{
    public static HitMarker Instance { get; private set; }
    
    private GameObject hitMarkerObject;
    private CanvasGroup canvasGroup;
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
        
        // Create the X shape using two rectangles
        RectTransform rect = hitMarkerObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(80, 80);
        
        // Line 1 ( \ )
        GameObject line1 = new GameObject("Line1");
        line1.transform.SetParent(hitMarkerObject.transform);
        RectTransform rect1 = line1.AddComponent<RectTransform>();
        rect1.sizeDelta = new Vector2(60, 6);
        rect1.anchoredPosition = Vector2.zero;
        rect1.localRotation = Quaternion.Euler(0, 0, 45);
        Image img1 = line1.AddComponent<Image>();
        img1.color = Color.white;
        
        // Line 2 ( / )
        GameObject line2 = new GameObject("Line2");
        line2.transform.SetParent(hitMarkerObject.transform);
        RectTransform rect2 = line2.AddComponent<RectTransform>();
        rect2.sizeDelta = new Vector2(60, 6);
        rect2.anchoredPosition = Vector2.zero;
        rect2.localRotation = Quaternion.Euler(0, 0, -45);
        Image img2 = line2.AddComponent<Image>();
        img2.color = Color.white;
        
        // Add canvas group for fading
        canvasGroup = hitMarkerObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }
    
    public void ShowHitMarker(bool isKill = false)
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        
        // Set color
        Image[] images = hitMarkerObject.GetComponentsInChildren<Image>();
        Color targetColor = isKill ? Color.red : Color.white;
        foreach (Image img in images)
        {
            img.color = targetColor;
        }
        
        hitMarkerObject.SetActive(true);
        canvasGroup.alpha = 1f;
        
        currentCoroutine = StartCoroutine(FadeHitMarker());
    }
    
    private IEnumerator FadeHitMarker()
    {
        yield return new WaitForSeconds(0.1f);
        
        float elapsed = 0f;
        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.1f);
            yield return null;
        }
        
        hitMarkerObject.SetActive(false);
        currentCoroutine = null;
    }
}