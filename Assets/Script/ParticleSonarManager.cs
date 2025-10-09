using UnityEngine;

public class ParticleSonarManager : MonoBehaviour
{
    [Header("Particle Prefab")]
    [SerializeField] private GameObject sonarPrefab;
    
    [Header("Detection Settings")]
    [SerializeField] private float maxRadius = 30f;
    [SerializeField] private float detectionSpeed = 15f;
    
    private ParticleSystem sonarParticle;
    private GameObject currentSonarInstance;
    private float currentRadius;
    private bool isScanning;
    private EagleVisionManager eagleVisionManager;

    void Start()
    {
        eagleVisionManager = GetComponent<EagleVisionManager>();
    }

    void Update()
    {
        if (isScanning)
        {
            currentRadius += detectionSpeed * Time.deltaTime;
            
            // HAPUS pemanggilan DetectObjectsAtRadius karena sudah diganti sistem area
            // if (eagleVisionManager != null)
            //     eagleVisionManager.DetectObjectsAtRadius(transform.position, currentRadius);
            
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
        // Instantiate prefab di posisi player
        if (sonarPrefab != null)
        {
            currentSonarInstance = Instantiate(sonarPrefab, transform.position, Quaternion.identity);
            
            // Get particle system dari child (Sphere)
            sonarParticle = currentSonarInstance.GetComponentInChildren<ParticleSystem>();
            
            if (sonarParticle != null)
            {
                sonarParticle.Play();
            }
            
            // Destroy setelah particle selesai
            float duration = sonarParticle != null ? sonarParticle.main.duration : 2f;
            Destroy(currentSonarInstance, duration + 0.5f);
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
        
        if (currentSonarInstance != null)
        {
            Destroy(currentSonarInstance);
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