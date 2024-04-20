using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestManager : MonoBehaviour
{
    [SerializeField] public DialogueManager dialogueManager;

    [Header("Quest Data")]
    public string title;
    public string description;

    [Header("Quest State")]
    public bool isActive;
    public bool isCompleted;

    [Header("Quest Log UI")]
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;

    // Event delegates for quest start and completion
    public delegate void QuestAction(QuestManager quest);
    public event QuestAction OnQuestStarted;
    public event QuestAction OnQuestCompleted;
    public QuestManager currentQuest;

    private static QuestManager instance;

    public static QuestManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple instances of QuestManager found!");
            return;
        }
        instance = this;
    }

    void Start()
    {
        questLogPanel.SetActive(false);
    }

    // Call this method to start the quest
    public void ActivateQuest()
    {
        Debug.Log($"Activating quest: {title}");

        isActive = true;
        isCompleted = false;

        if (questTitleText != null && questDescriptionText != null)
        {
            questTitleText.text = title;
            questDescriptionText.text = description;
            questLogPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Quest UI components are not assigned!");
        }

        OnQuestStarted?.Invoke(this);
    }

    private void UpdateQuestUIOnCompletion(QuestManager quest)
    {
        if (questTitleText == null || questDescriptionText == null || questLogPanel == null)
        {
            Debug.LogError("Quest UI components are not assigned!");
            return;
        }

        // Optionally, update the text to indicate completion
        questTitleText.text = "Quest Completed: " + quest.title;
        questDescriptionText.text = "You have successfully completed the quest.";

        //hide the quest log panel after some time
        StartCoroutine(HideQuestLogAfterDelay());
    }

    private IEnumerator HideQuestLogAfterDelay()
    {
        // Wait for a few seconds before hiding the quest log panel
        yield return new WaitForSeconds(5.0f);
        questLogPanel.SetActive(false);
    }

    // Call this method when the player completes the quest
    public void CompleteQuest()
    {
        if (currentQuest != null && !currentQuest.isCompleted)
        {
            currentQuest.CompleteQuest();
            Debug.Log("Quest Complete");
            UpdateQuestUIOnCompletion(currentQuest);
        }
    }
}
