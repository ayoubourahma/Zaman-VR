using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketController : MonoBehaviour
{
    [Header("Thrust")]
    public float thrustForce = 18f;
    public float maxUpSpeed = 12f;

    [Header("Tilt")]
    public float maxTiltAngle = 16f;
    public float idleTiltSpeed = 30f;

    [Header("Fuel")]
    public bool hasFuel = true;          // ← THIS is what you asked for
    public FuelSliderTest fuelSystem;    // reference

    Rigidbody rb;
    bool isThrusting;

    float currentTilt;
    int tiltDirection = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // fuel check
        hasFuel = fuelSystem && fuelSystem.fuel > 0f;

        // thrust only if fuel
        isThrusting = hasFuel && Input.GetButton("Jump");

        if (!isThrusting)
            UpdateIdleTilt();

        ApplyVisualRotation();
    }

    void FixedUpdate()
    {
        if (isThrusting)
            ApplyLockedThrust();
    }

    // ----------------------------

    void UpdateIdleTilt()
    {
        currentTilt += tiltDirection * idleTiltSpeed * Time.deltaTime;

        if (Mathf.Abs(currentTilt) >= maxTiltAngle)
        {
            currentTilt = Mathf.Clamp(currentTilt, -maxTiltAngle, maxTiltAngle);
            tiltDirection *= -1;
        }
    }

    void ApplyLockedThrust()
    {
        Vector3 thrustDir =
            Quaternion.Euler(0f, 0f, -currentTilt) * Vector3.up;

        rb.AddForce(thrustDir * thrustForce, ForceMode.Acceleration);

        if (rb.linearVelocity.y > maxUpSpeed)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, maxUpSpeed, rb.linearVelocity.z);
    }

    void ApplyVisualRotation()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -currentTilt);
    }

    // --- Public hooks ---
    public bool IsThrusting() => isThrusting;
}
