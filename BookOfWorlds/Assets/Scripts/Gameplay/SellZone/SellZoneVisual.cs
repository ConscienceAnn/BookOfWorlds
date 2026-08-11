using UnityEngine;

public class SellZoneVisual : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem glowParticles;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (glowParticles != null)
                glowParticles.Play();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (glowParticles != null)
                glowParticles.Stop();
        }
    }

   
}