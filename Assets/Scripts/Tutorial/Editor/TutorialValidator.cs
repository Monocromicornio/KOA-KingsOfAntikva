using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TutorialValidator : EditorWindow
{
    private TutorialSequence sequenceToValidate;
    private Vector2 scrollPosition;
    private List<string> validationErrors = new List<string>();
    private List<string> validationWarnings = new List<string>();

    [MenuItem("Window/Tutorial/Tutorial Validator")]
    public static void ShowWindow()
    {
        GetWindow<TutorialValidator>("Tutorial Validator");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tutorial Sequence Validator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Use esta ferramenta para validar sua sequência de tutorial antes de testar.", MessageType.Info);
        
        EditorGUILayout.Space();
        sequenceToValidate = (TutorialSequence)EditorGUILayout.ObjectField(
            "Tutorial Sequence",
            sequenceToValidate,
            typeof(TutorialSequence),
            false
        );

        EditorGUILayout.Space();

        GUI.enabled = sequenceToValidate != null;
        if (GUILayout.Button("Validate Tutorial", GUILayout.Height(30)))
        {
            ValidateTutorial();
        }
        GUI.enabled = true;

        if (validationErrors.Count > 0 || validationWarnings.Count > 0)
        {
            EditorGUILayout.Space();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (validationErrors.Count > 0)
            {
                EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                foreach (string error in validationErrors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }

            if (validationWarnings.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);
                foreach (string warning in validationWarnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void ValidateTutorial()
    {
        validationErrors.Clear();
        validationWarnings.Clear();

        if (sequenceToValidate == null)
        {
            validationErrors.Add("Nenhuma Tutorial Sequence selecionada!");
            return;
        }

        if (string.IsNullOrEmpty(sequenceToValidate.tutorialName))
        {
            validationWarnings.Add("Tutorial Sequence não tem nome definido.");
        }

        if (sequenceToValidate.steps == null || sequenceToValidate.steps.Length == 0)
        {
            validationErrors.Add("Tutorial Sequence não tem steps! Adicione pelo menos um TutorialStep.");
            return;
        }

        for (int i = 0; i < sequenceToValidate.steps.Length; i++)
        {
            TutorialStep step = sequenceToValidate.steps[i];
            if (step == null)
            {
                validationErrors.Add($"Step {i + 1}: Step é null! Remova ou configure este step.");
                continue;
            }

            ValidateStep(step, i + 1);
        }

        if (validationErrors.Count == 0 && validationWarnings.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Validação Completa",
                "Tutorial validado com sucesso! Nenhum erro ou aviso encontrado.",
                "OK"
            );
        }
    }

    private void ValidateStep(TutorialStep step, int stepNumber)
    {
        string stepPrefix = $"Step {stepNumber} ({step.name})";

        if (step.dialogue == null && step.stepType == TutorialStepType.DialogueOnly)
        {
            validationWarnings.Add($"{stepPrefix}: DialogueOnly sem Dialogue configurado.");
        }

        if (step.stepType != TutorialStepType.DialogueOnly)
        {
            if (step.piecesToSpawn == null || step.piecesToSpawn.Length == 0)
            {
                validationWarnings.Add($"{stepPrefix}: Step do tipo '{step.stepType}' sem peças para spawnar. O jogador não terá nada para interagir.");
            }
            else
            {
                for (int i = 0; i < step.piecesToSpawn.Length; i++)
                {
                    TutorialSpawnData spawnData = step.piecesToSpawn[i];
                    if (spawnData.piecePrefab == null)
                    {
                        validationErrors.Add($"{stepPrefix}: Piece {i} não tem prefab configurado!");
                    }

                    if (spawnData.fieldIndex < 0)
                    {
                        validationErrors.Add($"{stepPrefix}: Piece {i} tem Field Index negativo ({spawnData.fieldIndex})!");
                    }

                    if (spawnData.fieldIndex > 99)
                    {
                        validationWarnings.Add($"{stepPrefix}: Piece {i} tem Field Index muito alto ({spawnData.fieldIndex}). Verifique se o índice está correto.");
                    }
                }
            }
        }

        if (step.delayBeforeNextStep < 0)
        {
            validationWarnings.Add($"{stepPrefix}: Delay negativo ({step.delayBeforeNextStep}). Será considerado 0.");
        }

        if (step.stepType == TutorialStepType.WaitForAttack)
        {
            bool hasPlayerPiece = false;
            bool hasEnemyPiece = false;

            if (step.piecesToSpawn != null)
            {
                foreach (TutorialSpawnData spawn in step.piecesToSpawn)
                {
                    if (spawn.isPlayerPiece) hasPlayerPiece = true;
                    else hasEnemyPiece = true;
                }
            }

            if (!hasPlayerPiece)
            {
                validationWarnings.Add($"{stepPrefix}: WaitForAttack sem peça do jogador. Como o jogador vai atacar?");
            }

            if (!hasEnemyPiece)
            {
                validationWarnings.Add($"{stepPrefix}: WaitForAttack sem peça inimiga. Quem o jogador vai atacar?");
            }
        }

        if (step.stepType == TutorialStepType.WaitForMovement || step.stepType == TutorialStepType.WaitForSelection)
        {
            bool hasPlayerPiece = false;

            if (step.piecesToSpawn != null)
            {
                foreach (TutorialSpawnData spawn in step.piecesToSpawn)
                {
                    if (spawn.isPlayerPiece) hasPlayerPiece = true;
                }
            }

            if (!hasPlayerPiece)
            {
                validationWarnings.Add($"{stepPrefix}: {step.stepType} sem peça do jogador.");
            }
        }
    }
}
