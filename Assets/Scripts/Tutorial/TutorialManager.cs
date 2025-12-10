using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;

    [Header("References")]
    public TutorialSequence currentSequence;
    public BoardController boardController;
    public DialogueManager dialogueManager;

    [Header("State")]
    private int currentStepIndex = -1;
    private TutorialStep currentStep;
    private bool isWaitingForCondition;
    private bool tutorialActive;
    private List<MonoBehaviour> spawnedPieces = new List<MonoBehaviour>();

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Multiple TutorialManager instances detected!");
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (currentSequence != null)
        {
            StartTutorial(currentSequence);
        }
    }

    private void OnEnable()
    {
        TutorialEvents.OnPieceMoved += OnPieceMoved;
        TutorialEvents.OnPieceAttacked += OnPieceAttacked;
        TutorialEvents.OnPieceSelected += OnPieceSelected;
    }

    private void OnDisable()
    {
        TutorialEvents.OnPieceMoved -= OnPieceMoved;
        TutorialEvents.OnPieceAttacked -= OnPieceAttacked;
        TutorialEvents.OnPieceSelected -= OnPieceSelected;
    }

    public void StartTutorial(TutorialSequence sequence)
    {
        if (tutorialActive)
        {
            Debug.LogWarning("Tutorial already active!");
            return;
        }

        currentSequence = sequence;
        currentStepIndex = -1;
        tutorialActive = true;
        NextStep();
    }

    public void NextStep()
    {
        if (!tutorialActive) return;

        currentStepIndex++;

        if (currentStepIndex >= currentSequence.steps.Length)
        {
            EndTutorial();
            return;
        }

        currentStep = currentSequence.steps[currentStepIndex];
        StartCoroutine(ExecuteStep(currentStep));
    }

    private IEnumerator ExecuteStep(TutorialStep step)
    {
        isWaitingForCondition = false;

        Debug.Log($"[Tutorial] Executing Step: {currentStepIndex} - Type: {step.stepType}");

        step.onStepStart?.Invoke();

        if (step.clearBoardBeforeSpawn)
        {
            ClearSpawnedPieces();
        }

        if (step.piecesToSpawn != null && step.piecesToSpawn.Length > 0)
        {
            Debug.Log($"[Tutorial] Spawning {step.piecesToSpawn.Length} pieces");
            SpawnPieces(step.piecesToSpawn);
        }

        if (step.dialogue != null)
        {
            Debug.Log($"[Tutorial] Enqueueing dialogue: {step.dialogue.name}");
            dialogueManager.EnqueueDialogue(step.dialogue);
        }
        else
        {
            Debug.Log("[Tutorial] No dialogue for this step");
        }

        if (step.waitForDialogueEnd && step.dialogue != null)
        {
            Debug.Log("[Tutorial] Waiting for dialogue to end...");
            yield return new WaitUntil(() => !dialogueManager.isDialog);
            Debug.Log("[Tutorial] Dialogue ended");
        }

        switch (step.stepType)
        {
            case TutorialStepType.DialogueOnly:
                Debug.Log("[Tutorial] DialogueOnly - Completing step");
                CompleteStep();
                break;

            case TutorialStepType.WaitForMovement:
            case TutorialStepType.WaitForAttack:
            case TutorialStepType.WaitForSelection:
            case TutorialStepType.WaitForCustomCondition:
                Debug.Log($"[Tutorial] Waiting for condition: {step.stepType}");
                isWaitingForCondition = true;
                break;
        }
    }

    private void SpawnPieces(TutorialSpawnData[] spawnDataArray)
    {
        foreach (TutorialSpawnData spawnData in spawnDataArray)
        {
            if (spawnData.piecePrefab == null)
            {
                Debug.LogWarning("Piece prefab is null in spawn data!");
                continue;
            }

            GameField targetField = boardController.GetGameField(spawnData.fieldIndex);
            if (targetField == null)
            {
                Debug.LogWarning($"Invalid field index: {spawnData.fieldIndex}");
                continue;
            }

            Vector3 spawnPosition = targetField.transform.position;
            Quaternion spawnRotation = spawnData.isPlayerPiece ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

            GameObject spawnedObject = Instantiate(spawnData.piecePrefab, spawnPosition, spawnRotation);
            
            Piece onlinePiece = spawnedObject.GetComponent<Piece>();
            OfflinePiece offlinePiece = spawnedObject.GetComponent<OfflinePiece>();
            
            if (onlinePiece != null)
            {
                onlinePiece.SetFirstField(targetField);
                
                if (spawnData.isPlayerPiece)
                {
                    onlinePiece.TurnBluePiece();
                }
                else
                {
                    onlinePiece.TurnRedPiece();
                }
                
                onlinePiece.ActivePiece();
                spawnedObject.SetActive(true);
                spawnData.spawnedPiece = onlinePiece;
                spawnedPieces.Add(onlinePiece);
            }
            else if (offlinePiece != null)
            {
                offlinePiece.SetField(targetField);
                
                if (spawnData.isPlayerPiece)
                {
                    offlinePiece.TurnBluePiece();
                    Debug.Log($"[TutorialManager] Spawned BLUE piece: {offlinePiece.name} at field {targetField.index}");
                }
                else
                {
                    offlinePiece.TurnRedPiece();
                    Debug.Log($"[TutorialManager] Spawned RED piece: {offlinePiece.name} at field {targetField.index}");
                }
                
                offlinePiece.ActivePiece();
                spawnedObject.SetActive(true);
                spawnData.spawnedPiece = offlinePiece;
                spawnedPieces.Add(offlinePiece);
            }
            else
            {
                Debug.LogError($"Spawned piece {spawnedObject.name} doesn't have Piece or OfflinePiece component!");
                Destroy(spawnedObject);
            }
        }
    }

    private void ClearSpawnedPieces()
    {
        Debug.Log($"[TutorialManager] Clearing {spawnedPieces.Count} spawned pieces");
        
        foreach (MonoBehaviour piece in spawnedPieces)
        {
            if (piece != null)
            {
                if (piece is Piece onlinePiece && onlinePiece.field != null)
                {
                    onlinePiece.field.SetPiece(null);
                }
                else if (piece is OfflinePiece offlinePiece && offlinePiece.field != null)
                {
                    offlinePiece.field.SetOfflinePiece(null);
                }
                
                Destroy(piece.gameObject);
            }
        }
        spawnedPieces.Clear();
    }

    private void OnPieceMoved(MonoBehaviour piece, GameField fromField, GameField toField)
    {
        if (!isWaitingForCondition) return;
        if (currentStep.stepType != TutorialStepType.WaitForMovement) return;

        if (spawnedPieces.Contains(piece))
        {
            CompleteStep();
        }
    }

    private void OnPieceAttacked(MonoBehaviour attacker, MonoBehaviour target)
    {
        if (!isWaitingForCondition) return;
        if (currentStep.stepType != TutorialStepType.WaitForAttack) return;

        if (spawnedPieces.Contains(attacker))
        {
            CompleteStep();
        }
    }

    private void OnPieceSelected(MonoBehaviour piece)
    {
        if (!isWaitingForCondition) return;
        if (currentStep.stepType != TutorialStepType.WaitForSelection) return;

        if (spawnedPieces.Contains(piece))
        {
            CompleteStep();
        }
    }

    private void CompleteStep()
    {
        if (!tutorialActive) return;

        isWaitingForCondition = false;
        currentStep.onStepComplete?.Invoke();

        StartCoroutine(WaitAndProceed());
    }

    private IEnumerator WaitAndProceed()
    {
        yield return new WaitForSeconds(currentStep.delayBeforeNextStep);
        NextStep();
    }

    public void CompleteCurrentStep()
    {
        CompleteStep();
    }

    private void EndTutorial()
    {
        tutorialActive = false;
        ClearSpawnedPieces();
        Debug.Log($"Tutorial '{currentSequence.tutorialName}' completed!");
    }

    public void StopTutorial()
    {
        tutorialActive = false;
        isWaitingForCondition = false;
        ClearSpawnedPieces();
        StopAllCoroutines();
    }
}
