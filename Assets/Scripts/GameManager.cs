using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject resultPanel;
    public GameObject accusePanel;
    public int cluesFound = 0;
    public bool isGameOver = false;
    public TextMeshProUGUI resultMessage;
    public InteractionUI UI;

    private void Awake()
    {
        instance = this;
        //isGameOver = false;
    }

    public void MakeAccusation(string nameSuspect)
    {
        //if (isGameOver) return;
        accusePanel.SetActive(false);
        isGameOver = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (nameSuspect == "BodyGuard")
        {
            ShowWin();
        }
        else
        {
            ShowLoss(nameSuspect);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddClues()
    {
        cluesFound++;
    }

    public void CancelAccuse()
    {
        isGameOver = false ;
        accusePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowWin()
    {
        resultPanel.SetActive(true);
        resultMessage.text = "CHÍNH XÁC! Kael mỉm cười tháo lớp mặt nạ vệ sĩ: 'Ngươi rất khá, thám tử. Ta chính là Aris thật.'";
    }

    public void ShowLoss(string nameSuspect)
    {
        resultPanel.SetActive(true);
        if (nameSuspect == "Maya")
        {
            resultMessage.text = "SAI RỒI! Maya vô tội. Sự vội vàng của bạn đã để Tiến sĩ thật thất vọng rời đi.";
        }
        else
        {
            resultMessage.text = "SAI RỒI! Leo chỉ là kẻ trộm vặt. Hung thủ thật sự (Aris giả dạng) đã trốn thoát thành công.";
        }
    }
}
