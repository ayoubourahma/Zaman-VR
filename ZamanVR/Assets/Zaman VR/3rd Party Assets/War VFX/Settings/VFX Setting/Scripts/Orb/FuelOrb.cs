using UnityEngine;
using System.Collections;

public class RocketOrbCollector : MonoBehaviour
{
    [Header("Fuel")]
    public FuelSliderTest fuelSystem;
    public string orbTag = "FuelOrb";

    [Tooltip("Fuel added per second while staying inside (can be negative)")]
    public float fuelPerSecond = 0.25f;

    [Tooltip("Fuel added instantly on enter (can be negative)")]
    public float fuelOnEnter = 0.15f;

    [Header("Collection Effect")]
    public GameObject collectionEffect;
    public float effectDuration = 1f;

    [Header("Settings")]
    public bool destroyOnEnter = false; // small orbs = true, big bubbles = false

    bool isInside;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(orbTag)) return;

        isInside = true;

        // Instant fuel (small orb behavior)
        if (fuelSystem && fuelOnEnter != 0f)
            fuelSystem.Gain(fuelOnEnter);

        // Play effect once
        if (collectionEffect)
            StartCoroutine(PlayEffect());

        if (destroyOnEnter)
            Destroy(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(orbTag)) return;
        isInside = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!isInside) return;
        if (!other.CompareTag(orbTag)) return;
        if (!fuelSystem) return;

        // Continuous fuel (big bubble behavior)
        fuelSystem.Gain(fuelPerSecond * Time.deltaTime);
    }

    IEnumerator PlayEffect()
    {
        collectionEffect.SetActive(true);
        yield return new WaitForSeconds(effectDuration);
        collectionEffect.SetActive(false);
    }
}
