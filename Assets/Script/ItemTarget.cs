using UnityEngine;

public class ItemTarget : EagleVisionTarget
{
    [Header("Item Settings")]
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

    // Dipanggil oleh EagleVisionManager saat EV off
    public void StartFadeTimer()
    {
        if (isHighlighted)
        {
            highlightTimer = highlightDurationAfterEV;
            timerActive = true;
        }
    }

    // Dipanggil saat item diambil player
    public void OnPickedUp()
    {
        ResetToDefault();
        timerActive = false;
        
        // Destroy atau disable object
        gameObject.SetActive(false);
    }
}