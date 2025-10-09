using UnityEngine;

public class InteractableTarget : EagleVisionTarget
{
    protected override void Awake()
    {
        base.Awake(); // PENTING: Panggil base.Awake()
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    // Dipanggil saat player interact
    public void OnInteracted()
    {
        ResetToDefault();
        
        // Opsional: trigger interaksi, animasi, dll
    }
}