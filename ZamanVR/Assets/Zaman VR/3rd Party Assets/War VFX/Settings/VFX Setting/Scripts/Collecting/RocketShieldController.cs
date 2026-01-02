using UnityEngine;
using System.Collections;

public class RocketShieldController : MonoBehaviour
{
    [Header("Shield")]
    public GameObject shieldObject;          // Shield visual + collider
    public string shieldPickupTag = "Shield";
    public float shieldDuration = 5f;
    public int maxHits = 2;

    [Header("Debug")]
    public bool debugLogs = false;

    int currentHits;
    Coroutine shieldRoutine;

    void Start()
    {
        DisableShield();
    }

    // ---------------- PICKUP ----------------
    void OnTriggerEnter(Collider other)
    {
        // Shield pickup
        if (other.CompareTag(shieldPickupTag))
        {
            ActivateShield();
            Destroy(other.gameObject);
        }
    }

    // ---------------- SHIELD COLLISION ----------------
    void OnCollisionEnter(Collision collision)
    {
        if (!shieldObject || !shieldObject.activeSelf)
            return;

        // Ignore player self
        if (collision.collider.CompareTag("Player"))
            return;

        currentHits++;

        if (debugLogs)
            Debug.Log($"[Shield] Hit {currentHits}/{maxHits}");

        if (currentHits >= maxHits)
            DisableShield();
    }

    // ---------------- LOGIC ----------------
    void ActivateShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        currentHits = 0;
        shieldObject.SetActive(true);

        shieldRoutine = StartCoroutine(ShieldTimer());

        if (debugLogs)
            Debug.Log("[Shield] Activated");
    }

    IEnumerator ShieldTimer()
    {
        yield return new WaitForSeconds(shieldDuration);
        DisableShield();
    }

    void DisableShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        if (shieldObject)
            shieldObject.SetActive(false);

        if (debugLogs)
            Debug.Log("[Shield] Deactivated");
    }
}
