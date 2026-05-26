using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the win/lose result screens after a match ends.
/// Plays Animator-based animations, then reveals the pontuation panel and exit button.
/// </summary>
public class GameResultScreenController : MonoBehaviour
{
    // ─── Win Screen ──────────────────────────────────────────────────────────
    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private Animator winAnimator;
    [SerializeField] private GameObject winPontuationPanel;
    [SerializeField] private CanvasGroup winExitButtonGroup;
    [SerializeField] private Button winExitButton;
    [SerializeField] private TextMeshProUGUI winPontuationText;

    // ─── Lose Screen ─────────────────────────────────────────────────────────
    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private Animator loseAnimator;
    [SerializeField] private GameObject losePontuationPanel;
    [SerializeField] private CanvasGroup loseExitButtonGroup;
    [SerializeField] private Button loseExitButton;
    [SerializeField] private TextMeshProUGUI losePontuationText;

    // ─── Test Buttons ────────────────────────────────────────────────────────
    [Header("Test Buttons")]
    [SerializeField] private Button testWinButton;
    [SerializeField] private Button testLoseButton;

    // ─── During Game Panel ───────────────────────────────────────────────────
    [Header("During Game Panel")]
    [Tooltip("Panel that contains in-game HUD elements and should be hidden on result screen")]
    [SerializeField] private GameObject duringGamePanels;

    // ─── Animation Settings ──────────────────────────────────────────────────
    [Header("Animation Settings")]
    [SerializeField] private float pontuationDelay = 5f;
    [SerializeField] private float exitButtonFadeDuration = 0.4f;
    [SerializeField] private float exitButtonFadeStagger = 0.2f;

    // ─── Constants ───────────────────────────────────────────────────────────
    private const int WIN_POINTS = 50;
    private const int LOSE_POINTS = -20;
    private const string POINTS_FORMAT = "{0}{1} Influence Points";

    private void Awake()
    {
        // Hide screens and pontuation panels at start
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        if (winPontuationPanel != null) winPontuationPanel.SetActive(false);
        if (losePontuationPanel != null) losePontuationPanel.SetActive(false);

        // Wire exit buttons
        if (winExitButton != null)
            winExitButton.onClick.AddListener(OnExitClicked);
        if (loseExitButton != null)
            loseExitButton.onClick.AddListener(OnExitClicked);

        // Wire test buttons
        if (testWinButton != null)
            testWinButton.onClick.AddListener(() => ShowWinScreen(WIN_POINTS));
        if (testLoseButton != null)
            testLoseButton.onClick.AddListener(() => ShowLoseScreen(LOSE_POINTS));
    }

    /// <summary>Shows the victory screen with the given points.</summary>
    public void ShowWinScreen(int points)
    {
        SetPontuationText(winPontuationText, points);

        StartCoroutine(PlayResultSequence(
            winScreen, winAnimator, winPontuationPanel, winExitButtonGroup));
    }

    /// <summary>Shows the defeat screen with the given points.</summary>
    public void ShowLoseScreen(int points)
    {
        SetPontuationText(losePontuationText, points);

        StartCoroutine(PlayResultSequence(
            loseScreen, loseAnimator, losePontuationPanel, loseExitButtonGroup));
    }

    /// <summary>Handles the exit button click to return to menu.</summary>
    private void OnExitClicked()
    {
        if (MatchController.instance != null)
        {
            MatchController.instance.GoToMenu();
        }
    }

    // ─── Animation sequence ──────────────────────────────────────────────────

    private IEnumerator PlayResultSequence(
        GameObject screen,
        Animator animator,
        GameObject pontuationPanel,
        CanvasGroup exitButtonGroup)
    {
        // Prepare initial state
        if (exitButtonGroup != null) exitButtonGroup.alpha = 0f;
        if (pontuationPanel != null) pontuationPanel.SetActive(false);

        // Hide in-game HUD
        if (duringGamePanels != null) duringGamePanels.SetActive(false);

        // Activate screen — this also starts the Animator since it plays on awake
        screen.SetActive(true);

        // Ensure the animator is playing
        if (animator != null)
        {
            animator.enabled = true;
            animator.Play(0, -1, 0f);
        }

        // Step 1: Wait for pontuation delay then show pontuation
        yield return new WaitForSeconds(pontuationDelay);

        if (pontuationPanel != null)
        {
            pontuationPanel.SetActive(true);
        }

        // Step 2: Fade in exit button after stagger
        yield return new WaitForSeconds(exitButtonFadeStagger);

        if (exitButtonGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(exitButtonGroup, 0f, 1f, exitButtonFadeDuration));
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static void SetPontuationText(TextMeshProUGUI textComponent, int points)
    {
        if (textComponent == null) return;
        string sign = points >= 0 ? "+" : "";
        textComponent.text = string.Format(POINTS_FORMAT, sign, points);
    }

    /// <summary>Fades a CanvasGroup from startAlpha to endAlpha.</summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, EaseInOutQuad(t));
            yield return null;
        }

        group.alpha = to;
    }

    private static float EaseInOutQuad(float t)
        => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
}
