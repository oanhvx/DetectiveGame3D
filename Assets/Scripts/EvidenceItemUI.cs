using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EvidenceItemUI : MonoBehaviour, IPointerClickHandler
{
    EvidenceData data;
    EvidenceDetailUI detailUI;

    public void Setup(EvidenceData data, EvidenceDetailUI detailUI)
    {
        this.data = data;
        this.detailUI = detailUI;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        detailUI.ShowDetail(data);
    }
}
