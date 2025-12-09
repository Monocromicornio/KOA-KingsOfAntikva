using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial Sequence", menuName = "Tutorial/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    public string tutorialName;
    public TutorialStep[] steps;
}
