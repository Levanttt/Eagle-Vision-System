using UnityEngine;

public class EnemyTarget : EagleVisionTarget
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxDistanceFromPlayer = 50f;

    private Transform playerTransform;
    private bool permanentHighlight = false;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Hapus highlight kalau terlalu jauh
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

    public void OnKilled()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        enabled = false;
    }
}