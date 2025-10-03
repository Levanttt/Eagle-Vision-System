using UnityEngine;

public class MaterialHighlighter : MonoBehaviour, IHighlightable
{
    private Renderer rend;
    private Material originalMaterial;
    private int originalLayer;

    [Header("Highlight Material")]
    public Material highlightMaterial;

    public bool IsHighlighted { get; private set; }
    public bool IsScanned { get; private set; }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalMaterial = rend.sharedMaterial;
        
        originalLayer = gameObject.layer;
    }

    public void Highlight(Color color, int highlightLayer)
    {
        if (rend == null) return;

        // Pindah ke highlight layer (bypass grayscale)
        gameObject.layer = highlightLayer;

        if (highlightMaterial != null)
        {
            rend.material = highlightMaterial;
            
            if (highlightMaterial.HasProperty("_EmissionColor"))
                rend.material.SetColor("_EmissionColor", color * 5f);
            
            if (highlightMaterial.HasProperty("_BaseColor"))
                rend.material.SetColor("_BaseColor", color);
        }

        IsHighlighted = true;
    }

    public void MarkAsScanned()
    {
        IsScanned = true;
    }

    public void ClearHighlight()
    {
        // Kembalikan ke layer original
        gameObject.layer = originalLayer;
        
        if (rend != null && originalMaterial != null)
            rend.material = originalMaterial;

        IsHighlighted = false;
    }
}