using UnityEngine;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Drag Player GameObject kesini")]
    public Transform target;
    
    [Header("Orbit Settings")]
    [Tooltip("Jarak kamera dari player")]
    public float distance = 5f;
    
    [Tooltip("Tinggi kamera dari ground")]
    public float height = 2f;
    
    [Tooltip("Kecepatan rotate horizontal (kiri-kanan)")]
    public float horizontalSpeed = 100f;
    
    [Tooltip("Kecepatan rotate vertical (atas-bawah)")]
    public float verticalSpeed = 50f;
    
    [Header("Vertical Limits")]
    [Tooltip("Limit angle atas (derajat)")]
    public float maxVerticalAngle = 80f;
    
    [Tooltip("Limit angle bawah (derajat)")]
    public float minVerticalAngle = -30f;
    
    [Header("Smoothing")]
    [Tooltip("Smooth movement kamera (0 = instant, lebih besar = lebih smooth)")]
    public float smoothSpeed = 10f;
    
    [Header("Input")]
    [Tooltip("Pakai mouse untuk rotate (hold klik kanan)")]
    public bool useRightClick = true;
    
    [Tooltip("Atau rotate terus tanpa hold mouse")]
    public bool alwaysRotate = false;
    
    // Private variables
    private float currentHorizontalAngle;
    private float currentVerticalAngle;
    private Vector3 currentLookAtPos;
    
    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target belum di-assign! Drag Player ke slot Target.");
            enabled = false;
            return;
        }
        
        // Initial rotation dari posisi kamera sekarang
        Vector3 angles = transform.eulerAngles;
        currentHorizontalAngle = angles.y;
        currentVerticalAngle = angles.x;
        
        // Lock cursor (opsional, comment kalau gak mau)
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Input untuk rotate kamera
        bool canRotate = alwaysRotate || (useRightClick && Input.GetMouseButton(1)) || (!useRightClick);
        
        if (canRotate)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            currentHorizontalAngle += mouseX * horizontalSpeed * Time.deltaTime;
            currentVerticalAngle -= mouseY * verticalSpeed * Time.deltaTime;
            
            // Clamp vertical angle
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        
        // Hitung posisi kamera berdasarkan orbit
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
        Vector3 offset = new Vector3(0, height, -distance);
        Vector3 desiredPosition = target.position + rotation * offset;
        
        // Smooth camera movement
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Look at target dengan smooth
        Vector3 lookAtPosition = target.position + Vector3.up * height * 0.5f;
        currentLookAtPos = Vector3.Lerp(currentLookAtPos, lookAtPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(currentLookAtPos);
        
        // Unlock cursor dengan ESC (opsional)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    // Helper untuk draw gizmo
    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(target.position, 0.5f);
        Gizmos.DrawLine(target.position, transform.position);
    }
}