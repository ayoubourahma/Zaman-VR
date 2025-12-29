using UnityEngine;
using System.Collections.Generic;

public class BoatRider : MonoBehaviour
{
    private struct Rider
    {
        public Transform root;
        public Vector3 lastRiderPos;
    }

    private List<Transform> activeRiders = new List<Transform>();
    private Vector3 lastBoatPosition;
    private Quaternion lastBoatRotation;

    void Start()
    {
        lastBoatPosition = transform.position;
        lastBoatRotation = transform.rotation;
    }

    // We use LateUpdate to ensure the boat has moved first
    void LateUpdate()
    {
        // 1. Calculate how much the boat moved/rotated since the last frame
        Vector3 boatDeltaPos = transform.position - lastBoatPosition;
        Quaternion boatDeltaRot = transform.rotation * Quaternion.Inverse(lastBoatRotation);

        foreach (Transform riderRoot in activeRiders)
        {
            if (riderRoot == null) continue;

            // 2. Handle Rotation (Orbiting the player around the boat's center)
            // This prevents the boat from "sliding" out from under the player when it turns
            Vector3 relativePos = riderRoot.position - transform.position;
            Vector3 rotatedRelativePos = boatDeltaRot * relativePos;
            
            // 3. Apply the movement to the XR Rig Root
            // New Position = Boat Position + Rotated Offset + Straight translation
            riderRoot.position = transform.position + rotatedRelativePos + (boatDeltaPos - (boatDeltaPos * 0)); 
            
            // 4. Rotate the player root so they turn with the boat
            riderRoot.rotation = boatDeltaRot * riderRoot.rotation;
        }

        // Store current values for the next frame
        lastBoatPosition = transform.position;
        lastBoatRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detect the XR Rig. It searches for the top-most parent.
        // Make sure your XR Rig has a "Player" tag or change this check.
        if (other.CompareTag("Player") || other.name.Contains("XR") || other.name.Contains("OVR"))
        {
            Transform root = other.transform.root;
            if (!activeRiders.Contains(root))
            {
                activeRiders.Add(root);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Transform root = other.transform.root;
        if (activeRiders.Contains(root))
        {
            activeRiders.Remove(root);
        }
    }
}