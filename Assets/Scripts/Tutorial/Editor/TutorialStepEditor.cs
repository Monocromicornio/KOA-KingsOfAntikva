using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialStep))]
public class TutorialStepEditor : Editor
{
    private SerializedProperty dialogueProp;
    private SerializedProperty stepTypeProp;
    private SerializedProperty piecesToSpawnProp;
    private SerializedProperty clearBoardProp;
    private SerializedProperty waitForDialogueProp;
    private SerializedProperty onStepStartProp;
    private SerializedProperty onStepCompleteProp;
    private SerializedProperty delayProp;

    private void OnEnable()
    {
        dialogueProp = serializedObject.FindProperty("dialogue");
        stepTypeProp = serializedObject.FindProperty("stepType");
        piecesToSpawnProp = serializedObject.FindProperty("piecesToSpawn");
        clearBoardProp = serializedObject.FindProperty("clearBoardBeforeSpawn");
        waitForDialogueProp = serializedObject.FindProperty("waitForDialogueEnd");
        onStepStartProp = serializedObject.FindProperty("onStepStart");
        onStepCompleteProp = serializedObject.FindProperty("onStepComplete");
        delayProp = serializedObject.FindProperty("delayBeforeNextStep");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tutorial Step Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(dialogueProp);
        EditorGUILayout.PropertyField(stepTypeProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Board Setup", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(clearBoardProp);
        EditorGUILayout.PropertyField(piecesToSpawnProp, true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Completion Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(waitForDialogueProp);
        EditorGUILayout.PropertyField(delayProp);

        TutorialStepType stepType = (TutorialStepType)stepTypeProp.enumValueIndex;
        ShowStepTypeHelp(stepType);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onStepStartProp);
        EditorGUILayout.PropertyField(onStepCompleteProp);

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowStepTypeHelp(TutorialStepType stepType)
    {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(GetStepTypeDescription(stepType), MessageType.Info);
    }

    private string GetStepTypeDescription(TutorialStepType stepType)
    {
        switch (stepType)
        {
            case TutorialStepType.DialogueOnly:
                return "Esta etapa apenas mostra o diálogo e avança automaticamente.";
            
            case TutorialStepType.WaitForMovement:
                return "Aguarda o jogador mover uma das peças spawnadas nesta etapa.";
            
            case TutorialStepType.WaitForAttack:
                return "Aguarda o jogador atacar com uma das peças spawnadas nesta etapa.";
            
            case TutorialStepType.WaitForSelection:
                return "Aguarda o jogador selecionar uma das peças spawnadas nesta etapa.";
            
            case TutorialStepType.WaitForCustomCondition:
                return "Aguarda uma condição customizada. Use TutorialConditionChecker ou chame TutorialManager.CompleteCurrentStep().";
            
            default:
                return "";
        }
    }
}
