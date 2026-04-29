using System.Collections;
using UnityEngine;

/// <summary>
/// Controla a visibilidade do minimapa com animações de escala e fade.
/// O GameObject nunca é desativado — apenas o alpha e a escala são animados.
/// Chame Toggle() para alternar, ou Show() / Hide() diretamente.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MinimapToggle : MonoBehaviour
{
    [Header("Animação")]
    [Tooltip("Duração da animação de aparecer/desaparecer em segundos")]
    public float animationDuration = 0.25f;

    [Tooltip("Escala mínima durante a animação de saída (0 = desaparece completamente)")]
    [Range(0f, 1f)]
    public float minScale = 0f;

    [Header("Estado Inicial")]
    [Tooltip("Se verdadeiro, o minimapa começa visível. Se falso, começa oculto.")]
    public bool startVisible = false;

    [Tooltip("Se verdadeiro, ignora o estado inicial e aguarda o MatchIntroAnimator controlar o alpha de entrada.")]
    public bool controlledByIntroAnimator = false;

    // Escala original capturada no Awake — preserva o valor configurado no Inspector
    private Vector3 _restScale;
    private CanvasGroup _canvasGroup;
    private Coroutine _activeCoroutine;
    private bool _isVisible = false;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _restScale   = transform.localScale;

        if (controlledByIntroAnimator)
        {
            // O MatchIntroAnimator gerencia o alpha de entrada.
            // Apenas garante que a escala está pronta e o estado interno é coerente.
            _isVisible = true;
            transform.localScale = _restScale;
            return;
        }

        if (startVisible)
        {
            // Começa visível sem animação
            _isVisible                    = true;
            _canvasGroup.alpha            = 1f;
            _canvasGroup.interactable     = true;
            _canvasGroup.blocksRaycasts   = true;
            transform.localScale          = _restScale;
        }
        else
        {
            // Começa oculto sem animação
            _isVisible                    = false;
            _canvasGroup.alpha            = 0f;
            _canvasGroup.interactable     = false;
            _canvasGroup.blocksRaycasts   = false;
            transform.localScale          = _restScale * minScale;
        }
    }

    // ─── API pública ─────────────────────────────────────────────────────────

    /// <summary>Alterna entre visível e invisível.</summary>
    public void Toggle()
    {
        if (_isVisible) Hide();
        else            Show();
    }

    /// <summary>Exibe o minimapa com animação de crescimento.</summary>
    public void Show()
    {
        if (_isVisible) return;
        _isVisible = true;

        // Ativa interação imediatamente ao iniciar o show
        _canvasGroup.interactable   = true;
        _canvasGroup.blocksRaycasts = true;

        RunAnimation(0f, 1f, minScale, 1f);
    }

    /// <summary>Oculta o minimapa com animação de encolhimento.</summary>
    public void Hide()
    {
        if (!_isVisible) return;
        _isVisible = false;

        // Desativa interação imediatamente ao iniciar o hide
        _canvasGroup.interactable   = false;
        _canvasGroup.blocksRaycasts = false;

        RunAnimation(1f, 0f, 1f, minScale);
    }

    // ─── Animação ────────────────────────────────────────────────────────────

    private void RunAnimation(float alphaFrom, float alphaTo,
                               float scaleFrom, float scaleTo)
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);

        _activeCoroutine = StartCoroutine(Animate(alphaFrom, alphaTo, scaleFrom, scaleTo));
    }

    private IEnumerator Animate(float alphaFrom, float alphaTo,
                                 float scaleFrom, float scaleTo)
    {
        float t = 0f;

        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / animationDuration, 1f);

            float eased = alphaTo >= alphaFrom ? EaseOutBack(t) : EaseInQuart(t);

            _canvasGroup.alpha  = Mathf.Lerp(alphaFrom, alphaTo, Mathf.Clamp01(t));
            float s             = Mathf.LerpUnclamped(scaleFrom, scaleTo, eased);
            transform.localScale = _restScale * s;

            yield return null;
        }

        _canvasGroup.alpha   = alphaTo;
        transform.localScale = _restScale * scaleTo;
        _activeCoroutine     = null;
    }

    // ─── Easing ──────────────────────────────────────────────────────────────

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private static float EaseInQuart(float t) => t * t * t * t;
}
