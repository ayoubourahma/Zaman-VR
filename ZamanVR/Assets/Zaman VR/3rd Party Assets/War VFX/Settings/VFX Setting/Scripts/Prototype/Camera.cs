using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 2.5f, -10f);

    [Header("Follow")]
    public float followSpeed = 4f;

    [Header("Vertical Behavior")]
    public float lookAheadY = 2f;
    public float minY = -999f; // optional clamp

    Vector3 velocity;

    void FixedUpdate()
    {
        if (!target) return;

        // Desired position
        Vector3 desiredPos = target.position + offset;

        // Look-ahead upward only (important for climbing games)
        if (target.GetComponent<Rigidbody>())
        {
            float verticalVel = target.GetComponent<Rigidbody>().linearVelocity.y;
            desiredPos.y += Mathf.Clamp(verticalVel * 0.3f, 0f, lookAheadY);
        }

        // Prevent camera from going too low (optional)
        desiredPos.y = Mathf.Max(desiredPos.y, minY);

        // Smooth follow
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref velocity,
            1f / followSpeed
        );
    }
}
