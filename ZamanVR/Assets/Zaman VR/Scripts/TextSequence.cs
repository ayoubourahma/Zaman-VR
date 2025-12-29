using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Events;

public class TextSequence : MonoBehaviour
{
    public enum PlayMode
    {
        Automatic,
        Manual
    }

    [Header("UI Settings")]
    public TMP_Text textComponent;

    [Header("Play Settings")]
    public PlayMode playMode = PlayMode.Automatic;
    public bool playOnStart = true;

    [Tooltip("Used only in Automatic mode")]
    public float displayDuration = 3.0f;

    [Header("Fade Settings")]
    public bool useFade = true;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.5f;

    [Header("Content")]
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("On Complete")]
    public UnityEvent onSequenceFinished;

    private int currentIndex = 0;
    private Coroutine sequenceCoroutine;
    private bool isPlaying = false;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Get or add CanvasGroup for fading
        canvasGroup = textComponent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = textComponent.gameObject.AddComponent<CanvasGroup>();
        }
        
        if (useFade)
        {
            canvasGroup.alpha = 0f;
        }
    }

    void Start()
    {
        if (playOnStart)
        {
            StartSequence();
        }
    }

    // ---------- PUBLIC API ----------

    public void StartSequence()
    {
        if (textComponent == null || sentences == null || sentences.Length == 0)
        {
            Debug.LogWarning("TextSequence: Missing TextComponent or sentences.");
            return;
        }

        StopSequence();

        currentIndex = 0;
        isPlaying = true;

        if (playMode == PlayMode.Automatic)
        {
            sequenceCoroutine = StartCoroutine(AutoPlay());
        }
        else
        {
            sequenceCoroutine = StartCoroutine(ShowCurrentTextWithFade());
        }
    }

    public void Next()
    {
        if (!isPlaying || playMode != PlayMode.Manual)
            return;

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        currentIndex++;
        sequenceCoroutine = StartCoroutine(ShowCurrentTextWithFade());
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        isPlaying = false;
        
        if (useFade && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    // ---------- INTERNAL ----------

    private IEnumerator AutoPlay()
    {
        while (currentIndex < sentences.Length)
        {
            yield return ShowCurrentTextWithFade();
            
            // Wait for display duration (text is visible during this time)
            yield return new WaitForSeconds(displayDuration);
            
            // Fade out before moving to next
            if (useFade && currentIndex < sentences.Length - 1)
            {
                yield return FadeOut();
            }
            
            currentIndex++;
        }

        FinishSequence();
    }

    private IEnumerator ShowCurrentTextWithFade()
    {
        if (currentIndex >= sentences.Length)
        {
            FinishSequence();
            yield break;
        }

        // Set text while invisible
        textComponent.text = sentences[currentIndex];

        // Fade in
        if (useFade)
        {
            yield return FadeIn();
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    private void FinishSequence()
    {
        isPlaying = false;
        
        if (useFade)
        {
            StartCoroutine(FadeOutAndFinish());
        }
        else
        {
            onSequenceFinished?.Invoke();
        }
    }

    private IEnumerator FadeOutAndFinish()
    {
        yield return FadeOut();
        onSequenceFinished?.Invoke();
    }
}