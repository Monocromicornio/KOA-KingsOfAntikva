using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the win/lose screen animations after a match ends.
/// Animation sequence: Title appears large → waits → moves to rest position → background fades in → exit button fades in.
/// </summary>
public class GameResultScreenController : MonoBehaviour
{
    // ─── Win Screen ──────────────────────────────────────────────────────────
    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private RectTransform winTitlePanel;
    [SerializeField] private CanvasGroup winBackgroundGroup;
    [SerializeField] private CanvasGroup winExitButtonGroup;
    [SerializeField] private Button winExitButton;
    [SerializeField] private TextMeshProUGUI winPontuationText;

    // ─── Lose Screen ─────────────────────────────────────────────────────────
    [Header("Lose Screen")]
    [SerializeField] private GameObject loseScreen;
    [SerializeField] private RectTransform loseTitlePanel;
    [SerializeField] private CanvasGroup loseBackgroundGroup;
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
    [SerializeField] private float titleDisplayDuration = 2f;
    [SerializeField] private float titleScaleMultiplier = 2f;
    [SerializeField] private float titleAnimDuration = 0.5f;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private float fadeStagger = 0.2f;

    // ─── Constants ───────────────────────────────────────────────────────────
    private const int WIN_POINTS = 50;
    private const int LOSE_POINTS = -20;
    private const string POINTS_FORMAT = "{0}{1} Influence Points";

    // ─── Cached rest positions ───────────────────────────────────────────────
    private Vector2 _winTitleRestPos;
    private Vector2 _loseTitleRestPos;
    private Vector3 _winTitleRestScale;
    private Vector3 _loseTitleRestScale;

    private void Awake()
    {
        // Cache rest transforms
        if (winTitlePanel != null)
        {
            _winTitleRestPos = winTitlePanel.anchoredPosition;
            _winTitleRestScale = winTitlePanel.localScale;
        }

        if (loseTitlePanel != null)
        {
            _loseTitleRestPos = loseTitlePanel.anchoredPosition;
            _loseTitleRestScale = loseTitlePanel.localScale;
        }

        // Hide screens
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);

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
        if (winPontuationText != null)
        {
            string sign = points >= 0 ? "+" : "";
            winPontuationText.text = string.Format(POINTS_FORMAT, sign, points);
        }

        StartCoroutine(PlayResultAnimation(
            winScreen, winTitlePanel, winBackgroundGroup, winExitButtonGroup,
            _winTitleRestPos, _winTitleRestScale));
    }

    /// <summary>Shows the defeat screen with the given points.</summary>
    public void ShowLoseScreen(int points)
    {
        if (losePontuationText != null)
        {
            string sign = points >= 0 ? "+" : "";
            losePontuationText.text = string.Format(POINTS_FORMAT, sign, points);
        }

        StartCoroutine(PlayResultAnimation(
            loseScreen, loseTitlePanel, loseBackgroundGroup, loseExitButtonGroup,
            _loseTitleRestPos, _loseTitleRestScale));
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

    private IEnumerator PlayResultAnimation(
        GameObject screen,
        RectTransform titlePanel,
        CanvasGroup backgroundGroup,
        CanvasGroup exitButtonGroup,
        Vector2 titleRestPos,
        Vector3 titleRestScale)
    {
        // Prepare initial state
        if (backgroundGroup != null) backgroundGroup.alpha = 0f;
        if (exitButtonGroup != null) exitButtonGroup.alpha = 0f;

        // Hide in-game HUD
        if (duringGamePanels != null) duringGamePanels.SetActive(false);

        // Title starts large and centered
        if (titlePanel != null)
        {
            titlePanel.anchoredPosition = Vector2.zero;
            titlePanel.localScale = titleRestScale * titleScaleMultiplier;
        }

        // Activate screen
        screen.SetActive(true);

        // Step 1: Fade in the title
        yield return StartCoroutine(FadeCanvasGroupOnImage(titlePanel, 0f, 1f, titleAnimDuration));

        // Step 2: Hold title in center
        yield return new WaitForSeconds(titleDisplayDuration);

        // Step 3: Animate title to rest position and scale
        yield return StartCoroutine(AnimateTitleToRest(
            titlePanel, titleRestPos, titleRestScale, titleAnimDuration));

        // Step 4: Fade in background
        if (backgroundGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(backgroundGroup, 0f, 1f, fadeDuration));
        }

        // Step 5: Fade in exit button after stagger
        yield return new WaitForSeconds(fadeStagger);

        if (exitButtonGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(exitButtonGroup, 0f, 1f, fadeDuration));
        }
    }

    // ─── Animation helpers ───────────────────────────────────────────────────

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

    /// <summary>Fades an Image component on the RectTransform (for Title_Panel that has its own Canvas).</summary>
    private IEnumerator FadeCanvasGroupOnImage(RectTransform rt, float from, float to, float duration)
    {
        if (rt == null) yield break;

        var image = rt.GetComponent<Image>();
        var tmpTexts = rt.GetComponentsInChildren<TextMeshProUGUI>();

        float elapsed = 0f;

        SetImageAlpha(image, from);
        SetTextsAlpha(tmpTexts, from);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(from, to, EaseInOutQuad(t));
            SetImageAlpha(image, alpha);
            SetTextsAlpha(tmpTexts, alpha);
            yield return null;
        }

        SetImageAlpha(image, to);
        SetTextsAlpha(tmpTexts, to);
    }

    /// <summary>Animates the title panel from current position/scale to rest values.</summary>
    private IEnumerator AnimateTitleToRest(
        RectTransform titlePanel, Vector2 restPos, Vector3 restScale, float duration)
    {
        if (titlePanel == null) yield break;

        Vector2 startPos = titlePanel.anchoredPosition;
        Vector3 startScale = titlePanel.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = EaseInOutQuad(t);

            titlePanel.anchoredPosition = Vector2.Lerp(startPos, restPos, eased);
            titlePanel.localScale = Vector3.Lerp(startScale, restScale, eased);
            yield return null;
        }

        titlePanel.anchoredPosition = restPos;
        titlePanel.localScale = restScale;
    }

    // ─── Utility ─────────────────────────────────────────────────────────────

    private static void SetImageAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private static void SetTextsAlpha(TextMeshProUGUI[] texts, float alpha)
    {
        if (texts == null) return;
        foreach (var txt in texts)
        {
            if (txt == null) continue;
            Color c = txt.color;
            c.a = alpha;
            txt.color = c;
        }
    }

    private static float EaseInOutQuad(float t)
        => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
}
