using UnityEngine;

public class ACPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Kecepatan jalan")]
    public float walkSpeed = 3f;
    
    [Tooltip("Kecepatan lari")]
    public float runSpeed = 6f;
    
    [Tooltip("Kecepatan rotasi player")]
    public float rotationSpeed = 10f;
    
    [Header("Input")]
    [Tooltip("Tombol untuk lari (default: Left Shift)")]
    public KeyCode runKey = KeyCode.LeftShift;
    
    // Components
    private CharacterController controller;
    private Animator animator;
    
    // Movement
    private Vector3 moveDirection;
    private float currentSpeed;
    private float verticalVelocity;
    private float gravity = -15f;
    
    void Start()
    {
        // Get components
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        if (controller == null)
        {
            Debug.LogError("CharacterController tidak ditemukan! Tambahkan CharacterController ke Player.");
        }
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        HandleMovement();
        HandleGravity();
        HandleCursor();
    }
    
    void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(runKey);
        
        // Arah gerakan relatif ke kamera
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        
        // Flatten (no vertical component)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Calculate move direction
        moveDirection = (cameraForward * vertical + cameraRight * horizontal).normalized;
        
        // Set speed
        if (moveDirection.magnitude >= 0.1f)
        {
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Rotate player ke arah gerakan (smooth)
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Move player
            if (controller != null)
            {
                controller.Move(moveDirection * currentSpeed * Time.deltaTime);
            }
        }
        else
        {
            currentSpeed = 0f;
        }
        
        // Update animator (optional)
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
            animator.SetBool("IsRunning", isRunning && moveDirection.magnitude > 0.1f);
        }
    }
    
    void HandleGravity()
    {
        if (controller == null) return;
        
        // Apply gravity
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = -2f; // Small value to keep grounded
            }
            
            // Jump (optional - bisa dihapus kalau gak perlu)
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(2f * -gravity); // Jump height ~1 meter
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        
        // Apply vertical velocity
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
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
}