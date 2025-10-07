using UnityEngine;

public class ItemTarget : EagleVisionTarget
{
    [Header("Item Settings")]
    [SerializeField] private float maxDistanceFromPlayer = 50f;
    [SerializeField] private float highlightDurationAfterEV = 5f;

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
