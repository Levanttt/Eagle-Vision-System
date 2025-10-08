using UnityEngine;

public class ItemTarget : EagleVisionTarget
{
    [Header("Item Settings")]
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

    public void OnPickedUp()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        gameObject.SetActive(false);
    }

    public void OnDestroyed()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        gameObject.SetActive(false);
    }
}