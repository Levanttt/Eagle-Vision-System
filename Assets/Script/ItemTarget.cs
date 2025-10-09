using UnityEngine;

public class ItemTarget : EagleVisionTarget
{
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    // Dipanggil saat item di-pickup
    public void OnPickedUp()
    {
        ResetToDefault();
        gameObject.SetActive(false);
    }

    // Dipanggil saat item di-destroy
    public void OnDestroyed()
    {
        ResetToDefault();
        gameObject.SetActive(false);
    }
}