using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Evidence", menuName = "Evidence/EvidenceData")]
public class EvidenceData:ScriptableObject
{
    public string evidenceID;
    public string nameEvidence;
    public string title;
    public string description;
    public Sprite icon;
}