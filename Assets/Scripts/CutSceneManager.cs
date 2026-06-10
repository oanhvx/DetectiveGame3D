using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("Timeline")]
    public PlayableDirector director;
    public PlayableAsset timeline_Intro;

    [Header("Tắt Input Player")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;

    void Awake() { Instance = this; }

    public void PlayIntro()
    {
        playerMovement.enabled = false;
        mouseLook.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        director.playableAsset = timeline_Intro;
        director.stopped += OnIntroEnd;
        director.Play();
    }

    void OnIntroEnd(PlayableDirector d)
    {
        director.stopped -= OnIntroEnd;
        playerMovement.enabled = true;
        mouseLook.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Hàm này sẽ được Signal Track gọi để trigger dialogue
    public void TriggerIntroDialogue()
    {
        DialogueManager.instance.StartDialogue(introDialogue, npcAnimator);
    }

    [Header("Dialogue Cảnh Intro")]
    public Dialogue introDialogue;
    public Animator npcAnimator;
}
