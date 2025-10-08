using UnityEngine;

public class ParticleSonarManager : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem sonarParticle;
    
    [Header("Detection Settings")]
    [SerializeField] private float maxRadius = 30f;
    [SerializeField] private float detectionSpeed = 15f;
    [SerializeField] private LayerMask detectionLayers;
    
    private float currentRadius;
    private bool isScanning;
    private EagleVisionManager eagleVisionManager;

    void Start()
    {
        eagleVisionManager = GetComponent<EagleVisionManager>();
        
        if (sonarParticle != null)
            sonarParticle.Stop();
    }

    void Update()
    {
        if (isScanning)
        {
            currentRadius += detectionSpeed * Time.deltaTime;
            
            // Detect objects in current radius
            if (eagleVisionManager != null)
                eagleVisionManager.DetectObjectsAtRadius(transform.position, currentRadius);
            
            // Stop when max radius reached
            if (currentRadius >= maxRadius)
            {
                isScanning = false;
                currentRadius = 0f;
            }
        }
    }

    public void StartPulse()
    {
        if (sonarParticle != null)
        {
            sonarParticle.Play();
        }
        
        currentRadius = 0f;
        isScanning = true;
    }

    public void StopPulse()
    {
        if (sonarParticle != null)
        {
            sonarParticle.Stop();
        }
        
        isScanning = false;
        currentRadius = 0f;
    }

    void OnDrawGizmos()
    {
        if (isScanning && Application.isPlaying)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, maxRadius);
    }
}