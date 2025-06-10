using System.Collections;
using System.Collections.Generic;
using com.onlineobject.objectnet;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;
    public static NetworkConnectionType connection = NetworkConnectionType.Manual;
    public NetworkManager networkManager => NetworkManager.Instance();
    private List<Piece> allPieces;

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool finished { get; private set; }

    public Piece currentePiece { get; private set; }
    private bool homeTeamTurn = true; //False to start with home, true for away
    public TurnState turn { get; private set; }

    public SyncronizeTable syncronize;

    [SerializeField]
    private GameObject game;
    public PlayerSquad playerSquad;
    public EnemySquad enemySquad;
    public MachinePlayer machinePlayer;

    [Header("Feedback")]
    public SoundController soundController;
    [SerializeField]
    private AudioSource auChangeTurn;

    private void Awake()
    {
        instance = this;
        game.SetActive(false);
        turn = TurnState.wait;
        allPieces = new List<Piece>();
    }

    private void Start()
    {
        StartNetwork();
    }

    private void StartNetwork()
    {
        networkManager.ConfigureMode(connection);
        networkManager.SetServerAddress("127.0.0.1");
        networkManager.StartNetwork();
    }

    public void StartGame(TableData clientTable)
    {
        //SpawnTeste(); return;
        playerSquad.LoadPieces();
        playerSquad.LoadPieces(clientTable);
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(2);
        ChangeTurn();
    }

    public async void OnClientConnected(IClient client)
    {
        await NetworkGameObject.Instantiate(syncronize.gameObject, Vector3.up, Quaternion.identity);
    }

    public void SetPiece(Piece piece)
    {
        currentePiece = piece;
    }

    public void RemovePieceFromPlayerSquad(Piece piece)
    {
        if (!playerSquad.pieces.Contains(piece)) return;
        playerSquad.pieces.Remove(piece);
    }

    public void RemovePieceFromEnemySquad(FakePiece fakePiece)
    {
        if (enemySquad.fakePieces.Contains(fakePiece))
        {
            enemySquad.fakePieces.Remove(fakePiece);
        }

        if (!enemySquad.pieces.Contains(fakePiece.piece)) return;
        enemySquad.pieces.Remove(fakePiece.piece);
    }

    public void MadeActionOnTurn()
    {
        turn = TurnState.wait;
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
        else
        {
            bool endGame = CheckEndGame();
            if (endGame)
            {
                WinGame();
                return;
            }
        }

        homeTeamTurn = !homeTeamTurn;
        turn = homeTeamTurn ? TurnState.homeTeam : TurnState.awayTeam;
        if (connection == NetworkConnectionType.Manual && turn == TurnState.awayTeam)
        {
            machinePlayer.StartTurn();
        }
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

    public void OpenChest(TrunckPiece piece)
    {
        if (finished) return;
        if (piece.bluePiece)
        {
            SetEnemyWin();
            return;
        }

        SetPlayerWin();
        WinGame();
    }

    private void SetPlayerWin()
    {
        SetFinishGame(playerSquad.pieces.ToArray(), true);
        SetFinishGame(enemySquad.pieces.ToArray(), false);
    }

    private void SetEnemyWin()
    {
        SetFinishGame(enemySquad.pieces.ToArray(), true);
        SetFinishGame(playerSquad.pieces.ToArray(), false);
    }

    private void WinGame()
    {
        if (finished) return;
        finished = true;

        //VOLTAR AO MENU
    }

    public void SetFinishGame(Piece[] pieces, bool win)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.type == PieceType.Flag)
            {
                piece.SendMessage("OpenChest");
                continue;
            }
            piece.SendMessage(win? "Win" : "Lose");
        }
    }

    private bool CheckEndGame()
    {
        int players = CountActivePiece(playerSquad.pieces);
        if (players == 0)
        {
            SetEnemyWin();
            return true;
        }

        int enemies = CountActivePiece(enemySquad.pieces);
        if (enemies == 0)
        {
            SetPlayerWin();
            return true;
        }

        return false;
    }

    private int CountActivePiece(List<Piece> pieces)
    {
        int amount = 0;
        foreach (Piece piece in pieces)
        {
            if (piece.type == PieceType.Flag || piece.type == PieceType.Bomb) continue;
            amount++;
        }

        return amount;
    }
}
