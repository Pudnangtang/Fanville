using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private string KnotName;

    [Header("NPC Voice Settings")]
    [SerializeField] private float npcTypingSpeed = 0.05f; // Default typing speed for the NPC
    [SerializeField] private float npcVoicePitch = 1.0f;   // Default voice pitch for the NPC
    [SerializeField] private float npcBeepFrequency = 1.0f;   // Default beep frequency for the NPC

    [Header("Quest")]
    [SerializeField] public string questTitle;
    [SerializeField] public string questDescription;

    private bool playerInRange;

    public QuestManager questToStart;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
    }

    private void Update()
    {
        HandlePlayerInput();
    }


    private void HandlePlayerInput()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            visualCue.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                // If the story hasn't been set up, set it up and start the dialogue
                if (!DialogueManager.GetInstance().ifSetUpStory)
                {
                    DialogueManager.GetInstance().SetUpStory(inkJSON, npcTypingSpeed, npcVoicePitch, npcBeepFrequency);
                    DialogueManager.GetInstance().ifSetUpStory = true;
                }

                // Start the dialogue
                DialogueManager.GetInstance().StartDialogue(KnotName);


                // Do not activate the quest here; it should be activated within the dialogue at the right momement 
            }
        }
        else
        {
            visualCue.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }



}