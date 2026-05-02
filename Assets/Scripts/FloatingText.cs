using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;
    private float lifeTime = 1.2f;
    private float floatSpeed = 40f;
    private float fadeStartTime = 0.3f;
    private float remainingLife;
    private Vector3 startPosition;
    
    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = gameObject.AddComponent<TextMeshProUGUI>();
        }
    }
    
    public void Initialize(string message, Color color)
    {
        textComponent.text = message;
        textComponent.color = color;
        textComponent.fontSize = 24;
        textComponent.fontStyle = FontStyles.Bold;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        remainingLife = lifeTime;
        startPosition = transform.position;
    }
    
    private void Update()
    {
        if (Time.timeScale == 0f) return;
        
        remainingLife -= Time.unscaledDeltaTime; // Use unscaled time
        
        if (remainingLife <= 0)
        {
            Destroy(gameObject);
            return;
        }
        
        // Float upward (using unscaled delta time so it doesn't freeze)
        Vector3 pos = transform.position;
        pos.y += floatSpeed * Time.unscaledDeltaTime;
        transform.position = pos;
        
        // Fade out
        if (remainingLife < fadeStartTime)
        {
            Color color = textComponent.color;
            color.a = remainingLife / fadeStartTime;
            textComponent.color = color;
        }
    }
}