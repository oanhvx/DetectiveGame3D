using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/DialogueData")]
public class Dialogue : ScriptableObject
{
    public string characterName;
    public Sprite portrait;

    public bool isMonologue; // check độc thoại

    [TextArea(3, 10)]
    public string[] sentences;

    public Dialogue nextDialogue;
}
