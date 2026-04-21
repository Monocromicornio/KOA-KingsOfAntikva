using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MenuFlowController : MonoBehaviour
{
    // ─── Durations ───────────────────────────────────────────────────────────
    private const float ANIM_DURATION = 0.4f;
    private const float ANIM_STAGGER  = 0.07f;

    // ─── Main Menu elements ──────────────────────────────────────────────────
    [Header("Main Menu Elements")]
    public RectTransform playerInfos;
    public RectTransform playButton;
    public RectTransform rankingButton;
    public RectTransform configButton;

    // ─── Play Modes Panel ────────────────────────────────────────────────────
    [Header("Play Modes Panel")]
    public GameObject playModesPanel;

    [Tooltip("Card Partida Rankeada – anima da direita")]
    public RectTransform cardRanked;

    [Tooltip("Card Criar Lobby – anima do topo")]
    public RectTransform cardLobby;

    [Tooltip("Card Modo Offline – anima da esquerda")]
    public RectTransform cardOffline;

    public Button backButton;

    // ─── Ranked search UI ────────────────────────────────────────────────────
    [Header("Ranked Search UI")]
    [Tooltip("Container /HUD/Cancelar com Timer e Status que aparece durante a busca")]
    public GameObject cancelarPanel;

    [Tooltip("Componente TMP Timer dentro do Cancelar")]
    public TextMeshProUGUI timerText;

    public Button cancelarButton;

    // ─── Lobby Panel (Steam Viewer) ──────────────────────────────────────────
    [Header("Lobby Panel")]
    public GameObject steamViewer;
    public Button closeLobbyButton;

    // ─── Ranking & Config Panels ─────────────────────────────────────────────
    [Header("Ranking & Config Panels")]
    public GameObject rankingHUD;
    public Button closeRankingButton;
    public GameObject configsHUD;
    public Button closeConfigButton;

    // ─── Exit Panel ──────────────────────────────────────────────────────────
    [Header("Exit Panel")]
    public GameObject exitPanel;
    public Button exitButton;
    public Button exitYesButton;
    public Button exitCancelButton;

    // ─── Dependencies ────────────────────────────────────────────────────────
    [Header("Dependencies")]
    [Tooltip("Referência ao SavePieceOrder para chamar SavePieces() antes de abrir o painel de modos")]
    public SavePieceOrder savePieceOrder;

    // ─── Cached resting anchoredPositions ────────────────────────────────────
    private Vector2 _playerInfosRest;
    private Vector2 _playButtonRest;
    private Vector2 _rankingButtonRest;
    private Vector2 _configButtonRest;

    private Vector2 _cardRankedRest;
    private Vector2 _cardLobbyRest;
    private Vector2 _cardOfflineRest;

    // ─── Canvas reference size ───────────────────────────────────────────────
    private RectTransform _canvasRect;

    // ─── Ranked search state ─────────────────────────────────────────────────
    private bool _isSearching;
    private float _searchStartTime;
    private Coroutine _timerCoroutine;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        CacheRestPositions();
        WireButtons();
    }

    private void CacheRestPositions()
    {
        if (playerInfos)   _playerInfosRest  = playerInfos.anchoredPosition;
        if (playButton)    _playButtonRest    = playButton.anchoredPosition;
        if (rankingButton) _rankingButtonRest = rankingButton.anchoredPosition;
        if (configButton)  _configButtonRest  = configButton.anchoredPosition;

        if (cardRanked)  _cardRankedRest  = cardRanked.anchoredPosition;
        if (cardLobby)   _cardLobbyRest   = cardLobby.anchoredPosition;
        if (cardOffline) _cardOfflineRest = cardOffline.anchoredPosition;
    }

    private void WireButtons()
    {
        if (playButton != null)
        {
            var btn = playButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnPlayButtonClicked);
            }
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        if (cancelarButton != null)
        {
            cancelarButton.onClick.RemoveAllListeners();
            cancelarButton.onClick.AddListener(OnCancelSearchClicked);
        }

        if (closeLobbyButton != null)
        {
            closeLobbyButton.onClick.RemoveAllListeners();
            closeLobbyButton.onClick.AddListener(OnCloseLobbyClicked);
        }

        // Ranking
        var rankingBtn = rankingButton?.GetComponent<Button>();
        if (rankingBtn != null)
        {
            rankingBtn.onClick.RemoveAllListeners();
            rankingBtn.onClick.AddListener(OnRankingClicked);
        }
        if (closeRankingButton != null)
            closeRankingButton.onClick.AddListener(OnCloseRankingClicked);

        // Config
        var configBtn = configButton?.GetComponent<Button>();
        if (configBtn != null)
        {
            configBtn.onClick.RemoveAllListeners();
            configBtn.onClick.AddListener(OnConfigClicked);
        }
        if (closeConfigButton != null)
            closeConfigButton.onClick.AddListener(OnCloseConfigClicked);

        if (cardRanked  != null) cardRanked.GetComponent<Button>()?.onClick.AddListener(OnRankedClicked);
        if (cardLobby   != null) cardLobby.GetComponent<Button>()?.onClick.AddListener(OnLobbyClicked);
        if (cardOffline != null) cardOffline.GetComponent<Button>()?.onClick.AddListener(OnOfflineClicked);

        // Exit
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitClicked);
        }
        if (exitYesButton != null)
        {
            exitYesButton.onClick.RemoveAllListeners();
            exitYesButton.onClick.AddListener(OnExitYesClicked);
        }
        if (exitCancelButton != null)
        {
            exitCancelButton.onClick.RemoveAllListeners();
            exitCancelButton.onClick.AddListener(OnExitCancelClicked);
        }
    }

    // ─── Public Handlers ─────────────────────────────────────────────────────

    /// <summary>Clicou em Jogar: exibe PlayModesPanel imediatamente e esconde o menu em paralelo.</summary>
    public void OnPlayButtonClicked()
    {
        savePieceOrder?.SavePieces();
        playModesPanel.SetActive(true);
        AnimateCardsIn();
        HideMainMenu();
    }

    /// <summary>Clicou em Voltar: desativa PlayModesPanel instantaneamente e restaura o menu.</summary>
    public void OnBackClicked()
    {
        playModesPanel.SetActive(false);
        ShowMainMenu();
    }

    /// <summary>Modo Offline: fecha o painel; a lógica de jogo já segue via SavePieceOrder.Offline.</summary>
    public void OnOfflineClicked()
    {
        AnimateCardsOut(() => playModesPanel.SetActive(false));
    }

    /// <summary>Criar Lobby: fecha PlayModesPanel e abre Steam Viewer animado.</summary>
    public void OnLobbyClicked()
    {
        AnimateCardsOut(() =>
        {
            playModesPanel.SetActive(false);
            OpenSteamViewer();
        });
    }

    /// <summary>Fecha Steam Viewer e restaura menu inicial.</summary>
    public void OnCloseLobbyClicked()
    {
        CloseSteamViewer(() => ShowMainMenu());
    }

    /// <summary>Partida Rankeada: fecha PlayModesPanel, restaura menu e inicia busca.</summary>
    public void OnRankedClicked()
    {
        AnimateCardsOut(() =>
        {
            playModesPanel.SetActive(false);
            ShowMainMenu(() => StartRankedSearch());
        });
    }

    /// <summary>Cancela a busca rankeada.</summary>
    public void OnCancelSearchClicked()
    {
        StopRankedSearch();
    }

    /// <summary>Abre o painel de Ranking com animação de slide de cima para baixo.</summary>
    public void OnRankingClicked()   => SlideInFromTop(rankingHUD);

    /// <summary>Fecha o painel de Ranking com animação de slide para cima.</summary>
    public void OnCloseRankingClicked() => SlideOutToTop(rankingHUD);

    /// <summary>Abre o painel de Configurações com animação de slide de cima para baixo.</summary>
    public void OnConfigClicked()    => SlideInFromTop(configsHUD);

    /// <summary>Fecha o painel de Configurações com animação de slide para cima.</summary>
    public void OnCloseConfigClicked() => SlideOutToTop(configsHUD);

    /// <summary>Abre o painel de saída com animação de escala.</summary>
    public void OnExitClicked() => OpenPanel(exitPanel);

    /// <summary>Confirma a saída e fecha o jogo.</summary>
    public void OnExitYesClicked()
    {
        Debug.Log("[MenuFlowController] Exiting application...");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>Cancela a saída e fecha o painel.</summary>
    public void OnExitCancelClicked() => ClosePanel(exitPanel);

    // ─── Main Menu Animations ────────────────────────────────────────────────

    private void HideMainMenu(System.Action onComplete = null)
    {
        float w = _canvasRect.rect.width;
        float h = _canvasRect.rect.height;

        int pending = 0;
        System.Action decrement = () => { if (--pending == 0) onComplete?.Invoke(); };

        // PlayerInfos sai pela esquerda
        if (playerInfos)
        {
            pending++;
            StartCoroutine(MoveAnchored(playerInfos, _playerInfosRest,
                new Vector2(_playerInfosRest.x - w, _playerInfosRest.y),
                0f, decrement));
        }

        // PlayButton sai pela direita
        if (playButton)
        {
            pending++;
            StartCoroutine(MoveAnchored(playButton, _playButtonRest,
                new Vector2(_playButtonRest.x + w, _playButtonRest.y),
                ANIM_STAGGER, decrement));
        }

        // RankingButton sai pelo topo
        if (rankingButton)
        {
            pending++;
            StartCoroutine(MoveAnchored(rankingButton, _rankingButtonRest,
                new Vector2(_rankingButtonRest.x, _rankingButtonRest.y + h),
                0f, decrement));
        }

        // ConfigButton sai pelo topo
        if (configButton)
        {
            pending++;
            StartCoroutine(MoveAnchored(configButton, _configButtonRest,
                new Vector2(_configButtonRest.x, _configButtonRest.y + h),
                ANIM_STAGGER, decrement));
        }

        if (pending == 0) onComplete?.Invoke();
    }

    private void ShowMainMenu(System.Action onComplete = null)
    {
        float w = _canvasRect.rect.width;
        float h = _canvasRect.rect.height;

        int pending = 0;
        System.Action decrement = () => { if (--pending == 0) onComplete?.Invoke(); };

        if (playerInfos)
        {
            pending++;
            playerInfos.anchoredPosition = new Vector2(_playerInfosRest.x - w, _playerInfosRest.y);
            StartCoroutine(MoveAnchored(playerInfos, playerInfos.anchoredPosition,
                _playerInfosRest, 0f, decrement));
        }

        if (playButton)
        {
            pending++;
            playButton.anchoredPosition = new Vector2(_playButtonRest.x + w, _playButtonRest.y);
            StartCoroutine(MoveAnchored(playButton, playButton.anchoredPosition,
                _playButtonRest, ANIM_STAGGER, decrement));
        }

        if (rankingButton)
        {
            pending++;
            rankingButton.anchoredPosition = new Vector2(_rankingButtonRest.x, _rankingButtonRest.y + h);
            StartCoroutine(MoveAnchored(rankingButton, rankingButton.anchoredPosition,
                _rankingButtonRest, 0f, decrement));
        }

        if (configButton)
        {
            pending++;
            configButton.anchoredPosition = new Vector2(_configButtonRest.x, _configButtonRest.y + h);
            StartCoroutine(MoveAnchored(configButton, configButton.anchoredPosition,
                _configButtonRest, ANIM_STAGGER, decrement));
        }

        if (pending == 0) onComplete?.Invoke();
    }

    // ─── PlayModes Cards Animations ──────────────────────────────────────────

    private void AnimateCardsIn()
    {
        float w = _canvasRect.rect.width;
        float h = _canvasRect.rect.height;

        // Ranked: começa à direita
        cardRanked.anchoredPosition  = new Vector2(_cardRankedRest.x  + w, _cardRankedRest.y);
        // Lobby: começa acima
        cardLobby.anchoredPosition   = new Vector2(_cardLobbyRest.x,  _cardLobbyRest.y  + h);
        // Offline: começa à esquerda
        cardOffline.anchoredPosition = new Vector2(_cardOfflineRest.x - w, _cardOfflineRest.y);

        StartCoroutine(MoveAnchored(cardRanked,  cardRanked.anchoredPosition,  _cardRankedRest,  0f,                null));
        StartCoroutine(MoveAnchored(cardLobby,   cardLobby.anchoredPosition,   _cardLobbyRest,   ANIM_STAGGER,      null));
        StartCoroutine(MoveAnchored(cardOffline, cardOffline.anchoredPosition, _cardOfflineRest, ANIM_STAGGER * 2f, null));
    }

    private void AnimateCardsOut(System.Action onComplete = null)
    {
        float w = _canvasRect.rect.width;
        float h = _canvasRect.rect.height;

        StartCoroutine(MoveAnchored(cardRanked,  _cardRankedRest,  new Vector2(_cardRankedRest.x  + w, _cardRankedRest.y),  0f,                null));
        StartCoroutine(MoveAnchored(cardLobby,   _cardLobbyRest,   new Vector2(_cardLobbyRest.x,  _cardLobbyRest.y  + h),  ANIM_STAGGER,      null));
        StartCoroutine(MoveAnchored(cardOffline, _cardOfflineRest, new Vector2(_cardOfflineRest.x - w, _cardOfflineRest.y), ANIM_STAGGER * 2f, onComplete));
    }

    // ─── Slide From Top Panel Open / Close ──────────────────────────────────

    /// <summary>Desliza o painel de cima para baixo até a posição central.</summary>
    private void SlideInFromTop(GameObject panel)
    {
        panel.SetActive(true);
        var rt = panel.GetComponent<RectTransform>();
        float canvasHeight = _canvasRect.rect.height;
        float panelHeight = rt.rect.height;

        Vector2 restPosition = Vector2.zero;
        Vector2 startPosition = new Vector2(restPosition.x, canvasHeight + panelHeight);

        rt.anchoredPosition = startPosition;
        StartCoroutine(MoveAnchored(rt, startPosition, restPosition, 0f, null));
    }

    /// <summary>Desliza o painel para cima até sair da tela e desativa.</summary>
    private void SlideOutToTop(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        float canvasHeight = _canvasRect.rect.height;
        float panelHeight = rt.rect.height;

        Vector2 currentPosition = rt.anchoredPosition;
        Vector2 targetPosition = new Vector2(currentPosition.x, canvasHeight + panelHeight);

        StartCoroutine(MoveAnchored(rt, currentPosition, targetPosition, 0f, () =>
        {
            panel.SetActive(false);
            rt.anchoredPosition = Vector2.zero;
        }));
    }

    // ─── Generic Panel Open / Close ──────────────────────────────────────────

    private void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
        var rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        StartCoroutine(ScaleUniform(rt, 0f, 1f, 0f, EaseOutBack, null));
    }

    private void ClosePanel(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        StartCoroutine(ScaleUniform(rt, 1f, 0f, 0f, EaseInQuart, () =>
        {
            panel.SetActive(false);
            rt.localScale = Vector3.one;
        }));
    }

    // ─── Steam Viewer ────────────────────────────────────────────────────────

    private void OpenSteamViewer()
    {
        steamViewer.SetActive(true);
        var rt = steamViewer.GetComponent<RectTransform>();
        rt.localScale = Vector3.zero;
        StartCoroutine(ScaleUniform(rt, 0f, 1f, 0f, EaseOutBack, null));
    }

    private void CloseSteamViewer(System.Action onComplete = null)
    {
        var rt = steamViewer.GetComponent<RectTransform>();
        StartCoroutine(ScaleUniform(rt, 1f, 0f, 0f, EaseInQuart, () =>
        {
            steamViewer.SetActive(false);
            rt.localScale = Vector3.one;
            onComplete?.Invoke();
        }));
    }

    // ─── Ranked Search ───────────────────────────────────────────────────────

    private void StartRankedSearch()
    {
        _isSearching     = true;
        _searchStartTime = Time.time;

        if (playButton)    playButton.gameObject.SetActive(false);
        if (cancelarPanel) cancelarPanel.SetActive(true);

        _timerCoroutine = StartCoroutine(UpdateSearchTimer());
    }

    private void StopRankedSearch()
    {
        _isSearching = false;

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        if (cancelarPanel) cancelarPanel.SetActive(false);
        if (playButton)    playButton.gameObject.SetActive(true);
        if (timerText)     timerText.text = "00:00";
    }

    private IEnumerator UpdateSearchTimer()
    {
        while (_isSearching)
        {
            float elapsed = Time.time - _searchStartTime;
            int minutes   = Mathf.FloorToInt(elapsed / 60f);
            int seconds   = Mathf.FloorToInt(elapsed % 60f);

            if (timerText != null)
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            yield return null;
        }
    }

    // ─── Core animation coroutines ───────────────────────────────────────────

    /// <summary>Interpola anchoredPosition com easing EaseOutBack.</summary>
    private IEnumerator MoveAnchored(RectTransform rt, Vector2 from, Vector2 to,
                                     float delay, System.Action onComplete)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / ANIM_DURATION, 1f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutBack(t));
            yield return null;
        }

        rt.anchoredPosition = to;
        onComplete?.Invoke();
    }

    /// <summary>Interpola localScale uniformemente com a função de easing fornecida.</summary>
    private IEnumerator ScaleUniform(RectTransform rt, float from, float to,
                                     float delay, System.Func<float, float> easing,
                                     System.Action onComplete)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / ANIM_DURATION, 1f);
            float s = Mathf.LerpUnclamped(from, to, easing(t));
            rt.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        rt.localScale = new Vector3(to, to, 1f);
        onComplete?.Invoke();
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
