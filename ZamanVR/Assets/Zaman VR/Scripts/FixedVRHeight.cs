using UnityEngine;

public class FixedVRHeight : MonoBehaviour
{
    public Transform centerEye;   // CenterEyeAnchor
    public float standingHeight = 1.7f;
    public float seatedHeight = 1.2f;

    public bool isStanding = true;

    private float baseRigY;

    void Start()
    {
        // Save the initial Y position of the rig
        baseRigY = transform.position.y;
    }

    void LateUpdate()
    {
        float targetHeight = isStanding ? standingHeight : seatedHeight;

        // Headset height relative to the rig
        float headLocalY = centerEye.localPosition.y;

        // Set absolute Y position (no accumulation)
        Vector3 pos = transform.position;
        pos.y = baseRigY + (targetHeight - headLocalY);
        transform.position = pos;
    }

    public void SetStanding()
    {
        isStanding = true;
    }

    public void SetSeated()
    {
        isStanding = false;
    }
}
