using System.Linq;
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

    [Header("Duration Settings")]
    [SerializeField] private float activeDuration = 5f; // detik Eagle Vision aktif

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

    // Post-processing
    private ColorAdjustments colorAdjustments;
    private float targetSaturation = 0f;
    private float currentSaturation = 0f;

    void Start()
    {
        highlightLayer = LayerMask.NameToLayer(highlightLayerName);
        if (highlightLayer == -1)
            Debug.LogError($"Layer '{highlightLayerName}' not found! Please create it in Project Settings > Tags and Layers");

        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.saturation.overrideState = true;
            colorAdjustments.saturation.value = 0f;
            currentSaturation = 0f;
        }
        else
        {
            Debug.LogError("Color Adjustments not found in Post Process Volume!");
        }

        if (highlightCamera != null)
            highlightCamera.enabled = false;
    }

    void Update()
    {
        // --- INPUT: hanya respon kalau saat ini TIDAK aktif ---
        if (Input.GetKeyDown(activationKey) && !isActive)
        {
            ActivateEagleVision();
        }

        // Smooth transition grayscale
        if (colorAdjustments != null)
        {
            currentSaturation = Mathf.Lerp(currentSaturation, targetSaturation, Time.deltaTime * transitionSpeed);
            colorAdjustments.saturation.value = currentSaturation;
        }

        // Pulse update
        if (isPulsing)
            UpdatePulse();

        // Timer: jika aktif, hitung mundur dan otomatis matikan
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
        targetSaturation = -100f; // full grayscale
        if (highlightCamera != null) highlightCamera.enabled = true;

        StartPulse();

        activeTimer = activeDuration; // mulai timer
    }

    void DeactivateEagleVision()
    {
        isActive = false;
        targetSaturation = 0f; // kembali normal
        if (highlightCamera != null) highlightCamera.enabled = false;

        ClearAllHighlights();

        // pastikan pulse stop
        isPulsing = false;
        currentPulseRadius = 0f;
    }

    void StartPulse()
    {
        isPulsing = true;
        currentPulseRadius = 0f;
    }

    void UpdatePulse()
    {
        currentPulseRadius += pulseSpeed * Time.deltaTime;
        DetectObjectsInRadius(currentPulseRadius);

        if (currentPulseRadius >= pulseMaxRadius)
        {
            isPulsing = false;
            currentPulseRadius = 0f;
        }
    }

    void DetectObjectsInRadius(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider col in hitColliders)
        {
            if (!(col.CompareTag("EV_Enemy") || col.CompareTag("EV_Item") || col.CompareTag("EV_Interactable")))
                continue;

            // Cari komponen highlighter
            IHighlightable highlightable = col.GetComponent<IHighlightable>();

            // Auto-tambah jika belum ada (Opsi A)
            if (highlightable == null)
                highlightable = col.gameObject.AddComponent<MaterialHighlighter>();

            // Beri warna sesuai tag
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
