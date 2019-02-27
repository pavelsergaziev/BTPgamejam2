using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Element", fileName = "New Dialogue")]
public class DialogueElementSO : ScriptableObject
{

    [SerializeField]
    private SpeakerAndLine[] _lines;
        
    public SpeakerAndLine GetDialogueSpeakerAndLineByIndex(int index)
    {
        return _lines[index];
    }

    public int GetDialogueLength()
    {
        return _lines.Length;
    }
}
