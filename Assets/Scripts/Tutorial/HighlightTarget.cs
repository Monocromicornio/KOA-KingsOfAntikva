using UnityEngine;

[System.Serializable]
public class HighlightTarget
{
    public enum TargetType
    {
        None,
        UIElement,
        WorldObject,
        GameObjectByName
    }

    public TargetType targetType = TargetType.None;
    public RectTransform uiTarget;
    public Transform worldTarget;
    public string gameObjectName;

    public void Show()
    {
        if (TutorialHighlight.instance == null)
        {
            Debug.LogWarning("[HighlightTarget] TutorialHighlight instance not found!");
            return;
        }

        switch (targetType)
        {
            case TargetType.UIElement:
                if (uiTarget != null)
                {
                    TutorialHighlight.instance.ShowHighlight(uiTarget);
                }
                else
                {
                    Debug.LogWarning("[HighlightTarget] UI Target is null!");
                }
                break;

            case TargetType.WorldObject:
                if (worldTarget != null)
                {
                    TutorialHighlight.instance.ShowHighlight(worldTarget);
                }
                else
                {
                    Debug.LogWarning("[HighlightTarget] World Target is null!");
                }
                break;

            case TargetType.GameObjectByName:
                if (!string.IsNullOrEmpty(gameObjectName))
                {
                    GameObject obj = GameObject.Find(gameObjectName);
                    if (obj != null)
                    {
                        RectTransform rect = obj.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            TutorialHighlight.instance.ShowHighlight(rect);
                        }
                        else
                        {
                            TutorialHighlight.instance.ShowHighlight(obj.transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[HighlightTarget] GameObject '{gameObjectName}' not found!");
                    }
                }
                else
                {
                    Debug.LogWarning("[HighlightTarget] GameObject Name is empty!");
                }
                break;

            case TargetType.None:
                break;
        }
    }

    public void Hide()
    {
        if (TutorialHighlight.instance != null)
        {
            TutorialHighlight.instance.Hide();
        }
    }
}
