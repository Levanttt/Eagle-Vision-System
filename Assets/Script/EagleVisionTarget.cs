using UnityEngine;

public abstract class EagleVisionTarget : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] protected Material defaultMaterial;
    [SerializeField] protected Material highlightMaterial;

    protected Renderer rend;
    protected Material currentMaterialInstance; // TAMBAHAN BARU
    protected int originalLayer;
    protected bool isScanned = false;
    protected bool isHighlighted = false;

    public bool IsScanned => isScanned;
    public bool IsHighlighted => isHighlighted;

    protected virtual void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null && defaultMaterial == null)
        {
            defaultMaterial = rend.sharedMaterial;
        }
        
        originalLayer = gameObject.layer;
    }

    public virtual void Scan(Color color, int highlightLayer)
    {
        if (rend == null || highlightMaterial == null) return;

        isScanned = true;
        isHighlighted = true;
        gameObject.layer = highlightLayer;

        rend.material = highlightMaterial;
        currentMaterialInstance = rend.material; // SIMPAN REFERENCE
        
        if (currentMaterialInstance.HasProperty("_EmissionColor"))
            currentMaterialInstance.SetColor("_EmissionColor", color * 5f);
        
        if (currentMaterialInstance.HasProperty("_BaseColor"))
            currentMaterialInstance.SetColor("_BaseColor", color);
    }

    public virtual void RestoreLayer()
    {
        gameObject.layer = originalLayer;
    }

    // TAMBAHAN BARU - method untuk set render queue
    public virtual void SetRenderQueue(int queue)
    {
        if (currentMaterialInstance != null)
        {
            currentMaterialInstance.renderQueue = queue;
            Debug.Log($"[{gameObject.name}] Render queue set to: {currentMaterialInstance.renderQueue}");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] currentMaterialInstance is null!");
        }
    }

    public virtual void ResetToDefault()
    {
        if (rend != null && defaultMaterial != null)
        {
            rend.material = defaultMaterial;
        }
        
        // Destroy material instance yang dibuat
        if (currentMaterialInstance != null && currentMaterialInstance != highlightMaterial)
        {
            Destroy(currentMaterialInstance);
            currentMaterialInstance = null;
        }
        
        gameObject.layer = originalLayer;
        isHighlighted = false;
    }
}