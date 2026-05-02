using UnityEngine;
using UnityEngine.InputSystem;

public class SkyboxController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    private float currentRotation = 0f;
    
    [Header("Color Cycle Settings")]
    [SerializeField] private float colorSpeed = 0.2f;
    [SerializeField] private float colorIntensity = 0.8f;
    private float hue = 0f;
    
    void Update()
    {
        // ========== MOUSE ROTATION ==========
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            currentRotation += mouseDelta.x * rotationSpeed * Time.deltaTime;
            
            if (currentRotation >= 360f) currentRotation -= 360f;
            if (currentRotation < 0f) currentRotation += 360f;
        }
        
        // Apply rotation to skybox
        RenderSettings.skybox.SetFloat("_Rotation", currentRotation);
        
        // ========== COLOR CYCLING ==========
        hue += Time.deltaTime * colorSpeed;
        if (hue >= 1f) hue -= 1f;
        
        Color currentColor = Color.HSVToRGB(hue, 0.6f, colorIntensity);
        RenderSettings.skybox.SetColor("_Tint", currentColor);
        
        RenderSettings.ambientLight = currentColor * 0.3f;
    }
}