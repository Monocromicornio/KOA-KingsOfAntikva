using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reproduz a animação de intro da partida:
///   1. Câmera voa de uma posição inicial até a posição final.
///   2. Os perfis (Player e Enemy) surgem com fade no centro e deslizam para as posições originais.
///   3. Ao chegarem nas posições o TurnTimer é iniciado.
///
/// As preparações (captura de posições, ocultação dos perfis) acontecem em Awake,
/// permitindo que comecem enquanto o LoadingScreen ainda está ativo.
/// A animação só dispara quando o LoadingScreen some.
/// </summary>
public class MatchIntroAnimator : MonoBehaviour
{
    // ─── Camera ──────────────────────────────────────────────────────────────
    [Header("Camera")]
    public Transform cameraTransform;

    public Vector3 cameraStartPosition = new Vector3(0f, 2.9f, -10.6f);
    public Vector3 cameraEndPosition   = new Vector3(0f, -0.46f, -0.08f);

    [Tooltip("Duração do movimento de câmera em segundos")]
    public float cameraDuration = 2f;

    // ─── Profiles ────────────────────────────────────────────────────────────
    [Header("Profiles")]
    public RectTransform playerProfile;
    public RectTransform enemyProfile;

    [Tooltip("Duração do fade-in dos perfis")]
    public float fadeDuration = 0.6f;

    [Tooltip("Duração do deslize dos perfis até as posições originais")]
    public float slideDuration = 0.7f;

    // ─── Timer ───────────────────────────────────────────────────────────────
    [Header("Timer")]
    public TurnTimer turnTimer;

    // ─── Outros elementos do Canvas ──────────────────────────────────────────
    [Header("Outros elementos do Canvas")]
    [Tooltip("GameObjects que devem começar invisíveis e fazer fade-in ao final da intro")]
    public GameObject[] hudElements;

    [Tooltip("Duração do fade-in dos elementos de HUD")]
    public float hudFadeDuration = 0.5f;

    // ─── Internal state ──────────────────────────────────────────────────────
    private Vector2 _playerProfileRest;
    private Vector2 _enemyProfileRest;

    private CanvasGroup _playerCG;
    private CanvasGroup _enemyCG;

    private CanvasGroup[] _hudCanvasGroups;

    private const float CENTER_OFFSET = 80f; // distância entre os dois cards no centro

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Bloqueia o timer durante a intro — será reativado ao final da animação.
        // Desativar em Awake impede que Start() do TurnTimer rode prematuramente.
        if (turnTimer != null)
            turnTimer.enabled = false;

        // Preparações que podem acontecer durante o loading screen:
        // 1. Captura as posições de descanso dos perfis
        _playerProfileRest = playerProfile.anchoredPosition;
        _enemyProfileRest  = enemyProfile.anchoredPosition;

        // 2. Garante CanvasGroup nos perfis para controle de alpha
        _playerCG = GetOrAddCanvasGroup(playerProfile.gameObject);
        _enemyCG  = GetOrAddCanvasGroup(enemyProfile.gameObject);

        // 3. Oculta os perfis (alpha 0) enquanto o loading ainda está ativo
        _playerCG.alpha = 0f;
        _enemyCG.alpha  = 0f;

        // 4. Garante CanvasGroup e oculta os elementos de HUD extras
        _hudCanvasGroups = new CanvasGroup[hudElements?.Length ?? 0];
        for (int i = 0; i < _hudCanvasGroups.Length; i++)
        {
            _hudCanvasGroups[i]                     = GetOrAddCanvasGroup(hudElements[i]);
            _hudCanvasGroups[i].alpha               = 0f;
            _hudCanvasGroups[i].interactable        = false;
            _hudCanvasGroups[i].blocksRaycasts      = false;
        }

        // 5. Posiciona a câmera no início da animação
        if (cameraTransform != null)
            cameraTransform.localPosition = cameraStartPosition;
    }

    private void Start()
    {
        StartCoroutine(WaitForLoadingThenPlay());
    }

    // ─── Sequência principal ─────────────────────────────────────────────────

    private IEnumerator WaitForLoadingThenPlay()
    {
        // Aguarda o LoadingScreenManager existir e depois sumir
        yield return new WaitUntil(() =>
            LoadingScreenManager.Instance != null && !LoadingScreenManager.Instance.IsShowing());

        yield return StartCoroutine(PlayCameraIntro());
        yield return StartCoroutine(PlayProfilesIntro());

        // Profiles chegaram: fade-in do HUD e timer em paralelo
        StartCoroutine(FadeInHudElements());

        if (turnTimer != null)
        {
            turnTimer.enabled = true;
            // Aguarda um frame para o Start() do TurnTimer executar
            yield return null;
            turnTimer.StartTimer();
        }
    }

    // ─── Câmera ──────────────────────────────────────────────────────────────

    private IEnumerator PlayCameraIntro()
    {
        float t = 0f;

        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / cameraDuration, 1f);
            cameraTransform.localPosition = Vector3.LerpUnclamped(
                cameraStartPosition, cameraEndPosition, EaseInOutQuart(t));
            yield return null;
        }

        cameraTransform.localPosition = cameraEndPosition;
    }

    // ─── Perfis ───────────────────────────────────────────────────────────────

    private IEnumerator PlayProfilesIntro()
    {
        RectTransform canvasRT = playerProfile.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        float canvasW = canvasRT.rect.width;
        float canvasH = canvasRT.rect.height;

        // Profiles têm anchor (0,1) e pivot (0,1) — topo-esquerda.
        // anchoredPosition é o deslocamento do anchor até o pivot.
        // Para centrar visualmente o card:
        //   X do pivot = canvasCenterX ± gap  (pivot é borda esquerda do card)
        //   Y do pivot = -canvasCenterY + elementH/2  (pivot é borda superior, então sobe metade)
        float canvasCenterX = canvasW * 0.5f;
        float canvasCenterY = canvasH * 0.5f;

        Vector2 playerCenter = new Vector2(
            canvasCenterX - CENTER_OFFSET - playerProfile.rect.width,
            -canvasCenterY + playerProfile.rect.height * 0.5f
        );
        Vector2 enemyCenter = new Vector2(
            canvasCenterX + CENTER_OFFSET,
            -canvasCenterY + enemyProfile.rect.height * 0.5f
        );

        // Posiciona os cards no centro antes do fade
        playerProfile.anchoredPosition = playerCenter;
        enemyProfile.anchoredPosition  = enemyCenter;

        // Fade in em paralelo
        yield return StartCoroutine(FadeInProfiles());

        // Desliza ambos para as posições originais em paralelo
        bool slidesDone = false;
        int pending = 2;
        System.Action onSlide = () => { if (--pending == 0) slidesDone = true; };

        StartCoroutine(SlideAnchored(playerProfile, playerCenter, _playerProfileRest, onSlide));
        StartCoroutine(SlideAnchored(enemyProfile,  enemyCenter,  _enemyProfileRest,  onSlide));

        yield return new WaitUntil(() => slidesDone);
    }

    private IEnumerator FadeInHudElements()
    {
        if (_hudCanvasGroups == null || _hudCanvasGroups.Length == 0) yield break;

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / hudFadeDuration, 1f);
            float alpha = EaseOutQuart(t);
            foreach (var cg in _hudCanvasGroups)
                cg.alpha = alpha;
            yield return null;
        }

        foreach (var cg in _hudCanvasGroups)
        {
            cg.alpha          = 1f;
            cg.interactable   = true;
            cg.blocksRaycasts = true;
        }
    }

    private IEnumerator FadeInProfiles()
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / fadeDuration, 1f);
            float alpha    = EaseOutQuart(t);
            _playerCG.alpha = alpha;
            _enemyCG.alpha  = alpha;
            yield return null;
        }

        _playerCG.alpha = 1f;
        _enemyCG.alpha  = 1f;
    }

    private IEnumerator SlideAnchored(RectTransform rt, Vector2 from, Vector2 to,
                                       System.Action onComplete)
    {
        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / slideDuration, 1f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutBack(t));
            yield return null;
        }

        rt.anchoredPosition = to;
        onComplete?.Invoke();
    }

    // ─── Easing ──────────────────────────────────────────────────────────────

    private static float EaseInOutQuart(float t)
    {
        return t < 0.5f
            ? 8f * t * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 4f) * 0.5f;
    }

    private static float EaseOutQuart(float t)
    {
        return 1f - Mathf.Pow(1f - t, 4f);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
