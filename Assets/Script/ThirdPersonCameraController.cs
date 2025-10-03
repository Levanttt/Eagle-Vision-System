using UnityEngine;
using Cinemachine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    [Header("Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    
    private CinemachinePOV pov;
    
    void Start()
    {
        // Ambil komponen POV dari virtual camera
        pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        
        // Lock cursor di tengah layar (opsional)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        // Input mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        // Update axis POV
        pov.m_HorizontalAxis.Value += mouseX;
        pov.m_VerticalAxis.Value -= mouseY; // Minus biar inversi natural
    }
}