using UnityEngine;

public class EagleVisionObject : MonoBehaviour
{
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;
    private bool isHighlighted = false;
    private int originalLayer;
    
    public bool IsHighlighted => isHighlighted;
    
    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        
        // Store original layer
        originalLayer = gameObject.layer;
    }
    
    public void Highlight(Color color, int highlightLayer)  // ← 2 parameters!
    {
        if (objectRenderer == null) return;
        
        // Move object to highlight layer
        gameObject.layer = highlightLayer;
        
        // Create super bright unlit material
        highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        
        // Set bright glowing color
        highlightMaterial.SetColor("_BaseColor", color);
        
        // Apply highlight material
        objectRenderer.material = highlightMaterial;
        isHighlighted = true;
    }
    
    public void ClearHighlight()
    {
        // Restore original layer
        gameObject.layer = originalLayer;
        
        // Restore original material
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
        
        // Clean up highlight material
        if (highlightMaterial != null)
        {
            Destroy(highlightMaterial);
        }
        
        isHighlighted = false;
        
        // Remove this component
        Destroy(this);
    }
}