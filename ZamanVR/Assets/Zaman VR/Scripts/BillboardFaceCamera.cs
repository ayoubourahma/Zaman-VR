using UnityEngine;

public class BillboardFaceCamera : MonoBehaviour
{
    [Header("Billboard Settings")]
    [Tooltip("Leave empty to auto-find the main camera")]
    public Camera targetCamera;
    
    [Tooltip("Lock the Y-axis rotation (useful for standing characters)")]
    public bool lockYAxis = true;
    
    [Tooltip("Invert the facing direction")]
    public bool invertFacing = false;
    
    [Tooltip("Initial local rotation offset to preserve (e.g., 90 on X-axis)")]
    public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);

    private Transform camTransform;

    void Start()
    {
        // If no camera assigned, find the main camera (works for VR too)
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        if (targetCamera != null)
        {
            camTransform = targetCamera.transform;
        }
        else
        {
            Debug.LogWarning("No camera found! Billboard won't work.");
        }
    }

    void LateUpdate()
    {
        if (camTransform == null) return;

        // Calculate direction to camera
        Vector3 directionToCamera = camTransform.position - transform.position;
        
        // Invert if needed
        if (invertFacing)
        {
            directionToCamera = -directionToCamera;
        }

        // Lock Y-axis if enabled (keeps billboard upright)
        if (lockYAxis)
        {
            directionToCamera.y = 0;
        }

        // Rotate to face the camera
        if (directionToCamera != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToCamera);
            
            // Apply the offset rotation to preserve your initial setup
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            transform.rotation = lookRotation * offsetRotation;
        }
    }
}