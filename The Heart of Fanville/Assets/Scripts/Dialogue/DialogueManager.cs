using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;

    // Add a way to hold quest data that could come from the dialogue or an external source
    [Header("Quest Data")]
    [SerializeField] private string questTitle; // Set these in the Inspector or dynamically via code
    [SerializeField] private string questDescription;

    [Header("Quest UI")]
    [SerializeField] private TextMeshProUGUI questTitleText; // Reference to the title TextMeshProUGUI component
    [SerializeField] private TextMeshProUGUI questDescriptionText; // Reference to the description TextMeshProUGUI component
    [SerializeField] private GameObject questLogPanel; // Reference to the quest log panel

    [Header("Choices UI")]
    [SerializeField] private Button[] choiceButtons;
    private TextMeshProUGUI[] choicesText;

    [Header("Typing Effect")]
    [SerializeField] private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;

    [Header("Voice")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] voiceClips;
    [SerializeField] private float voicePitch = 1.0f;
    [SerializeField] private float beepFrequency = 1.0f;

    [Header("Player Control")]
    [SerializeField] private PlayerController playerController;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSONAsset; // Add this line

    [SerializeField] private bool stopAudioSource;

    public QuestManager currentQuest;


    public bool ifSetUpStory = false;  // To determine if the story is set up or not

    private const string SPEAKER_TAG = "speaker";
    private const string PORTRAIT_TAG = "portrait";
    private const string LAYOUT_TAG = "layout";

    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }

    private bool isTyping; // New field to track typing state

    private bool isWaitingForChoiceToBeMade = false;

    private static DialogueManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more than one Dialogue Manager in the scene");
            return;
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        if (instance == null)
        {
            Debug.LogError("DialogueManager instance is null!");
        }
        return instance;
    }


    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        SetUpStory(inkJSONAsset,typingSpeed, voicePitch,beepFrequency);

        if (choiceButtons == null)
        {
            Debug.LogError("Choice buttons array is not initialized.");
            return;
        }

        choicesText = new TextMeshProUGUI[choiceButtons.Length];

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int localIndex = i;
            choiceButtons[i].onClick.AddListener(() => MakeChoice(localIndex));
            choicesText[i] = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
        }

    }


    private void Update()
    {
        Debug.Log($"Dialogue playing: {dialogueIsPlaying}, Waiting for choice: {isWaitingForChoiceToBeMade}");
        if (!dialogueIsPlaying)
        {
            return;
        }

        // Use 'Space' key for continuing dialogue
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key pressed to continue dialogue");

            // Check if the dialogue is not waiting for a choice to be made
            if (!isWaitingForChoiceToBeMade)
            {
                if (isTyping)
                {
                    CompleteSentence();
                }
                else
                {
                    ContinueStory();
                }
            }
        }

        // Use 'Enter' key exclusively for making choices
        if (isWaitingForChoiceToBeMade && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Enter key pressed for choice");

            // Implement your choice selection logic here.
            // For example, select the first choice or a highlighted choice.
            MakeChoice(0); // This is an example; modify as needed for your choice logic.
        }
    }

    public Story GetInkStory()
    {
        if (currentStory == null)
        {
            Debug.LogError("currentStory is null. Ink story might not be initialized properly.");
        }
        return currentStory;
    }


    private void StartQuest(QuestManager quest)
    {
        if (quest == null)
        {
            Debug.LogError("Attempted to start a null quest.");
            return;
        }

        // Set the quest details and activate the quest
        quest.title = questTitle;
        quest.description = questDescription;
        quest.ActivateQuest(); // Activate the quest

        currentQuest = quest;

        // Update the quest UI
        UpdateQuestUIOnStart(quest);
    }


    private void UpdateQuestUIOnStart(QuestManager quest)
    {
        if (questTitleText != null && questDescriptionText != null && questLogPanel != null)
        {
            questTitleText.text = quest.title;
            questDescriptionText.text = quest.description;
            questLogPanel.SetActive(true); // Show the quest log panel
        }
        else
        {
            Debug.LogError("Quest UI components are not assigned!");
        }
    }

    private void UpdateQuestUIOnCompletion(QuestManager quest)
    {
        // Check if the quest log UI components are assigned
        if (questTitleText == null || questDescriptionText == null || questLogPanel == null)
        {
            Debug.LogError("Quest UI components are not assigned!");
            return;
        }

        // Optionally, update the text to indicate completion
        questTitleText.text = "Quest Completed: " + quest.title;
        questDescriptionText.text = "You have successfully completed the quest.";

        // If you want to hide the quest log panel after some time
        StartCoroutine(HideQuestLogAfterDelay());

        // Alternatively, you could simply deactivate the quest log panel immediately
        // questLogPanel.SetActive(false);
    }

    // Coroutine to hide the quest log panel after a delay
    private IEnumerator HideQuestLogAfterDelay()
    {
        // Wait for a few seconds before hiding the quest log panel
        yield return new WaitForSeconds(5.0f);
        questLogPanel.SetActive(false);
    }


    //Additional logic for during quest

    public void CompleteQuest()
    {
        if (currentQuest != null && !currentQuest.isCompleted)
        {
            currentQuest.CompleteQuest();
            Debug.Log("Quest Complete");
            UpdateQuestUIOnCompletion(currentQuest); // Add this line
            // Additional logic for when the quest is completed
        }
    }

    // Call this to refresh the dialogue choices after an update in the story's state
    public void RefreshDialogueUI()
    {
        Debug.Log("Refreshing dialogue UI");
        if (dialogueIsPlaying)
        {
            // Re-display the current content to update the choices
            //DisplayChoices();
            
            // This should force the dialogue UI to refresh and show new choices
            ContinueStory();
        }
    }

    public void SetUpStory(TextAsset inkJSON, float npcTypingSpeed, float npcVoicePitch, float npcBeepFrequency)
    {
        if (inkJSON == null)
        {
            Debug.LogError("Ink JSON asset is null.");
            return;
        }

        Debug.Log("Setting up Ink story");
        currentStory = new Story(inkJSON.text);

        // Set the typing speed and voice pitch from the parameters
        typingSpeed = npcTypingSpeed;
        audioSource.pitch = npcVoicePitch;

        beepFrequency = npcBeepFrequency;

    }

    public void StartDialogue(string KnotName)
    {
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);

        // reset portrait, layout, and speaker
        displayNameText.text = "???";

        // Freeze the player movement
        if (playerController != null)
        {
            playerController.SetCanMove(false);
        }

        currentStory.ChoosePathString(KnotName);

        ContinueStory();
    }

    private IEnumerator ExitDialogueMode()
    {
        Debug.Log("Exiting Dialogue Mode...");
        yield return new WaitForSeconds(0.2f);

        // Unfreeze the player movement
        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        Debug.Log("Dialogue Mode Exited");
    }


    private void ContinueStory()
    {
        // If the story can continue
        if (currentStory.canContinue)
        {
            // Stop any ongoing typing coroutine
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // Start typing out the next line of dialogue
            typingCoroutine = StartCoroutine(TypeSentence(currentStory.Continue()));

            // Display the choices if there are any
            DisplayChoices();
            //handle tags
            HandleTags(currentStory.currentTags);
        }
        // If the story can't continue but there are choices available
        else if (currentStory.currentChoices.Count > 0)
        {
            dialogueIsPlaying = true;
            DisplayChoices();
        }
        // If the story can't continue and there are no choices available, exit dialogue mode
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    // Call this method when the player makes a choice in the dialogue
    public void OnMakeChoice(Choice choice)
    {
        // Choose the choice index in the Ink story
        currentStory.ChooseChoiceIndex(choice.index);

        // Check for tags associated with this choice
        if (currentStory.currentTags.Contains("start_quest"))
        {
            StartQuest();
        }

        if (choice.tags != null && choice.tags.Count > 0)
        {
            foreach (string tag in choice.tags)
            {
                if (tag.Equals("start_quest"))
                {
                    StartQuest();
                }
                else if (tag.Equals("complete_quest"))
                {
                    CompleteQuest();
                }
            }
        }

        // Continue the story after making a choice
        ContinueStory();
    }

    private void StartQuest()
    {
        // Logic to start the quest, like updating a quest manager or dialogue manager
        if (currentQuest != null)
        {
            currentQuest.isActive = true;
            // Update quest UI and other related components
            questLogPanel.SetActive(true);

            // Assuming UpdateQuestUIOnStart is a method that takes a QuestManager as a parameter
            UpdateQuestUIOnStart(currentQuest); // 'currentQuest' instead of 'quest'
        }
        else
        {
            Debug.LogError("No current quest is set.");
        }
    }


    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            // Log the tag we're handling
            Debug.Log("Handling tag: " + tag);

            // If it's a simple tag without a colon, handle those cases first
            if (tag.Equals("start_quest"))
            {
                StartQuest(currentQuest);
                // Find the QuestManager component in the scene and start the quest
                QuestManager questManager = FindObjectOfType<QuestManager>();
                if (questManager != null)
                {
                    // Set the quest details from the dialogue manager to the quest manager
                    questManager.title = questTitle;
                    questManager.description = questDescription;
                    questManager.ActivateQuest();
                    currentQuest = questManager;
                }
                else
                {
                    Debug.LogError("QuestManager component not found in the scene.");
                }
                continue; // Skip the rest of the loop for this tag
            }
            else if (tag.Equals("complete_quest"))
            {
                // Complete the quest
                CompleteQuest();
                continue; // Skip the rest of the loop for this tag
            }

            // Handle tags with key-value pairs separated by a colon
            string[] splitTag = tag.Split(':');
            if (splitTag.Length == 2)
            {
                string tagKey = splitTag[0].Trim();
                string tagValue = splitTag[1].Trim();

                switch (tagKey)
                {
                    case SPEAKER_TAG:
                        displayNameText.text = tagValue;
                        break;
                    default:
                        Debug.LogWarning("Unhandled tag: " + tag);
                        break;
                }
            }
            else
            {
                // If the tag isn't a known single tag or a key-value pair, log an error
                Debug.LogError("Tag format not recognized: " + tag);
            }
        }
    }



    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";
        float nextBeepTime = 0f; // Time when next beep should play

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            if (Time.time >= nextBeepTime)
            {
                if (voiceClips != null && voiceClips.Length > 0 && audioSource != null && !audioSource.isPlaying)
                {
                    AudioClip randomBeep = voiceClips[Random.Range(0, voiceClips.Length)];
                    audioSource.PlayOneShot(randomBeep);
                    nextBeepTime = Time.time + beepFrequency;
                }
            }
            yield return new WaitForSeconds(typingSpeed);
        }
        isTyping = false;
    }




    private void CompleteSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        dialogueText.text = currentStory.currentText; // Display full text immediately
        isTyping = false; // Update typing status
    }
    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // Check if there are any choices to display.
        isWaitingForChoiceToBeMade = currentChoices.Count > 0;

        if (isWaitingForChoiceToBeMade)
        {
            Debug.Log("Choices available: " + currentChoices.Count);

            // If there are choices, enable the choice UI and set the text for each one.
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (i < currentChoices.Count)
                {
                    choiceButtons[i].gameObject.SetActive(true); // Activate the button gameobject
                    choicesText[i].text = currentChoices[i].text;
                }
                else
                {
                    // Disable any choice UI that is not needed.
                    choiceButtons[i].gameObject.SetActive(false); // Deactivate the button gameobject
                }
            }

            // Automatically select the first choice for better UX.
            StartCoroutine(SelectFirstChoice());
        }
        else
        {
            // If no choices are present, make sure they are all disabled.
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                choiceButtons[i].gameObject.SetActive(false); // Deactivate the button gameobject
            }
        }
    }


    private IEnumerator SelectFirstChoice()
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choiceButtons[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (currentStory == null)
        {
            Debug.LogError("currentStory is null.");
            return;
        }

        if (currentStory.currentChoices == null)
        {
            Debug.LogError("currentChoices is null.");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentStory.currentChoices.Count)
        {
            Debug.LogError($"choiceIndex {choiceIndex} is out of bounds.");
            return;
        }

        Debug.Log("Choice made: " + choiceIndex);

        Choice choice = currentStory.currentChoices[choiceIndex];
        if (choice == null)
        {
            Debug.LogError($"Choice at index {choiceIndex} is null.");
            return;
        }

        bool shouldContinue = true;

        if (choice.tags != null && choice.tags.Count > 0)
        {
            foreach (string tag in choice.tags)
            {
                if (tag.Equals("start_quest"))
                {
                    // Assuming there's only one QuestManager in the scene attached to a GameObject.
                    QuestManager questManager = FindObjectOfType<QuestManager>();
                    if (questManager != null)
                    {
                        questManager.title = questTitle; // Set the title of the quest.
                        questManager.description = questDescription; // Set the description of the quest.
                        questManager.ActivateQuest();
                        currentQuest = questManager;

                    }
                    else
                    {
                        Debug.LogError("QuestManager not found in the scene.");
                    }
                }
            }
        }

        // Choose the choice index in the Ink story
        currentStory.ChooseChoiceIndex(choiceIndex);

        if (shouldContinue)
        {
            ContinueStory();
        }
    }


}