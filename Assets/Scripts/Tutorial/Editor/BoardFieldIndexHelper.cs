using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoardController))]
public class BoardFieldIndexHelper : Editor
{
    private bool showFieldIndices = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tutorial Helper", EditorStyles.boldLabel);
        
        showFieldIndices = EditorGUILayout.Toggle("Show Field Indices in Scene", showFieldIndices);

        if (showFieldIndices)
        {
            EditorGUILayout.HelpBox("Os índices dos campos serão mostrados na Scene View. Use isso para descobrir qual índice usar no tutorial.", MessageType.Info);
        }

        SceneView.RepaintAll();
    }

    private void OnSceneGUI()
    {
        if (!showFieldIndices) return;

        BoardController board = (BoardController)target;
        if (board == null || board.gameFields == null) return;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;

        for (int i = 0; i < board.gameFields.Length; i++)
        {
            GameField field = board.gameFields[i];
            if (field == null) continue;

            Vector3 position = field.transform.position + Vector3.up * 0.5f;
            Handles.Label(position, i.ToString(), style);

            Handles.color = new Color(1f, 1f, 0f, 0.3f);
            Handles.DrawWireCube(field.transform.position, Vector3.one * 0.8f);
        }
    }
}
