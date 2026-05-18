using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("DefaultDialogue")]
    public Dialogue defaultDialogue;

    [Header("DialogueWithEvidence")]
    public string requiredEvidenceID;
    public Dialogue evidenceDialogue;
    //public string message;

    private bool clusGranted;
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void interact()
    {
        if (DialogueManager.instance.isDialogueActive) return;
        if (!string.IsNullOrEmpty(requiredEvidenceID) && EvidenceManager.Instance.HasEvidence(requiredEvidenceID))
        {
            DialogueManager.instance.StartDialogue(evidenceDialogue, animator);
            //DialogueManager.instance.ShowSuggest(message);
            if (!clusGranted)
            {
                GameManager.instance.AddClues();
                clusGranted = true;
            }
        }
        else
        {
            DialogueManager.instance.StartDialogue(defaultDialogue, animator);
        }
    }
}
