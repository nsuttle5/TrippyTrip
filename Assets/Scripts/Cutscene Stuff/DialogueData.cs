using System;
using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Cutscenes/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public int startNodeId = 0;
    public DialogueNode[] nodes;

    public DialogueNode GetNode(int id)
    {
        foreach (var n in nodes)
        {
            if (n.id == id) return n;
        }
        return null;
    }
}

[Serializable]
public class DialogueNode
{
    public int id;
    public string speakerName;

    [TextArea(2, 5)]
    public string text;

    public AudioClip voiceClip;

    [Tooltip("Leave empty for a linear line that auto-advances via autoAdvanceNextId.")]
    public DialogueResponse[] responses;

    [Tooltip("Only used when responses is empty. -1 ends the dialogue.")]
    public int autoAdvanceNextId = -1;
}

[Serializable]
public class DialogueResponse
{
    [TextArea(1, 3)]
    public string responseText;

    [Tooltip("-1 ends the dialogue.")]
    public int nextNodeId;
}
