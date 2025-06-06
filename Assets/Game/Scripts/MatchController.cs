using System.Collections;
using System.Collections.Generic;
using com.onlineobject.objectnet;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController instance;
    public static NetworkConnectionType connection = NetworkConnectionType.Manual;
    public NetworkManager networkManager => NetworkManager.Instance();

    [Header("Game objs")]
    public BoardController boardController;

    public GameMode gameMode;
    public GameMode.GameType gameType => gameMode.type;

    public bool finished { get; private set; }

    public Piece currentePiece { get; private set; }
    private bool blueTurn = false; //False to start with blue, true for red
    public TurnState turn { get; private set; }

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
    }

    private void Start()
    {
        if (connection == NetworkConnectionType.Manual)
        {
            StartCoroutine(LoadGame());
        }
        else
        {
            StartNetwork();
        }
    }

    private void StartNetwork()
    {
        networkManager.ConfigureMode(connection);
        networkManager.SetServerAddress("127.0.0.1");
        networkManager.StartNetwork();
    }

    public void StartGame()
    {
        StartCoroutine(LoadGame());
    }

    public void OnConnected(IClient client)
    {
        StartGame();
    }

    public void OnConnectedClient(IClient client)
    {
        StartGame();
    }

    public void OnConnectedT(IChannel client)
    {
        StartGame();
    }

    private IEnumerator LoadGame()
    {
        while (!boardController.isFinished())
        {
            yield return new WaitForEndOfFrame();
        }

        game.SetActive(true);
        SpawnPieces();
        StartCoroutine(ChangeTurn(0));
    }

    public void SpawnPieces()
    {
        bool fromStart = connection != NetworkConnectionType.Client;
        playerSquad.LoadPieces(fromStart);
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
        if (finished) return;
        bool endGame = CheckEndGame();
        if (endGame)
        {
            WinGame();
            return;
        }

        StartCoroutine(ChangeTurn(2));
    }

    private IEnumerator ChangeTurn(float time)
    {
        print("---------------------|||------------------------");
        yield return new WaitForSeconds(time);
        blueTurn = !blueTurn;
        turn = blueTurn ? TurnState.blue : TurnState.red;
        print("É a vez do " + turn);
        if (turn == TurnState.red)
        {
            machinePlayer.StartTurn();
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
