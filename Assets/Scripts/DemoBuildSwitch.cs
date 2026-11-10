using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Um único check para alternar o menu entre a build normal e a build da Demo.
///
/// Demo (demoBuild ligado):
///   - PlayModesPanel/Tutorial (atalho)     -> desativado
///   - PlayModesPanel/BG/Ranked (Button)    -> interactable desligado
///   - PlayModesPanel/BG/Ranked/Locker      -> ativado
///   - PlayModesPanel/BG/Lobby              -> desativado
///   - PlayModesPanel/BG/Tutorial (card)    -> ativado
///   - MenuFlowController.cardLobby         -> aponta para o card Tutorial
///
/// Build normal (demoBuild desligado) é exatamente o contrário.
///
/// Colocar este componente no objeto "Scene" da cena PositionParts. As referências são preenchidas
/// sozinhas a partir do MenuFlowController. No Editor a mudança é aplicada ao marcar/desmarcar o check
/// (com Undo); em runtime ela é aplicada de novo no Awake, antes do MenuFlowController, para a build
/// sempre sair com o modo marcado aqui.
/// </summary>
[DefaultExecutionOrder(-100)]
public class DemoBuildSwitch : MonoBehaviour
{
    // Caminhos relativos ao MenuFlowController.playModesPanel
    private const string TutorialShortcutPath = "Tutorial";
    private const string RankedCardPath = "BG/Ranked";
    private const string RankedLockerPath = "BG/Ranked/Locker";
    private const string LobbyCardPath = "BG/Lobby";
    private const string TutorialCardPath = "BG/Tutorial";

    [Header("Build")]
    [Tooltip("Marcado: build da Demo (Ranked bloqueado e card Tutorial no lugar do Lobby). Desmarcado: build normal.")]
    [SerializeField] private bool demoBuild;

    [Header("Referências (preenchidas automaticamente)")]
    [SerializeField] private MenuFlowController menuFlow;
    [Tooltip("PlayModesPanel/Tutorial – botão de atalho do tutorial")]
    [SerializeField] private GameObject tutorialShortcut;
    [Tooltip("PlayModesPanel/BG/Ranked – o Button do card Ranked")]
    [SerializeField] private Button rankedButton;
    [Tooltip("PlayModesPanel/BG/Ranked/Locker – ícone de cadeado")]
    [SerializeField] private GameObject rankedLocker;
    [Tooltip("PlayModesPanel/BG/Lobby – card Criar Lobby")]
    [SerializeField] private GameObject lobbyCard;
    [Tooltip("PlayModesPanel/BG/Tutorial – card Tutorial usado na Demo")]
    [SerializeField] private GameObject tutorialCard;

    /// <summary>True quando a cena está configurada para a build da Demo.</summary>
    public bool IsDemoBuild => demoBuild;

    private void Awake()
    {
        Apply();
    }

    /// <summary>Aplica o modo escolhido no check a todos os objetos do menu.</summary>
    [ContextMenu("Aplicar agora")]
    public void Apply()
    {
#if UNITY_EDITOR
        _changedSomething = false;
#endif

        FindMissingReferences();
        if (menuFlow == null)
        {
            Debug.LogWarning("[DemoBuildSwitch] MenuFlowController não encontrado na cena; nada foi alterado.", this);
            return;
        }

        bool demo = demoBuild;

        SetActive(tutorialShortcut, !demo);
        SetInteractable(rankedButton, !demo);
        SetActive(rankedLocker, demo);
        SetActive(lobbyCard, !demo);
        SetActive(tutorialCard, demo);
        SetCardLobby(demo ? tutorialCard : lobbyCard);

#if UNITY_EDITOR
        if (_changedSomething && !Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            Debug.Log($"[DemoBuildSwitch] Menu configurado para a build {(demo ? "DEMO" : "NORMAL")}.", this);
        }
#endif
    }

    // ─── Referências ─────────────────────────────────────────────────────────

    private void FindMissingReferences()
    {
        if (menuFlow == null)
            menuFlow = FindFirstObjectByType<MenuFlowController>(FindObjectsInactive.Include);

        if (menuFlow == null || menuFlow.playModesPanel == null) return;

        Transform panel = menuFlow.playModesPanel.transform;

        bool changed = false;
        changed |= FillObject(ref tutorialShortcut, panel, TutorialShortcutPath);
        changed |= FillObject(ref rankedLocker, panel, RankedLockerPath);
        changed |= FillObject(ref lobbyCard, panel, LobbyCardPath);
        changed |= FillObject(ref tutorialCard, panel, TutorialCardPath);

        if (rankedButton == null)
        {
            Transform ranked = panel.Find(RankedCardPath);
            if (ranked != null)
            {
                rankedButton = ranked.GetComponent<Button>();
                changed |= rankedButton != null;
            }
        }

        if (tutorialShortcut == null || rankedButton == null || rankedLocker == null || lobbyCard == null || tutorialCard == null)
        {
            Debug.LogWarning("[DemoBuildSwitch] Alguma referência não foi encontrada em PlayModesPanel " +
                             "(Tutorial, BG/Ranked, BG/Ranked/Locker, BG/Lobby, BG/Tutorial). " +
                             "Arraste-a manualmente neste componente.", this);
        }

#if UNITY_EDITOR
        if (changed && !Application.isPlaying) EditorUtility.SetDirty(this);
#endif
    }

    private static bool FillObject(ref GameObject field, Transform panel, string path)
    {
        if (field != null) return false;

        Transform found = panel.Find(path);
        if (found == null) return false;

        field = found.gameObject;
        return true;
    }

    // ─── Alterações (com Undo no Editor) ─────────────────────────────────────

    private void SetActive(GameObject target, bool active)
    {
        if (target == null || target.activeSelf == active) return;

        RecordChange(target);
        target.SetActive(active);
    }

    private void SetInteractable(Button button, bool interactable)
    {
        if (button == null || button.interactable == interactable) return;

        RecordChange(button);
        button.interactable = interactable;
    }

    /// <summary>Faz o MenuFlowController animar/usar este card no lugar do "Card Lobby".</summary>
    private void SetCardLobby(GameObject card)
    {
        if (card == null) return;

        RectTransform rect = card.transform as RectTransform;
        if (rect == null || menuFlow.cardLobby == rect) return;

        RecordChange(menuFlow);
        menuFlow.cardLobby = rect;
    }

    /// <summary>Chamado antes de cada alteração: registra Undo e marca a cena como alterada (só no Editor).</summary>
    private void RecordChange(Object target)
    {
#if UNITY_EDITOR
        if (Application.isPlaying) return;

        _changedSomething = true;
        Undo.RecordObject(target, "Demo Build Switch");
        EditorUtility.SetDirty(target);
        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
#endif
    }

#if UNITY_EDITOR
    private bool _changedSomething;
    private bool _applyQueued;

    // Ao adicionar o componente, já procura as referências
    private void Reset()
    {
        FindMissingReferences();
    }

    // Marcar ou desmarcar o check no Inspector aplica a mudança (fora do Play Mode)
    private void OnValidate()
    {
        if (Application.isPlaying || EditorUtility.IsPersistent(this) || _applyQueued) return;

        _applyQueued = true;
        EditorApplication.delayCall += () =>
        {
            _applyQueued = false;
            if (this != null) Apply();
        };
    }
#endif
}
