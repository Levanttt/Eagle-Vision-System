using UnityEngine;

public class MaterialHighlighter : MonoBehaviour, IHighlightable
{
    private Renderer rend;
    private Material originalMaterial;
    private int originalLayer;

    [Header("Highlight Material (Temporary)")]
    public Material highlightMaterial;

    [Header("Highlight Material (Permanent for Enemy)")]
    public Material permanentEnemyMaterial;

    public bool IsHighlighted { get; private set; }
    public bool IsScanned { get; private set; }

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
            originalMaterial = rend.sharedMaterial;
        
        // Store original layer
        originalLayer = gameObject.layer;
    }

    // Interface implementation
    public void Highlight(Color color, int highlightLayer)
    {
        Highlight(color, false, highlightLayer);
    }

    // Overload dengan layer control
    public void Highlight(Color color, bool permanent = false, int highlightLayer = 0)
    {
        if (rend == null) return;

        if (permanent)
        {
            // Enemy → glowing permanen
            if (permanentEnemyMaterial != null)
            {
                rend.material = permanentEnemyMaterial;
                rend.material.SetColor("_EmissionColor", color);
                IsScanned = true;
                
                // PENTING: Kembalikan ke layer original
                // Jadi Main Camera bisa render enemy setelah EV mati
                gameObject.layer = originalLayer;
            }
        }
        else
        {
            // Item/interactable → glowing sementara
            // Pindah ke highlight layer (bypass grayscale saat EV aktif)
            gameObject.layer = highlightLayer;
            
            if (highlightMaterial != null)
            {
                rend.material = highlightMaterial;
                rend.material.SetColor("_EmissionColor", color);
            }
        }

        IsHighlighted = true;
    }

    public void ClearHighlight()
    {
        if (IsScanned)
        {
            // Enemy sudah discan → tetap glowing, jangan clear
            return;
        }

        // Restore layer
        gameObject.layer = originalLayer;
        
        if (rend != null && originalMaterial != null)
            rend.material = originalMaterial;

        IsHighlighted = false;
    }
}