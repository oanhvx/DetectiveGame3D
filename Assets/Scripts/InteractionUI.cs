using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public GameObject prompt;

    public void Show()
    {
        prompt.SetActive(true);
    }

    public void Hide()
    {
        prompt.SetActive(false);
    }
}
