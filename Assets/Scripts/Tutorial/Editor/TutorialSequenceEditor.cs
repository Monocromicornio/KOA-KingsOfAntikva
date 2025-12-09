using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialSequence))]
public class TutorialSequenceEditor : Editor
{
    private SerializedProperty tutorialNameProp;
    private SerializedProperty stepsProp;

    private void OnEnable()
    {
        tutorialNameProp = serializedObject.FindProperty("tutorialName");
        stepsProp = serializedObject.FindProperty("steps");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tutorial Sequence", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(tutorialNameProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total Steps: {stepsProp.arraySize}", EditorStyles.helpBox);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(stepsProp, true);

        if (stepsProp.arraySize > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Arraste os Tutorial Steps na ordem que deseja executá-los.", MessageType.Info);
            
            DrawStepsList();
        }
        else
        {
            EditorGUILayout.HelpBox("Adicione Tutorial Steps para criar uma sequência.", MessageType.Warning);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawStepsList()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview da Sequência", EditorStyles.boldLabel);
        
        for (int i = 0; i < stepsProp.arraySize; i++)
        {
            SerializedProperty stepProp = stepsProp.GetArrayElementAtIndex(i);
            TutorialStep step = stepProp.objectReferenceValue as TutorialStep;
            
            if (step != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"{i + 1}.", GUILayout.Width(30));
                EditorGUILayout.LabelField(step.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"[{step.stepType}]", EditorStyles.miniLabel, GUILayout.Width(150));
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
