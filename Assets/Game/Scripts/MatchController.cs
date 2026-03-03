using System;
using System.Collections;
using System.Collections.Generic;
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
                                 (networkManager.IsServerStarted() || networkManager.IsConnected() || networkManager.IsClientConnection());

    public NetworkSteamManager steamManager => NetworkSteamManager.Instance();
    private List<Piece> allPieces;

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool hasStarted { get; private set; }
    public bool finished { get; private set; }
    private bool homeTeamTurn = false; //False to start with home, true for away

    public TurnState currentTurn { get; private set; }
    public TurnState myTurn { get; private set; }
    public TurnState turn { get; private set; }

    Callback<ClientDisconnectedEventArgs> clientDisconnected;
    Callback<ServerDisconnectedEventArgs> serverDisconnected;

    Callback<SteamNetConnectionStatusChangedCallback_t> steamConnectionStatusChanged;

    public SyncronizeTable syncronize;

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
    private Button exit;

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
        exit.gameObject.SetActive(false);

        Debug.Log($"[MatchController] Awake — IsConnected: {networkManager.IsConnected()} | IsServerConnection: {networkManager.IsServerConnection()} | IsClientConnection: {networkManager.IsClientConnection()} | myTurn: {myTurn}");

        if (cameraPos && networkManager.IsClientConnection())
        {
            cameraPos.transform.eulerAngles = new Vector3(0, 180, 0);
        }
    }

    void Start()
    {
        Debug.Log($"[MatchController] Start — IsServerConnection: {networkManager.IsServerConnection()} | IsClientConnection: {networkManager.IsClientConnection()} | IsConnected: {networkManager.IsConnected()} | HasConnection: {networkManager.HasConnection()} | hasStarted: {hasStarted}");

        if (networkManager.IsServerConnection())
        {
            // HOST: NÃO instancia em Start() — o cliente ainda pode estar carregando a cena.
            // OnClientConnected dispara após o cliente reconectar na nova cena, que é o
            // momento correto para replicar o SyncronizeTable.
            Debug.Log("[MatchController] Host online: aguardando OnClientConnected para instanciar SyncronizeTable");
        }
        else if (networkManager.IsClientConnection())
        {
            // CLIENT: aguarda SyncronizeTable replicado do host via OnClientConnected.
            Debug.Log("[MatchController] Client online: aguardando SyncronizeTable replicado do host");
        }
        else
        {
            // OFFLINE: jogo local sem rede.
            Debug.Log("[MatchController] Modo offline: iniciando jogo local");
            myTurn = TurnState.homeTeam;
            playerSquad.LoadPieces();
            enemySquad.LoadPieces();
            StartCoroutine(StartGame());
        }
    }

    private bool syncronizeTableInstantiated = false;

    private async void InstantiateSyncronizeTable()
    {
        if (syncronizeTableInstantiated) return;
        syncronizeTableInstantiated = true;
        Debug.Log("[MatchController] InstantiateSyncronizeTable — criando via rede");
        await NetworkGameObject.Instantiate(syncronize.gameObject, Vector3.up, Quaternion.identity);
    }

    private void OnEnable()
    {
        // clientDisconnected = Callback<ClientDisconnectedEventArgs>.Create(OnClientDisconnected);
        // serverDisconnected = Callback<ServerDisconnectedEventArgs>.Create(OnServerDisconnected);
        steamConnectionStatusChanged = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamConnectionStatusChanged);
    }

    private void OnDisable()
    {
        //clientDisconnected.Dispose();
       // serverDisconnected.Dispose();
        steamConnectionStatusChanged.Dispose();
    }

    public void StartGame(TableData clientTable)
    {
        Debug.Log("[MatchController] Starting game with client table as Parameter");
        playerSquad.LoadPieces();
        playerSquad.LoadPieces(clientTable);
        StartCoroutine(StartGame());
    }

    /// <summary>
    /// Sets hasStarted to true on the client side, called via network sync from the server.
    /// </summary>
    public void SetHasStarted()
    {
        if (hasStarted) return;
        hasStarted = true;
        Debug.Log("[MatchController] hasStarted definido como true (via sync do servidor)");
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        hasStarted = true;
        Debug.Log("[MatchController] Game has started");
        ChangeTurn();
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
        Debug.Log($"[MatchController] OnClientConnected disparado — instanciando SyncronizeTable");
        InstantiateSyncronizeTable();
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
        SceneLoadingHandler.LoadSceneWithLoading("PositionParts", "Retornando ao menu...");
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
        if (SyncronizeTable.instance == null)
        {
            ChangeTurnImmediate();
        }
        else
        {
            SyncronizeTable.instance.SetChangeTurn();
        }
    }

    public void ChangeTurnImmediate()
    {
        Debug.Log($"[MatchController] ChangeTurnImmediate — hasStarted: {hasStarted} | finished: {finished} | IsServer: {networkManager.IsServerConnection()}");

        if (finished) return;

        // Garante hasStarted = true no cliente: ChangeTurnImmediate chega via
        // NetworkExecute(ChangeTurn) e é o caminho confiável de sincronização.
        if (!hasStarted)
        {
            hasStarted = true;
            Debug.Log("[MatchController] hasStarted forçado para true via ChangeTurnImmediate (sync de rede)");
        }

        if (!game.activeSelf)
        {
            game.SetActive(true);
            ActivePieces();
        }
        else if (CheckEndGame()) return;

        homeTeamTurn = !homeTeamTurn;
        currentTurn = homeTeamTurn ? TurnState.homeTeam : TurnState.awayTeam;
        turn = currentTurn;

        Debug.Log($"[MatchController] Turno definido — currentTurn: {currentTurn} | myTurn: {myTurn} | IsMyTurn: {IsMyTurn()}");

        ResetPiecesForNewTurn();

        if (!hasConnection && currentTurn == TurnState.awayTeam)
        {
            machinePlayer.gameObject.SetActive(true);
            machinePlayer.StartTurn();
        }

        if (turnTimer != null)
        {
            turnTimer.OnTurnChanged();
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
        if (MatchEvents.isRanked == true)
        {
            PlayerProfileManager.Instance.AddPoints(50);   // Ganha 50 pts
        }
        //PlayerProfileManager.Instance.UpdateRankingPosition(3); // Atualizar posi��o no ranking:

        SetFinishGame(playerSquad.pieces.ToArray(), true);
        SetFinishGame(enemySquad.pieces.ToArray(), false);
    }

    private void SetEnemyWin()
    {
        if (finished) return;
        Debug.Log("Lost Game");
        if (MatchEvents.isRanked == true)
        {
            PlayerProfileManager.Instance.AddPoints(-20);  // Perde 20 pts (m�nimo 0)
        }
        //PlayerProfileManager.Instance.UpdateRankingPosition(3); // Atualizar posi��o no ranking:

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

        if(exit != null) exit.gameObject.SetActive(true);

        Invoke(nameof(CloseLobby), 2f);
    }

    public void Surrender()
    {
        SetEnemyWin();
        FinishGame();
    }
    public void SetFinishGame(Piece[] pieces, bool win)
    {
        if (pieces.Length == 0) return;
        foreach (Piece piece in pieces)
        {
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