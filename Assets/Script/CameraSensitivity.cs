using UnityEngine;
using Cinemachine;

public class CameraSensitivity : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float sensitivity = 1f;
    
    private CinemachinePOV pov;
    
    void Start()
    {
        pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
    }
    
    void Update()
    {
        // Adjust sensitivity on the fly
        pov.m_HorizontalAxis.m_MaxSpeed = 200 * sensitivity;
        pov.m_VerticalAxis.m_MaxSpeed = 150 * sensitivity;
    }
}