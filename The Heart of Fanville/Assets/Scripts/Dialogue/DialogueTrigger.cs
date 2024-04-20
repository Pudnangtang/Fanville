using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;
    
    [Header("NPC Script")]
    [SerializeField] private NPC npc;

    private bool playerInRange;

    private void Awake()
    {
        playerInRange = false;
        visualCue.SetActive(false);
        npc = GetComponent<NPC>();  // If NPC script is on the same GameObject
        if (npc == null)
        {
            npc = GetComponentInChildren<NPC>();  // If NPC script is on a child GameObject
        }
        if (npc == null)
        {
            Debug.LogError("NPC component not found on the GameObject or its children");
        }
    }

        private void Update()
    {
        HandlePlayerInput();
    }


    private void HandlePlayerInput()
    {
        if (playerInRange)
        {
            visualCue.SetActive(true);

            DialogueManager dialogueManager = DialogueManager.GetInstance();
            if (dialogueManager == null)
            {
                Debug.LogError("DialogueManager instance is null");
                return; // Stop further processing since DialogueManager is required.
            }

            if (!dialogueManager.dialogueIsPlaying && Input.GetKeyDown(KeyCode.E))
            {
                if (!dialogueManager.ifSetUpStory)
                {
                    // Here, check if npc or any of its properties are null
                    if (npc == null)
                    {
                        Debug.LogError("NPC reference is null");
                        return; // Stop further processing since NPC is required.
                    }
                    if (npc.inkJSONAsset == null)
                    {
                        Debug.LogError("NPC Ink JSON Asset is not assigned");
                        return; // Stop further processing since Ink JSON Asset is required.
                    }

                    dialogueManager.SetUpStory(npc.inkJSONAsset, npc.npcTypingSpeed, npc.npcVoicePitch, npc.npcBeepFrequency);
                    dialogueManager.ifSetUpStory = true;
                }

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
            //visualCue.SetActive(true);
            Debug.Log("Player entered NPC range.");
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            //visualCue.SetActive(false);
            Debug.Log("Player exited NPC range.");
        }
    }



}