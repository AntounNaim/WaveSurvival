using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }
    
    [SerializeField] private GameObject floatingTextPrefab;
    
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();
    private float notificationSpacing = 60f;
    private Vector2 startPosition = new Vector2(120, -60);
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
        CreateNotificationPanel();
    }
    
    public void ClearAllNotifications()
    {
        // Clear all active notifications
        while (activeNotifications.Count > 0)
        {
            GameObject notif = activeNotifications.Dequeue();
            if (notif != null)
                Destroy(notif);
        }
        activeNotifications.Clear();
    }

        public void ShowFloatingText(string message, Vector3 worldPosition, Color color)
    {
        ShowFloatingText(message, color);
    }
    
    public void ShowFloatingText(string message, Color color)
    {
        if (floatingTextPrefab == null)
        {
            CreateDefaultTextPrefab();
        }
        
        GameObject notification = Instantiate(floatingTextPrefab, transform);
        
        TextMeshProUGUI text = notification.GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = message;
            text.color = color;
            text.fontSize = 48;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            
            text.outlineWidth = 0.3f;
            text.outlineColor = Color.black;
        }
        
        RectTransform rect = notification.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0, 1);
        
        rect.anchoredPosition = new Vector2(180, 80) + new Vector2(0, -activeNotifications.Count * notificationSpacing);
        
        rect.sizeDelta = new Vector2(350, 60);
        
        activeNotifications.Enqueue(notification);
        
        while (activeNotifications.Count > 4)
        {
            GameObject oldest = activeNotifications.Dequeue();
            if (oldest != null)
                Destroy(oldest);
        }
        
        ShiftNotifications();
        StartCoroutine(RemoveNotification(notification));
    }
    
    private void ShiftNotifications()
    {
        int index = 0;
        foreach (GameObject notif in activeNotifications)
        {
            if (notif != null)
            {
                RectTransform rect = notif.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(180, 80) + new Vector2(0, -index * notificationSpacing);
            }
            index++;
        }
    }
    
    private IEnumerator RemoveNotification(GameObject notification)
    {
        yield return new WaitForSeconds(1.2f);
        
        // Check if notification still exists
        if (notification == null)
        {
            RemoveFromQueue(notification);
            yield break;
        }
        
        CanvasGroup group = notification.GetComponent<CanvasGroup>();
        if (group == null)
            group = notification.AddComponent<CanvasGroup>();
        
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            if (group != null && notification != null)
                group.alpha = 1f - (elapsed / 0.3f);
            yield return null;
        }
        
        RemoveFromQueue(notification);
        
        if (notification != null)
            Destroy(notification);
        
        ShiftNotifications();
    }
    
    private void RemoveFromQueue(GameObject notification)
    {
        if (activeNotifications.Contains(notification))
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            while (activeNotifications.Count > 0)
            {
                GameObject item = activeNotifications.Dequeue();
                if (item != null && item != notification)
                    newQueue.Enqueue(item);
            }
            activeNotifications = newQueue;
        }
    }
    
    private void CreateDefaultTextPrefab()
    {
        floatingTextPrefab = new GameObject("FloatingTextPrefab");
        TextMeshProUGUI tmp = floatingTextPrefab.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 48;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        
        RectTransform rect = floatingTextPrefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(350, 60);
    }
    
    private void CreateNotificationPanel()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        canvas.sortingOrder = 1000;
    }
}