using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MenuFlowController : MonoBehaviour
{
    // ─── Durations ───────────────────────────────────────────────────────────
    private const float ANIM_DURATION = 0.4f;
    private const float ANIM_STAGGER  = 0.07f;

    // ─── Room slot label ─────────────────────────────────────────────────────
    private const string ROOM_LABEL_FORMAT = "Sala ({0})";

    // ─── Main Menu elements ──────────────────────────────────────────────────
    [Header("Main Menu Elements")]
    public RectTransform playerInfos;
    public RectTransform playButton;
    public RectTransform cancelSearchButton;
    public RectTransform rightMenuBackground;

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

    // ─── Lobby Panel (Steam Viewer) ──────────────────────────────────────────
    [Header("Lobby Panel")]
    public GameObject steamViewer;
    public Button closeLobbyButton;

    // ─── Room Slot (ocupa a área do Play Button) ─────────────────────────────
    [Header("Room Slot")]
    [Tooltip("Cancel_Search_Button/Room_Infos/Status – exibe o nome da sala criada ou o status da busca")]
    public TextMeshProUGUI roomStatusText;

    [Tooltip("Cancel_Search_Button/Room_Infos/Timer – usado apenas na busca rankeada")]
    public GameObject roomTimer;

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
    private Vector2 _cancelSearchButtonRest;
    private Vector2 _rightMenuBackgroundRest;

    private Vector2 _cardRankedRest;
    private Vector2 _cardLobbyRest;
    private Vector2 _cardOfflineRest;

    // ─── Canvas reference size ───────────────────────────────────────────────
    private RectTransform _canvasRect;

    // ─── Cached Play Button component ────────────────────────────────────────
    private Button _playButtonComponent;

    // ─── Ranked search state ─────────────────────────────────────────────────
    private bool _isSearching;

    // ─── Room slot state (Cancel_Search_Button substitui o Play Button) ──────
    private bool _isRoomSlotActive;

    /// <summary>Indica se a busca rankeada está em andamento.</summary>
    public bool IsSearching => _isSearching;

    /// <summary>Indica se o painel da sala está ocupando a área do botão Play.</summary>
    public bool IsRoomSlotActive => _isRoomSlotActive;

    /// <summary>
    /// O slot Play só aceita cliques fora dos estados de busca rankeada e de sala criada.
    /// </summary>
    public bool IsPlaySlotInteractable => !_isSearching && !_isRoomSlotActive;

    /// <summary>RectTransform que ocupa a área do botão Play no estado atual.</summary>
    private RectTransform CurrentPlaySlot => _isRoomSlotActive ? cancelSearchButton : playButton;

    /// <summary>Posição de repouso do RectTransform que ocupa a área do botão Play.</summary>
    private Vector2 CurrentPlaySlotRest => _isRoomSlotActive ? _cancelSearchButtonRest : _playButtonRest;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

        DisableRoomStatusLocalization();
        CacheRestPositions();
        WireButtons();
    }

    /// <summary>
    /// O texto da sala é definido em runtime; o LocalizedText do mesmo objeto
    /// sobrescreveria o nome do lobby pelo fallback ao ser reativado.
    /// </summary>
    private void DisableRoomStatusLocalization()
    {
        if (roomStatusText == null) return;

        var localizedText = roomStatusText.GetComponent<LocalizedText>();
        if (localizedText != null) localizedText.enabled = false;
    }

    private void CacheRestPositions()
    {
        if (playerInfos)          _playerInfosRest          = playerInfos.anchoredPosition;
        if (playButton)           _playButtonRest            = playButton.anchoredPosition;
        if (cancelSearchButton)   _cancelSearchButtonRest    = cancelSearchButton.anchoredPosition;
        if (rightMenuBackground)  _rightMenuBackgroundRest   = rightMenuBackground.anchoredPosition;

        if (cardRanked)  _cardRankedRest  = cardRanked.anchoredPosition;
        if (cardLobby)   _cardLobbyRest   = cardLobby.anchoredPosition;
        if (cardOffline) _cardOfflineRest = cardOffline.anchoredPosition;

        // Cancel fica escondido atrás do Play no início
        _isRoomSlotActive = false;
        if (cancelSearchButton) cancelSearchButton.gameObject.SetActive(false);
        if (playButton)         playButton.gameObject.SetActive(true);
    }

    private void WireButtons()
    {
        if (playButton != null)
        {
            _playButtonComponent = playButton.GetComponent<Button>();
            if (_playButtonComponent != null)
            {
                _playButtonComponent.onClick.RemoveAllListeners();
                _playButtonComponent.onClick.AddListener(OnPlayButtonClicked);
            }
        }

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        // Cancel_Search_Button cancela a busca rankeada ou fecha a sala criada.
        // NÃO usar RemoveAllListeners aqui: os managers de lobby também se registram
        // neste botão e a ordem de Awake/Start não é garantida.
        if (cancelSearchButton != null)
        {
            var btn = cancelSearchButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnCancelSearchClicked);
        }

        if (closeLobbyButton != null)
        {
            closeLobbyButton.onClick.RemoveAllListeners();
            closeLobbyButton.onClick.AddListener(OnCloseLobbyClicked);
        }

        // Ranking e Config – botões dentro do Right_Menu_BackGround
        if (closeRankingButton != null)
            closeRankingButton.onClick.AddListener(OnCloseRankingClicked);

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
        // Durante a busca rankeada ou com a sala criada o slot é apenas informativo:
        // o painel de modos só volta a abrir após cancelar a busca ou fechar a sala.
        if (!IsPlaySlotInteractable)
        {
            Debug.Log("[MenuFlowController] Play bloqueado: busca ou sala em andamento");
            return;
        }

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

    /// <summary>
    /// Chamado quando a sala foi criada: fecha o Steam Viewer, troca o botão Play
    /// pelo painel da sala (com o nome do lobby) e exibe o botão de cancelar.
    /// </summary>
    /// <param name="lobbyName">Nome informado pelo jogador para a sala.</param>
    public void EnterLobbyRoom(string lobbyName)
    {
        string roomLabel = string.IsNullOrEmpty(lobbyName)
            ? "Aguardando jogadores..."
            : string.Format(ROOM_LABEL_FORMAT, lobbyName);

        if (playModesPanel != null) playModesPanel.SetActive(false);

        if (steamViewer != null && steamViewer.activeSelf)
        {
            CloseSteamViewer(() =>
            {
                ActivateRoomSlot(roomLabel, false);
                ShowMainMenu();
            });
        }
        else
        {
            ActivateRoomSlot(roomLabel, false);
            ShowMainMenu();
        }
    }

    /// <summary>Sai do estado de sala e devolve o botão Play, sem tocar na rede.</summary>
    public void ExitLobbyRoom()
    {
        DeactivateRoomSlot();
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

    /// <summary>Cancela a busca rankeada ou fecha a sala criada e restaura o botão Play.</summary>
    public void OnCancelSearchClicked()
    {
        LobbyCleanupHelper.CloseLobbyProperly();
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

        // PlayButton (ou o painel da sala, quando ativo) sai pela direita
        RectTransform hidingSlot = CurrentPlaySlot;
        if (hidingSlot)
        {
            Vector2 slotRest = CurrentPlaySlotRest;
            pending++;
            StartCoroutine(MoveAnchored(hidingSlot, slotRest,
                new Vector2(slotRest.x + w, slotRest.y),
                ANIM_STAGGER, decrement));
        }

        // Right_Menu_BackGround (ranking + config + exit) sai pelo topo
        if (rightMenuBackground)
        {
            pending++;
            StartCoroutine(MoveAnchored(rightMenuBackground, _rightMenuBackgroundRest,
                new Vector2(_rightMenuBackgroundRest.x, _rightMenuBackgroundRest.y + h),
                0f, decrement));
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

        RectTransform showingSlot = CurrentPlaySlot;
        if (showingSlot)
        {
            Vector2 slotRest = CurrentPlaySlotRest;
            pending++;
            showingSlot.anchoredPosition = new Vector2(slotRest.x + w, slotRest.y);
            StartCoroutine(MoveAnchored(showingSlot, showingSlot.anchoredPosition,
                slotRest, ANIM_STAGGER, decrement));
        }

        if (rightMenuBackground)
        {
            pending++;
            rightMenuBackground.anchoredPosition = new Vector2(_rightMenuBackgroundRest.x, _rightMenuBackgroundRest.y + h);
            StartCoroutine(MoveAnchored(rightMenuBackground, rightMenuBackground.anchoredPosition,
                _rightMenuBackgroundRest, 0f, decrement));
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
        StartCoroutine(MoveAnchoredSmooth(rt, startPosition, restPosition, 0f, null));
    }

    /// <summary>Desliza o painel para cima até sair da tela e desativa.</summary>
    private void SlideOutToTop(GameObject panel)
    {
        var rt = panel.GetComponent<RectTransform>();
        float canvasHeight = _canvasRect.rect.height;
        float panelHeight = rt.rect.height;

        Vector2 currentPosition = rt.anchoredPosition;
        Vector2 targetPosition = new Vector2(currentPosition.x, canvasHeight + panelHeight);

        StartCoroutine(MoveAnchoredSmooth(rt, currentPosition, targetPosition, 0f, () =>
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

    // ─── Room Slot / Ranked Search ───────────────────────────────────────────

    /// <summary>Substitui o botão Play pelo painel da sala e exibe o botão de cancelar.</summary>
    /// <param name="roomLabel">Texto a exibir; null mantém o texto atual.</param>
    /// <param name="showTimer">Exibe o cronômetro (usado apenas na busca rankeada).</param>
    private void ActivateRoomSlot(string roomLabel, bool showTimer)
    {
        _isRoomSlotActive = true;

        SetPlayButtonInteractable(false);

        if (playButton)
        {
            playButton.anchoredPosition = _playButtonRest;
            playButton.gameObject.SetActive(false);
        }

        if (roomTimer) roomTimer.SetActive(showTimer);

        if (cancelSearchButton)
        {
            cancelSearchButton.anchoredPosition = _cancelSearchButtonRest;
            cancelSearchButton.gameObject.SetActive(true);
        }

        if (roomStatusText != null && roomLabel != null)
            roomStatusText.text = roomLabel;
    }

    /// <summary>Esconde o painel da sala e devolve o botão Play à sua posição de repouso.</summary>
    private void DeactivateRoomSlot()
    {
        _isRoomSlotActive = false;

        if (cancelSearchButton)
        {
            cancelSearchButton.gameObject.SetActive(false);
            cancelSearchButton.anchoredPosition = _cancelSearchButtonRest;
        }

        if (roomTimer) roomTimer.SetActive(true);

        if (playButton)
        {
            playButton.anchoredPosition = _playButtonRest;
            playButton.gameObject.SetActive(true);
        }

        SetPlayButtonInteractable(true);
    }

    /// <summary>
    /// Habilita ou bloqueia o clique no botão Play. Usado para garantir que o slot
    /// não abra o painel de modos enquanto há busca ou sala ativa.
    /// </summary>
    /// <param name="isInteractable">true libera o clique; false bloqueia.</param>
    private void SetPlayButtonInteractable(bool isInteractable)
    {
        if (_playButtonComponent != null)
            _playButtonComponent.interactable = isInteractable;
    }

    private void StartRankedSearch()
    {
        _isSearching = true;

        // O texto de status da busca é definido pelo UISteamLobbyList
        ActivateRoomSlot(null, true);
    }

    private void StopRankedSearch()
    {
        _isSearching = false;

        DeactivateRoomSlot();
    }

    // ─── Core animation coroutines ───────────────────────────────────────────

    /// <summary>Interpola anchoredPosition com easing EaseOutBack (usado no menu principal).</summary>
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

    /// <summary>Interpola anchoredPosition com easing EaseInOutQuad (sem bounce, usado em painéis).</summary>
    private IEnumerator MoveAnchoredSmooth(RectTransform rt, Vector2 from, Vector2 to,
                                           float delay, System.Action onComplete)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        float t = 0f;
        while (t < 1f)
        {
            t = Mathf.Min(t + Time.deltaTime / ANIM_DURATION, 1f);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseInOutQuad(t));
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

    private static float EaseInOutQuad(float t)
        => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
}
