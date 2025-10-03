using UnityEngine;

public abstract class EagleVisionTarget : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] protected Material defaultMaterial;
    [SerializeField] protected Material highlightMaterial;

    protected Renderer rend;
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
        
        if (highlightMaterial.HasProperty("_EmissionColor"))
            rend.material.SetColor("_EmissionColor", color * 5f);
        
        if (highlightMaterial.HasProperty("_BaseColor"))
            rend.material.SetColor("_BaseColor", color);
    }

    public virtual void ResetToDefault()
    {
        if (rend != null && defaultMaterial != null)
        {
            rend.material = defaultMaterial;
        }
        
        gameObject.layer = originalLayer;
        isHighlighted = false;
    }
}