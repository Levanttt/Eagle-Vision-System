using UnityEngine;

public class SimpleOrbitMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Kecepatan jalan")]
    public float moveSpeed = 5f;
    
    [Tooltip("Kecepatan lari (hold Shift)")]
    public float sprintSpeed = 8f;
    
    [Tooltip("Kecepatan rotasi player")]
    public float rotationSpeed = 10f;
    
    [Header("Camera Settings")]
    [Tooltip("Jarak kamera dari player")]
    public float cameraDistance = 5f;
    
    [Tooltip("Tinggi kamera dari player")]
    public float cameraHeight = 2f;
    
    [Tooltip("Sensitivitas horizontal (kiri-kanan)")]
    public float horizontalSensitivity = 3f;
    
    [Tooltip("Sensitivitas vertical (atas-bawah)")]
    public float verticalSensitivity = 3f;
    
    [Tooltip("Lock cursor saat play")]
    public bool lockCursor = true;
    
    [Header("Camera Limits")]
    [Tooltip("Batas atas kamera (derajat)")]
    public float maxVerticalAngle = 70f;
    
    [Tooltip("Batas bawah kamera (derajat)")]
    public float minVerticalAngle = -40f;
    
    [Header("Smoothing")]
    [Tooltip("Smooth camera movement")]
    public float cameraSmoothing = 10f;
    
    // Components
    private CharacterController characterController;
    private Camera mainCamera;
    
    // Camera rotation
    private float currentHorizontalAngle = 0f;
    private float currentVerticalAngle = 20f;
    
    // Movement
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    void Start()
    {
        // Get components
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera tidak ditemukan! Pastikan ada Camera dengan tag 'MainCamera'");
        }
        
        // Cursor setup
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Initialize camera angle dari posisi sekarang
        if (mainCamera != null)
        {
            Vector3 angles = mainCamera.transform.eulerAngles;
            currentHorizontalAngle = angles.y;
            currentVerticalAngle = angles.x;
        }
    }
    
    void LateUpdate()
    {
        HandleCameraOrbit();
        HandleMovement();
        HandleCursor();
    }
    
    void HandleCameraOrbit()
    {
        if (mainCamera == null) return;
        
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // Update rotation angles
        currentHorizontalAngle += mouseX * horizontalSensitivity;
        currentVerticalAngle -= mouseY * verticalSensitivity;
        
        // Clamp vertical angle
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        
        // Calculate camera position (orbit around player)
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
        Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
        Vector3 desiredPosition = transform.position + rotation * offset;
        
        // Smooth camera movement
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position, 
            desiredPosition, 
            cameraSmoothing * Time.deltaTime
        );
        
        // Look at player
        Vector3 lookAtPosition = transform.position + Vector3.up * cameraHeight * 0.5f;
        mainCamera.transform.LookAt(lookAtPosition);
    }
    
    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        
        // Grounded check
        bool isGrounded = characterController != null ? characterController.isGrounded : true;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Movement direction relative to camera
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;
        
        // Flatten (no vertical movement)
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();
        
        // Calculate move direction
        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;
        
        // Apply movement
        if (moveDirection.magnitude >= 0.1f)
        {
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Move
            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            
            if (characterController != null)
            {
                characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
            }
            else
            {
                // Fallback kalau gak ada CharacterController
                transform.position += moveDirection * currentSpeed * Time.deltaTime;
            }
        }
        
        // Gravity
        velocity.y += gravity * Time.deltaTime;
        
        if (characterController != null)
        {
            characterController.Move(velocity * Time.deltaTime);
        }
    }
    
    void HandleCursor()
    {
        // Toggle cursor dengan ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    
    // Helper untuk visualisasi di Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw camera orbit radius
        Gizmos.color = Color.cyan;
        Vector3 offset = new Vector3(0, cameraHeight, -cameraDistance);
        Gizmos.DrawWireSphere(transform.position, offset.magnitude);
    }
}