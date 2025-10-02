using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialHighlighter : MonoBehaviour, IHighlightable
{
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;
    private int originalLayer;

    public bool IsHighlighted { get; private set; }

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        originalLayer = gameObject.layer;
    }

    public void Highlight(Color color, int highlightLayer)
    {
        if (IsHighlighted) return;

        gameObject.layer = highlightLayer;

        // Buat Unlit material
        highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        highlightMaterial.SetColor("_BaseColor", color);

        // Tambahin emission supaya Bloom bisa nangkep
        if (highlightMaterial.HasProperty("_EmissionColor"))
        {
            highlightMaterial.EnableKeyword("_EMISSION");
            highlightMaterial.SetColor("_EmissionColor", color * 10f); // multiplier biar HDR terang
        }

        objectRenderer.material = highlightMaterial;
        IsHighlighted = true;
    }


    public void ClearHighlight()
    {
        gameObject.layer = originalLayer;

        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }

        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
            highlightMaterial = null;
        }

        IsHighlighted = false;
    }
}
