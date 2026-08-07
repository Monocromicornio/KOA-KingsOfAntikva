using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.onlineobject.objectnet;
using com.onlineobject.objectnet.embedded;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;
    public NetworkManager networkManager => NetworkManager.Instance();
    public bool hasConnection => networkManager.HasConnection() &&
                                 (networkManager.IsServerStarted() || networkManager.IsConnected());

    public NetworkSteamManager steamManager => NetworkSteamManager.Instance();
    private List<Piece> allPieces;

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;


    public bool hasStarted { get; private set; }
    public bool finished { get; private set; }
    private bool _playerWon;

    public bool isLoadingScreenFinished;

    private bool changeTurnPending = false;

    private bool homeTeamTurn = false; //False to start with home, true for away

    public TurnState currentTurn { get; private set; }
    public TurnState myTurn { get; private set; }
    public TurnState turn { get; private set; }

    Callback<ClientDisconnectedEventArgs> clientDisconnected;
    Callback<ServerDisconnectedEventArgs> serverDisconnected;

    Callback<SteamNetConnectionStatusChangedCallback_t> steamConnectionStatusChanged;

    public SyncronizeTable syncronize;

    public OnlineTurnManager onlineTurnManager;
    [SerializeField]
    private GameObject game;
    public PlayerSquad playerSquad;
    public EnemySquad enemySquad;
    public MachinePlayer machinePlayer;
    public Transform cameraPos;

    [Header("Feedback")]
    public SoundController soundController;
    [SerializeField]
    private AudioSource auChangeTurn;

    [Header("UI")]
    [SerializeField]
    private GameResultScreenController resultScreen;

    [Header("Turn Timer")]
    [SerializeField]
    private TurnTimer turnTimer;

    private void Awake()
    {
        instance = this;
        hasStarted = false;
        game.SetActive(false);
        currentTurn = TurnState.wait;
        turn = TurnState.undefined;
        myTurn = networkManager.IsServerConnection() ? TurnState.homeTeam : TurnState.awayTeam;
        allPieces = new List<Piece>();

        if (cameraPos && networkManager.IsClientConnection())
        {
            cameraPos.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void Start()
    {
        if (networkManager.IsConnected())
        {
            _ = NetworkGameObject.Instantiate(syncronize.gameObject, Vector3.up, Quaternion.identity);
            _ = NetworkGameObject.Instantiate(onlineTurnManager.gameObject, Vector3.up, Quaternion.identity);
        }

        else if (!networkManager.IsServerConnection())
        {
            myTurn = TurnState.homeTeam;
            playerSquad.LoadPieces();
            enemySquad.LoadPieces();
            StartCoroutine(StartGame());
        }
    }

    private void OnEnable()
    {
        // clientDisconnected = Callback<ClientDisconnectedEventArgs>.Create(OnClientDisconnected);
        // serverDisconnected = Callback<ServerDisconnectedEventArgs>.Create(OnServerDisconnected);
        LoadingEvents.OnLoadingFinished += OnLoadingFinished;
        steamConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamConnectionStatusChanged);

    }

    private void OnDisable()
    {
        //clientDisconnected.Dispose();
        // serverDisconnected.Dispose();
        LoadingEvents.OnLoadingFinished -= OnLoadingFinished;
        steamConnectionStatusChanged.Dispose();
    }


    private void OnLoadingFinished()
    {
        if (networkManager.IsConnected())
        {
            _ = NetworkGameObject.Instantiate(onlineTurnManager.gameObject, Vector3.up, Quaternion.identity);
        }
    }
    public async void StartGame(TableData clientTable)
    {
        await Task.WhenAll(
            playerSquad.LoadPieces(),
            playerSquad.LoadPieces(clientTable)

        );
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);

        ChangeTurn();

        while (isLoadingScreenFinished == false)
        {
            yield return null;
        }

        if (turnTimer != null)
        {
            turnTimer.OnTurnChanged();
        }
    }

    public void OnSteamConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        Debug.Log("[MatchController] Steam Connection Status Changed to " + callback.m_info.m_eState);
        if (callback.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer)
        {
            SetPlayerWin();
            FinishGame();
        }
        else if (callback.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None)
        {
            FinishGame();
        }
    }


    public async void OnClientConnected(IClient client)
    {
        Debug.Log("[MatchController] Client connected");
        await NetworkGameObject.Instantiate(syncronize.gameObject, Vector3.up, Quaternion.identity);
    }

    public void OnClientDisconnected(ClientDisconnectedEventArgs client)
    {
        Debug.Log("[MatchController] Client disconnected with id " + client.Id);
        FinishGame();
    }

    public void OnServerDisconnected(ServerDisconnectedEventArgs server)
    {
        Debug.Log("[MatchController] Server disconnected");
        FinishGame();
    }
    public void Disconnected(IClient client)
    {
        GoToMenu();
    }

    public void OnError(Exception error)
    {
        Debug.Log("ERROR: " + error.Message);
        GoToMenu();
    }

    public void GoToMenu()
    {
        Debug.Log("[MatchController] Saindo da partida...");

        CloseLobby();

        StopAllCoroutines();

        Debug.Log("[MatchController] Carregando cena PositionParts...");
        SceneLoadingHandler.LoadSceneWithLoading("PositionParts", "Returning to the menu...");
    }

    public void CloseLobby()
    {
        if (hasConnection)
        {
            Debug.Log("[MatchController] Fechando lobby e desconectando...");
            LobbyCleanupHelper.CloseLobbyProperly();
        }

        SyncronizeTable.ResetAll();
    }

    /// <summary>
    /// Removes a player piece from playerSquad when it dies.
    /// Enemy pieces are removed via OnDestroyFakePiece when their GameObject is actually destroyed.
    /// </summary>
    public void OnDestroyPiece(Piece piece)
    {
        if (playerSquad.pieces.Contains(piece))
            playerSquad.pieces.Remove(piece);
    }

    public void OnDestroyFakePiece(FakePiece fakePiece)
    {
        if (enemySquad.fakePieces.Contains(fakePiece))
        {
            enemySquad.fakePieces.Remove(fakePiece);
        }

        if (enemySquad.pieces.Contains(fakePiece.piece))
        {
            enemySquad.pieces.Remove(fakePiece.piece);   
        }
    }

    public void AddPieceFromPlayerSquad(Piece piece)
    {
        if (playerSquad.pieces.Contains(piece)) return;
        playerSquad.pieces.Add(piece);
    }

    public void AddPieceFromEnemySquad(FakePiece fakePiece)
    {
        if (!enemySquad.fakePieces.Contains(fakePiece))
        {
            enemySquad.fakePieces.Add(fakePiece);
        }

        if (!enemySquad.pieces.Contains(fakePiece.piece))
        {
            enemySquad.pieces.Add(fakePiece.piece);
        }
    }

    public void MadeActionOnTurn()
    {
        currentTurn = TurnState.wait;
        
        if (turnTimer != null)
        {
            turnTimer.OnPlayerMadeMove();
        }
    }

    public void ChangeTurn()
    {
        // Duplicate-call guard is only needed online, where multiple code paths
        // (local piece coroutine + network packet receiver) can both fire for the same action.
        if (hasConnection && changeTurnPending)
        {
            Debug.LogWarning($"[MatchController] ChangeTurn BLOCKED — already pending (online). Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} on {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType?.Name}");
            return;
        }

        if (hasConnection) changeTurnPending = true;
        Debug.Log($"[MatchController] ChangeTurn called — pending={changeTurnPending}, turn={turn}, myTurn={myTurn}, Caller: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name} on {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().DeclaringType?.Name}");

        if (SyncronizeTable.Instance == null)
        {
            ChangeTurnImmediate();
        }
        else
        {
            OnlineTurnManager.Instance.SetChangeTurn();
        }
    }

    public void ChangeTurnImmediate()
    {
        Debug.Log($"[MatchController] ChangeTurnImmediate called — resetting pending. turn={turn}, myTurn={myTurn}");
        changeTurnPending = false;

        if (finished) return;
        if (!game.activeSelf)
        {
            game.SetActive(true);
            ActivePieces();
        }
        else if (CheckEndGame()) return;

        homeTeamTurn = !homeTeamTurn;
        currentTurn = homeTeamTurn ? TurnState.homeTeam : TurnState.awayTeam;
        turn = currentTurn;

        Debug.Log($"[MatchController] Turn changed to {turn}");

        ResetPiecesForNewTurn();

        if (!hasConnection && currentTurn == TurnState.awayTeam)
        {
            machinePlayer.gameObject.SetActive(true);
            machinePlayer.StartTurn();
        }
        
        if (turnTimer != null && isLoadingScreenFinished == true)
        {
            turnTimer.OnTurnChanged();
        }

        if(hasStarted == false)
        {
            hasStarted = true;
        }
    }

    public bool IsMyTurn()
    {
       // if (!hasConnection) return true;
        return myTurn == turn;
    }

    public void OnInstantiatedPiece(Piece piece)
    {
        if (allPieces.Contains(piece)) return;
        allPieces.Add(piece);
    }
    
    public void ActivePieces()
    {
        foreach (Piece piece in allPieces)
        {
            piece.ActivePiece();
        }
    }

    public void ResetPiecesForNewTurn()
    {
        foreach (Piece piece in allPieces)
        {
            piece.ResetTurnAction();
        }
    }

    private void SetPlayerWin()
    {
        if (finished) return;
        Debug.Log("Won Game");
        _playerWon = true;
        if (MatchEvents.isRanked == true)
        {
            PlayerProfileManager.Instance.AddPoints(50);   // Ganha 50 pts
        }

        SetFinishGame(playerSquad.pieces.ToArray(), true);
        SetFinishGame(enemySquad.pieces.ToArray(), false);
    }

    private void SetEnemyWin()
    {
        if (finished) return;
        Debug.Log("Lost Game");
        _playerWon = false;
        if (MatchEvents.isRanked == true)
        {
            PlayerProfileManager.Instance.AddPoints(-20);  // Perde 20 pts (mínimo 0)
        }

        SetFinishGame(enemySquad.pieces.ToArray(), true);
        SetFinishGame(playerSquad.pieces.ToArray(), false);
    }

    public void FinishGame()
    {
        if (finished) return;
        finished = true;
        currentTurn = TurnState.undefined;
        turn = currentTurn;

        if (turnTimer != null)
        {
            turnTimer.StopTimer();
        }

        // Show result screen
        if (resultScreen != null)
        {
            if (_playerWon)
                resultScreen.ShowWinScreen(50);
            else
                resultScreen.ShowLoseScreen(-20);
        }

        Invoke(nameof(CloseLobby), 2f);
    }

    public void Surrender()
    {
        SetEnemyWin();
        FinishGame();
    }
    /// <summary>
    /// Applies end-game state to the given pieces.
    /// Uses standard SetWin/SetLose with network sync so both clients see the result.
    /// </summary>
    public void SetFinishGame(Piece[] pieces, bool win)
    {
        if (pieces.Length == 0) return;
        foreach (Piece piece in pieces)
        {
            if (piece == null) continue;
            if (win) piece.SetWin();
            else piece.SetLose();
        }
    }

    private bool CheckEndGame()
    {
        int players = CountActivePiece(playerSquad.pieces);
        if (players == 0)
        {
            SetEnemyWin();
            if (finished) return true;
            FinishGame();
            return true;
        }

        int enemies = CountActivePiece(enemySquad.pieces);
        if (enemies == 0)
        {
            SetPlayerWin();
            if (finished) return true;
            FinishGame();
            return true;
        }

        return false;
    }

    private int CountActivePiece(List<Piece> pieces)
    {
        if (pieces.Count == 0) return 0;
        int amount = 0;
        foreach (Piece piece in pieces)
        {
            if (piece == null || piece.isDying) continue;

            if (piece.type == PieceType.Flag)
            {
                TrunckPiece trunckPiece = piece.GetComponent<TrunckPiece>();
                if (trunckPiece.opened) return 0;
                continue;
            }

            if (piece.type == PieceType.Bomb) continue;
            amount++;
        }

        return amount;
    }
}

public static class MatchEvents
{
    public static bool isRanked;

    public static void SetRankedMatch(bool ranked)
    {
        isRanked = ranked;
    }
}