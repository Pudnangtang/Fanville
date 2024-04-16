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
        if (playerInRange && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            visualCue.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!DialogueManager.GetInstance().ifSetUpStory)
                {
                    DialogueManager.GetInstance().SetUpStory(inkJSON, npcTypingSpeed, npcVoicePitch, npcBeepFrequency);
                    DialogueManager.GetInstance().ifSetUpStory = true;
                }

                // Start the dialogue
                NPC.GetInstance().NPC(KnotName);


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