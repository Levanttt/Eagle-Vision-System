using UnityEngine;

public class InteractableTarget : EagleVisionTarget
{
    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    public void OnInteracted()
    {
        ResetToDefault();
        
    }
}