using UnityEngine;

public class CollisionParticlePlayer : MonoBehaviour
{
    [Header("Particle Systems to Play")]
    [Tooltip("Assign any particle systems here, they will play on collision.")]
    public ParticleSystem[] particleSystems;

    [Header("Settings")]
    [Tooltip("Optional: play only once per collision")]
    public bool playOncePerCollision = true;

    // To prevent double triggers
    private bool hasPlayed = false;

    void OnCollisionEnter(Collision collision)
    {
        if (playOncePerCollision && hasPlayed)
            return;

        PlayParticles();
        hasPlayed = true;
    }

    /// <summary>
    /// Plays all particle systems assigned in the inspector
    /// </summary>
    public void PlayParticles()
    {
        if (particleSystems == null || particleSystems.Length == 0)
            return;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
                ps.Play();
        }
    }

    /// <summary>
    /// Reset hasPlayed to allow replay
    /// </summary>
    public void ResetPlay()
    {
        hasPlayed = false;
    }
}