using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using static UnityEditor.Progress;

public class EvidenceItem : MonoBehaviour
{
    //public static EvidenceItem Instance;
    public EvidenceData data;
    public Dialogue MonologueBeforeGetEvidence;
    public Dialogue MonologueAfterGetEvidence;
    
    private bool isprocessing = false;
    private bool isfirtLook = false;
    
    public bool IsFirtLook
    {
        set { isfirtLook = value; }
        get { return isfirtLook; }
    }
    
    public bool IsProcessing 
    {
        set { isprocessing = value; }
        get { return isprocessing; }
    }
    public void OnInteraction(Animator animator)
    {
        StartCoroutine(MonologueAndPick(animator));
    }
    public void BeforeGet(Animator animator)
    {
        StartCoroutine(BeforeGetEvidence(animator));
    }
    IEnumerator BeforeGetEvidence(Animator ani)
    {
        IsFirtLook = true;
        IsProcessing = true;
        Debug.Log("isProcessing : " + IsProcessing);
        DialogueManager.instance.StartDialogue(MonologueBeforeGetEvidence, ani);
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        GameManager.instance.UI.Show();
        while (DialogueManager.instance.isDialogueActive) yield return null;
        GameManager.instance.UI.Hide();
        IsProcessing = false;
    }
    IEnumerator MonologueAndPick(Animator ani)
    {
        IsProcessing = true;
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        DialogueManager.instance.StartDialogue(MonologueAfterGetEvidence, ani);
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return null;
        }
        IsProcessing = false;
        GetComponent<Collider>().enabled = false;
        Debug.Log("isProcessing : " + IsProcessing);
    }
}
