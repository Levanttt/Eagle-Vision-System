using UnityEngine;

public class SimpleCameraOrbit : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Drag Player kesini
    
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float sideOffset = 0.5f;
    
    private float currentRotation = 0f;
    
    void LateUpdate()
    {
        if (target == null) return;
        
        // Input untuk rotate kamera (Q/E atau Mouse saat hold klik kanan)
        if (Input.GetKey(KeyCode.Q))
            currentRotation -= rotationSpeed * Time.deltaTime * 50f;
        
        if (Input.GetKey(KeyCode.E))
            currentRotation += rotationSpeed * Time.deltaTime * 50f;
        
        // ATAU pakai mouse kalau hold klik kanan
        if (Input.GetMouseButton(1)) // Klik kanan
        {
            currentRotation += Input.GetAxis("Mouse X") * rotationSpeed * 50f * Time.deltaTime;
        }
        
        // Hitung posisi kamera
        Quaternion rotation = Quaternion.Euler(0, currentRotation, 0);
        Vector3 offset = new Vector3(sideOffset, height, -distance);
        Vector3 desiredPosition = target.position + rotation * offset;
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
        
        // Look at player
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}