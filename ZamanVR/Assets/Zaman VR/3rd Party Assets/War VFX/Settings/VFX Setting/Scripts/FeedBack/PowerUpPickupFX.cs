using UnityEngine;

public class PowerUpMaterialFX : MonoBehaviour
{
    [Header("Rocket Target")]
    [Tooltip("Rocket renderers affected by the power-up")]
    public Renderer[] targetRenderers;

    [Header("Orb Visuals")]
    [Tooltip("Renderers to disable when orb is picked")]
    public Renderer[] orbRenderers;
    public Collider orbCollider;
    public bool disableOrbGameObject = false;

    [Header("Material")]
    public Material targetMaterial;

    [Header("Shader Property")]
    public string powerProperty = "_Power";

    [Header("Animation")]
    public float duration = 0.5f;
    public AnimationCurve powerCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Reset")]
    public float resetDelay = 0.1f;

    [Header("Debug")]
    public bool enableDebug = false;

    bool isPlaying;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        ValidateSetup();
        SetPower(0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (isPlaying) return;
        if (!other.CompareTag("Player")) return;

        if (enableDebug)
            Debug.Log("[PowerUpFX] Orb collected", this);

        DisableOrbVisuals();
        Play();
    }

    // ----------------------------

    public void Play()
    {
        isPlaying = true;
        StopAllCoroutines();
        StartCoroutine(PowerRoutine());
    }

    System.Collections.IEnumerator PowerRoutine()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float value = powerCurve.Evaluate(t);
            SetPower(value);
            yield return null;
        }

        yield return new WaitForSeconds(resetDelay);
        SetPower(0f);
        isPlaying = false;
    }

    // ----------------------------

    void SetPower(float value)
    {
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (!targetRenderers[i]) continue;

            targetRenderers[i].GetPropertyBlock(mpb);
            mpb.SetFloat(powerProperty, value);
            targetRenderers[i].SetPropertyBlock(mpb);
        }
    }

    void DisableOrbVisuals()
    {
        if (orbCollider)
            orbCollider.enabled = false;

        for (int i = 0; i < orbRenderers.Length; i++)
        {
            if (orbRenderers[i])
                orbRenderers[i].enabled = false;
        }

        if (disableOrbGameObject)
            gameObject.SetActive(false);
    }

    void ValidateSetup()
    {
        if (targetMaterial)
        {
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                if (targetRenderers[i])
                    targetRenderers[i].sharedMaterial = targetMaterial;
            }
        }
    }
}
