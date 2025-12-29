using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class BoatWaypointFollower : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> waypoints;
    public float stoppingDistance = 1.0f;
    public bool loop = true;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float acceleration = 1.5f;
    public float rotationSpeed = 1.2f; // Lower = slower, "heavier" turns
    
    [Header("Boat Feel (Visual Only)")]
    public float bobbingSpeed = 1.5f;
    public float bobbingAmount = 0.15f;
    public float bankingAmount = 5.0f; // Tilt when turning
    [Header("Event to fire at the end")]
    public UnityEvent onFinalDestinationReached;
    private int currentWaypointIndex = 0;
    private float currentSpeed = 0f;
    private Vector3 basePosition;
    private bool isFinished = false;

    void Start()
    {
        if (waypoints.Count > 0)
        {
            basePosition = transform.position;
        }
    }

    void Update()
    {
        if (waypoints.Count == 0 || isFinished) return;

        MoveBoat();
        ApplyBoatJuice(); // Adds the bobbing and tilting
    }

    void MoveBoat()
    {
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);

        // 1. Smooth Acceleration & Deceleration
        // Slow down as we approach the final waypoint (if not looping)
        float targetSpeed = maxSpeed;
        if (!loop && currentWaypointIndex == waypoints.Count - 1 && distance < 5f)
        {
            targetSpeed = Mathf.Lerp(0, maxSpeed, distance / 5f);
        }
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        // 2. Translate Position
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // 3. Smooth Rotation (Boat-like turning)
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // We use Slerp for a weighted, smooth turn
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 4. Waypoint Switching
        if (distance < stoppingDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                if (loop) currentWaypointIndex = 0;
                else isFinished = true;
                onFinalDestinationReached.Invoke();
            }
        }
    }

    void ApplyBoatJuice()
    {
        // Simple Sine Wave Bobbing
        float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;
        transform.position = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);

        // Banking (Tilt the boat slightly based on how much it is turning)
        // This makes it look like the boat is leaning into the water
        float turnAngle = Vector3.SignedAngle(transform.forward, waypoints[currentWaypointIndex].position - transform.position, Vector3.up);
        float tilt = Mathf.Clamp(turnAngle, -bankingAmount, bankingAmount);
        
        // Apply tilt to the Z axis
        transform.rotation *= Quaternion.Euler(0, 0, -tilt * (currentSpeed / maxSpeed));
    }

    // Visual helper in Editor
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Count; i++)
        {
            Gizmos.DrawSphere(waypoints[i].position, 0.5f);
            if (i < waypoints.Count - 1)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            else if (loop)
                Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
        }
    }
}