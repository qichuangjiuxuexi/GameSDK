using UnityEngine;

public class DialogueData
{
    public string DialoguePath { private set; get; }
    public Vector3 DialoguePosition { set; get; }


    public DialogueData(string path, Vector3 dialoguePosition)
    {
        DialoguePath = path;
        DialoguePosition = dialoguePosition;
    }
}