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
    
    [Tooltip("Nome do GameObject filho para dar highlight (opcional). Se vazio, usa o GameObject principal.")]
    public string childObjectName;
    
    [Tooltip("Ativar o GameObject automaticamente ao mostrar o highlight e desativar ao esconder?")]
    public bool autoToggleGameObject = true;
    
    private GameObject trackedGameObject;
    private bool wasOriginallyInactive;

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
                    GameObject obj = FindGameObjectEvenIfInactive(gameObjectName);
                    if (obj != null)
                    {
                        trackedGameObject = obj;
                        wasOriginallyInactive = !obj.activeSelf;
                        
                        if (autoToggleGameObject && wasOriginallyInactive)
                        {
                            obj.SetActive(true);
                            Debug.Log($"[HighlightTarget] Auto-activated GameObject: {gameObjectName}");
                        }
                        
                        GameObject targetObj = obj;
                        
                        if (!string.IsNullOrEmpty(childObjectName))
                        {
                            Transform childTransform = obj.transform.Find(childObjectName);
                            if (childTransform != null)
                            {
                                targetObj = childTransform.gameObject;
                            }
                            else
                            {
                                Debug.LogWarning($"[HighlightTarget] Child '{childObjectName}' not found in '{gameObjectName}'!");
                            }
                        }
                        
                        RectTransform rect = targetObj.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            TutorialHighlight.instance.ShowHighlight(rect);
                        }
                        else
                        {
                            TutorialHighlight.instance.ShowHighlight(targetObj.transform);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[HighlightTarget] GameObject '{gameObjectName}' not found in scene. It may be spawned later.");
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
        
        if (autoToggleGameObject && trackedGameObject != null && wasOriginallyInactive)
        {
            trackedGameObject.SetActive(false);
            Debug.Log($"[HighlightTarget] Auto-deactivated GameObject: {trackedGameObject.name}");
        }
        
        trackedGameObject = null;
        wasOriginallyInactive = false;
    }
    
    private GameObject FindGameObjectEvenIfInactive(string name)
    {
        GameObject[] allObjects = UnityEngine.Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == name && obj.scene.IsValid())
            {
                return obj;
            }
        }
        return null;
    }
}
