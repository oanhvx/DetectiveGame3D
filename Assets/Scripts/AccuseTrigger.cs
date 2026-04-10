using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccuseTrigger : MonoBehaviour
{
    public void OpenAccuseMenu()
    {
        GameManager.instance.accusePanel.SetActive(true);
        GameManager.instance.isGameOver = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
