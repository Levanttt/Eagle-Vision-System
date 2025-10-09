using System.Collections.Generic;
using UnityEngine;

public abstract class EagleVisionTarget : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] protected Material baseMaterial;
    [SerializeField] protected Material highlightMaterial;

    protected bool isScanned = false;
    protected bool isHighlighted = false;

    private List<Renderer> allRenderers = new List<Renderer>();
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private Dictionary<Renderer, int> originalLayers = new Dictionary<Renderer, int>();

    private Color currentHighlightColor;
    private int targetLayer;

    public bool IsScanned => isScanned;
    public bool IsHighlighted => isHighlighted;

    protected virtual void Awake()
    {
        // Get all renderers dari parent dan semua children
        GetAllRenderers();
        
        // Save original materials
        SaveOriginalMaterials();
    }

    void GetAllRenderers()
    {
        allRenderers.Clear();

        // Get renderer di parent
        Renderer parentRenderer = GetComponent<Renderer>();
        if (parentRenderer != null)
            allRenderers.Add(parentRenderer);

        // Get semua renderer di children (termasuk nested children)
        Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in childRenderers)
        {
            if (!allRenderers.Contains(renderer))
                allRenderers.Add(renderer);
        }

        Debug.Log($"{gameObject.name}: Found {allRenderers.Count} renderers");
    }

    void SaveOriginalMaterials()
    {
        originalMaterials.Clear();
        originalLayers.Clear();

        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            // Save original materials (buat array copy)
            Material[] mats = renderer.sharedMaterials;
            Material[] matsCopy = new Material[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                matsCopy[i] = mats[i];
            }
            originalMaterials[renderer] = matsCopy;

            // Save original layer
            originalLayers[renderer] = renderer.gameObject.layer;
        }
    }

    public virtual void Scan(Color color, int highlightLayer)
    {
        if (isHighlighted) return;

        isScanned = true;
        isHighlighted = true;
        currentHighlightColor = color;
        targetLayer = highlightLayer;

        ApplyHighlight();
    }

    void ApplyHighlight()
    {
        if (highlightMaterial == null)
        {
            Debug.LogWarning($"{gameObject.name}: Highlight material is null!");
            return;
        }

        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            // Create instance untuk setiap renderer
            Material highlightInstance = new Material(highlightMaterial);
            highlightInstance.SetColor("_BaseColor", currentHighlightColor);
            highlightInstance.SetColor("_EmissionColor", currentHighlightColor);

            // Replace SEMUA materials dengan highlight material
            Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = highlightInstance;
            }
            renderer.materials = newMaterials;

            // Change layer untuk highlight camera
            renderer.gameObject.layer = targetLayer;
        }
    }

    public virtual void ResetToDefault()
    {
        if (!isHighlighted) return;

        isHighlighted = false;

        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;

            // Restore original materials
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.materials = originalMaterials[renderer];
            }

            // Restore original layer
            if (originalLayers.ContainsKey(renderer))
            {
                renderer.gameObject.layer = originalLayers[renderer];
            }
        }
    }

    protected virtual void OnDestroy()
    {
        ResetToDefault();
    }
}