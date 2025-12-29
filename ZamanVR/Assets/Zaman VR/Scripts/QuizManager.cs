using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class QuizQuestion
{
    [TextArea(2, 5)]
    public string question;
    
    public string choiceA;
    public string choiceB;
    public string choiceC;
    
    [Tooltip("0 = A, 1 = B, 2 = C")]
    [Range(0, 2)]
    public int correctAnswerIndex;
    
    [Header("Rewards & Penalties")]
    public int moneyReward = 100;
    public int moneyPenalty = 0;
    
    [Header("Events")]
    public UnityEvent onCorrectAnswer;
    public UnityEvent onWrongAnswer;
}

public class QuizManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text questionText;
    public TMP_Text choiceAText;
    public TMP_Text choiceBText;
    public TMP_Text choiceCText;
    public GameObject quizPanel;
    public GameObject questionsHolder;
    public Image chestImage;
    public Sprite fullChestSprite;
    public Sprite emptyChestSprite;
    
    [Header("Quiz Data")]
    public List<QuizQuestion> questions = new List<QuizQuestion>();
    
    [Header("Settings")]
    public bool testMode = false;
    public bool randomizeQuestions = false;
    public bool allowRetry = false;
    public float chestDisplayDuration = 2.0f;
    
    [Header("Player Reference")]
    public PlayerMoney playerMoney;
    
    [Header("Events")]
    public UnityEvent onQuizStarted;
    public UnityEvent onQuizCompleted;
    public UnityEvent onAllQuestionsAnswered;
    
    private int currentQuestionIndex = 0;
    private List<int> availableQuestions = new List<int>();
    private HashSet<int> answeredQuestions = new HashSet<int>();
    private bool isQuizActive = false;
    private Coroutine chestCoroutine;

    void Start()
    {
        if (!testMode && quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
        
        if (chestImage != null)
        {
            chestImage.gameObject.SetActive(false);
        }
        
        
        InitializeQuestionList();
        
        if (testMode)
        {
            StartQuiz();
        }
    }

    private void InitializeQuestionList()
    {
        availableQuestions.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            availableQuestions.Add(i);
        }
        
        if (randomizeQuestions)
        {
            ShuffleList(availableQuestions);
        }
    }

    /// <summary>
    /// Start the quiz from the beginning
    /// </summary>
    public void StartQuiz()
    {
        if (questions.Count == 0)
        {
            Debug.LogWarning("No questions available!");
            return;
        }
        
        if (!allowRetry)
        {
            answeredQuestions.Clear();
        }
        
        InitializeQuestionList();
        currentQuestionIndex = 0;
        isQuizActive = true;
        
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
        
        onQuizStarted?.Invoke();
        ShowCurrentQuestion();
    }

    /// <summary>
    /// Show a specific question by index
    /// </summary>
    public void ShowQuestion(int questionIndex)
    {
        if (questionIndex < 0 || questionIndex >= questions.Count)
        {
            Debug.LogWarning($"Question index {questionIndex} is out of range!");
            return;
        }
        
        if (!allowRetry && answeredQuestions.Contains(questionIndex))
        {
            Debug.Log($"Question {questionIndex} already answered!");
            return;
        }
        
        currentQuestionIndex = questionIndex;
        isQuizActive = true;
        
        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }
        
        ShowCurrentQuestion();
    }

    /// <summary>
    /// Show the next question in sequence
    /// </summary>
    public void ShowNextQuestion()
    {
        if (currentQuestionIndex < availableQuestions.Count - 1)
        {
            currentQuestionIndex++;
            ShowCurrentQuestion();
        }
        else
        {
            EndQuiz();
        }
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= availableQuestions.Count)
        {
            EndQuiz();
            return;
        }
        
        int questionIndex = availableQuestions[currentQuestionIndex];
        QuizQuestion question = questions[questionIndex];
        
        // Display question and choices
        if (questionText != null)
            questionText.text = question.question;
        
        if (choiceAText != null)
            choiceAText.text = question.choiceA;
        
        if (choiceBText != null)
            choiceBText.text = question.choiceB;
        
        if (choiceCText != null)
            choiceCText.text = question.choiceC;
    }

    /// <summary>
    /// Call this when player selects choice A
    /// </summary>
    public void SelectChoiceA()
    {
        SubmitAnswer(0);
    }

    /// <summary>
    /// Call this when player selects choice B
    /// </summary>
    public void SelectChoiceB()
    {
        SubmitAnswer(1);
    }

    /// <summary>
    /// Call this when player selects choice C
    /// </summary>
    public void SelectChoiceC()
    {
        SubmitAnswer(2);
    }

    private void SubmitAnswer(int selectedAnswerIndex)
    {
        if (!isQuizActive || currentQuestionIndex >= availableQuestions.Count)
            return;
        
        int questionIndex = availableQuestions[currentQuestionIndex];
        QuizQuestion question = questions[questionIndex];
        
        // Mark this question as answered
        answeredQuestions.Add(questionIndex);
        
        // Check if answer is correct
        if (selectedAnswerIndex == question.correctAnswerIndex)
        {
            OnCorrectAnswer(question);
        }
        else
        {
            OnWrongAnswer(question);
        }
    }

    private void OnCorrectAnswer(QuizQuestion question)
    {
        Debug.Log("Correct Answer!");
        
        // Add money reward
        if (playerMoney != null && question.moneyReward > 0)
        {
            playerMoney.AddMoney(question.moneyReward);
        }
        
        // Invoke events
        question.onCorrectAnswer?.Invoke();
        
        // Show full chest and proceed to next question
        if (chestCoroutine != null)
        {
            StopCoroutine(chestCoroutine);
        }
        chestCoroutine = StartCoroutine(ShowChestAndContinue(true));
    }

    private void OnWrongAnswer(QuizQuestion question)
    {
        Debug.Log("Wrong Answer!");
        
        // Apply money penalty
        if (playerMoney != null && question.moneyPenalty > 0)
        {
            playerMoney.RemoveMoney(question.moneyPenalty);
        }
        
        // Invoke events
        question.onWrongAnswer?.Invoke();
        
        // Show empty chest and proceed to next question
        if (chestCoroutine != null)
        {
            StopCoroutine(chestCoroutine);
        }
        chestCoroutine = StartCoroutine(ShowChestAndContinue(false));
    }
    
    private System.Collections.IEnumerator ShowChestAndContinue(bool isCorrect)
    {
        // Hide quiz panel
        if (questionsHolder != null)
        {
            questionsHolder.SetActive(false);
        }
        
        // Show appropriate chest
        if (isCorrect && chestImage != null)
        {
            chestImage.sprite = fullChestSprite;
        }
        else if (!isCorrect && chestImage != null)
        {
            chestImage.sprite = emptyChestSprite;
        }
        chestImage.gameObject.SetActive(true);
        // Wait for display duration
        yield return new WaitForSeconds(chestDisplayDuration);
        
        // Hide chest panels
        if (chestImage != null)
        {
            chestImage.gameObject.SetActive(false);
        }
        

        
        // Show quiz panel again and proceed to next question
        if (questionsHolder != null)
        {
            questionsHolder.SetActive(true);
        }
        
        ShowNextQuestion();
    }

    /// <summary>
    /// End the current quiz
    /// </summary>
    public void EndQuiz()
    {
        isQuizActive = false;
        
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
        
        onQuizCompleted?.Invoke();
        
        // Check if all questions have been answered
        if (answeredQuestions.Count >= questions.Count)
        {
            onAllQuestionsAnswered?.Invoke();
        }
    }

    /// <summary>
    /// Reset the quiz (clear answered questions)
    /// </summary>
    public void ResetQuiz()
    {
        answeredQuestions.Clear();
        InitializeQuestionList();
        currentQuestionIndex = 0;
    }

    /// <summary>
    /// Get the number of questions answered
    /// </summary>
    public int GetAnsweredCount()
    {
        return answeredQuestions.Count;
    }

    /// <summary>
    /// Get total number of questions
    /// </summary>
    public int GetTotalQuestions()
    {
        return questions.Count;
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}

// ===== EXAMPLE USAGE =====
/*
Setup:
1. Create a QuizManager GameObject
2. Assign UI elements (question text, choice texts, quiz panel)
3. Create a PlayerMoney component (see below)
4. Add questions in the inspector:
   - Question: "What is 2+2?"
   - Choices: "3", "4", "5"
   - Correct Answer Index: 1 (B)
   - Money Reward: 100

5. Connect button onClick events:
   - Button A -> QuizManager.SelectChoiceA()
   - Button B -> QuizManager.SelectChoiceB()
   - Button C -> QuizManager.SelectChoiceC()

6. Start quiz with: quizManager.StartQuiz();
*/