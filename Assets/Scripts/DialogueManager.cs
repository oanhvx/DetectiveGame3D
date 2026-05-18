using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI pressKey;
    public bool isDialogueActive = false;
    public static DialogueManager instance;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public GameObject dialoguePanel;
    public TextMeshProUGUI showSuggest;
    public AudioSource typingAudioSource;
    public AudioClip typingClip;
    private Dialogue activateDialogue;
    private Queue<string> sentences;
    private Animator animator;

    private void Awake()
    {
        instance = this;
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue, Animator ani)
    {
        this.animator = ani; // Bắt buộc phải gán biến animator để sử dụng ở các hàm khác
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
        //pressKey.text = "press Space to continuous";
        if (dialogue.isMonologue)
        {
            nameText.text = "";
            portraitImage.gameObject.SetActive(false);
            dialogueText.color = Color.cyan;
            dialogueText.fontStyle = FontStyles.Italic;
        }
        else
        {
            nameText.text = dialogue.characterName;
            portraitImage.sprite = dialogue.portrait;
            dialogueText.color = Color.white;
            //dialogueText.fontStyle = FontStyles.Italic;
        }
        activateDialogue = dialogue;
        isDialogueActive = true;
        dialoguePanel.SetActive(true);

        sentences.Clear();
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentences();
    }

    public void DisplayNextSentences()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        // 1. Bật âm thanh và animation hội thoại khi bắt đầu chạy chữ
        if (typingAudioSource != null && typingClip != null)
        {
            typingAudioSource.clip = typingClip;
            typingAudioSource.loop = true;
            typingAudioSource.volume = 0.7f;
            if (!typingAudioSource.isPlaying) typingAudioSource.Play();
        }

        if (animator != null) animator.SetBool("IsTalking", true);

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        // 2. Tắt âm thanh và animation khi dòng chữ đã chạy xong (để NPC ngậm miệng lại khi hết câu)
        if (typingAudioSource != null) typingAudioSource.Stop();
        if (animator != null) animator.SetBool("IsTalking", false);
    }

    void EndDialogue()
    {
        Dialogue cursorDialogue = activateDialogue;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        
        // Đảm bảo tắt âm thanh và animation khi kết thúc hội thoại (ví dụ: người chơi bấm skip)
        if (typingAudioSource != null) typingAudioSource.Stop();
        if (animator != null) animator.SetBool("IsTalking", false);
        
        isDialogueActive = false;
        dialoguePanel.SetActive(false);
        if (cursorDialogue != null && cursorDialogue.nextDialogue != null)
        {
            StartDialogue(cursorDialogue.nextDialogue, animator);//chay dialogue trong dialogue ban đầu đảm bảo hội thoại giữa nhân vật và npc so le
        }
        else
        {
            animator = null;
        }
        //pressKey.text = "press E to talk";
    }

    private void Update()
    {
        if(isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            DisplayNextSentences();
        }
    }

    //public void ShowSuggest(string message)
    //{
    //    showSuggest.text = message;
    //    showSuggest.gameObject.SetActive(true);
    //    Invoke("HideSuggest", 5f);
    //}

    //private void HideSuggest()
    //{
    //    showSuggest.gameObject.SetActive(false);
    //}
}
