using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Configuration")]
    [Tooltip("O DialogueBase ScriptableObject que será iniciado")]
    public DialogueBase dialogueToTrigger;

    [Header("Trigger Settings")]
    [Tooltip("Iniciar o diálogo automaticamente no Start?")]
    public bool triggerOnStart = true;

    [Tooltip("Tempo de delay em segundos antes de iniciar o diálogo")]
    public float startDelay = 0f;

    private void Start()
    {
        if (triggerOnStart)
        {
            if (startDelay > 0f)
            {
                Invoke(nameof(TriggerDialogue), startDelay);
            }
            else
            {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue()
    {
        if (dialogueToTrigger == null)
        {
            Debug.LogWarning("[DialogueTrigger] Nenhum DialogueBase foi atribuído!");
            return;
        }

        if (DialogueManager.instance == null)
        {
            Debug.LogError("[DialogueTrigger] DialogueManager.instance não encontrado na cena!");
            return;
        }

        Debug.Log($"[DialogueTrigger] Iniciando diálogo: {dialogueToTrigger.name}");
        DialogueManager.instance.EnqueueDialogue(dialogueToTrigger);
    }
}
