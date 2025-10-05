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
        // Item tetap highlight sampai jauh dari player (seperti enemy)
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

    public void KeepHighlightButRestoreLayer()
    {
        //RestoreLayer();
        //SetRenderQueue(2500);
    }

    // Dipanggil saat item diambil player
    public void OnPickedUp()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        
        // Destroy atau disable object
        gameObject.SetActive(false);
    }

    // Dipanggil saat item di-"kill"/hancurkan
    public void OnDestroyed()
    {
        ResetToDefault();
        permanentHighlight = false;
        isScanned = false;
        
        // Bisa tambah efek hancur, partikel, dll
        gameObject.SetActive(false);
    }
}