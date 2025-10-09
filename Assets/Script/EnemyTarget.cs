using UnityEngine;

public class EnemyTarget : EagleVisionTarget
{   
    protected override void Awake()
    {
        base.Awake();
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    // Dipanggil saat enemy mati
    public void OnKilled()
    {
        ResetToDefault();
        enabled = false;
    }
}