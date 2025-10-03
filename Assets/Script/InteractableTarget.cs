using UnityEngine;

public class InteractableTarget : EagleVisionTarget
{
    [Header("Interactable Settings")]
    [SerializeField] private float highlightDurationAfterEV = 3f;

    private float highlightTimer = 0f;
    private bool timerActive = false;

    void Update()
    {
        if (timerActive)
        {
            highlightTimer -= Time.deltaTime;
            
            if (highlightTimer <= 0f)
            {
                ResetToDefault();
                timerActive = false;
            }
        }
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    public void StartFadeTimer()
    {
        if (isHighlighted)
        {
            highlightTimer = highlightDurationAfterEV;
            timerActive = true;
        }
    }

    // Dipanggil saat player interact
    public void OnInteracted()
    {
        ResetToDefault();
        timerActive = false;
        
        // Opsional: trigger interaksi, animasi, dll
    }
}