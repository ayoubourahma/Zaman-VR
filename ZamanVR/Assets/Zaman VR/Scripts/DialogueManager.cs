using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public enum PlayMode
    {
        Automatic,
        Manual
    }

    public enum DialogueLocation
    {
        PlayerHUD,
        NPCPanel
    }

    [Header("UI References")]
    public TMP_Text playerHUDText;
    public GameObject playerHUDPanel;

    [Header("Play Settings")]
    public PlayMode playMode = PlayMode.Automatic;
    public float textDisplayDuration = 3.0f;

    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private bool isDisplayingDialogue = false;
    private Coroutine dialogueCoroutine;
    private DialogueEntry currentEntry;
    private bool waitingForNext = false;

    // Currently active NPC panel
    private TMP_Text currentNPCText;
    private GameObject currentNPCPanel;

    public static DialogueManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (playerHUDPanel != null)
        {
            playerHUDPanel.SetActive(false);
        }
    }

    public void ShowDialogue(string text, 
        DialogueLocation location = DialogueLocation.PlayerHUD,
        TMP_Text npcText = null,
        GameObject npcPanel = null,
        UnityEvent onComplete = null)
    {
        StopCurrentDialogue();
        dialogueQueue.Clear();
        
        dialogueQueue.Enqueue(new DialogueEntry(text, location, npcText, npcPanel, onComplete));
        
        if (!isDisplayingDialogue)
        {
            dialogueCoroutine = StartCoroutine(ProcessDialogueQueue());
        }
    }

    public void QueueDialogueSequence(string[] texts, 
        TMP_Text npcText = null, 
        GameObject npcPanel = null,
        UnityEvent onSequenceComplete = null)
    {
        if (texts == null || texts.Length == 0) return;

        for (int i = 0; i < texts.Length; i++)
        {
            // First line goes to NPC panel, rest go to player HUD
            DialogueLocation location = (i == 0 && npcPanel != null) 
                ? DialogueLocation.NPCPanel 
                : DialogueLocation.PlayerHUD;

            // Only the last line gets the completion event
            UnityEvent completeEvent = (i == texts.Length - 1) ? onSequenceComplete : null;
            
            dialogueQueue.Enqueue(new DialogueEntry(
                texts[i], 
                location,
                npcText,
                npcPanel,
                completeEvent
            ));
        }
        
        if (!isDisplayingDialogue)
        {
            dialogueCoroutine = StartCoroutine(ProcessDialogueQueue());
        }
    }

    public void QueueDialogue(string text, 
        DialogueLocation location = DialogueLocation.PlayerHUD,
        TMP_Text npcText = null,
        GameObject npcPanel = null,
        UnityEvent onComplete = null)
    {
        dialogueQueue.Enqueue(new DialogueEntry(text, location, npcText, npcPanel, onComplete));
        
        if (!isDisplayingDialogue)
        {
            dialogueCoroutine = StartCoroutine(ProcessDialogueQueue());
        }
    }

    public void Next()
    {
        if (playMode != PlayMode.Manual || !isDisplayingDialogue || !waitingForNext)
            return;

        waitingForNext = false;
    }

    public void StopCurrentDialogue()
    {
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
            dialogueCoroutine = null;
        }
        
        isDisplayingDialogue = false;
        
        if (playerHUDPanel != null)
            playerHUDPanel.SetActive(false);
        
        if (currentNPCPanel != null)
            currentNPCPanel.SetActive(false);

        currentNPCText = null;
        currentNPCPanel = null;
    }

    public void ClearQueue()
    {
        dialogueQueue.Clear();
    }

    public bool IsPlaying()
    {
        return isDisplayingDialogue;
    }

    private IEnumerator ProcessDialogueQueue()
    {
        isDisplayingDialogue = true;

        while (dialogueQueue.Count > 0)
        {
            currentEntry = dialogueQueue.Dequeue();

            // Determine which UI element to use
            TMP_Text targetText;
            GameObject targetPanel;

            if (currentEntry.location == DialogueLocation.NPCPanel && currentEntry.npcPanel != null && currentEntry.npcText != null)
            {
                targetText = currentEntry.npcText;
                targetPanel = currentEntry.npcPanel;
                currentNPCText = targetText;
                currentNPCPanel = targetPanel;

                // Hide player HUD, show NPC panel
                if (playerHUDPanel != null)
                    playerHUDPanel.SetActive(false);
                
                targetPanel.SetActive(true);
            }
            else
            {
                targetText = playerHUDText;
                targetPanel = playerHUDPanel;
                
                // Hide NPC panel, show player HUD
                if (currentNPCPanel != null)
                    currentNPCPanel.SetActive(false);
                
                if (playerHUDPanel != null)
                    playerHUDPanel.SetActive(true);
            }

            // Set text immediately
            if (targetText != null)
            {
                targetText.text = currentEntry.text;
            }

            // Automatic mode: wait for duration
            if (playMode == PlayMode.Automatic)
            {
                yield return new WaitForSeconds(textDisplayDuration);
            }
            // Manual mode: wait for Next() to be called
            else
            {
                waitingForNext = true;
                yield return new WaitUntil(() => !waitingForNext);
            }

            // Invoke line-specific callback
            currentEntry.onComplete?.Invoke();
        }

        // Hide all panels when done
        if (playerHUDPanel != null)
            playerHUDPanel.SetActive(false);
        
        if (currentNPCPanel != null)
            currentNPCPanel.SetActive(false);

        isDisplayingDialogue = false;
        currentNPCText = null;
        currentNPCPanel = null;
    }

    private class DialogueEntry
    {
        public string text;
        public DialogueLocation location;
        public TMP_Text npcText;
        public GameObject npcPanel;
        public UnityEvent onComplete;

        public DialogueEntry(string text, DialogueLocation location, 
            TMP_Text npcText = null, GameObject npcPanel = null, UnityEvent onComplete = null)
        {
            this.text = text;
            this.location = location;
            this.npcText = npcText;
            this.npcPanel = npcPanel;
            this.onComplete = onComplete;
        }
    }
}