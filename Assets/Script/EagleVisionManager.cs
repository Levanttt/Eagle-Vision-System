using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EagleVisionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Camera highlightCamera;
    [SerializeField] private GameObject pulseWavePrefab;

    [Header("Settings")]
    [SerializeField] private KeyCode activationKey = KeyCode.V;
    [SerializeField] private float transitionSpeed = 5f;
    [SerializeField] private float pulseSpeed = 10f;
    [SerializeField] private float pulseMaxRadius = 30f;

    [Header("Duration Settings")]
    [SerializeField] private float activeDuration = 5f;
    private float activeTimer;

    [Header("Enemy Memory")]
    [SerializeField] private int maxTrackedEnemies = 5;
    private List<MaterialHighlighter> scannedEnemies = new List<MaterialHighlighter>();

    [Header("Object Colors")]
    [SerializeField] private Color enemyColor = new Color(3f, 0f, 0f);
    [SerializeField] private Color itemColor = new Color(3f, 3f, 0f);
    [SerializeField] private Color interactableColor = new Color(0f, 3f, 3f);

    [Header("Layer Settings")]
    [SerializeField] private string highlightLayerName = "EagleVisionHighlight";
    private int highlightLayer;

    [Header("Visual Polish")]
    [SerializeField] private float vignetteIntensity = 0.45f;
    [SerializeField] private float bloomIntensity = 5f;

    // State
    private bool isActive;
    private bool isPulsing;
    private float currentPulseRadius;

    // Pulse visual
    private GameObject currentPulseWave;
    private Material pulseMaterial;

    // Post-processing
    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private Bloom bloom;

    private float targetSaturation;
    private float currentSaturation;
    private float targetVignetteIntensity;
    private float currentVignetteIntensity;
    private float targetBloomIntensity;
    private float currentBloomIntensity;

    void Start()
    {
        // Get highlight layer
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found!");

        // Post-processing setup
        if (postProcessVolume != null)
        {
            if (postProcessVolume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments.saturation.overrideState = true;
                colorAdjustments.saturation.value = 0f;
            }

            if (postProcessVolume.profile.TryGet(out vignette))
            {
                vignette.intensity.overrideState = true;
                vignette.intensity.value = 0f;
            }

            if (postProcessVolume.profile.TryGet(out bloom))
            {
                bloom.intensity.overrideState = true;
                bloom.intensity.value = 0f;
            }
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(activationKey) && !isActive)
            ActivateEagleVision();

        if (colorAdjustments != null)
        {
            currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
            colorAdjustments.saturation.value = currentSaturation;
        }

        if (vignette != null)
        {
            currentVignetteIntensity = Mathf.Lerp(currentVignetteIntensity, targetVignetteIntensity, Time.deltaTime * transitionSpeed);
            vignette.intensity.value = currentVignetteIntensity;
        }

        if (bloom != null)
        {
            currentBloomIntensity = Mathf.Lerp(currentBloomIntensity, targetBloomIntensity, Time.deltaTime * transitionSpeed);
            bloom.intensity.value = currentBloomIntensity;
        }

        if (isPulsing)
            UpdatePulse();

        if (isActive)
        {
            activeTimer -= Time.deltaTime;
            if (activeTimer <= 0f)
                DeactivateEagleVision();
        }
    }

    void ActivateEagleVision()
    {
        isActive = true;
        targetSaturation = -100f;
        targetVignetteIntensity = vignetteIntensity;
        targetBloomIntensity = bloomIntensity;

        if (highlightCamera != null)
            highlightCamera.enabled = true;

        // Auto-highlight scanned enemies
        foreach (var enemy in scannedEnemies)
        {
            if (enemy != null)
            {
                enemy.Highlight(enemyColor, highlightLayer);
            }
        }

        StartPulse();
        activeTimer = activeDuration;
    }

    void DeactivateEagleVision()
    {
        isActive = false;
        targetSaturation = 0f;
        targetVignetteIntensity = 0f;
        targetBloomIntensity = 0f;

        if (highlightCamera != null)
            highlightCamera.enabled = false;

        ClearAllHighlights();

        isPulsing = false;
        currentPulseRadius = 0f;

        if (currentPulseWave != null)
            Destroy(currentPulseWave);
    }

    void StartPulse()
    {
        isPulsing = true;
        currentPulseRadius = 0f;

        if (pulseWavePrefab != null)
        {
            currentPulseWave = Instantiate(pulseWavePrefab, transform.position, Quaternion.identity);
            
            // Pulse wave juga di highlight layer
            currentPulseWave.layer = highlightLayer;

            var pulseRenderer = currentPulseWave.GetComponent<Renderer>();
            if (pulseRenderer != null)
            {
                pulseMaterial = new Material(pulseRenderer.sharedMaterial);
                pulseRenderer.material = pulseMaterial;
            }
        }
    }

    void UpdatePulse()
    {
        currentPulseRadius += pulseSpeed * Time.deltaTime;

        if (currentPulseWave != null)
        {
            float scale = currentPulseRadius * 2f;
            currentPulseWave.transform.localScale = Vector3.one * scale;

            if (pulseMaterial != null)
            {
                float alpha = 1f - (currentPulseRadius / pulseMaxRadius);
                Color currentColor = pulseMaterial.GetColor("_BaseColor");
                currentColor.a = alpha * 0.5f;
                pulseMaterial.SetColor("_BaseColor", currentColor);
            }
        }

        DetectObjectsInRadius(currentPulseRadius);

        if (currentPulseRadius >= pulseMaxRadius)
        {
            isPulsing = false;
            currentPulseRadius = 0f;

            if (currentPulseWave != null)
                Destroy(currentPulseWave);

            if (pulseMaterial != null)
                Destroy(pulseMaterial);
        }
    }

    void DetectObjectsInRadius(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in hitColliders)
        {
            if (!(col.CompareTag("EV_Enemy") || col.CompareTag("EV_Item") || col.CompareTag("EV_Interactable")))
                continue;

            MaterialHighlighter highlightable = col.GetComponent<MaterialHighlighter>();
            if (highlightable == null)
                highlightable = col.gameObject.AddComponent<MaterialHighlighter>();

            if (col.CompareTag("EV_Enemy"))
            {
                highlightable.Highlight(enemyColor, highlightLayer);
                
                if (!highlightable.IsScanned)
                {
                    highlightable.MarkAsScanned();
                    
                    if (!scannedEnemies.Contains(highlightable))
                    {
                        if (scannedEnemies.Count >= maxTrackedEnemies)
                        {
                            scannedEnemies.RemoveAt(0);
                        }
                        scannedEnemies.Add(highlightable);
                    }
                }
            }
            else if (col.CompareTag("EV_Item"))
            {
                highlightable.Highlight(itemColor, highlightLayer);
            }
            else if (col.CompareTag("EV_Interactable"))
            {
                highlightable.Highlight(interactableColor, highlightLayer);
            }
        }
    }

    void ClearAllHighlights()
    {
        var highlightables = FindObjectsOfType<MaterialHighlighter>();
        foreach (var h in highlightables)
        {
            h.ClearHighlight();
        }
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