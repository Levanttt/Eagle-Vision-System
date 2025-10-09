using UnityEngine;

public class HidingTarget : EagleVisionTarget
{
    [Header("Hiding Spot Settings")]
    [SerializeField] private bool canHidePlayer = true;
    [SerializeField] private Transform hidePosition; // Posisi player saat hide
    [SerializeField] private float interactionDistance = 2f;

    private bool isPlayerHiding = false;
    private Transform playerTransform;

    protected override void Awake()
    {
        base.Awake();
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // Optional: Auto-show interaction prompt when player nearby
        if (canHidePlayer && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance <= interactionDistance && !isPlayerHiding)
            {
                // TODO: Show UI prompt "Press E to Hide"
            }
        }
    }

    public override void Scan(Color color, int highlightLayer)
    {
        base.Scan(color, highlightLayer);
    }

    // Dipanggil saat player masuk hiding spot
    public void OnPlayerEnter()
    {
        if (!canHidePlayer) return;

        isPlayerHiding = true;
        
        // Move player ke hide position
        if (hidePosition != null && playerTransform != null)
        {
            playerTransform.position = hidePosition.position;
            playerTransform.rotation = hidePosition.rotation;
        }

        // TODO: Disable player visibility to enemies
        // TODO: Play hide animation
        // TODO: Change camera angle

        Debug.Log($"Player hiding in {gameObject.name}");
    }

    // Dipanggil saat player keluar hiding spot
    public void OnPlayerExit()
    {
        if (!isPlayerHiding) return;

        isPlayerHiding = false;

        // TODO: Enable player visibility
        // TODO: Play exit animation
        // TODO: Restore camera

        Debug.Log($"Player exited {gameObject.name}");
    }

    // Check apakah player sedang bersembunyi
    public bool IsPlayerHiding()
    {
        return isPlayerHiding;
    }

    void OnDrawGizmos()
    {
        // Draw interaction range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        // Draw hide position
        if (hidePosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hidePosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, hidePosition.position);
        }
    }
}