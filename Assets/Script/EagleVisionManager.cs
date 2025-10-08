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
        // Toggle on/off dengan tombol yang sama
        if (Input.GetKeyDown(toggleKey))
        {
            if (isActive)
                DeactivateEagleVision();
            else
                ActivateEagleVision();
        }

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
        targetSaturation = -80f;
        targetVignetteIntensity = 0.65f;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.overrideState = true;
            colorAdjustments.postExposure.value = -2f;

            colorAdjustments.contrast.overrideState = true;
            colorAdjustments.contrast.value = 25f;

            colorAdjustments.colorFilter.overrideState = true;
            colorAdjustments.colorFilter.value = new Color(0.45f, 0.65f, 1.1f);
        }

        if (vignette != null)
        {
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 1f;
            vignette.color.overrideState = true;
            vignette.color.value = Color.black;
        }


        playerTarget?.ActivateEagleVision();

        if (highlightCamera != null)
            highlightCamera.enabled = true;

        foreach (var enemy in scannedEnemies)
            enemy?.Scan(enemyColor, highlightLayer);

        foreach (var item in scannedItems)
            item?.Scan(itemColor, highlightLayer);

        if (sonarPulse != null)
            sonarPulse.StartPulse();
    }


    void DeactivateEagleVision()
    {
        isActive = false;
        targetSaturation = 0f;
        targetVignetteIntensity = 0f;

        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = 0f;
            colorAdjustments.contrast.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;

        foreach (var item in scannedItems)
            item?.ResetToDefault();

        foreach (var enemy in scannedEnemies)
            enemy?.ResetToDefault();

        var interactables = FindObjectsOfType<InteractableTarget>();
        foreach (var interactable in interactables)
            interactable.ResetToDefault();

        if (sonarPulse != null)
            sonarPulse.StopPulse();

        playerTarget?.DeactivateEagleVision();
    }


    // Method yang dipanggil oleh SonarPulseManager
    public void DetectObjectsAtRadius(Vector3 center, float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(center, radius);

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
                            scannedEnemies.RemoveAt(0);
                        scannedEnemies.Add(enemy);
                    }
                }
            }
            else if (col.CompareTag("EV_Item"))
            {
                ItemTarget item = col.GetComponent<ItemTarget>();
                if (item == null)
                    item = col.gameObject.AddComponent<ItemTarget>();

                if (!item.IsScanned)
                {
                    item.Scan(itemColor, highlightLayer);
                    if (!scannedItems.Contains(item))
                    {
                        if (scannedItems.Count >= maxTrackedItems)
                            scannedItems.RemoveAt(0);
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
        // Gizmos handled by SonarPulseManager
    }
}