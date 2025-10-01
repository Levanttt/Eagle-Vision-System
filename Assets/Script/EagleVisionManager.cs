using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EagleVisionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Camera highlightCamera;
    
    [Header("Settings")]
    [SerializeField] private KeyCode activationKey = KeyCode.V;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseMaxRadius = 30f;
    
    [Header("Object Colors")]
    [SerializeField] private Color enemyColor = new Color(3f, 0f, 0f); // Bright red
    [SerializeField] private Color itemColor = new Color(3f, 3f, 0f); // Bright yellow
    [SerializeField] private Color interactableColor = new Color(0f, 3f, 3f); // Bright cyan
    
    [Header("Layer Settings")]
    [SerializeField] private string highlightLayerName = "EagleVisionHighlight";
    private int highlightLayer;
    
    // State
    private bool isActive = false;
    private float currentPulseRadius = 0f;
    private bool isPulsing = false;
    
    // Post-processing
    private ColorAdjustments colorAdjustments;
    private float targetSaturation = 0f;
    private float currentSaturation = 0f;
    
    void Start()
    {
        // Get highlight layer index
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        
        if (highlightLayer == -1)
        {
            Debug.LogError($"Layer '{highlightLayerName}' not found! Please create it in Project Settings > Tags and Layers");
        }
        
        // Get Color Adjustments from Volume
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            // Set initial state
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 0f;
            currentSaturation = 0f;
        }
        else
        {
            Debug.LogError("Color Adjustments not found in Post Process Volume!");
        }
        
        // Disable highlight camera initially
        if (highlightCamera != null)
        {
            highlightCamera.enabled = false;
        }
    }
    
    void Update()
    {
        // Toggle Eagle Vision with V key
        if (Input.GetKeyDown(activationKey))
        {
            ToggleEagleVision();
        }
        
        // Smooth transition for grayscale effect
        if (colorAdjustments != null)
        {
            currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
            colorAdjustments.saturation.value = currentSaturation;
        }
        
        // Update pulse wave
        if (isPulsing)
        {
            UpdatePulse();
        }
    }
    
    void ToggleEagleVision()
    {
        isActive = !isActive;
        
        if (isActive)
        {
            ActivateEagleVision();
        }
        else
        {
            DeactivateEagleVision();
        }
    }
    
    void ActivateEagleVision()
    {
        Debug.Log("Eagle Vision ACTIVATED");
        
        // Enable grayscale
        if (colorAdjustments != null)
        {
            targetSaturation = -100f; // Full grayscale
        }
        
        // Enable highlight camera
        if (highlightCamera != null)
        {
            highlightCamera.enabled = true;
        }
        
        // Start pulse wave
        StartPulse();
    }
    
    void DeactivateEagleVision()
    {
        Debug.Log("Eagle Vision DEACTIVATED");
        
        // Disable grayscale
        targetSaturation = 0f; // Back to normal colors
        
        // Disable highlight camera
        if (highlightCamera != null)
        {
            highlightCamera.enabled = false;
        }
        
        // Clear all highlights
        ClearAllHighlights();
    }
    
    void StartPulse()
    {
        isPulsing = true;
        currentPulseRadius = 0f;
    }
    
    void UpdatePulse()
    {
        // Expand pulse radius
        currentPulseRadius += pulseSpeed * Time.deltaTime;
        
        // Detect objects within pulse radius
        DetectObjectsInRadius(currentPulseRadius);
        
        // Stop pulse when max radius reached
        if (currentPulseRadius >= pulseMaxRadius)
        {
            isPulsing = false;
            currentPulseRadius = 0f;
        }
    }
    
    void DetectObjectsInRadius(float radius)
    {
        // Find all objects in sphere around player
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);
        
        foreach (Collider col in hitColliders)
        {
            // Check object tag and highlight accordingly
            if (col.CompareTag("EV_Enemy"))
            {
                HighlightObject(col.gameObject, enemyColor);
            }
            else if (col.CompareTag("EV_Item"))
            {
                HighlightObject(col.gameObject, itemColor);
            }
            else if (col.CompareTag("EV_Interactable"))
            {
                HighlightObject(col.gameObject, interactableColor);
            }
        }
    }
    
    void HighlightObject(GameObject obj, Color color)
    {
        // Check if already highlighted
        EagleVisionObject evObj = obj.GetComponent<EagleVisionObject>();
        if (evObj == null)
        {
            evObj = obj.AddComponent<EagleVisionObject>();
        }
        
        if (!evObj.IsHighlighted)
        {
            evObj.Highlight(color, highlightLayer);
            Debug.Log($"Highlighted: {obj.name} with color {color}");
        }
    }
    
    void ClearAllHighlights()
    {
        // Find all highlighted objects and clear them
        EagleVisionObject[] highlightedObjects = FindObjectsOfType<EagleVisionObject>();
        foreach (EagleVisionObject obj in highlightedObjects)
        {
            obj.ClearHighlight();
        }
    }
    
    // Visualize pulse radius in Scene view
    void OnDrawGizmos()
    {
        if (isPulsing && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, currentPulseRadius);
        }
        
        // Draw max radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pulseMaxRadius);
    }
}