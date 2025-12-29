using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Position Settings")]
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private bool followPosition = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 5f;
    
    [Header("Smooth Follow")]
    [SerializeField] private bool smoothFollow = false;
    [SerializeField] private float followSpeed = 5f;

    void Start()
    {
        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
            return;

        // Calculate target position
        if (followPosition)
        {
            Vector3 targetPosition = cameraTransform.position + 
                                    cameraTransform.forward * distanceFromCamera + 
                                    cameraTransform.TransformDirection(offset);

            if (smoothFollow)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = targetPosition;
            }
        }

        // Calculate target rotation
        if (followRotation)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);

            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}