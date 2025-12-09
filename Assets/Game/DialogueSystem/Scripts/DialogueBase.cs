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
        [HideInInspector]
        public string text;
        public string stringId;
        public string speaker;
        public Sprite portrait;
        public UnityEvent myEvent;        
    }

    public Info[] dialogueInfo;
}
