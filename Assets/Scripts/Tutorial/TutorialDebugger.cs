using UnityEngine;

public class TutorialDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool showStepTransitions = true;
    public bool showEventTriggers = true;
    
    [Header("Keyboard Shortcuts")]
    public KeyCode skipStepKey = KeyCode.N;
    public KeyCode restartTutorialKey = KeyCode.R;

    private TutorialManager tutorialManager;

    private void Awake()
    {
        tutorialManager = TutorialManager.instance;
        if (tutorialManager == null)
        {
            tutorialManager = FindFirstObjectByType<TutorialManager>();
        }
    }

    private void OnEnable()
    {
        if (showEventTriggers)
        {
            TutorialEvents.OnPieceMoved += OnPieceMoved;
            TutorialEvents.OnPieceAttacked += OnPieceAttacked;
            TutorialEvents.OnPieceSelected += OnPieceSelected;
        }
    }

    private void OnDisable()
    {
        TutorialEvents.OnPieceMoved -= OnPieceMoved;
        TutorialEvents.OnPieceAttacked -= OnPieceAttacked;
        TutorialEvents.OnPieceSelected -= OnPieceSelected;
    }

    private void Update()
    {
        if (tutorialManager == null) return;

        if (Input.GetKeyDown(skipStepKey))
        {
            SkipCurrentStep();
        }

        if (Input.GetKeyDown(restartTutorialKey))
        {
            RestartTutorial();
        }
    }

    private void SkipCurrentStep()
    {
        if (tutorialManager != null)
        {
            LogDebug("⏭️ Skipping current step...");
            tutorialManager.CompleteCurrentStep();
        }
    }

    private void RestartTutorial()
    {
        if (tutorialManager != null && tutorialManager.currentSequence != null)
        {
            LogDebug("🔄 Restarting tutorial...");
            tutorialManager.StopTutorial();
            tutorialManager.StartTutorial(tutorialManager.currentSequence);
        }
    }

    private void OnPieceMoved(MonoBehaviour piece, GameField fromField, GameField toField)
    {
        LogDebug($"🚶 Piece Moved: {piece.name} from field {fromField.index} to {toField.index}");
    }

    private void OnPieceAttacked(MonoBehaviour attacker, MonoBehaviour target)
    {
        LogDebug($"⚔️ Attack: {attacker.name} attacked {target.name}");
    }

    private void OnPieceSelected(MonoBehaviour piece)
    {
        LogDebug($"👆 Piece Selected: {piece.name}");
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[Tutorial Debug] {message}");
        }
    }

    private void OnGUI()
    {
        if (!enableDebugLogs) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUIStyle titleStyle = new GUIStyle(GUI.skin.box);
        titleStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Tutorial Debugger", titleStyle);
        
        if (tutorialManager != null && tutorialManager.currentSequence != null)
        {
            GUILayout.Label($"Tutorial: {tutorialManager.currentSequence.tutorialName}");
            GUILayout.Label($"Tutorial Active: {(TutorialModeController.IsTutorialActive() ? "Yes" : "No")}");
            
            GUILayout.Space(10);
            GUILayout.Label("Shortcuts:");
            GUILayout.Label($"[{skipStepKey}] Skip Step");
            GUILayout.Label($"[{restartTutorialKey}] Restart Tutorial");
        }
        else
        {
            GUILayout.Label("No tutorial active");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
