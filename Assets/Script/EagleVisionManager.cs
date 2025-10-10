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

    [Header("Sound Effects - AC Style")]
    [SerializeField] private AudioClip eagleVisionActivateSound;  
    [SerializeField] private AudioClip eagleVisionDeactivateSound; 
    private AudioSource audioSource;

    [Header("Layer Settings")]
    [SerializeField] private string highlightLayerName = "EagleVisionHighlight";
    private int highlightLayer;

    [Header("Visual Polish")]
    [SerializeField] private float vignetteIntensity = 0.20f;
    [SerializeField] private float bloomIntensity = 1.5f; // DIKURANGI: dari 5f jadi 1.5f (sangat subtle)

    private bool isActive;
    private bool initialPulseCompleted = false;

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;
    private Bloom bloom;

    private float targetSaturation;
    private float currentSaturation;
    private float targetVignetteIntensity;
    private float currentVignetteIntensity;
    private float targetBloomIntensity;
    private float currentBloomIntensity;
    private HashSet<EagleVisionTarget> currentlyHighlightedTargets = new HashSet<EagleVisionTarget>();
    private Coroutine scanCoroutine;

    void Start()
    {
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found!");

        // Setup AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

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
                
                // Optional: Atur threshold bloom agar lebih subtle
                bloom.threshold.overrideState = true;
                bloom.threshold.value = 0.8f; // Hanya area yang terang sekali yang bloom
            }
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isActive)
                DeactivateEagleVision();
            else
                ActivateEagleVision();
        }

        UpdatePostProcessing();

        if (isActive && sonarPulse != null && !sonarPulse.IsScanning && !initialPulseCompleted)
        {
            initialPulseCompleted = true;
        }
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
        initialPulseCompleted = false;
        targetSaturation = -60f;
        targetVignetteIntensity = vignetteIntensity;
        targetBloomIntensity = bloomIntensity; // 1.5f - sangat subtle

        if (eagleVisionActivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(eagleVisionActivateSound);
        }

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

        if (scanCoroutine != null)
            StopCoroutine(scanCoroutine);
        scanCoroutine = StartCoroutine(ContinuousScanRoutine());

        if (sonarPulse != null)
            sonarPulse.StartPulse();
    }

    void DeactivateEagleVision()
    {
        isActive = false;
        initialPulseCompleted = false;
        targetSaturation = 0f;
        targetVignetteIntensity = 0f;
        targetBloomIntensity = 0f; // Bloom mati

        if (eagleVisionDeactivateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(eagleVisionDeactivateSound);
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;

        if (scanCoroutine != null)
        {
            StopCoroutine(scanCoroutine);
            scanCoroutine = null;
        }

        ResetAllHighlightedTargets();

        if (sonarPulse != null)
            sonarPulse.StopPulse();

        playerTarget?.DeactivateEagleVision();
    }

    IEnumerator ContinuousScanRoutine()
    {
        yield return null;
        
        while (isActive)
        {
            ScanAreaAroundPlayer();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ScanAreaAroundPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, eagleVisionRadius, targetLayerMask);
        HashSet<EagleVisionTarget> currentFrameTargets = new HashSet<EagleVisionTarget>();

        foreach (Collider col in hitColliders)
        {
            EagleVisionTarget target = GetTargetComponent(col);
            if (target != null && !target.IsHighlighted)
            {
                bool shouldHighlight = false;

                if (sonarPulse != null && sonarPulse.IsScanning)
                {
                    if (sonarPulse.IsObjectInCurrentPulseRange(col.transform.position))
                    {
                        shouldHighlight = true;
                    }
                }

                else if (initialPulseCompleted)
                {
                    shouldHighlight = true;
                }

                if (shouldHighlight)
                {
                    Color targetColor = GetColorForTag(col.tag);
                    target.Scan(targetColor, highlightLayer);
                    currentlyHighlightedTargets.Add(target);
                    currentFrameTargets.Add(target);
                }
            }
            else if (target != null && target.IsHighlighted)
            {
                currentFrameTargets.Add(target);
            }
        }

        if (initialPulseCompleted)
        {
            HashSet<EagleVisionTarget> targetsToRemove = new HashSet<EagleVisionTarget>();
            foreach (var target in currentlyHighlightedTargets)
            {
                if (target == null) continue;
                
                if (!IsObjectInEagleVisionArea(target.transform.position))
                {
                    target.ResetToDefault();
                    targetsToRemove.Add(target);
                }
            }

            foreach (var target in targetsToRemove)
            {
                currentlyHighlightedTargets.Remove(target);
            }
        }
    }

    bool IsObjectInEagleVisionArea(Vector3 objectPosition)
    {
        float distance = Vector3.Distance(transform.position, objectPosition);
        return distance <= eagleVisionRadius;
    }

    EagleVisionTarget GetTargetComponent(Collider collider)
    {
        EagleVisionTarget target = collider.GetComponent<EagleVisionTarget>();
        
        if (target == null)
        {
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, eagleVisionRadius);
    }
}