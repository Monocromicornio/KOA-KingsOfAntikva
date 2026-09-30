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

        [Header("Localization")]
        [Tooltip("Localization table key for the dialogue text. When empty or missing in the table, 'text' is used.")]
        public string textKey;
        [Tooltip("Localization table key for the speaker name. When empty or missing in the table, 'speaker' is used.")]
        public string speakerKey;

        public Sprite portraitLeft;
        public Sprite portraitRight;
        public UnityEvent myEvent;        
        public bool isRightPortrait;
        public bool isDoublePortrait;

        /// <summary>
        /// Returns the dialogue text in the current language, falling back to the raw 'text' field.
        /// </summary>
        public string GetLocalizedText() => LocalizationManager.GetTextOrFallback(textKey, text);

        /// <summary>
        /// Returns the speaker name in the current language, falling back to the raw 'speaker' field.
        /// </summary>
        public string GetLocalizedSpeaker() => LocalizationManager.GetTextOrFallback(speakerKey, speaker);
    }

    public Info[] dialogueInfo;
}
