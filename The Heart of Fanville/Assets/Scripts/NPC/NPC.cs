using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] public DialogueManager dialogueManager;

    [Header("NPC Voice Settings")]
    [SerializeField] public float npcTypingSpeed = 0.05f; 
    [SerializeField] public float npcVoicePitch = 1.0f; 
    [SerializeField] public float npcBeepFrequency = 1.0f; 
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip[] voiceClips;

    [Header("Ink JSON")]
    [SerializeField] public TextAsset inkJSONAsset;

    [Header("NPC Knot Start")]
    [SerializeField] public string KnotName;

    public bool playerInRange = false;  // Track if player is in range

    // This function is called by the DialogueTrigger
    public void TriggerDialogue()
    {
        DialogueManager.GetInstance().StartDialogue(this);
    }

}
