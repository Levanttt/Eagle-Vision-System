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
    private List<EnemyTarget> scannedEnemies = new List<EnemyTarget>();

    [Header("Item Memory")] 
    [SerializeField] private int maxTrackedItems = 10;
    private List<ItemTarget> scannedItems = new List<ItemTarget>(); 

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

    private bool isActive;
    private bool isPulsing;
    private float currentPulseRadius;

    private GameObject currentPulseWave;
    private Material pulseMaterial;

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
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found!");

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
                enemy.Scan(enemyColor, highlightLayer);
            }
        }

        // Auto-highlight scanned items (NEW)
        foreach (var item in scannedItems)
        {
            if (item != null)
            {
                item.Scan(itemColor, highlightLayer);
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

        // Item: permanent highlight, restore layer
        foreach (var item in scannedItems)
        {
            if (item != null)
            {
                item.KeepHighlightButRestoreLayer();
            }
        }

        // Interactable: fade timer
        var interactables = FindObjectsOfType<InteractableTarget>();
        foreach (var interactable in interactables)
        {
            interactable.StartFadeTimer();
            interactable.RestoreLayer();
        }

        // Enemy: permanent highlight, restore layer
        foreach (var enemy in scannedEnemies)
        {
            if (enemy != null)
            {
                enemy.KeepHighlightButRestoreLayer();
            }
        }

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
            if (col.CompareTag("EV_Enemy"))
            {
                EnemyTarget enemy = col.GetComponent<EnemyTarget>();
                if (enemy == null)
                    enemy = col.gameObject.AddComponent<EnemyTarget>();

                if (!enemy.IsScanned)
                {
                    enemy.Scan(enemyColor, highlightLayer);
                    
                    if (!scannedEnemies.Contains(enemy))
                    {
                        if (scannedEnemies.Count >= maxTrackedEnemies)
                        {
                            scannedEnemies.RemoveAt(0);
                        }
                        scannedEnemies.Add(enemy);
                    }
                }
            }
            else if (col.CompareTag("EV_Item"))
            {
                ItemTarget item = col.GetComponent<ItemTarget>();
                if (item == null)
                    item = col.gameObject.AddComponent<ItemTarget>();

                if (!item.IsScanned) // NEW: Cek apakah sudah discan
                {
                    item.Scan(itemColor, highlightLayer);
                    
                    if (!scannedItems.Contains(item))
                    {
                        if (scannedItems.Count >= maxTrackedItems)
                        {
                            scannedItems.RemoveAt(0);
                        }
                        scannedItems.Add(item);
                    }
                }
            }
            else if (col.CompareTag("EV_Interactable"))
            {
                InteractableTarget interactable = col.GetComponent<InteractableTarget>();
                if (interactable == null)
                    interactable = col.gameObject.AddComponent<InteractableTarget>();

                interactable.Scan(interactableColor, highlightLayer);
            }
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