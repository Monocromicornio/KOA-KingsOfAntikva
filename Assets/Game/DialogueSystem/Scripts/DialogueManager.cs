using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    Image boxDialogue, portraitLeft, portraitRight;
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
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (boxDialogue.gameObject.activeInHierarchy)
        {
            if (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Space) || UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Return))
            {
                if (dialogueInfo.Count > 0 || isCurrentlyTyping)
                {
                    DequeueDialogue();
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    public void EnqueueDialogue(DialogueBase db)
    {
        boxDialogue.gameObject.SetActive(true);

        dialogueInfo.Clear();

        dialogueBase = db;
        
        if (db != null && db.dialogueInfo != null)
        {
            foreach (DialogueBase.Info info in db.dialogueInfo)
            {
                dialogueInfo.Enqueue(info);
            }           
        
            DequeueDialogue();
        }
        else
        {
            Debug.LogWarning("[DialogueManager] DialogueBase or dialogueInfo is null!");
        }
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

        if (!info.isRightPortrait)
        {
            portraitLeft.sprite = info.portraitLeft;
            portraitLeft.gameObject.SetActive(info.portraitLeft != null);
            portraitRight.gameObject.SetActive(false);
        }
        else
        {
            portraitRight.sprite = info.portraitRight;
            portraitRight.gameObject.SetActive(info.portraitRight != null);
            portraitLeft.gameObject.SetActive(false);
        }        
        
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

    private void EndDialogue()
    {
        Debug.Log("[DialogueManager] EndDialogue called");
        boxDialogue.gameObject.SetActive(false);
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);
        dialogueInfo.Clear();
        isCurrentlyTyping = false;
    }
    
}
