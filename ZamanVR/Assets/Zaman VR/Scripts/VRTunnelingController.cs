using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class VRTunnelingController : MonoBehaviour
{
    [Header("Shader Settings")]
    [SerializeField] private Material tunnelingMaterial;
    [SerializeField] private string aperturePropertyName = "_ApertureSize";
    [SerializeField] private string featheringPropertyName = "_FeatheringEffect";
    
    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 1.0f;
    [SerializeField] private AnimationCurve apertureCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float minAperture = 0.0f; // Fully closed
    [SerializeField] private float maxAperture = 1.0f; // Fully open
    
    [Header("VR Camera Rig")]
    [SerializeField] private Transform ovrCameraRig;
    
    [Header("Teleport Locations")]
    [SerializeField] private Transform[] teleportLocations;
    [SerializeField] private int currentLocationIndex = 0;
    [SerializeField] private bool loopLocations = false; // Loop back to start when reaching the end
    
    [Header("Events")]
    [SerializeField] private UnityEvent onTeleportStart;
    [SerializeField] private UnityEvent onTeleportComplete;
    [SerializeField] private UnityEvent onReachedLastLocation;
    
    private bool isTransitioning = false;
    private Coroutine currentTransition;

    private void Start()
    {
        // Initialize shader to fully open
        if (tunnelingMaterial != null)
        {
            tunnelingMaterial.SetFloat(aperturePropertyName, maxAperture);
        }
        
        // Teleport to starting location if set
        if (teleportLocations.Length > 0 && currentLocationIndex < teleportLocations.Length)
        {
            TeleportImmediate(currentLocationIndex);
        }
    }

    /// <summary>
    /// Call this method from Unity Button OnClick event
    /// Teleports to the next location in sequence
    /// </summary>
    public void TeleportToNextLocation()
    {
        if (teleportLocations.Length == 0)
        {
            Debug.LogError("No teleport locations assigned!");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("Already transitioning, skipping teleport request");
            return;
        }

        // Calculate next location index
        int nextIndex = currentLocationIndex + 1;

        // Check if we've reached the end
        if (nextIndex >= teleportLocations.Length)
        {
            if (loopLocations)
            {
                nextIndex = 0; // Loop back to start
            }
            else
            {
                Debug.LogWarning("Already at the last location!");
                onReachedLastLocation?.Invoke();
                return;
            }
        }

        TeleportToLocation(nextIndex);
    }

    /// <summary>
    /// Call this method from Unity Button OnClick event
    /// Teleports to the previous location in sequence
    /// </summary>
    public void TeleportToPreviousLocation()
    {
        if (teleportLocations.Length == 0)
        {
            Debug.LogError("No teleport locations assigned!");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("Already transitioning, skipping teleport request");
            return;
        }

        // Calculate previous location index
        int prevIndex = currentLocationIndex - 1;

        // Check if we've reached the beginning
        if (prevIndex < 0)
        {
            if (loopLocations)
            {
                prevIndex = teleportLocations.Length - 1; // Loop to end
            }
            else
            {
                Debug.LogWarning("Already at the first location!");
                return;
            }
        }

        TeleportToLocation(prevIndex);
    }

    /// <summary>
    /// Teleport to a specific location by index
    /// Can be called from OnClick with a specific index
    /// </summary>
    public void TeleportToLocation(int locationIndex)
    {
        if (locationIndex < 0 || locationIndex >= teleportLocations.Length)
        {
            Debug.LogError($"Invalid teleport location index: {locationIndex}");
            return;
        }

        if (isTransitioning)
        {
            Debug.LogWarning("Already transitioning, skipping teleport request");
            return;
        }

        if (teleportLocations[locationIndex] == null)
        {
            Debug.LogError($"Teleport location at index {locationIndex} is null!");
            return;
        }

        currentLocationIndex = locationIndex;
        StartTeleport(teleportLocations[locationIndex].position, teleportLocations[locationIndex].rotation);
    }

    /// <summary>
    /// Reset to the first location
    /// </summary>
    public void ResetToFirstLocation()
    {
        TeleportToLocation(0);
    }

    /// <summary>
    /// Teleport immediately without animation (useful for initialization)
    /// </summary>
    public void TeleportImmediate(int locationIndex)
    {
        if (locationIndex < 0 || locationIndex >= teleportLocations.Length)
        {
            Debug.LogError($"Invalid teleport location index: {locationIndex}");
            return;
        }

        if (teleportLocations[locationIndex] == null)
        {
            Debug.LogError($"Teleport location at index {locationIndex} is null!");
            return;
        }

        currentLocationIndex = locationIndex;
        
        if (ovrCameraRig != null)
        {
            ovrCameraRig.position = teleportLocations[locationIndex].position;
            ovrCameraRig.rotation = teleportLocations[locationIndex].rotation;
        }
    }

    private void StartTeleport(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        currentTransition = StartCoroutine(TeleportWithTunneling(targetPosition, targetRotation));
    }

    private IEnumerator TeleportWithTunneling(Vector3 targetPosition, Quaternion targetRotation)
    {
        isTransitioning = true;
        onTeleportStart?.Invoke();

        // Phase 1: Close the vignette (aperture goes from max to min)
        yield return StartCoroutine(AnimateAperture(maxAperture, minAperture, transitionDuration * 0.5f));

        // Teleport while fully closed
        if (ovrCameraRig != null)
        {
            ovrCameraRig.position = targetPosition;
            ovrCameraRig.rotation = targetRotation;
        }
        else
        {
            Debug.LogError("OVR Camera Rig is not assigned!");
        }

        // Optional: Small delay at peak closure
        yield return new WaitForSeconds(0.1f);

        // Phase 2: Open the vignette (aperture goes from min to max)
        yield return StartCoroutine(AnimateAperture(minAperture, maxAperture, transitionDuration * 0.5f));

        isTransitioning = false;
        currentTransition = null;
        onTeleportComplete?.Invoke();
    }

    private IEnumerator AnimateAperture(float startValue, float endValue, float duration)
    {
        if (tunnelingMaterial == null)
        {
            Debug.LogError("Tunneling material is not assigned!");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curveValue = apertureCurve.Evaluate(t);
            float currentAperture = Mathf.Lerp(startValue, endValue, curveValue);

            tunnelingMaterial.SetFloat(aperturePropertyName, currentAperture);

            yield return null;
        }

        // Ensure we reach the exact end value
        tunnelingMaterial.SetFloat(aperturePropertyName, endValue);
    }

    /// <summary>
    /// Manually control the aperture size (0 = closed, 1 = open)
    /// </summary>
    public void SetAperture(float value)
    {
        if (tunnelingMaterial != null)
        {
            tunnelingMaterial.SetFloat(aperturePropertyName, Mathf.Clamp01(value));
        }
    }

    /// <summary>
    /// Get current location index
    /// </summary>
    public int GetCurrentLocationIndex()
    {
        return currentLocationIndex;
    }

    /// <summary>
    /// Check if at last location
    /// </summary>
    public bool IsAtLastLocation()
    {
        return currentLocationIndex >= teleportLocations.Length - 1;
    }

    /// <summary>
    /// Check if at first location
    /// </summary>
    public bool IsAtFirstLocation()
    {
        return currentLocationIndex == 0;
    }

    private void OnValidate()
    {
        // Clamp current location index in editor
        if (teleportLocations != null && teleportLocations.Length > 0)
        {
            currentLocationIndex = Mathf.Clamp(currentLocationIndex, 0, teleportLocations.Length - 1);
        }
    }
}