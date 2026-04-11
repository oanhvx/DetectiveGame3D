using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interacDistance = 3f;
    public Transform eyeOfcamera;

    void Update()
    {
        CheckInteractable();
    }
    void CheckInteractable()
    {
        if (GameManager.instance.isGameOver) return;
        Ray ray = new Ray(eyeOfcamera.transform.position, eyeOfcamera.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interacDistance))
        {
            Debug.Log("looking at " + hit.collider.name);
            EvidenceItem evidence = hit.collider.GetComponent<EvidenceItem>();
            NPCInteraction npc = hit.collider.GetComponent<NPCInteraction>();
            AccuseTrigger trigger = hit.collider.GetComponent<AccuseTrigger>();
            if(trigger != null && GameManager.instance.cluesFound >= 2)
            {               
                DialogueManager.instance.pressKey.text = "press M to open menu accuse";
                GameManager.instance.UI.Show();
                if (GameManager.instance.accusePanel.activeSelf) GameManager.instance.UI.Hide();
                //Debug.Log("find evidence : "+ evidence.data.title);
                if (Input.GetKeyDown(KeyCode.M))
                {
                    //ui.Hide();
                    trigger.OpenAccuseMenu();
                }
                return;
               
            }
            if (npc != null)
            {
                DialogueManager.instance.pressKey.text = "press E to talk";
                GameManager.instance.UI.Show();
                if (DialogueManager.instance.isDialogueActive) 
                {
                    DialogueManager.instance.pressKey.text = "press Space to continuous";
                }
                //Debug.Log("find evidence : "+ evidence.data.title);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    npc.interact();
                }
                    return;
            }
            if (evidence != null)
            {
                if (!evidence.FIRTLOOK())
                {
                    evidence.BeforeGet();
                }
                if (evidence.ISPROCESSING()) return;
                DialogueManager.instance.pressKey.text = "press E to pick";
                GameManager.instance.UI.Show();
                if (Input.GetKeyDown(KeyCode.E))
                {
                    CollectEvidence(evidence);
                    evidence.OnInteraction();
                }
                return;
            }
        }
        GameManager.instance.UI.Hide();
    }
    void CollectEvidence(EvidenceItem evidence)
    {
        EvidenceManager.Instance.AddEvidence(evidence.data);
        //Debug.Log("collected " + evidence.data.title);
        //Destroy(evidence.gameObject);
        evidence.GetComponent<MeshRenderer>().enabled = false;
        evidence.GetComponent<Collider>().enabled = false;
    }
}
