using UnityEngine;

public class TutorialSceneSetup : MonoBehaviour
{
    [Header("Tutorial Configuration")]
    public TutorialManager tutorialManager;
    public BoardController boardController;
    public DialogueManager dialogueManager;
    public MatchController matchController;

    [Header("Tutorial to Run")]
    public TutorialSequence tutorialSequence;

    private void Awake()
    {
        SetupTutorialMode();
    }

    private void Start()
    {
        if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager not assigned!");
            return;
        }

        tutorialManager.boardController = boardController;
        tutorialManager.dialogueManager = dialogueManager;
        tutorialManager.currentSequence = tutorialSequence;

        if (tutorialSequence != null)
        {
            tutorialManager.StartTutorial(tutorialSequence);
        }
    }

    private void SetupTutorialMode()
    {
        if (matchController != null)
        {
            matchController.enabled = false;
        }
    }
}
