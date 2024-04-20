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
    [SerializeField] private AudioSource audioSource;

    [Header("Choices UI")]
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Typing Effect")]
    private Coroutine typingCoroutine;
    private float typingSpeed;

    [Header("Player Controls")]
    [SerializeField] private PlayerController playerController;

    [Header("Quest Management")]
    [SerializeField] private QuestManager questManager;

    public bool ifSetUpStory = false;
    
    private const string SPEAKER_TAG = "speaker";

    private Story currentStory;
    private NPC currentNPC;
    public bool dialogueIsPlaying { get; private set; }
    private bool isTyping;
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

        if (choiceButtons == null)
        {
            Debug.LogError("Choice buttons array is not initialized.");
            return;
        }

        choicesText = new TextMeshProUGUI[choices.Length];
        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
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

        if (isWaitingForChoiceToBeMade && Input.GetKeyDown(KeyCode.Return))
        {
            ContinueStory();
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

    public void RefreshDialogueUI()
    {
        if (currentStory.canContinue)
        {
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

        currentStory = new Story(inkJSON.text);
        dialogueText.maxVisibleCharacters = 0;
        audioSource.pitch = npcVoicePitch;
    }

    public void StartDialogue(NPC npc)
    {
        SetUpStory(npc.inkJSONAsset, npc.npcTypingSpeed, npc.npcVoicePitch, npc.npcBeepFrequency);
        dialoguePanel.SetActive(true);
        dialogueIsPlaying = true;
        RefreshDialogueUI();
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        if (playerController != null)
        {
            playerController.SetCanMove(true);
        }

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeSentence(currentStory.Continue()));
            DisplayChoices();
            HandleTags(currentStory.currentTags);
        }
        else if (currentStory.currentChoices.Count > 0)
        {
            dialogueIsPlaying = true;
            DisplayChoices();
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }
    }

    public void OnMakeChoice(Choice choice)
    {
        currentStory.ChooseChoiceIndex(choice.index);

        if (currentStory.currentTags.Contains("start_quest"))
        {
            QuestManager.GetInstance().ActivateQuest();
        }

        if (choice.tags != null && choice.tags.Count > 0)
        {
            foreach (string tag in choice.tags)
            {
                if (tag.Equals("start_quest"))
                {
                    QuestManager.GetInstance().ActivateQuest();
                }
                else if (tag.Equals("complete_quest"))
                {
                    QuestManager.GetInstance().CompleteQuest();
                }
            }
        }

        ContinueStory();
    }


    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            Debug.Log("Handling tag: " + tag);

            string[] parts = tag.Split(':');
            if (parts.Length == 2)
            {
                string command = parts[0].Trim();
                string value = parts[1].Trim();

                switch (command)
                {
                    case "start_quest":
                        StartQuest(value);
                        break;
                    case "complete_quest":
                        CompleteQuest(value);
                        break;
                    case SPEAKER_TAG:
                        displayNameText.text = value;
                        break;
                    default:
                        Debug.LogWarning($"Unhandled tag: {tag}");
                        break;
                }
            }
            else
            {
                Debug.LogError($"Tag format not recognized: {tag}");
            }
        }
    }

    private void StartQuest(string questTitle)
    {
        if (questManager != null && questManager.currentQuest != null && questManager.currentQuest.title == questTitle)
        {
            questManager.ActivateQuest();
        }
        else
        {
            Debug.LogError("QuestManager component not assigned in DialogueManager.");
        }
    }

    private void CompleteQuest(string questTitle)
    {
        if (questManager != null && questManager.currentQuest != null && questManager.currentQuest.title == questTitle)
        {
            questManager.CompleteQuest();
        }
        else
        {
            Debug.LogError("No such quest active or QuestManager is null.");
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(1.0f / typingSpeed); // Typing speed affects delay
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // defensive check to make sure our UI can support the number of choices coming in
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given than the UI can support. Number of choices given: "
                + currentChoices.Count);
        }

        int index = 0;
        // enable and initialize the choices up to the amount of choices for this line of dialogue
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }
        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());
    }

    private IEnumerator SelectFirstChoice()
    {
        // Event System requires we clear it first, then wait
        // for at least one frame before we set the current selected object.
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
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

}