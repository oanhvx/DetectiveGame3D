using UnityEngine;
using TMPro;

public class EvidenceDetailUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    //public GameObject emptyState; // (Tuỳ chọn) object hiển thị "Chọn 1 bằng chứng" khi chưa chọn gì

    void OnEnable()
    {
        // Hiện empty state khi mở notebook, chưa chọn gì
        //if (emptyState != null)
        //    emptyState.SetActive(true);
        if (titleText != null) titleText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    public void ShowDetail(EvidenceData data)
    {
        //if (emptyState != null)
        //    emptyState.SetActive(false);

        titleText.text = "Title: " + data.title;
        descriptionText.text = "Decription: " + data.description;
        Debug.Log("[EvidenceDetailUI] Showing: " + data.title);
    }
}
