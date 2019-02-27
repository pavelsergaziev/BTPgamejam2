using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class DialogueOutputter
{
    private DialogueSO _dialogue;
    private int _index;
    private SpeakerAndLine _speakerAndLine;

    private Text _characterTextUI, _lineTextUI;

    private string[] _characters;

    public DialogueOutputter(Text characterTextUI, Text LineTextUI)
    {
        _characterTextUI = characterTextUI;
        _lineTextUI = LineTextUI;
    }

    public void LoadCharacters(GameObject[] charactersInOrder)
    {
        _characters = new string[charactersInOrder.Length];
        for (int i = 0; i < charactersInOrder.Length; i++)
        {
            _characters[i] = charactersInOrder[i].name;
        }
    }

    public void LoadDialogue(DialogueSO dialogue)
    {
        _dialogue = dialogue;
        Debug.Log(_dialogue);
        _index = 0;
    }
    
    public bool TryShowNextDialoguePiece()
    {
        if (!_characterTextUI.transform.parent.parent.gameObject.activeSelf)
            _characterTextUI.transform.parent.parent.gameObject.SetActive(true);

        if (_index >= _dialogue.GetDialogueLength())
        {
            //выключаем панель
            _characterTextUI.transform.parent.parent.gameObject.SetActive(false);
            return false;
        }

        _characterTextUI.text = _characters[_dialogue.GetDialogueSpeakerAndLineByIndex(_index).SpeakerId];
        _lineTextUI.text = _dialogue.GetDialogueSpeakerAndLineByIndex(_index++).Line;
        

        return true;
    }
}