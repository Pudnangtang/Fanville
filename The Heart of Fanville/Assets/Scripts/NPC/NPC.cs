using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] public DialogueManager dialogueManager;

    [Header("NPC Voice Settings")]
    [SerializeField] public float npcTypingSpeed = 0.05f; // Default typing speed for the NPC
    [SerializeField] public float npcVoicePitch = 1.0f;   // Default voice pitch for the NPC
    [SerializeField] public float npcBeepFrequency = 1.0f;   // Default beep frequency for the NPC
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip[] voiceClips;

    [Header("Ink JSON")]
    [SerializeField] public TextAsset inkJSONAsset;

    [Header("NPC Knot Start")]
    [SerializeField] public string KnotName;
}
