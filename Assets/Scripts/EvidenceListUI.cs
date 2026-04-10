using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvidenceListUI : MonoBehaviour
{
    public Transform content;        // ScrollView > Viewport > Content
    public GameObject itemPrefab;    // Prefab có Button + TextMeshProUGUI (xem hướng dẫn)
    public EvidenceDetailUI detailUI;

    //gọi sau mỗi lần được bật
    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (EvidenceManager.Instance == null)
        {
            Debug.LogWarning("[EvidenceListUI] EvidenceManager chưa sẵn sàng!");
            return;
        }

        var list = EvidenceManager.Instance.collectedEvidence;
        Debug.Log("[EvidenceListUI] Refresh — evidence count: " + list.Count);

        // Xóa các item cũ
        Debug.Log("content == "+content);
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Tạo item mới cho từng evidence
        foreach (var ev in list)
        {
            //clone prefab => child của content
            GameObject obj = Instantiate(itemPrefab, content);

            // Lấy TMP text ở bất kỳ level nào trong prefab
            TextMeshProUGUI label = obj.GetComponentInChildren<TextMeshProUGUI>();
            Image iconn = obj.GetComponent<Image>();
            if (label != null && iconn != null)
            {
                label.text = ev.nameEvidence;
                iconn.sprite = ev.icon;
            }
            else
                Debug.LogWarning("[EvidenceListUI] itemPrefab thiếu TextMeshProUGUI!");

            // Gán sự kiện click cho Button
            Button btn = obj.GetComponentInChildren<Button>();
            if (btn != null)
            {
                EvidenceData capturedEv = ev; // capture để tránh closure bug
                //khi click vào button thì gọi hàm ShowDetail của EvidenceDetailUI với lamda 
                btn.onClick.AddListener(() => detailUI.ShowDetail(capturedEv));
            }
            else
            {
                //add evidenceItemUI vào obj thay cho button để tự gọi hàm OnPointerClick() 
                EvidenceItemUI itemUI = obj.GetComponent<EvidenceItemUI>();
                if (itemUI == null)
                    itemUI = obj.AddComponent<EvidenceItemUI>();
                itemUI.Setup(ev, detailUI);
            }
        }
    }
}
