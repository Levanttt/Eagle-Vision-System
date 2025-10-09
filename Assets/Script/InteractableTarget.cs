using UnityEngine;

public class InteractableTarget : EagleVisionTarget
{
    protected override void Awake()
    {
        base.Awake(); 
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    public void OnInteracted()
    {
        ResetToDefault();
    }
}