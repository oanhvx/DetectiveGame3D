using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    public static EvidenceManager Instance;
    public List<EvidenceData> collectedEvidence = new List<EvidenceData>();
    //public TextMeshProUGUI messageAfterGetEvidence;

    void Awake()
    {
        Instance = this;
    }

    public bool HasEvidence(string targetID)
    {
        foreach (EvidenceData evidence in collectedEvidence)
        {
            if (evidence.evidenceID.Equals(targetID, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public void AddEvidence(EvidenceData data)
    {
        collectedEvidence.Add(data);
        Debug.Log("[EvidenceManager] Added: " + data.title + " | Total: " + collectedEvidence.Count);
    }
}
