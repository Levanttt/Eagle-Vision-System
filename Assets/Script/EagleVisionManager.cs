using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class EagleVisionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private Camera highlightCamera;
    [SerializeField] private ParticleSonarManager sonarPulse;
    [SerializeField] private PlayerTarget playerTarget; 

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.V;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Eagle Vision Area")]
    [SerializeField] private float eagleVisionRadius = 30f;
    [SerializeField] private LayerMask targetLayerMask = -1;

    [Header("Object Colors")]
    [SerializeField] private Color enemyColor = new Color(3f, 0f, 0f);
    [SerializeField] private Color itemColor = new Color(3f, 3f, 0f);
    [SerializeField] private Color interactableColor = new Color(0f, 3f, 3f);
    [SerializeField] private Color hidingSpotColor = new Color(3f, 3f, 3f);

    [Header("Layer Settings")]
    [SerializeField] private string highlightLayerName = "EagleVisionHighlight";
    private int highlightLayer;

    [Header("Visual Polish")]
    [SerializeField] private float vignetteIntensity = 0.45f;
    [SerializeField] private float bloomIntensity = 5f;

    private bool isActive;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private Bloom bloom;

    private float targetSaturation;
    private float currentSaturation;
    private float targetVignetteIntensity;
    private float currentVignetteIntensity;
    private float targetBloomIntensity;
    private float currentBloomIntensity;

    // Untuk tracking target yang sedang di-highlight
    private HashSet<EagleVisionTarget> currentlyHighlightedTargets = new HashSet<EagleVisionTarget>();
    private Coroutine scanCoroutine;

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
        // Toggle on/off dengan tombol yang sama
        if (Input.GetKeyDown(toggleKey))
        {
            if (isActive)
                DeactivateEagleVision();
            else
                ActivateEagleVision();
        }

        UpdatePostProcessing();
    }

    void UpdatePostProcessing()
    {
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
    }

    void ActivateEagleVision()
    {
        isActive = true;
        targetSaturation = -60f;
        targetVignetteIntensity = vignetteIntensity;
        targetBloomIntensity = bloomIntensity;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = -3f;
            
            colorAdjustments.colorFilter.overrideState = true;
            colorAdjustments.colorFilter.value = new Color(0.6f, 0.7f, 0.9f);
        }

        playerTarget?.ActivateEagleVision();
        
        if (highlightCamera != null)
            highlightCamera.enabled = true;

        // HAPUS bagian yang menyalakan scannedEnemies dan scannedItems
        // JANGAN langsung nyalain objek dari memory sebelumnya

        // Mulai continuous scanning - ini yang akan menyalakan objek secara bertahap
        if (scanCoroutine != null)
            StopCoroutine(scanCoroutine);
        scanCoroutine = StartCoroutine(ContinuousScanRoutine());

        if (sonarPulse != null)
            sonarPulse.StartPulse();
    }

    void DeactivateEagleVision()
    {
        isActive = false;
        targetSaturation = 0f;
        targetVignetteIntensity = 0f;
        targetBloomIntensity = 0f;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;

        // Hentikan scanning
        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
            scanCoroutine = null;
        }

        // Reset semua target yang sedang di-highlight
        ResetAllHighlightedTargets();

        if (sonarPulse != null)
            sonarPulse.StopPulse();

        playerTarget?.DeactivateEagleVision();
    }

    IEnumerator ContinuousScanRoutine()
    {
        // Tunggu 1 frame dulu biar post processing transition mulai dulu
        yield return null;
        
        while (isActive)
        {
            ScanAreaAroundPlayer();
            yield return new WaitForSeconds(0.1f); // Scan setiap 0.1 detik untuk performa
        }
    }

    void ScanAreaAroundPlayer()
    {
        // Dapatkan semua collider dalam radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, eagleVisionRadius, targetLayerMask);
        HashSet<EagleVisionTarget> currentFrameTargets = new HashSet<EagleVisionTarget>();

        foreach (Collider col in hitColliders)
        {
            EagleVisionTarget target = GetTargetComponent(col);
            if (target != null && !target.IsHighlighted)
            {
                Color targetColor = GetColorForTag(col.tag);
                target.Scan(targetColor, highlightLayer);
                currentlyHighlightedTargets.Add(target);
                currentFrameTargets.Add(target);
            }
            else if (target != null && target.IsHighlighted)
            {
                currentFrameTargets.Add(target);
            }
        }

        // Reset target yang keluar dari jangkauan
        HashSet<EagleVisionTarget> targetsToRemove = new HashSet<EagleVisionTarget>();
        foreach (var target in currentlyHighlightedTargets)
        {
            if (target == null) continue;
            
            if (!currentFrameTargets.Contains(target))
            {
                target.ResetToDefault();
                targetsToRemove.Add(target);
            }
        }

        // Hapus dari tracking
        foreach (var target in targetsToRemove)
        {
            currentlyHighlightedTargets.Remove(target);
        }
    }

    EagleVisionTarget GetTargetComponent(Collider collider)
    {
        // Cari komponen EagleVisionTarget yang sudah ada
        EagleVisionTarget target = collider.GetComponent<EagleVisionTarget>();
        
        if (target == null)
        {
            // Cari di parent atau children
            target = collider.GetComponentInParent<EagleVisionTarget>();
            if (target == null)
                target = collider.GetComponentInChildren<EagleVisionTarget>();
        }

        return target;
    }

    Color GetColorForTag(string tag)
    {
        switch (tag)
        {
            case "EV_Enemy": return enemyColor;
            case "EV_Item": return itemColor;
            case "EV_Interactable": return interactableColor;
            case "EV_HidingSpot": return hidingSpotColor;
            default: return Color.white;
        }
    }

    void ResetAllHighlightedTargets()
    {
        foreach (var target in currentlyHighlightedTargets)
        {
            if (target != null)
                target.ResetToDefault();
        }
        currentlyHighlightedTargets.Clear();
    }

    void OnDrawGizmosSelected()
    {
        // Visualize eagle vision radius in scene view
        if (isActive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, eagleVisionRadius);
        }
    }
}