using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "New Tutorial Step", menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [Header("Dialogue")]
    public DialogueBase dialogue;

    [Header("Step Configuration")]
    public TutorialStepType stepType = TutorialStepType.DialogueOnly;

    [Header("Highlight")]
    [Tooltip("Configure o highlight para destacar elementos específicos durante este step")]
    public HighlightTarget highlightTarget;

    [Header("Board Setup")]
    public TutorialSpawnData[] piecesToSpawn;
    public bool clearBoardBeforeSpawn = true;

    [Header("Completion Conditions")]
    public bool waitForDialogueEnd = true;

    [Header("Events")]
    public UnityEvent onStepStart;
    public UnityEvent onStepComplete;

    [Header("Advanced")]
    public float delayBeforeNextStep = 0.5f;
}
