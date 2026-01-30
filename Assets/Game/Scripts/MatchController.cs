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
    public bool hasConnection => networkManager.HasConnection();
    public NetworkSteamManager steamManager => NetworkSteamManager.Instance();
    private List<Piece> allPieces;

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

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

    private void Awake()
    {
        instance = this;
        game.SetActive(false);
        currentTurn = TurnState.wait;
        turn = TurnState.undefined;
        myTurn = networkManager.IsServerConnection() ? TurnState.homeTeam : TurnState.awayTeam;
        allPieces = new List<Piece>();
        exit.gameObject.SetActive(false);

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
        playerSquad.LoadPieces();
        playerSquad.LoadPieces(clientTable);
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        ChangeTurn();
    }

    public void OnSteamConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t callback)
    {
        Debug.Log("[MatchController] Steam Connection Status Changed to " + callback.m_info.m_eState);
        if (callback.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None)
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
        SceneManager.LoadScene("PositionParts");
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

        if (!hasConnection && currentTurn == TurnState.awayTeam)
        {
            machinePlayer.gameObject.SetActive(true);
            machinePlayer.StartTurn();
        }
    }

    public bool IsMyTurn()
    {
        if (!hasConnection) return true;
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

    private void SetPlayerWin()
    {
        PlayerProfileManager.Instance.AddPoints(50);   // Ganha 50 pts
       
        //PlayerProfileManager.Instance.UpdateRankingPosition(3); // Atualizar posição no ranking:

        SetFinishGame(playerSquad.pieces.ToArray(), true);
        SetFinishGame(enemySquad.pieces.ToArray(), false);
    }

    private void SetEnemyWin()
    {
        PlayerProfileManager.Instance.AddPoints(-20);  // Perde 20 pts (mínimo 0)

        //PlayerProfileManager.Instance.UpdateRankingPosition(3); // Atualizar posição no ranking:

        SetFinishGame(enemySquad.pieces.ToArray(), true);
        SetFinishGame(playerSquad.pieces.ToArray(), false);
    }

    public void FinishGame()
    {
        if (finished) return;
        finished = true;
        currentTurn = TurnState.undefined;
        turn = currentTurn;


        if(exit != null) exit.gameObject.SetActive(true);

        CloseLobby();
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
        if (finished) return true;

        int players = CountActivePiece(playerSquad.pieces);
        if (players == 0)
        {
            SetEnemyWin();
            FinishGame();
            return true;
        }

        int enemies = CountActivePiece(enemySquad.pieces);
        if (enemies == 0)
        {
            SetPlayerWin();
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
