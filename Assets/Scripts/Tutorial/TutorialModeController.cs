using UnityEngine;

public class TutorialModeController : MonoBehaviour
{
    public static TutorialModeController instance;
    public static bool isTutorialMode { get; private set; }

    [Header("Tutorial Settings")]
    public bool enableTutorialMode = true;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        isTutorialMode = enableTutorialMode;
    }

    public static bool IsTutorialActive()
    {
        return instance != null && isTutorialMode;
    }

    public void SetTutorialMode(bool enabled)
    {
        isTutorialMode = enabled;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            isTutorialMode = false;
        }
    }
}
