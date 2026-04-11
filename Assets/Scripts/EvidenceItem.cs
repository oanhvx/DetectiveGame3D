using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Evidence", menuName = "Evidence/EvidenceData")]
public class EvidenceData:ScriptableObject
{
    public string evidenceID;
    public string nameEvidence;
    public string title;
    public string description;
    public Sprite icon;
}

public class EvidenceItem : MonoBehaviour
{
    //public static EvidenceItem Instance;
    public EvidenceData data;
    public Dialogue MonologueBeforeGetEvidence;
    public Dialogue MonologueAfterGetEvidence;
    
    private bool isProcessing = false;
    private bool firtLook = false;
    
    public bool FIRTLOOK()
    {
        return firtLook;
    }
    public bool ISPROCESSING()
    {
        return isProcessing;
    }
    public void OnInteraction()
    {
        StartCoroutine(MonologueAndPick());
    }
    public void BeforeGet()
    {
        StartCoroutine(BeforeGetEvidence());
    }
    IEnumerator BeforeGetEvidence()
    {
        firtLook = true;
        isProcessing = true;
        Debug.Log("isProcessing : " + isProcessing);
        DialogueManager.instance.StartDialogue(MonologueBeforeGetEvidence);
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        GameManager.instance.UI.Show();
        while (DialogueManager.instance.isDialogueActive) yield return null;
        GameManager.instance.UI.Hide();
        isProcessing = false;
    }
    IEnumerator MonologueAndPick()
    {
        isProcessing = true;
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        DialogueManager.instance.StartDialogue(MonologueAfterGetEvidence);
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return null;
        }
        GameManager.instance.UI.Hide();
        isProcessing = false;
        Debug.Log("isProcessing : " + isProcessing);
    }
}
