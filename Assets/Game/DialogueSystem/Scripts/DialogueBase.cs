using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Tutorial/Dialogues")]
public class DialogueBase : ScriptableObject
{
    [System.Serializable]
    public class Info
    {        
        public string text;       
        public string speaker;
        public Sprite portraitLeft;
        public Sprite portraitRight;
        public UnityEvent myEvent;        
        public bool isRightPortrait;
    }

    public Info[] dialogueInfo;
}
