using UnityEngine;
using System;

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

    // Public property untuk akses current radius
    public float CurrentRadius => currentRadius;
    public bool IsScanning => isScanning;

    // Event untuk notify ketika pulse selesai
    public event Action OnPulseComplete;

    void Start()
    {
        eagleVisionManager = GetComponent<EagleVisionManager>();
    }

    void Update()
    {
        if (isScanning)
        {
            float previousRadius = currentRadius;
            currentRadius += detectionSpeed * Time.deltaTime;
            
            // Stop when max radius reached
            if (currentRadius >= maxRadius)
            {
                isScanning = false;
                currentRadius = 0f;
                
                // Trigger event ketika pulse selesai
                OnPulseComplete?.Invoke();
            }
        }
    }

    // Method untuk cek apakah objek dalam range pulse current
    public bool IsObjectInCurrentPulseRange(Vector3 objectPosition)
    {
        if (!isScanning) return false;
        
        float distance = Vector3.Distance(transform.position, objectPosition);
        return distance <= currentRadius;
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