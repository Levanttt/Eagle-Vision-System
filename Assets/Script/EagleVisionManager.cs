using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EagleVisionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Camera highlightCamera;
    [SerializeField] private GameObject pulseWavePrefab; // NEW: Prefab untuk pulse visual

    [Header("Settings")]
    [SerializeField] private KeyCode activationKey = KeyCode.V;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseMaxRadius = 30f;

    [Header("Duration Settings")]
    [SerializeField] private float activeDuration = 5f;

    [Header("Object Colors")]
    [SerializeField] private Color enemyColor = new Color(3f, 0f, 0f);
    [SerializeField] private Color itemColor = new Color(3f, 3f, 0f);
    [SerializeField] private Color interactableColor = new Color(0f, 3f, 3f);

    [Header("Layer Settings")]
    [SerializeField] private string highlightLayerName = "EagleVisionHighlight";
    private int highlightLayer;

    // State
    private bool isActive = false;
    private float activeTimer = 0f;
    private float currentPulseRadius = 0f;
    private bool isPulsing = false;
    
    // Pulse visual
    private GameObject currentPulseWave;
    private Renderer pulseRenderer;
    private Material pulseMaterial;

    // Post-processing
    private ColorAdjustments colorAdjustments;
    private float targetSaturation = 0f;
    private float currentSaturation = 0f;

    void Start()
    {
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found!");

        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 0f;
            currentSaturation = 0f;
        }
        else
        {
            Debug.LogError("Color Adjustments not found!");
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(activationKey) && !isActive)
        {
            ActivateEagleVision();
        }

        if (colorAdjustments != null)
        {
            currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
            colorAdjustments.saturation.value = currentSaturation;
        }

        if (isPulsing)
            UpdatePulse();

        if (isActive)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
            {
                DeactivateEagleVision();
            }
        }
    }

    void ActivateEagleVision()
    {
        isActive = true;
        targetSaturation = -100f;
        if (highlightCamera != null) highlightCamera.enabled = true;

        StartPulse();
        activeTimer = activeDuration;
    }

    void DeactivateEagleVision()
    {
        isActive = false;
        targetSaturation = 0f;
        if (highlightCamera != null) highlightCamera.enabled = false;

        ClearAllHighlights();
        
        isPulsing = false;
        currentPulseRadius = 0f;
        
        // Destroy pulse visual if still exists
        if (currentPulseWave != null)
        {
            Destroy(currentPulseWave);
            currentPulseWave = null;
        }
    }

    void StartPulse()
    {
        isPulsing = true;
        currentPulseRadius = 0f;
        
        // Spawn pulse wave visual
        if (pulseWavePrefab != null)
        {
            currentPulseWave = Instantiate(pulseWavePrefab, transform.position, Quaternion.identity);
            
            // Set to highlight layer so it bypasses grayscale
            currentPulseWave.layer = highlightLayer;
            
            pulseRenderer = currentPulseWave.GetComponent<Renderer>();
            
            if (pulseRenderer != null)
            {
                // Create instance material to avoid affecting prefab
                pulseMaterial = new Material(pulseRenderer.sharedMaterial);
                pulseRenderer.material = pulseMaterial;
            }
        }
    }

    void UpdatePulse()
    {
        currentPulseRadius += pulseSpeed * Time.deltaTime;
        
        // Update pulse visual
        if (currentPulseWave != null)
        {
            // Scale sphere to match radius
            float scale = currentPulseRadius * 2f; // diameter = radius * 2
            currentPulseWave.transform.localScale = Vector3.one * scale;
            
            // Fade out as it expands
            if (pulseMaterial != null)
            {
                float alpha = 1f - (currentPulseRadius / pulseMaxRadius);
                Color currentColor = pulseMaterial.GetColor("_BaseColor");
                currentColor.a = alpha * 0.5f; // Max alpha 0.5 for subtle effect
                pulseMaterial.SetColor("_BaseColor", currentColor);
            }
        }
        
        // Detect objects
        DetectObjectsInRadius(currentPulseRadius);

        // Stop pulse when max radius reached
        if (currentPulseRadius >= pulseMaxRadius)
        {
            isPulsing = false;
            currentPulseRadius = 0f;
            
            // Destroy pulse visual
            if (currentPulseWave != null)
            {
                Destroy(currentPulseWave);
                currentPulseWave = null;
            }
            
            if (pulseMaterial != null)
            {
                Destroy(pulseMaterial);
                pulseMaterial = null;
            }
        }
    }

    void DetectObjectsInRadius(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in hitColliders)
        {
            if (!(col.CompareTag("EV_Enemy") || col.CompareTag("EV_Item") || col.CompareTag("EV_Interactable")))
                continue;

            IHighlightable highlightable = col.GetComponent<IHighlightable>();

            if (highlightable == null)
                highlightable = col.gameObject.AddComponent<MaterialHighlighter>();

            if (col.CompareTag("EV_Enemy"))
                highlightable.Highlight(enemyColor, highlightLayer);
            else if (col.CompareTag("EV_Item"))
                highlightable.Highlight(itemColor, highlightLayer);
            else if (col.CompareTag("EV_Interactable"))
                highlightable.Highlight(interactableColor, highlightLayer);
        }
    }

    void ClearAllHighlights()
    {
        var highlightables = FindObjectsOfType<MonoBehaviour>(true).OfType<IHighlightable>();
        foreach (var h in highlightables)
            h.ClearHighlight();
    }

    void OnDrawGizmos()
    {
        if (isPulsing && Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, currentPulseRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pulseMaxRadius);
    }
}