using UnityEngine;

public class CarArrivalTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Car"))
        {
            triggered = true;
            CutsceneManager.Instance.PlayIntro();
        }
    }
}
