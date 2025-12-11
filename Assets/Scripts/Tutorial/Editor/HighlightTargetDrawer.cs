using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(HighlightTarget))]
public class HighlightTargetDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var targetTypeProperty = property.FindPropertyRelative("targetType");
        var uiTargetProperty = property.FindPropertyRelative("uiTarget");
        var worldTargetProperty = property.FindPropertyRelative("worldTarget");
        var gameObjectNameProperty = property.FindPropertyRelative("gameObjectName");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        Rect currentRect = new Rect(position.x, position.y, position.width, lineHeight);

        EditorGUI.PropertyField(currentRect, targetTypeProperty);
        currentRect.y += lineHeight + spacing;

        HighlightTarget.TargetType targetType = (HighlightTarget.TargetType)targetTypeProperty.enumValueIndex;

        switch (targetType)
        {
            case HighlightTarget.TargetType.UIElement:
                EditorGUI.PropertyField(currentRect, uiTargetProperty, new GUIContent("UI Target"));
                break;

            case HighlightTarget.TargetType.WorldObject:
                EditorGUI.PropertyField(currentRect, worldTargetProperty, new GUIContent("World Target"));
                break;

            case HighlightTarget.TargetType.GameObjectByName:
                EditorGUI.PropertyField(currentRect, gameObjectNameProperty, new GUIContent("GameObject Name"));
                break;

            case HighlightTarget.TargetType.None:
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var targetTypeProperty = property.FindPropertyRelative("targetType");
        HighlightTarget.TargetType targetType = (HighlightTarget.TargetType)targetTypeProperty.enumValueIndex;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        if (targetType == HighlightTarget.TargetType.None)
        {
            return lineHeight;
        }

        return (lineHeight + spacing) * 2;
    }
}
