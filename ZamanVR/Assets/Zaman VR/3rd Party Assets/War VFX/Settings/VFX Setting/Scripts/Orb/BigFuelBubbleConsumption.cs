using UnityEngine;

public class BigFuelBubbleConsumption : MonoBehaviour
{
    [Header("Target Bubble")]
    public Transform bubbleTransform;    // The bubble to scale down
    public Vector3 startScale = Vector3.one;
    public Vector3 minScale = Vector3.zero;

    [Header("Consumption")]
    public float consumptionSpeed = 0.5f; // How fast it shrinks

    [Header("Trigger Settings")]
    public string playerTag = "Player";

    private bool isConsuming = false;

    void Reset()
    {
        bubbleTransform = transform; // default to self
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        isConsuming = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        isConsuming = false;
    }

    void Update()
    {
        if (!isConsuming || bubbleTransform == null) return;

        // Scale down over time
        bubbleTransform.localScale = Vector3.MoveTowards(
            bubbleTransform.localScale,
            minScale,
            consumptionSpeed * Time.deltaTime
        );

        // Optional: destroy when fully consumed
        if (bubbleTransform.localScale == minScale)
        {
            gameObject.SetActive(false); // or Destroy(gameObject);
        }
    }

    public void ResetBubble()
    {
        if (bubbleTransform)
            bubbleTransform.localScale = startScale;
        isConsuming = false;
        gameObject.SetActive(true);
    }
}
