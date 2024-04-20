using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    
    [Header("NPC Script")]
    private NPC npc;

    private bool playerInRange;

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
        if (npc.playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            visualCue.SetActive(true);
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!DialogueManager.GetInstance().ifSetUpStory)
                {
                    // Use NPC's settings to setup the story
                    DialogueManager.GetInstance().SetUpStory(npc.inkJSONAsset, npc.npcTypingSpeed, npc.npcVoicePitch, npc.npcBeepFrequency);
                    DialogueManager.GetInstance().ifSetUpStory = true;
                }

                // Start the dialogue using NPC's method
                npc.TriggerDialogue();
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