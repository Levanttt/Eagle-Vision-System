using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 2f;
    public float runSpeed = 5f;

    private Animator animator;
    private Rigidbody rb;

    private Vector3 moveInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Normalisasi input biar diagonal ga lebih cepat
        moveInput = new Vector3(moveX, 0, moveZ).normalized;

        // Kirim parameter ke Animator
        animator.SetFloat("Horizontal", moveX);
        animator.SetFloat("Vertical", moveZ * (isRunning ? 2f : 1f)); 
        // Jadi 0-1 buat jalan, 0-2 buat lari
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // Move sesuai arah input relatif kamera/player
        Vector3 move = transform.TransformDirection(moveInput) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }
}
