using UnityEngine;

public class EnemyTarget : EagleVisionTarget
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxDistanceFromPlayer = 50f;
    [SerializeField] private float highlightDurationAfterEV = 5f; // waktu sebelum hilang highlight

    private Transform playerTransform;
    private bool permanentHighlight = false;
    private float highlightTimer = 0f;
    private bool timerActive = false;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Timer auto-reset setelah EV
        if (timerActive)
        {
            highlightTimer -= Time.deltaTime;
            if (highlightTimer <= 0f)
            {
                ResetToDefault();
                timerActive = false;
                permanentHighlight = false;
            }
        }

        // Hapus highlight kalau terlalu jauh (opsional)
        if (isScanned && permanentHighlight && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance > maxDistanceFromPlayer)
            {
                ResetToDefault();
                permanentHighlight = false;
            }
        }
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
        permanentHighlight = true;
    }

    public void StartFadeTimer()
    {
        if (isHighlighted)
        {
            highlightTimer = highlightDurationAfterEV;
            timerActive = true;
        }
    }

    public void OnKilled()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        enabled = false;
    }
}
