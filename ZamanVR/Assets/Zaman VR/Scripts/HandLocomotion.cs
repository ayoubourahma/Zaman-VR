using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class HandLocomotion : MonoBehaviour
{
    [Header("Hand References")]
    [SerializeField] private OVRHand leftHand;
    [SerializeField] private OVRHand rightHand;
    
    [Header("Movement Settings")]
    [SerializeField] private Transform playerRig;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float minSwingSpeed = 0.5f;
    
    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.1f;
    
    private Vector3 lastLeftHandPos;
    private Vector3 lastRightHandPos;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;

    void Start()
    {
        if (playerRig == null)
            playerRig = transform;
            
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
            
        // Initialize hand positions
        if (leftHand != null)
            lastLeftHandPos = leftHand.transform.position;
        if (rightHand != null)
            lastRightHandPos = rightHand.transform.position;
    }

    void Update()
    {
        if (!IsHandTrackingActive())
            return;

        Vector3 moveDirection = CalculateMovementFromHands();
        
        // Smooth the movement
        targetVelocity = Vector3.SmoothDamp(targetVelocity, moveDirection, ref currentVelocity, smoothTime);
        
        // Apply movement relative to camera forward direction
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = cameraTransform.right;
        right.y = 0;
        right.Normalize();
        
        Vector3 movement = (forward * targetVelocity.z + right * targetVelocity.x) * Time.deltaTime;
        playerRig.position += movement;
    }

    private Vector3 CalculateMovementFromHands()
    {
        Vector3 totalMovement = Vector3.zero;
        int activeHands = 0;

        // Calculate left hand swing
        if (leftHand != null && leftHand.IsTracked)
        {
            Vector3 leftHandPos = leftHand.transform.position;
            Vector3 leftVelocity = (leftHandPos - lastLeftHandPos) / Time.deltaTime;
            
            // Only count downward and forward swings
            if (leftVelocity.y < -minSwingSpeed)
            {
                totalMovement += new Vector3(leftVelocity.x, 0, Mathf.Abs(leftVelocity.y)) * speedMultiplier;
                activeHands++;
            }
            
            lastLeftHandPos = leftHandPos;
        }

        // Calculate right hand swing
        if (rightHand != null && rightHand.IsTracked)
        {
            Vector3 rightHandPos = rightHand.transform.position;
            Vector3 rightVelocity = (rightHandPos - lastRightHandPos) / Time.deltaTime;
            
            // Only count downward and forward swings
            if (rightVelocity.y < -minSwingSpeed)
            {
                totalMovement += new Vector3(rightVelocity.x, 0, Mathf.Abs(rightVelocity.y)) * speedMultiplier;
                activeHands++;
            }
            
            lastRightHandPos = rightHandPos;
        }

        // Average if both hands are swinging
        if (activeHands > 1)
            totalMovement /= activeHands;

        return totalMovement;
    }

    private bool IsHandTrackingActive()
    {
        bool leftTracked = leftHand != null && leftHand.IsTracked;
        bool rightTracked = rightHand != null && rightHand.IsTracked;
        return leftTracked || rightTracked;
    }
}