using UnityEngine;

public class TutorialConditionChecker : MonoBehaviour
{
    private bool conditionMet;

    public void SetConditionMet()
    {
        conditionMet = true;
        CompleteTutorialStep();
    }

    public void CheckCondition(bool condition)
    {
        if (condition)
        {
            SetConditionMet();
        }
    }

    private void CompleteTutorialStep()
    {
        if (TutorialManager.instance != null)
        {
            TutorialManager.instance.CompleteCurrentStep();
        }
    }

    private void OnDestroy()
    {
        conditionMet = false;
    }
}
