using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DialoguePhase
{
    public string phaseName;
    
    [TextArea(3, 10)]
    public string[] dialogueLines;
    
    [Tooltip("The text component for this phase's NPC panel")]
    public TMP_Text npcPanelText;
    
    [Tooltip("The panel GameObject for this phase")]
    public GameObject npcPanel;
    
    public UnityEvent onPhaseStarted;
    public UnityEvent onPhaseFinished;
}

public class CharacterDialogue : MonoBehaviour
{
    [Header("Dialogue Phases")]
    public List<DialoguePhase> dialoguePhases = new List<DialoguePhase>();

    [Header("Trigger Settings")]
    public bool speakOnStart = false;
    public int startPhaseIndex = 0;
    public bool speakOnTriggerEnter = false;
    public bool speakOnlyOnce = true;
    public string playerTag = "Player";

    [Header("Events")]
    public UnityEvent onAnyDialogueStarted;
    public UnityEvent onAllPhasesComplete;

    private int currentPhaseIndex = -1;
    private HashSet<int> spokenPhases = new HashSet<int>();

    void Start()
    {
        // Hide all NPC panels initially
        foreach (var phase in dialoguePhases)
        {
            if (phase.npcPanel != null)
            {
                phase.npcPanel.SetActive(false);
            }
        }

        if (speakOnStart && startPhaseIndex >= 0 && startPhaseIndex < dialoguePhases.Count)
        {
            SpeakPhase(startPhaseIndex);
        }
    }

    /// <summary>
    /// Speak a specific dialogue phase by index
    /// </summary>
    public void SpeakPhase(int phaseIndex)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager not found in scene!");
            return;
        }

        if (phaseIndex < 0 || phaseIndex >= dialoguePhases.Count)
        {
            Debug.LogWarning($"Phase index {phaseIndex} is out of range!");
            return;
        }

        DialoguePhase phase = dialoguePhases[phaseIndex];

        if (phase.dialogueLines == null || phase.dialogueLines.Length == 0)
        {
            Debug.LogWarning($"Phase '{phase.phaseName}' has no dialogue lines!");
            return;
        }

        if (speakOnlyOnce && spokenPhases.Contains(phaseIndex))
        {
            Debug.Log($"Phase '{phase.phaseName}' already spoken and speakOnlyOnce is enabled.");
            return;
        }

        currentPhaseIndex = phaseIndex;
        spokenPhases.Add(phaseIndex);

        onAnyDialogueStarted?.Invoke();
        phase.onPhaseStarted?.Invoke();

        // Create a callback for when this phase finishes
        UnityEvent phaseCompleteEvent = new UnityEvent();
        phaseCompleteEvent.AddListener(() => OnPhaseComplete(phaseIndex));

        // Queue all dialogue lines for this phase
        DialogueManager.Instance.QueueDialogueSequence(
            phase.dialogueLines,
            phase.npcPanelText,
            phase.npcPanel,
            phaseCompleteEvent
        );
    }

    /// <summary>
    /// Speak the next phase in sequence
    /// </summary>
    public void SpeakNextPhase()
    {
        int nextPhase = currentPhaseIndex + 1;
        if (nextPhase < dialoguePhases.Count)
        {
            SpeakPhase(nextPhase);
        }
        else
        {
            Debug.Log("No more phases to speak!");
            onAllPhasesComplete?.Invoke();
        }
    }

    /// <summary>
    /// Speak a phase by name
    /// </summary>
    public void SpeakPhaseByName(string phaseName)
    {
        for (int i = 0; i < dialoguePhases.Count; i++)
        {
            if (dialoguePhases[i].phaseName == phaseName)
            {
                SpeakPhase(i);
                return;
            }
        }
        Debug.LogWarning($"Phase with name '{phaseName}' not found!");
    }

    /// <summary>
    /// Speak a specific line from a specific phase
    /// </summary>
    public void SpeakLineFromPhase(int phaseIndex, int lineIndex)
    {
        if (phaseIndex < 0 || phaseIndex >= dialoguePhases.Count)
            return;

        DialoguePhase phase = dialoguePhases[phaseIndex];

        if (phase.dialogueLines == null || lineIndex < 0 || lineIndex >= phase.dialogueLines.Length)
            return;

        if (DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.ShowDialogue(
            phase.dialogueLines[lineIndex],
            DialogueManager.DialogueLocation.NPCPanel,
            phase.npcPanelText,
            phase.npcPanel
        );
    }

    /// <summary>
    /// Reset specific phase so it can be spoken again
    /// </summary>
    public void ResetPhase(int phaseIndex)
    {
        spokenPhases.Remove(phaseIndex);
    }

    /// <summary>
    /// Reset all phases
    /// </summary>
    public void ResetAllPhases()
    {
        spokenPhases.Clear();
        currentPhaseIndex = -1;
    }

    public bool HasSpokenPhase(int phaseIndex)
    {
        return spokenPhases.Contains(phaseIndex);
    }

    public int GetCurrentPhaseIndex()
    {
        return currentPhaseIndex;
    }

    private void OnPhaseComplete(int phaseIndex)
    {
        if (phaseIndex >= 0 && phaseIndex < dialoguePhases.Count)
        {
            dialoguePhases[phaseIndex].onPhaseFinished?.Invoke();
        }

        // Check if all phases are complete
        if (spokenPhases.Count >= dialoguePhases.Count)
        {
            onAllPhasesComplete?.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (speakOnTriggerEnter && other.CompareTag(playerTag))
        {
            if (!speakOnlyOnce || !spokenPhases.Contains(startPhaseIndex))
            {
                SpeakPhase(startPhaseIndex);
            }
        }
    }
}

// ===== EXAMPLE USAGE =====
/*
Setup in Inspector:
1. Add CharacterDialogue to your companion NPC
2. In "Dialogue Phases" list, add multiple phases:
   - Phase 0: "Tutorial_Start" (Welcome dialogue + tutorial panel)
   - Phase 1: "Tutorial_Complete" (Congratulations + different panel)
   - Phase 2: "First_Enemy" (Warning about enemies + combat panel)
   - Phase 3: "Boss_Approaching" (Boss tips + boss panel)
   - etc.

3. For each phase:
   - Set phase name
   - Add dialogue lines
   - Assign the specific NPC panel for that phase
   - Set up phase events if needed

4. Trigger phases from other scripts:
   - companion.SpeakPhase(0); // Speak first phase
   - companion.SpeakNextPhase(); // Speak next phase in sequence
   - companion.SpeakPhaseByName("Boss_Approaching"); // Speak by name

Example:
public class GameController : MonoBehaviour
{
    public CharacterDialogue companion;

    void OnPlayerReachCheckpoint()
    {
        companion.SpeakNextPhase();
    }

    void OnBossSpawn()
    {
        companion.SpeakPhaseByName("Boss_Approaching");
    }
}
*/