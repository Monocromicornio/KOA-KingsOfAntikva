using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;
    public bool isDialog
    {
        get
        {
            return boxDialogue.gameObject.activeInHierarchy;
        }
    }

    public float delay = 0.001f;

    [SerializeField]
    Image boxDialogue, dialoguePortrait;
    [SerializeField]
    TMP_Text dialogueText, dialogueName;
    Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>();

    private bool isCurrentlyTyping;
    private string completeText;

    DialogueBase dialogueBase;

    public void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Fix this" + gameObject.name);
        }
        else
        {
            instance = this;
        }
        boxDialogue.gameObject.SetActive(false);
    }

    void LoadStrings(StringTable stringTable)
    {
        if (dialogueBase == null) return;

        foreach (DialogueBase.Info info in dialogueBase.dialogueInfo)
        {
            info.text = GetLocalizedString(stringTable, info.stringId);
            dialogueInfo.Enqueue(info);
        }

        DequeueDialogue();
    }

    string GetLocalizedString(StringTable table, string entryName)
    {
        var entry = table.GetEntry(entryName);
        return entry.GetLocalizedString();
    }

    public void EnqueueDialogue(DialogueBase db)
    {
        boxDialogue.gameObject.SetActive(true);
        dialogueInfo.Clear();

        dialogueBase = db;
        dialogueBase.stringTable.TableChanged += LoadStrings;
    }

    public void DequeueDialogue()
    {
        if (isCurrentlyTyping)
        {
            CompleteText();
            StopAllCoroutines();
            isCurrentlyTyping = false;
            return;
        }

        if (dialogueInfo.Count == 0)
        {           
            return;
        }


        DialogueBase.Info info = dialogueInfo.Dequeue();
        completeText = info.text;

        dialogueName.text = info.speaker;
        dialogueText.text = info.text;
        dialoguePortrait.sprite = info.portrait;
        
        dialogueText.text = "";
        StartCoroutine(TypeText(info));
               
        info.myEvent.Invoke();
    }

    IEnumerator TypeText(DialogueBase.Info info)
    {
        isCurrentlyTyping = true;

        foreach(char c in info.text.ToCharArray())
        {
            yield return new WaitForSeconds(delay);
            dialogueText.text += c;
        }

        isCurrentlyTyping = false;
    }

    private void CompleteText()
    {
        dialogueText.text = completeText;
    }
    
}
