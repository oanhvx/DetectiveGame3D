using UnityEngine;

public class NotebookUI : MonoBehaviour
{
    public GameObject panel;
    //public EvidenceListUI evidenceListUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool newState = !panel.activeSelf;
            panel.SetActive(newState);
            if(newState)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
