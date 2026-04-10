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
    
    public bool ISPROCESSING()
    {
        return isProcessing;
    }
    public void OnInteraction(EvidenceItem Item)
    {
        StartCoroutine(MonologueAndPick(Item));
    }

    IEnumerator MonologueAndPick(EvidenceItem item)
    {
        isProcessing = true;
        Debug.Log("isProcessing : "+ isProcessing);
        DialogueManager.instance.StartDialogue(MonologueBeforeGetEvidence);
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        GameManager.instance.UI.Show();
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return null;
        }
        DialogueManager.instance.pressKey.text = "press E to pick";
        bool isPressedE = false;
        while(!isPressedE)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isPressedE = true;
            }
            yield return null;
        }

        EvidenceManager.Instance.AddEvidence(item.data);
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        DialogueManager.instance.StartDialogue(MonologueAfterGetEvidence);
        DialogueManager.instance.pressKey.text = "press Space to continuous";
        while (DialogueManager.instance.isDialogueActive)
        {
            yield return null;
        }
        GameManager.instance.UI.Hide();
        isProcessing = false;
        Debug.Log("isProcessing : " + isProcessing);
    }
}
