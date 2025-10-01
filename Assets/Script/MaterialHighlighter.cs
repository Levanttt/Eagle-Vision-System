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

        highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        highlightMaterial.SetColor("_BaseColor", color * 5f); // emissive lebih terang

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
