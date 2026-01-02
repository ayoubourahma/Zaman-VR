using UnityEngine;

public class RocketTakeOffLandDetector : MonoBehaviour
{
    [Header("Settings")]
    public float groundCheckDistance = 1f;      // Distance to check for ground
    public LayerMask groundLayer;               // Assign ground layers here
    public float velocityThreshold = 0.5f;      // Minimum vertical speed to count as takeoff/landing

    [Header("VFX Events")]
    public GameObject takeOffVFX;
    public GameObject landingVFX;

    private bool isGrounded;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        isGrounded = false;
    }

    void Update()
    {
        bool grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);

        float verticalVel = rb.linearVelocity.y;

        // LANDING
        if (!isGrounded && grounded && verticalVel <= velocityThreshold)
        {
            isGrounded = true;
            PlayLandingVFX();
        }

        // TAKEOFF
        if (isGrounded && !grounded && verticalVel > velocityThreshold)
        {
            isGrounded = false;
            PlayTakeOffVFX();
        }
    }

    void PlayTakeOffVFX()
    {
        if (takeOffVFX)
        {
            takeOffVFX.SetActive(true);
            // Optional: reset after particle lifetime
            // Destroy(takeOffVFX, 2f);
        }
    }

    void PlayLandingVFX()
    {
        if (landingVFX)
        {
            landingVFX.SetActive(true);
            // Optional: reset after particle lifetime
            // Destroy(landingVFX, 2f);
        }
    }
}
